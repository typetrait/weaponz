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

    // Debug Graphics
    private Pipeline? _debugPipeline;
    private CommandList? _debugCommandList;
    private ResourceSet? _debugResourceSet;
    private readonly DeviceBuffer _debugVertexBuffer;

    private readonly SampleShaders _sampleShaders;

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
        _sampleShaders = new SampleShaders(GraphicsDevice.ResourceFactory);
        var shaderSetDescription = new ShaderSetDescription([vertexLayoutDescription], _sampleShaders.ShaderGroup1);

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

        // Debug
        _debugVertexBuffer = GraphicsDevice.ResourceFactory.CreateBuffer(
            new BufferDescription(
                VertexPositionColor.SizeInBytes * 2,
                BufferUsage.VertexBuffer
            )
        );
        CreateDebugPipelineAndCommandList();
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
        //GraphicsDevice.SwapBuffers();
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
            _commandList.SetGraphicsResourceSet(0, ResourceSet);
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

    public void BeginDebugFrame(CameraSceneObject activeCamera)
    {
        _debugCommandList?.Begin();
        _debugCommandList?.SetFramebuffer(GraphicsDevice.SwapchainFramebuffer);
        _debugCommandList?.SetPipeline(_debugPipeline);

        _debugCommandList?.UpdateBuffer(_projectionUniformBuffer, 0, activeCamera.Camera.Projection);
        _debugCommandList?.UpdateBuffer(_viewUniformBuffer, 0, activeCamera.Camera.View);

        _debugCommandList?.SetGraphicsResourceSet(0, _debugResourceSet);
    }

    public void EndDebugFrame()
    {
        _debugCommandList?.End();
        GraphicsDevice.SubmitCommands(_debugCommandList);
    }

    public void DrawLine(Vector3 start, Vector3 end, Vector3 color)
    {
        var vertices = new[]
        {
            new VertexPositionColor(start, color),
            new VertexPositionColor(end, color)
        };

        _debugCommandList?.UpdateBuffer(_debugVertexBuffer, 0, vertices);
        _debugCommandList?.SetVertexBuffer(0, _debugVertexBuffer);

        _debugCommandList?.Draw(2);
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
                lights.Select(
                    light => new PointLight(
                        light.GlobalTransform.Position,
                        new Vector3(
                            light.Light.Color.X,
                            light.Light.Color.Y,
                            light.Light.Color.Z
                        )
                    )
                ).ToArray()
            );

            _commandList.UpdateBuffer(_lightingUniformBuffer, 0, lightingBuffer);
            _commandList.SetGraphicsResourceSet(0, ResourceSet);
        }
    }

    private void CreateDebugPipelineAndCommandList()
    {
        VertexLayoutDescription vertexLayout = VertexPositionColor.GetLayout();

        // Resource layout
        var resourceLayoutDescription = new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("ProjectionBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
            new ResourceLayoutElementDescription("ViewBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)
        );

        ResourceLayout resourceLayout = GraphicsDevice.ResourceFactory.CreateResourceLayout(resourceLayoutDescription);

        _debugResourceSet = GraphicsDevice.ResourceFactory.CreateResourceSet(
            new ResourceSetDescription(
                resourceLayout,
                _projectionUniformBuffer,
                _viewUniformBuffer
            )
        );

        ShaderSetDescription shaders = new([vertexLayout], _sampleShaders.LineShaderGroup);

        GraphicsPipelineDescription pipelineDescription = new()
        {
            BlendState = BlendStateDescription.SingleAlphaBlend,
            ResourceLayouts = [resourceLayout],
            DepthStencilState = DepthStencilStateDescription.Disabled,
            RasterizerState = new RasterizerStateDescription(
                FaceCullMode.None,
                PolygonFillMode.Solid,
                FrontFace.CounterClockwise,
                depthClipEnabled: false,
                scissorTestEnabled: false
            ),
            PrimitiveTopology = PrimitiveTopology.LineList,
            ShaderSet = shaders,
            Outputs = GraphicsDevice.SwapchainFramebuffer.OutputDescription,
        };

        _debugPipeline = GraphicsDevice.ResourceFactory.CreateGraphicsPipeline(pipelineDescription);
        _debugCommandList = GraphicsDevice.ResourceFactory.CreateCommandList();
    }

    //private Pipeline CreatePipeline()
    //{
    //    ResourceLayoutDescription resourceLayoutDescription = new(
    //        new ResourceLayoutElementDescription("ProjectionBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
    //        new ResourceLayoutElementDescription("ViewBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)
    //    );

    //    ResourceLayout resourceLayout = GraphicsDevice.ResourceFactory.CreateResourceLayout(resourceLayoutDescription);

    //    RasterizerStateDescription rasterizerStateDescription = new(
    //        cullMode: FaceCullMode.Back,
    //        fillMode: PolygonFillMode.Solid,
    //        frontFace: FrontFace.CounterClockwise,
    //        depthClipEnabled: true,
    //        scissorTestEnabled: false
    //    );

    //    GraphicsPipelineDescription pipelineDescription = new()
    //    {
    //        //ShaderSet = shaderSetDescription,
    //        ResourceLayouts = [resourceLayout],
    //        RasterizerState = rasterizerStateDescription,
    //        PrimitiveTopology = PrimitiveTopology.TriangleList,
    //        BlendState = BlendStateDescription.SingleOverrideBlend,
    //        DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
    //        Outputs = GraphicsDevice.SwapchainFramebuffer.OutputDescription,
    //    };

    //    return GraphicsDevice.ResourceFactory.CreateGraphicsPipeline(pipelineDescription);
    //}
}
