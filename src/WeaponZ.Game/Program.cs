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

    private DeviceBuffer? _vertexBuffer;

    private DeviceBuffer? _viewProjectionUniformBuffer;

    private Pipeline? _pipeline;

    private Vertex[]? _vertices;

    const string VertexShaderSource = @"#version 460 core

    layout (location = 0) in vec3 Position;

    layout (std140, binding = 0) uniform Matrices
    {
        mat4 projection; // offset = 0
        mat4 view; // offset = 64 ((4 * 4) * 4 = 64)
    };

    void main()
    {

        gl_Position = projection * view * vec4(Position, 1.0);
    }";

    const string FragmentShaderSource = @"#version 460 core

    layout (location = 0) out vec3 fsout_Color;

    void main()
    {
        fsout_Color = vec3(1.0, 0, 0);
    }";


    // literally everything else

    private OrthographicCamera? _orthographicCamera;

    public static void Main(string[] args) => new Program().Run(args);

    public void Run(string[] args)
    {
        Console.WriteLine("Hello, WeaponZ!");

        WindowCreateInfo windowCI = new()
        {
            X = 100,
            Y = 100,
            WindowWidth = 800,
            WindowHeight = 600,
            WindowTitle = "WeaponZ"
        };

        Sdl2Window window = VeldridStartup.CreateWindow(ref windowCI);

        GraphicsDeviceOptions options = new()
        {
            PreferStandardClipSpaceYDirection = true,
            PreferDepthRangeZeroToOne = true
        };

        _graphicsDevice = VeldridStartup.CreateGraphicsDevice(window, options);

        //_graphicsDevice = VeldridStartup.CreateDefaultOpenGLGraphicsDevice(options, window, GraphicsBackend.OpenGL);

        ResourceFactory factory = _graphicsDevice.ResourceFactory;

        var vertexShaderDescription = new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(VertexShaderSource), "main");
        var fragmentShaderDescription = new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(FragmentShaderSource), "main");
        var shaders = factory.CreateFromSpirv(vertexShaderDescription, fragmentShaderDescription);

        VertexLayoutDescription[] vld = [
            new VertexLayoutDescription(
            [
                new("Position", VertexElementFormat.Float3, VertexElementSemantic.TextureCoordinate),
            ])
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

        ResourceLayout resourcesLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("Matrices", ResourceKind.UniformBuffer, ShaderStages.Vertex)
            )
        );

        pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
        pipelineDescription.ResourceLayouts = [resourcesLayout];

        pipelineDescription.Outputs = _graphicsDevice.SwapchainFramebuffer.OutputDescription;

        _pipeline = factory.CreateGraphicsPipeline(pipelineDescription);

        _commandList = factory.CreateCommandList();

        _vertices =
        [
            new(new Vector3(-0.5f, -0.5f, 0.0f)),
            new(new Vector3(0.5f, -0.5f, 0.0f)),
            new(new Vector3(0.0f, 0.5f, 0.0f))
        ];

        BufferDescription vertexBufferDescription = new(36, BufferUsage.VertexBuffer);
        _vertexBuffer = factory.CreateBuffer(vertexBufferDescription);

        // viewModel
        BufferDescription viewProjectUniformBufferDescription = new(ViewProjectionMatrix.SizeInBytes, BufferUsage.UniformBuffer | BufferUsage.Dynamic);
        _viewProjectionUniformBuffer = factory.CreateBuffer(viewProjectUniformBufferDescription);

        _orthographicCamera = new OrthographicCamera(
            windowCI.WindowWidth,
            windowCI.WindowHeight,
            0.1f,
            100.0f,
            new Vector3(0.0f, 0.0f, 3.0f)
        );

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

        _commandList.Begin();

        _commandList.SetFramebuffer(graphicsDevice.SwapchainFramebuffer);

        _commandList.ClearColorTarget(0, RgbaFloat.Grey);

        _commandList.SetVertexBuffer(0, _vertexBuffer);
        _commandList.UpdateBuffer(_vertexBuffer, 0, _vertices);

        if (_orthographicCamera is null) return;
        _commandList.UpdateBuffer(_viewProjectionUniformBuffer, 0, _orthographicCamera.ViewProjection);

        _commandList.SetPipeline(_pipeline);
        _commandList.Draw(3);

        _commandList.End();
        graphicsDevice.SubmitCommands(_commandList);
        graphicsDevice.SwapBuffers();
    }
}

public struct Vertex(Vector3 position)
{
    public Vector3 Position = position;
}
