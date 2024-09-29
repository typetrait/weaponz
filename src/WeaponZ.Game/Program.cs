using System.Numerics;
using ImGuiNET;
using Veldrid;
using Veldrid.StartupUtilities;
using WeaponZ.Game.Input;
using WeaponZ.Game.Models;
using WeaponZ.Game.Render;
using WeaponZ.Game.Scene;
using WeaponZ.Game.Util;
using MouseState = WeaponZ.Game.Input.MouseState;

namespace WeaponZ.Game;

public class Program : IInputContext
{
    // Rendering
    private Pipeline? _pipeline;
    private ResourceSet? _resourceSet;
    private CommandList? _commandList;
    private GraphicsDevice? _graphicsDevice;

    // Imgui
    private ImGuiRenderer? _imguiRenderer;

    // Camera
    private OrthographicCamera? _orthographicCamera;

    // Input
    private KeyboardState? _keyboardState;
    private MouseState? _mouseState;

    // Uniform buffers
    private DeviceBuffer? _projectionUniformBuffer;
    private DeviceBuffer? _viewUniformBuffer;
    private DeviceBuffer? _modelUniformBuffer;
    private DeviceBuffer? _lightingUniformBuffer;

    // Scene
    private SceneGraph? _sceneGraph;
    private PawnSceneObject? _bunnyProp;
    private PawnSceneObject? _bunnyProp2;
    private Transform? _transform;
    private LightingBuffer _lightingBuffer;

    private ISceneObject? _selectedObject = null;

    private int _transformOptionSelection = 0;

    public event EventHandler<MouseButtonEventArgs>? MouseButtonPressed;
    public event EventHandler<MouseButtonEventArgs>? MouseButtonReleased;
    public event EventHandler<MouseEventArgs>? MouseMoved;
    public event EventHandler? KeyPressed;
    public event EventHandler? KeyReleased;

    /// <summary>
    /// Entry point
    /// </summary>
    public static void Main(string[] args) => new Program().Run(args);

    /// <summary>
    /// Run the game
    /// </summary>
    public void Run(string[] _)
    {
        // Log general information
        Console.WriteLine("Current directory: " + Directory.GetCurrentDirectory());

        // Window
        var windowCreateInfo = new WindowCreateInfo
        {
            X = 100,
            Y = 100,
            WindowWidth = 800,
            WindowHeight = 600,
            WindowTitle = "WeaponZ",
        };

        var window = VeldridStartup.CreateWindow(ref windowCreateInfo);

        // Graphics device options
        var graphicsDeviceOptions = new GraphicsDeviceOptions
        {
            Debug = true,
            SyncToVerticalBlank = true,
            PreferDepthRangeZeroToOne = true,
            PreferStandardClipSpaceYDirection = true,
            SwapchainDepthFormat = PixelFormat.R32_Float,
        };

        // Default graphics device
        _graphicsDevice = VeldridStartup.CreateGraphicsDevice(window, graphicsDeviceOptions);
        // // OpenGL graphics device
        // _graphicsDevice = VeldridStartup.CreateDefaultOpenGLGraphicsDevice(
        //     graphicsDeviceOptions,
        //     window,
        //     GraphicsBackend.OpenGL
        // );
        // // Vulkan graphics device
        // _graphicsDevice = VeldridStartup.CreateVulkanGraphicsDevice(graphicsDeviceOptions, window);

        // Resource factory
        var resourceFactory = _graphicsDevice.ResourceFactory;

        // Vertex layout descriptions
        var vertexLayoutDescription = new VertexLayoutDescription(
            new VertexElementDescription(
                "Position",
                VertexElementFormat.Float3,
                VertexElementSemantic.TextureCoordinate
            ),
            new VertexElementDescription( //
                "Normal",
                VertexElementFormat.Float3,
                VertexElementSemantic.TextureCoordinate
            )
        );

        // Uniform buffers
        _projectionUniformBuffer = resourceFactory.CreateBuffer(
            new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic)
        );
        _viewUniformBuffer = resourceFactory.CreateBuffer(
            new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic)
        );
        _modelUniformBuffer = resourceFactory.CreateBuffer(
            new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic)
        );
        _lightingUniformBuffer = resourceFactory.CreateBuffer(
            new BufferDescription(32, BufferUsage.UniformBuffer | BufferUsage.Dynamic)
        );

        // Resource layout
        var resourceLayoutDescription = new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("ProjectionBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
            new ResourceLayoutElementDescription("ViewBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
            new ResourceLayoutElementDescription("ModelBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
            new ResourceLayoutElementDescription("LightingBuffer", ResourceKind.UniformBuffer, ShaderStages.Fragment)
        );

        var resourceLayout = resourceFactory.CreateResourceLayout(resourceLayoutDescription);

        // Resource set
        _resourceSet = resourceFactory.CreateResourceSet(
            new ResourceSetDescription(
                resourceLayout,
                _projectionUniformBuffer,
                _viewUniformBuffer,
                _modelUniformBuffer,
                _lightingUniformBuffer
            )
        );

        // Shaders
        var sampleShaders = new SampleShaders(resourceFactory);
        var shaderSetDescription = new ShaderSetDescription([vertexLayoutDescription], sampleShaders.ShaderGroup1);

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
            Outputs = _graphicsDevice.SwapchainFramebuffer.OutputDescription,
        };

        _pipeline = resourceFactory.CreateGraphicsPipeline(pipelineDescription);

        // Command list
        _commandList = resourceFactory.CreateCommandList();

        // Imgui
        _imguiRenderer = new ImGuiRenderer(_graphicsDevice, pipelineDescription.Outputs, window.Width, window.Height);

        // Camera
        _orthographicCamera = new OrthographicCamera(
            windowCreateInfo.WindowWidth,
            windowCreateInfo.WindowHeight,
            0.1f,
            100.0f,
            new Vector3(0.0f, 0.0f, 3.0f)
        );

        // Inputs
        _keyboardState = new KeyboardState();
        _mouseState = new MouseState();

        // Timing
        var frameTimer = new FrameTimer();
        var deltaTime = TimeSpan.Zero;

        // Scene graph
        var rootNode = new GroupSceneObject("Root");
        _sceneGraph = new SceneGraph(rootNode);

        // Models
        _transform = new Transform() { Scale = new Vector3(0.002f) };
        var transform2 = new Transform() { Scale = new Vector3(0.001f) };
        transform2.TranslateY(400.0f);

        var models = new SampleModels();
        var modelBufferFactory = new ModelBufferFactory(resourceFactory);
        var bunnyModelBuffer = modelBufferFactory.CreateModelBuffer<Vertex>(models.Bunny);

        _bunnyProp = new PawnSceneObject("Bunny", _transform, bunnyModelBuffer);
        _bunnyProp2 = new PawnSceneObject("Bunny 2", transform2, bunnyModelBuffer);

        _sceneGraph.AppendTo(_sceneGraph.Root, _bunnyProp);
        _sceneGraph.AppendTo(_sceneGraph.Root.Children[0], _bunnyProp2);

        _sceneGraph.AppendTo(_sceneGraph.Root, new CameraSceneObject("Default Camera", new Transform(), _orthographicCamera, this));

        _lightingBuffer = new LightingBuffer(
            new Vector4(_orthographicCamera.Position, 1.0f),
            new Vector4(0.0f, 0.0f, 3.0f, 1.0f)
        );

        // Main loop
        while (window.Exists)
        {
            InputSnapshot inputSnapshot = window.PumpEvents();
            _keyboardState.UpdateFromSnapshot(inputSnapshot);
            _mouseState.UpdateFromSnapshot(inputSnapshot);

            _imguiRenderer.Update((float)deltaTime.TotalSeconds, inputSnapshot);

            Draw(deltaTime);

            frameTimer.SleepUntilTargetFrameTime(Config.TargetFps);
            deltaTime = frameTimer.Restart();
        }
    }

    public void Draw(TimeSpan deltaTime)
    {
        // Validate resources
        if (
            _commandList is null
            || _graphicsDevice is null
            || _orthographicCamera is null
            || _keyboardState is null
            || _mouseState is null
            || _sceneGraph is null
            || _transform is null
            || _imguiRenderer is null
        )
        {
            throw new InvalidOperationException("Failed to initialize resources.");
        }

        // Update camera
        _orthographicCamera.Update(_keyboardState, _mouseState, deltaTime);

        // Update transforms
        //_bunnyProp2!.Transform.RotateY(1.0f * (float)deltaTime.TotalSeconds);

        // Setup imgui windows
        SetupSceneGraphUi(_sceneGraph);

        if (_selectedObject is not null)
        {
            SetupTransformUi(_selectedObject.Transform);
        }

        // Begin commands
        _commandList.Begin();
        _commandList.SetFramebuffer(_graphicsDevice.SwapchainFramebuffer);
        _commandList.ClearColorTarget(0, new RgbaFloat(0.1f, 0.1f, 0.1f, 1.0f));
        _commandList.ClearDepthStencil(1.0f);
        _commandList.SetPipeline(_pipeline);

        // Camera buffers
        _commandList.SetGraphicsResourceSet(0, _resourceSet);
        _commandList.UpdateBuffer(_projectionUniformBuffer, 0, _orthographicCamera.Projection);
        _commandList.UpdateBuffer(_viewUniformBuffer, 0, _orthographicCamera.View);

        // Lighting buffers
        _commandList.SetGraphicsResourceSet(0, _resourceSet);
        _commandList.UpdateBuffer(_lightingUniformBuffer, 0, _lightingBuffer);

        // Model buffers
        DrawSceneGraph(_sceneGraph);

        // Render imgui
        _imguiRenderer.Render(_graphicsDevice, _commandList);

        // End frame
        _commandList.End();
        _graphicsDevice.SubmitCommands(_commandList);
        _graphicsDevice.SwapBuffers();
    }

    /// <summary>
    /// Draws the scene graph ui
    /// </summary>
    private void SetupSceneGraphUi(SceneGraph sceneGraph)
    {
        ImGui.Begin("Scene Graph");

        DrawSceneGraphUiNode(sceneGraph.Root);

        ImGui.End();
    }

    /// <summary>
    /// Draws the scene graph ui nodes
    /// </summary>
    private void DrawSceneGraphUiNode(ISceneObject sceneObject)
    {
        if (sceneObject is null) return;

        var treeNodeFlags = ImGuiTreeNodeFlags.OpenOnArrow;
        if (_selectedObject == sceneObject)
        {
            treeNodeFlags |= ImGuiTreeNodeFlags.Selected;
        }

        bool nodeOpen = ImGui.TreeNodeEx(sceneObject.DisplayName, treeNodeFlags);

        if (ImGui.IsItemClicked())
        {
            _selectedObject = sceneObject;
        }

        if (nodeOpen)
        {
            foreach (var child in sceneObject.Children)
            {
                DrawSceneGraphUiNode(child);
            }
            ImGui.TreePop();
        }
    }


    /// <summary>
    /// Draws the transform ui
    /// </summary>
    private void SetupTransformUi(Transform transform)
    {
        ImGui.Begin("Scene Object");

        Transform t = _transformOptionSelection == 0 ? _selectedObject!.GlobalTransform : _selectedObject!.Transform;

        ImGui.Text($"{_selectedObject?.DisplayName} [{_selectedObject?.Kind}]");

        if (ImGui.TreeNodeEx("Transform"))
        {
            if (ImGui.TreeNodeEx("Position", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.DragFloat3("", ref t.Position);
                ImGui.TreePop();
            }

            if (ImGui.TreeNodeEx("Rotation", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.DragFloat3("", ref t.Rotation, 0.02f);
                ImGui.TreePop();
            }

            if (ImGui.TreeNodeEx("Scale", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.DragFloat3("", ref t.Scale, 0.0002f);
                ImGui.TreePop();
            }

            ImGui.RadioButton("Global", ref _transformOptionSelection, 0);
            ImGui.SameLine();
            ImGui.RadioButton("Local", ref _transformOptionSelection, 1);

            ImGui.TreePop();
        }

        ImGui.End();
    }

    /// <summary>
    /// Draws the scene graph
    /// </summary>
    private void DrawSceneGraph(SceneGraph sceneGraph)
    {
        DrawSceneGraphNode(sceneGraph.Root);
    }

    /// <summary>
    /// Draws a scene graph node
    /// </summary>
    private void DrawSceneGraphNode(ISceneObject sceneObject)
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
            _commandList!.SetGraphicsResourceSet(0, _resourceSet);
            _commandList.UpdateBuffer(_modelUniformBuffer, 0, pawn.GlobalTransform.Matrix);

            // Update vertex buffer
            _commandList.SetVertexBuffer(0, pawn.ModelBuffer.VertexBuffer);
            _commandList.UpdateBuffer(pawn.ModelBuffer.VertexBuffer, 0, pawn.ModelBuffer.Model.GetVertices());

            // Update index buffer
            _commandList.SetIndexBuffer(pawn.ModelBuffer.IndexBuffer, IndexFormat.UInt32);
            _commandList.UpdateBuffer(pawn.ModelBuffer.IndexBuffer, 0, _bunnyProp!.ModelBuffer.Model.GetIndices());

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
}
