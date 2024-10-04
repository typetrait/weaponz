using System.Numerics;

using Veldrid;

using WeaponZ.Game.Scene;

namespace WeaponZ.Game.Render;

public class Renderer
{
    public GraphicsDevice GraphicsDevice { get; }
    public Pipeline Pipeline { get; }

    public OutputDescription PipelineOutput => GraphicsDevice.SwapchainFramebuffer.OutputDescription;

    public ResourceSet ResourceSet { get; }

    public readonly CommandList _commandList;

    // Uniform buffers
    private readonly DeviceBuffer _projectionUniformBuffer;
    private readonly DeviceBuffer _viewUniformBuffer;
    private readonly DeviceBuffer _modelUniformBuffer;
    private readonly DeviceBuffer _lightingUniformBuffer;

    public Renderer(GraphicsDevice graphicsDevice)
    {
        GraphicsDevice = graphicsDevice;

        // Resource layout
        var resourceLayoutDescription = new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("ProjectionBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
            new ResourceLayoutElementDescription("ViewBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
            new ResourceLayoutElementDescription("ModelBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
            new ResourceLayoutElementDescription("LightingBuffer", ResourceKind.UniformBuffer, ShaderStages.Fragment)
        );

        var resourceLayout = GraphicsDevice.ResourceFactory.CreateResourceLayout(resourceLayoutDescription);

        // Uniform buffers
        _projectionUniformBuffer = GraphicsDevice.ResourceFactory.CreateBuffer(
            new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic)
        );

        _viewUniformBuffer = GraphicsDevice.ResourceFactory.CreateBuffer(
            new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic)
        );

        _modelUniformBuffer = GraphicsDevice.ResourceFactory.CreateBuffer(
            new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic)
        );

        _lightingUniformBuffer = GraphicsDevice.ResourceFactory.CreateBuffer(
            new BufferDescription(8224, BufferUsage.UniformBuffer | BufferUsage.Dynamic)
        );

        // Vertex layout descriptions
        VertexLayoutDescription vertexLayoutDescription = Vertex.GetLayout();

        // Shaders
        var sampleShaders = new SampleShaders(GraphicsDevice.ResourceFactory);
        var shaderSetDescription = new ShaderSetDescription([vertexLayoutDescription], sampleShaders.ShaderGroup1);

        // Resource set
        ResourceSet = GraphicsDevice.ResourceFactory.CreateResourceSet(
            new ResourceSetDescription(
                resourceLayout,
                _projectionUniformBuffer,
                _viewUniformBuffer,
                _modelUniformBuffer,
                _lightingUniformBuffer
            )
        );

        // Rasterizer
        var rasterizerStateDescription = new RasterizerStateDescription(
            cullMode: FaceCullMode.Back,
            fillMode: PolygonFillMode.Solid,
            frontFace: FrontFace.CounterClockwise,
            depthClipEnabled: true,
            scissorTestEnabled: false
        );

        // Pipeline
        var pipelineDescription = new GraphicsPipelineDescription
        {
            ShaderSet = shaderSetDescription,
            ResourceLayouts = [resourceLayout],
            RasterizerState = rasterizerStateDescription,
            PrimitiveTopology = PrimitiveTopology.TriangleList,
            BlendState = BlendStateDescription.SingleOverrideBlend,
            DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
            Outputs = GraphicsDevice.SwapchainFramebuffer.OutputDescription,
        };

        Pipeline = GraphicsDevice.ResourceFactory.CreateGraphicsPipeline(pipelineDescription);

        _commandList = graphicsDevice.ResourceFactory.CreateCommandList();
    }

    public void BeginFrame(CameraSceneObject activeCamera, SceneGraph sceneGraph)
    {
        _commandList.Begin();
        _commandList.SetFramebuffer(GraphicsDevice.SwapchainFramebuffer);
        _commandList.ClearColorTarget(0, new RgbaFloat(0.1f, 0.1f, 0.1f, 1.0f));
        _commandList.ClearDepthStencil(1.0f);
        _commandList.SetPipeline(Pipeline);

        UpdateCameraUniforms(activeCamera);
        UpdateLightUniforms(activeCamera, SceneGraph.FindAllByKind(sceneGraph.Root, SceneObjectKind.Light).Cast<LightSceneObject>());
    }

    public void EndFrame()
    {
        _commandList.End();
        GraphicsDevice.SubmitCommands(_commandList);
        GraphicsDevice.SwapBuffers();
    }

    public void DrawSceneGraphNode(ISceneObject sceneObject)
    {
        sceneObject.UpdateGlobalTransform();

        if (sceneObject.Kind is SceneObjectKind.Group && sceneObject is GroupSceneObject group)
        {
            // Iterate children
            foreach (var child in group.Children)
            {
                DrawSceneGraphNode(child);
            }

            return;
        }

        if (sceneObject.Kind is SceneObjectKind.Pawn && sceneObject is PawnSceneObject pawn)
        {
            // Update transform buffer
            _commandList!.SetGraphicsResourceSet(0, ResourceSet);
            _commandList.UpdateBuffer(_modelUniformBuffer, 0, pawn.GlobalTransform.Matrix);

            // Update vertex buffer
            _commandList.SetVertexBuffer(0, pawn.ModelBuffer.VertexBuffer);
            _commandList.UpdateBuffer(pawn.ModelBuffer.VertexBuffer, 0, pawn.ModelBuffer.Model.GetVertices());

            // Update index buffer
            _commandList.SetIndexBuffer(pawn.ModelBuffer.IndexBuffer, IndexFormat.UInt32);
            _commandList.UpdateBuffer(pawn.ModelBuffer.IndexBuffer, 0, pawn.ModelBuffer.Model.GetIndices());

            // Draw
            _commandList.DrawIndexed(pawn.ModelBuffer.Model.GetIndexCount());

            // Iterate children
            foreach (var child in pawn.Children)
            {
                DrawSceneGraphNode(child);
            }

            return;
        }
    }

    private void UpdateCameraUniforms(CameraSceneObject camera)
    {
        _commandList.UpdateBuffer(_projectionUniformBuffer, 0, camera.Camera.Projection);
        _commandList.UpdateBuffer(_viewUniformBuffer, 0, camera.Camera.View);

        _commandList.SetGraphicsResourceSet(0, ResourceSet);
    }

    private void UpdateLightUniforms(CameraSceneObject activeCamera, IEnumerable<LightSceneObject> lights)
    {
        if (lights.Any())
        {
            LightingBuffer lightingBuffer = new(
                new Vector4(activeCamera.Camera.Position, 1.0f),
                lights.Select(l => new PointLight(l.Transform.Position, new Vector3(l.Light.Color.X, l.Light.Color.Y, l.Light.Color.Z))).ToArray()
            );

            _commandList.UpdateBuffer(_lightingUniformBuffer, 0, lightingBuffer);
            _commandList.SetGraphicsResourceSet(0, ResourceSet);
        }
    }

    private Pipeline CreatePipeline()
    {
        return null!;
    }
}
