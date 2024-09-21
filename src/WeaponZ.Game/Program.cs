﻿using System.Numerics;
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

    private DeviceBuffer? _lightingUniformBuffer;

    private ResourceSet? _resourceSet;

    private Vertex[]? _vertices;

    private OrthographicCamera? _orthographicCamera;

    private Keyboard? _keyboard;

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

        _lightingUniformBuffer = factory.CreateBuffer(
            new(32, BufferUsage.UniformBuffer | BufferUsage.Dynamic));

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
                ),
                new ResourceLayoutElementDescription(
                    "LightingBuffer",
                    ResourceKind.UniformBuffer,
                    ShaderStages.Fragment
                )
            )
        );

        pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleStrip;

        pipelineDescription.ResourceLayouts = [resourcesLayout];

        pipelineDescription.Outputs = _graphicsDevice.SwapchainFramebuffer.OutputDescription;

        _pipeline = factory.CreateGraphicsPipeline(pipelineDescription);

        _vertices =
        [
            new(new Vector3(-0.5f, -0.5f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f)),
            new(new Vector3(0.5f, -0.5f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f)),
            new(new Vector3(0.0f, 0.5f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f))
        ];

        _vertexBuffer = factory.CreateBuffer(new(36 * 2, BufferUsage.VertexBuffer));

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

        _keyboard = new Keyboard();

        while (window.Exists)
        {
            InputSnapshot inputSnapshot = window.PumpEvents();
            _keyboard.UpdateFromSnapshot(inputSnapshot);

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

        var model = Matrix4x4.CreateTranslation(new Vector3(0.0f, 0.0f, 0.0f));
        _commandList.UpdateBuffer(_modelUniformBuffer, 0, model);

        var lightingBuffer = new LightingBuffer(new Vector4(_orthographicCamera.Position, 1.0f), new Vector4(0.0f, 0.0f, 0.1f, 1.0f));
        _commandList.UpdateBuffer(_lightingUniformBuffer, 0, lightingBuffer);

        _commandList.SetVertexBuffer(0, _vertexBuffer);
        _commandList.UpdateBuffer(_vertexBuffer, 0, _vertices);

        _commandList.Draw(3);
        _commandList.End();

        _orthographicCamera.Update(_keyboard, 0.0f);

        graphicsDevice.SubmitCommands(_commandList);
        graphicsDevice.SwapBuffers();
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

public struct Vertex(Vector3 position, Vector3 normal)
{
    public Vector3 Position = position;
    public Vector3 Normal = normal;
}

public struct LightingBuffer(Vector4 cameraPosition, Vector4 lightPosition)
{
    public Vector4 CameraPosition = cameraPosition;
    public Vector4 LightPosition = lightPosition;
}
