using System.Numerics;
using System.Text;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.SPIRV;
using Veldrid.StartupUtilities;

namespace WeaponZ.Game;

public class Program
{
    private GraphicsDevice? _graphicsDevice;
    private CommandList? _commandList;
    private Pipeline? _pipeline;

    private DeviceBuffer? _vertexBuffer;

    private DeviceBuffer? _projectionUniformBuffer;
    private DeviceBuffer? _viewUniformBuffer;
    private DeviceBuffer? _modelUniformBuffer;

    private ResourceSet? _resourceSet;

    private Vertex[]? _vertices;

    private OrthographicCamera? _orthographicCamera;

    public static void Main(string[] args) => new Program().Run(args);

    public void Run(string[] args)
    {
        Console.WriteLine("Hello, WeaponZ!");

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
            new() { PreferStandardClipSpaceYDirection = true, PreferDepthRangeZeroToOne = true };

        _graphicsDevice = VeldridStartup.CreateGraphicsDevice(window, options);

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
                ]
            )
        ];

        var pipelineDescription = new GraphicsPipelineDescription();

        pipelineDescription.ShaderSet = new ShaderSetDescription(vld, shaders);

        pipelineDescription.BlendState = BlendStateDescription.SingleOverrideBlend;

        pipelineDescription.DepthStencilState = new DepthStencilStateDescription(
            depthTestEnabled: true,
            depthWriteEnabled: true,
            comparisonKind: ComparisonKind.LessEqual
        );

        pipelineDescription.RasterizerState = new RasterizerStateDescription(
            cullMode: FaceCullMode.None,
            fillMode: PolygonFillMode.Solid,
            frontFace: FrontFace.Clockwise,
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
                )
            )
        );

        pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleStrip;

        pipelineDescription.ResourceLayouts = [resourcesLayout];

        pipelineDescription.Outputs = _graphicsDevice.SwapchainFramebuffer.OutputDescription;

        _pipeline = factory.CreateGraphicsPipeline(pipelineDescription);

        _vertices =
        [
            new(new Vector3(-0.5f, -0.5f, 0.0f)),
            new(new Vector3(0.5f, -0.5f, 0.0f)),
            new(new Vector3(0.0f, 0.5f, 0.0f))
        ];

        _vertexBuffer = factory.CreateBuffer(new(36, BufferUsage.VertexBuffer));

        _resourceSet = factory.CreateResourceSet(
            new ResourceSetDescription(
                resourcesLayout,
                _projectionUniformBuffer,
                _viewUniformBuffer,
                _modelUniformBuffer
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

        while (window.Exists)
        {
            window.PumpEvents();
            Draw(_graphicsDevice);
        }
    }

    public void Draw(GraphicsDevice graphicsDevice)
    {
        if (_commandList is null)
        {
            return;
        }

        if (_orthographicCamera is null)
        {
            return;
        }

        _commandList.Begin();
        _commandList.SetFramebuffer(graphicsDevice.SwapchainFramebuffer);
        _commandList.ClearColorTarget(0, RgbaFloat.CornflowerBlue);

        _commandList.SetPipeline(_pipeline);
        _commandList.SetGraphicsResourceSet(0, _resourceSet);

        _commandList.UpdateBuffer(_projectionUniformBuffer, 0, _orthographicCamera.Projection);
        _commandList.UpdateBuffer(_viewUniformBuffer, 0, _orthographicCamera.View);

        var model = Matrix4x4.CreateTranslation(new Vector3(0, 0.45f, 0));
        _commandList.UpdateBuffer(_modelUniformBuffer, 0, model);

        _commandList.SetVertexBuffer(0, _vertexBuffer);
        _commandList.UpdateBuffer(_vertexBuffer, 0, _vertices);

        _commandList.Draw(3);
        _commandList.End();

        graphicsDevice.SubmitCommands(_commandList);
        graphicsDevice.SwapBuffers();
    }

    const string VertexShaderSource =
        @"  #version 460 core

            layout (location = 0) in vec3 Position;

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
                gl_Position = Projection * View * Model * vec4(Position, 1.0);
            }
        ";

    const string FragmentShaderSource =
        @"  #version 460 core

            layout (location = 0) out vec3 fsout_Color;

            void main()
            {
                fsout_Color = vec3(1.0, 0, 0);
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

public struct Vertex(Vector3 position)
{
    public Vector3 Position = position;
}
