using System.Numerics;
using System.Text;
using ImGuiNET;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.SPIRV;
using Veldrid.StartupUtilities;
using WeaponZ.Game.Input;
using WeaponZ.Game.SceneGraph;
using MouseState = WeaponZ.Game.Input.MouseState;

namespace WeaponZ.Game;

public class Program
{
    private GraphicsDevice? _graphicsDevice;
    private CommandList? _commandList;
    private Pipeline? _pipeline;
    private DeviceBuffer? _vertexBuffer;
    private DeviceBuffer? _indexBuffer;

    private DeviceBuffer? _projectionUniformBuffer;
    private DeviceBuffer? _viewUniformBuffer;
    private DeviceBuffer? _modelUniformBuffer;

    private DeviceBuffer? _lightingUniformBuffer;

    private ResourceSet? _resourceSet;

    private ImGuiRenderer? _imguiRenderer;

    private OrthographicCamera? _orthographicCamera;

    private KeyboardState? _keyboardState;
    private MouseState? _mouseState;

    private float _rotation = 0.0f;

    private Transform? _transform;

    private SceneGraphImpl _sceneGraph;

    private PawnSceneObject _bunnyProp;
    private PawnSceneObject _bunnyProp2;

    public static void Main(string[] args) => new Program().Run(args);

    public void Run(string[] args)
    {
        Console.WriteLine("Hello, WeaponZ!");
        Console.WriteLine("Current directory: " + Directory.GetCurrentDirectory());

        WindowCreateInfo windowCI =
            new()
            {
                X = 100,
                Y = 100,
                WindowWidth = 800,
                WindowHeight = 600,
                WindowTitle = "WeaponZ"
            };

        Sdl2Window window = VeldridStartup.CreateWindow(ref windowCI);

        GraphicsDeviceOptions options =
            new(true, PixelFormat.R32_Float, true)
            {
                PreferStandardClipSpaceYDirection = true,
                PreferDepthRangeZeroToOne = true,
            };
        _graphicsDevice = VeldridStartup.CreateGraphicsDevice(window, options);

        //_graphicsDevice = VeldridStartup.CreateDefaultOpenGLGraphicsDevice(options, window, GraphicsBackend.OpenGL);
        //_graphicsDevice = VeldridStartup.CreateVulkanGraphicsDevice(options, window);

        ResourceFactory factory = _graphicsDevice.ResourceFactory;

        _projectionUniformBuffer = factory.CreateBuffer(
            new(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic)
        );

        _viewUniformBuffer = factory.CreateBuffer(
            new(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic)
        );

        _modelUniformBuffer = factory.CreateBuffer(
            new(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic)
        );

        _lightingUniformBuffer = factory.CreateBuffer(
            new(32, BufferUsage.UniformBuffer | BufferUsage.Dynamic)
        );

        var vertexShaderDescription = new ShaderDescription(
            ShaderStages.Vertex,
            Encoding.UTF8.GetBytes(VertexShaderSource),
            "main"
        );

        var fragmentShaderDescription = new ShaderDescription(
            ShaderStages.Fragment,
            Encoding.UTF8.GetBytes(FragmentShaderSource),
            "main"
        );

        var shaders = factory.CreateFromSpirv(vertexShaderDescription, fragmentShaderDescription);

        VertexLayoutDescription[] vld =
        [
            new VertexLayoutDescription(
                [
                    new(
                        "Position",
                        VertexElementFormat.Float3,
                        VertexElementSemantic.TextureCoordinate
                    ),
                    new(
                        "Normal",
                        VertexElementFormat.Float3,
                        VertexElementSemantic.TextureCoordinate
                    )
                ]
            )
        ];

        var pipelineDescription = new GraphicsPipelineDescription();

        pipelineDescription.ShaderSet = new ShaderSetDescription(vld, shaders);

        pipelineDescription.BlendState = BlendStateDescription.SingleOverrideBlend;

        pipelineDescription.DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual;

        pipelineDescription.RasterizerState = new RasterizerStateDescription(
            cullMode: FaceCullMode.Back,
            fillMode: PolygonFillMode.Solid,
            frontFace: FrontFace.CounterClockwise,
            depthClipEnabled: true,
            scissorTestEnabled: false
        );

        ResourceLayout resourcesLayout = factory.CreateResourceLayout(
            new ResourceLayoutDescription(
                new ResourceLayoutElementDescription(
                    "ProjectionBuffer",
                    ResourceKind.UniformBuffer,
                    ShaderStages.Vertex
                ),
                new ResourceLayoutElementDescription(
                    "ViewBuffer",
                    ResourceKind.UniformBuffer,
                    ShaderStages.Vertex
                ),
                new ResourceLayoutElementDescription(
                    "ModelBuffer",
                    ResourceKind.UniformBuffer,
                    ShaderStages.Vertex
                ),
                new ResourceLayoutElementDescription(
                    "LightingBuffer",
                    ResourceKind.UniformBuffer,
                    ShaderStages.Fragment
                )
            )
        );

        var _rootNode = new GroupSceneObject("Root");
        _sceneGraph = new SceneGraphImpl(_rootNode);

        // TODO: NOT HERE
        _transform = new Transform() { Scale = new Vector3(0.002f), };

        var transform2 = new Transform() { Scale = new Vector3(0.002f), };

        transform2.TranslateY(0.2f);

        var models = new SampleModels();

        var modelBufferFactory = new ModelBufferFactory(factory);
        var bunnyModelBuffer = modelBufferFactory.CreateModelBuffer<Vertex>(models.Bunny);

        _bunnyProp = new PawnSceneObject("Bunny", _transform, bunnyModelBuffer);
        _bunnyProp2 = new PawnSceneObject("Bunny 2", transform2, bunnyModelBuffer);

        _sceneGraph.AppendTo(_sceneGraph.Root, _bunnyProp);
        _sceneGraph.AppendTo(_sceneGraph.Root.Children[0], _bunnyProp2);

        //_sceneGraph.AppendTo(_sceneGraph.Root, new PawnSceneObject("Prop 2", null, null));
        //_sceneGraph.AppendTo(_sceneGraph.Root.Children[0], new PawnSceneObject("Prop 3", null, null));
        //

        pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleList;

        pipelineDescription.ResourceLayouts = [resourcesLayout];

        pipelineDescription.Outputs = _graphicsDevice.SwapchainFramebuffer.OutputDescription;

        _pipeline = factory.CreateGraphicsPipeline(pipelineDescription);

        _imguiRenderer = new ImGuiRenderer(
            _graphicsDevice,
            pipelineDescription.Outputs,
            window.Width,
            window.Height
        );

        _vertexBuffer = factory.CreateBuffer(
            new(
                _bunnyProp.ModelBuffer.Model.GetVertexCount() * Vertex.SizeInBytes,
                BufferUsage.VertexBuffer
            )
        );

        _indexBuffer = factory.CreateBuffer(
            new BufferDescription(
                _bunnyProp.ModelBuffer.Model.GetIndexCount() * sizeof(uint),
                BufferUsage.IndexBuffer
            )
        );

        // _graphicsDevice.UpdateBuffer(_indexBuffer, 0, _bunnyProp.ModelBuffer.Model.GetIndices());

        _resourceSet = factory.CreateResourceSet(
            new ResourceSetDescription(
                resourcesLayout,
                _projectionUniformBuffer,
                _viewUniformBuffer,
                _modelUniformBuffer,
                _lightingUniformBuffer
            )
        );

        _orthographicCamera = new OrthographicCamera(
            windowCI.WindowWidth,
            windowCI.WindowHeight,
            0.1f,
            100.0f,
            new Vector3(0.0f, 0.0f, 3.0f)
        );

        _commandList = factory.CreateCommandList();

        Console.WriteLine("Projection:");
        LogMatrix4x4(_orthographicCamera.Projection);
        Console.WriteLine("View:");
        LogMatrix4x4(_orthographicCamera.View);

        _keyboardState = new KeyboardState();
        _mouseState = new MouseState();

        var frameTimer = new FrameTimer();
        uint targetFps = 60;
        TimeSpan deltaTime = TimeSpan.Zero;

        while (window.Exists)
        {
            InputSnapshot inputSnapshot = window.PumpEvents();
            _keyboardState.UpdateFromSnapshot(inputSnapshot);
            _mouseState.UpdateFromSnapshot(inputSnapshot);

            _imguiRenderer.Update((float)deltaTime.TotalSeconds, inputSnapshot);

            Draw(_graphicsDevice, deltaTime);

            frameTimer.SleepUntilTargetFrameTime(targetFps);
            deltaTime = frameTimer.Restart();
        }
    }

    private float _selection = 0;

    public void Draw(GraphicsDevice graphicsDevice, TimeSpan deltaTime)
    {
        if (_commandList is null)
        {
            return;
        }

        if (_orthographicCamera is null || _keyboardState is null || _mouseState is null)
        {
            return;
        }

        _transform!.RotateY(1.0f * (float)deltaTime.TotalSeconds);

        ImGui.Begin("Scene Graph");
        DrawSceneGraph(_sceneGraph.Root);
        ImGui.End();

        ImGui.Begin("Transform");

        if (ImGui.TreeNodeEx("Position", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.DragFloat3("", ref _transform.Position);
            ImGui.TreePop();
        }

        if (ImGui.TreeNodeEx("Rotation", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.DragFloat3("", ref _transform.Rotation, 0.02f);
            ImGui.TreePop();
        }

        if (ImGui.TreeNodeEx("Scale", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.DragFloat3("", ref _transform.Scale, 0.0002f);
            ImGui.TreePop();
        }

        ImGui.End();

        _commandList.Begin();
        _commandList.SetFramebuffer(graphicsDevice.SwapchainFramebuffer);
        _commandList.ClearColorTarget(0, RgbaFloat.CornflowerBlue);
        _commandList.ClearDepthStencil(1.0f);

        _commandList.SetPipeline(_pipeline);
        _commandList.SetGraphicsResourceSet(0, _resourceSet);

        // Uniform Buffers
        _commandList.UpdateBuffer(_projectionUniformBuffer, 0, _orthographicCamera.Projection);
        _commandList.UpdateBuffer(_viewUniformBuffer, 0, _orthographicCamera.View);

        //_commandList.UpdateBuffer(_modelUniformBuffer, 0, _bunnyProp.Transform.Matrix);

        var lightingBuffer = new LightingBuffer(
            new Vector4(_orthographicCamera.Position, 1.0f),
            new Vector4(0.0f, 0.0f, 3.0f, 1.0f)
        );
        _commandList.UpdateBuffer(_lightingUniformBuffer, 0, lightingBuffer);

        //_commandList.SetVertexBuffer(0, _vertexBuffer);
        //_commandList.UpdateBuffer(_vertexBuffer, 0, _bunnyProp.ModelBuffer.Model.GetVertices());

        //_commandList.SetIndexBuffer(_indexBuffer, IndexFormat.UInt32);
        //_commandList.DrawIndexed(_bunnyProp.ModelBuffer.Model.GetIndexCount());

        //
        DrawSceneGraphPawns(_sceneGraph.Root, _commandList);
        //

        _imguiRenderer!.Render(_graphicsDevice, _commandList);

        _commandList.End();

        _orthographicCamera.Update(_keyboardState, _mouseState, deltaTime);

        graphicsDevice.SubmitCommands(_commandList);
        graphicsDevice.SwapBuffers();
    }

    private void DrawSceneGraph(ISceneObject? startingNode)
    {
        if (startingNode is null)
        {
            return;
        }

        if (ImGui.TreeNode(startingNode.DisplayName))
        {
            foreach (var child in startingNode.Children)
            {
                DrawSceneGraph(child);
            }

            ImGui.TreePop();
        }
    }

    private void DrawSceneGraphPawns(ISceneObject? startingNode, CommandList commandList)
    {
        if (startingNode is null)
        {
            return;
        }

        if (startingNode.Kind is SceneObjectKind.Group)
        {
            if (startingNode is not GroupSceneObject group)
            {
                return;
            }

            foreach (var child in group.Children)
            {
                DrawSceneGraphPawns(child, commandList);
            }
        }
        else if (startingNode is PawnSceneObject pawn)
        {
            commandList.SetGraphicsResourceSet(0, _resourceSet);

            commandList.UpdateBuffer(_modelUniformBuffer, 0, pawn.Transform.Matrix);

            commandList.SetVertexBuffer(0, pawn.ModelBuffer.VertexBuffer);
            commandList.UpdateBuffer(
                pawn.ModelBuffer.VertexBuffer,
                0,
                pawn.ModelBuffer.Model.GetVertices()
            );

            commandList.SetIndexBuffer(pawn.ModelBuffer.IndexBuffer, IndexFormat.UInt32);
            commandList.UpdateBuffer(
                pawn.ModelBuffer.IndexBuffer,
                0,
                _bunnyProp.ModelBuffer.Model.GetIndices()
            );

            commandList.DrawIndexed(pawn.ModelBuffer.Model.GetIndexCount());

            foreach (var child in pawn.Children)
            {
                DrawSceneGraphPawns(child, commandList);
            }
        }
    }

    const string VertexShaderSource =
        @"  #version 460 core

            layout (location = 0) in vec3 Position;
            layout (location = 1) in vec3 Normal;

            layout (location = 0) out vec3 fsout_Normal;
            layout (location = 1) out vec4 fsout_Position;

            layout (set = 0, binding = 0) uniform ProjectionBuffer
            {
                mat4 Projection;
            };

            layout (set = 0, binding = 1) uniform ViewBuffer
            {
                mat4 View;
            };

            layout (set = 0, binding = 2) uniform ModelBuffer
            {
                mat4 Model;
            };

            void main()
            {
                fsout_Normal = mat3(transpose(inverse(Model))) * Normal;
                fsout_Position = Model * vec4(Position, 1.0);
                gl_Position = Projection * View * Model * vec4(Position, 1.0);
            }
        ";

    const string FragmentShaderSource =
        @"  #version 460 core

            layout (location = 0) in vec3 fsin_Normal;
            layout (location = 1) in vec4 fsin_Position;

            layout (location = 0) out vec4 fsout_Color;

            layout (set = 0, binding = 3) uniform LightingBuffer
            {
                vec4 CameraPosition;
                vec4 LightPosition;
            };

            void main()
            {
                vec3 objectColor = vec3(0.8f, 0.0f, 0.5f);

                vec3 lightColor = vec3(1.0f, 1.0f, 1.0f);

                //vec3 lightDirection = normalize(vec3(0.0f, 0.0f, 0.1f) - fsin_Position.xyz);
                vec3 lightDirection = normalize(LightPosition.xyz - fsin_Position.xyz);
                vec3 normal = normalize(fsin_Normal);

                float diffuseIntensity = max(dot(lightDirection, normal), 0.0f);
                vec3 diffuseColor = lightColor * diffuseIntensity;

                vec3 viewDirection = normalize(CameraPosition.xyz - fsin_Position.xyz);
                vec3 reflectDirection = reflect(-lightDirection, normal);

                float specularIntensity = pow(max(dot(viewDirection, reflectDirection), 0.0), 128);
                vec3 specularColor = 0.5f * specularIntensity * lightColor;

                float ambientStrength = 0.1f;
                vec3 ambientColor = ambientStrength * lightColor;

                vec3 result = (ambientColor + diffuseColor + specularColor) * objectColor;
                fsout_Color = vec4(result, 1.0);
            }
        ";

    public static void LogMatrix4x4(Matrix4x4 matrix)
    {
        Console.WriteLine($"{matrix.M11}, {matrix.M12}, {matrix.M13}, {matrix.M14}");
        Console.WriteLine($"{matrix.M21}, {matrix.M22}, {matrix.M23}, {matrix.M24}");
        Console.WriteLine($"{matrix.M31}, {matrix.M32}, {matrix.M33}, {matrix.M34}");
        Console.WriteLine($"{matrix.M41}, {matrix.M42}, {matrix.M43}, {matrix.M44}");
    }
}

public interface IVertex
{
    static abstract uint SizeInBytes { get; }
}

public struct Vertex(Vector3 position, Vector3 normal) : IVertex
{
    public Vector3 Position = position;
    public Vector3 Normal = normal;

    // 3 floats for position = 12 bytes
    // 3 floats for normal = 12 bytes
    // Total = 24 bytes
    public static uint SizeInBytes => 24;
}

public struct LightingBuffer(Vector4 cameraPosition, Vector4 lightPosition)
{
    public Vector4 CameraPosition = cameraPosition;
    public Vector4 LightPosition = lightPosition;
}
