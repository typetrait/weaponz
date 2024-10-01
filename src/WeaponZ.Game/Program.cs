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
    private GraphicsDevice? _graphicsDevice;
    private Renderer? _renderer;

    // ImGui
    private ImGuiRenderer? _imGuiRenderer;

    // Camera
    private OrthographicCamera? _orthographicCamera;
    private CameraSceneObject? _mainCamera;

    // Input
    private KeyboardState? _keyboardState;
    private MouseState? _mouseState;

    // Scene
    private SceneGraph? _sceneGraph;
    private PawnSceneObject? _bunnyProp;
    private PawnSceneObject? _bunnyProp2;
    private Transform? _transform;

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

        // OpenGL graphics device
        // _graphicsDevice = VeldridStartup.CreateDefaultOpenGLGraphicsDevice(
        //     graphicsDeviceOptions,
        //     window,
        //     GraphicsBackend.OpenGL
        // );

        // Vulkan graphics device
        // _graphicsDevice = VeldridStartup.CreateVulkanGraphicsDevice(graphicsDeviceOptions, window);

        _renderer = new Renderer(_graphicsDevice);

        // Imgui
        _imGuiRenderer = new ImGuiRenderer(_graphicsDevice, _renderer.PipelineOutput, window.Width, window.Height);

        // Camera
        _orthographicCamera = new OrthographicCamera(
            windowCreateInfo.WindowWidth,
            windowCreateInfo.WindowHeight,
            0.1f,
            100.0f,
            new Vector3(0.0f, 0.0f, 3.0f)
        );
        _mainCamera = new CameraSceneObject("Default Camera", new Transform(), _orthographicCamera, this);

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
        var modelBufferFactory = new ModelBufferFactory(_graphicsDevice.ResourceFactory);
        var bunnyModelBuffer = modelBufferFactory.CreateModelBuffer<Vertex>(models.Bunny);

        _bunnyProp = new PawnSceneObject("Bunny", _transform, bunnyModelBuffer);
        _bunnyProp2 = new PawnSceneObject("Bunny 2", transform2, bunnyModelBuffer);

        _sceneGraph.AppendTo(_sceneGraph.Root, _bunnyProp);
        _sceneGraph.AppendTo(_sceneGraph.Root.Children[0], _bunnyProp2);

        _sceneGraph.AppendTo(_sceneGraph.Root, _mainCamera);

        var light1 = new LightSceneObject("Light 1", new Transform() { Position = new Vector3(1.4f, 1.0f, 2.0f) }, new Vector3(1.0f));
        _sceneGraph.AppendTo(_sceneGraph.Root, light1);

        var light2 = new LightSceneObject("Light 2", new Transform() { Position = new Vector3(-2.6f, -3.0f, 1.0f) }, new Vector3(0.2f, 0.1f, 0.5f));
        _sceneGraph.AppendTo(_sceneGraph.Root, light2);

        SetupImGuiStyles();

        // Main loop
        while (window.Exists)
        {
            InputSnapshot inputSnapshot = window.PumpEvents();
            _keyboardState.UpdateFromSnapshot(inputSnapshot);
            _mouseState.UpdateFromSnapshot(inputSnapshot);

            _imGuiRenderer.Update((float)deltaTime.TotalSeconds, inputSnapshot);

            Draw(deltaTime);

            frameTimer.SleepUntilTargetFrameTime(Config.TargetFps);
            deltaTime = frameTimer.Restart();
        }
    }

    public void Draw(TimeSpan deltaTime)
    {
        // Validate resources
        if (_graphicsDevice is null
            || _orthographicCamera is null
            || _keyboardState is null
            || _mouseState is null
            || _sceneGraph is null
            || _transform is null
            || _imGuiRenderer is null
        )
        {
            throw new InvalidOperationException("Failed to initialize resources.");
        }

        // Update camera
        _orthographicCamera.Update(_keyboardState, _mouseState, deltaTime);

        // Setup ImGui windows
        SetupSceneGraphUi(_sceneGraph);

        if (_selectedObject is not null)
        {
            SetupTransformUi(_selectedObject.Transform);
        }

        _renderer?.BeginFrame(_mainCamera!, _sceneGraph);

        // Draw Scene
        _renderer?.DrawSceneGraphNode(_sceneGraph.Root);

        // Draw ImGui
        _imGuiRenderer.Render(_graphicsDevice, _renderer?._commandList);

        _renderer?.EndFrame();
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

        if (sceneObject.Children.Count < 1)
        {
            treeNodeFlags |= ImGuiTreeNodeFlags.Leaf;
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

        ImGui.Separator();

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

    public static void SetupImGuiStyles()
    {
        ImGuiStylePtr style = ImGui.GetStyle();
        var colors = style.Colors;
        colors[(int)ImGuiCol.Text] = new Vector4(1.00f, 1.00f, 1.00f, 1.00f);
        colors[(int)ImGuiCol.TextDisabled] = new Vector4(0.50f, 0.50f, 0.50f, 1.00f);
        colors[(int)ImGuiCol.WindowBg] = new Vector4(0.10f, 0.10f, 0.10f, 1.00f);
        colors[(int)ImGuiCol.ChildBg] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
        colors[(int)ImGuiCol.PopupBg] = new Vector4(0.19f, 0.19f, 0.19f, 0.92f);
        colors[(int)ImGuiCol.Border] = new Vector4(0.19f, 0.19f, 0.19f, 0.29f);
        colors[(int)ImGuiCol.BorderShadow] = new Vector4(0.00f, 0.00f, 0.00f, 0.24f);
        colors[(int)ImGuiCol.FrameBg] = new Vector4(0.05f, 0.05f, 0.05f, 0.54f);
        colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.19f, 0.19f, 0.19f, 0.54f);
        colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.20f, 0.22f, 0.23f, 1.00f);
        colors[(int)ImGuiCol.TitleBg] = new Vector4(0.00f, 0.00f, 0.00f, 1.00f);
        colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.06f, 0.06f, 0.06f, 1.00f);
        colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(0.00f, 0.00f, 0.00f, 1.00f);
        colors[(int)ImGuiCol.MenuBarBg] = new Vector4(0.14f, 0.14f, 0.14f, 1.00f);
        colors[(int)ImGuiCol.ScrollbarBg] = new Vector4(0.05f, 0.05f, 0.05f, 0.54f);
        colors[(int)ImGuiCol.ScrollbarGrab] = new Vector4(0.34f, 0.34f, 0.34f, 0.54f);
        colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.40f, 0.40f, 0.40f, 0.54f);
        colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(0.56f, 0.56f, 0.56f, 0.54f);
        colors[(int)ImGuiCol.CheckMark] = new Vector4(0.33f, 0.67f, 0.86f, 1.00f);
        colors[(int)ImGuiCol.SliderGrab] = new Vector4(0.34f, 0.34f, 0.34f, 0.54f);
        colors[(int)ImGuiCol.SliderGrabActive] = new Vector4(0.56f, 0.56f, 0.56f, 0.54f);
        colors[(int)ImGuiCol.Button] = new Vector4(0.05f, 0.05f, 0.05f, 0.54f);
        colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.19f, 0.19f, 0.19f, 0.54f);
        colors[(int)ImGuiCol.ButtonActive] = new Vector4(0.20f, 0.22f, 0.23f, 1.00f);
        colors[(int)ImGuiCol.Header] = new Vector4(0.00f, 0.00f, 0.00f, 0.52f);
        colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.00f, 0.00f, 0.00f, 0.36f);
        colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.20f, 0.22f, 0.23f, 0.33f);
        colors[(int)ImGuiCol.Separator] = new Vector4(0.28f, 0.28f, 0.28f, 0.29f);
        colors[(int)ImGuiCol.SeparatorHovered] = new Vector4(0.44f, 0.44f, 0.44f, 0.29f);
        colors[(int)ImGuiCol.SeparatorActive] = new Vector4(0.40f, 0.44f, 0.47f, 1.00f);
        colors[(int)ImGuiCol.ResizeGrip] = new Vector4(0.28f, 0.28f, 0.28f, 0.29f);
        colors[(int)ImGuiCol.ResizeGripHovered] = new Vector4(0.44f, 0.44f, 0.44f, 0.29f);
        colors[(int)ImGuiCol.ResizeGripActive] = new Vector4(0.40f, 0.44f, 0.47f, 1.00f);
        colors[(int)ImGuiCol.Tab] = new Vector4(0.00f, 0.00f, 0.00f, 0.52f);
        colors[(int)ImGuiCol.TabHovered] = new Vector4(0.14f, 0.14f, 0.14f, 1.00f);
        colors[(int)ImGuiCol.TabActive] = new Vector4(0.20f, 0.20f, 0.20f, 0.36f);
        colors[(int)ImGuiCol.TabUnfocused] = new Vector4(0.00f, 0.00f, 0.00f, 0.52f);
        colors[(int)ImGuiCol.TabUnfocusedActive] = new Vector4(0.14f, 0.14f, 0.14f, 1.00f);
        colors[(int)ImGuiCol.DockingPreview] = new Vector4(0.33f, 0.67f, 0.86f, 1.00f);
        colors[(int)ImGuiCol.DockingEmptyBg] = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
        colors[(int)ImGuiCol.PlotLines] = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
        colors[(int)ImGuiCol.PlotLinesHovered] = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
        colors[(int)ImGuiCol.PlotHistogram] = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
        colors[(int)ImGuiCol.PlotHistogramHovered] = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
        colors[(int)ImGuiCol.TableHeaderBg] = new Vector4(0.00f, 0.00f, 0.00f, 0.52f);
        colors[(int)ImGuiCol.TableBorderStrong] = new Vector4(0.00f, 0.00f, 0.00f, 0.52f);
        colors[(int)ImGuiCol.TableBorderLight] = new Vector4(0.28f, 0.28f, 0.28f, 0.29f);
        colors[(int)ImGuiCol.TableRowBg] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
        colors[(int)ImGuiCol.TableRowBgAlt] = new Vector4(1.00f, 1.00f, 1.00f, 0.06f);
        colors[(int)ImGuiCol.TextSelectedBg] = new Vector4(0.20f, 0.22f, 0.23f, 1.00f);
        colors[(int)ImGuiCol.DragDropTarget] = new Vector4(0.33f, 0.67f, 0.86f, 1.00f);
        colors[(int)ImGuiCol.NavHighlight] = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
        colors[(int)ImGuiCol.NavWindowingHighlight] = new Vector4(1.00f, 0.00f, 0.00f, 0.70f);
        colors[(int)ImGuiCol.NavWindowingDimBg] = new Vector4(1.00f, 0.00f, 0.00f, 0.20f);
        colors[(int)ImGuiCol.ModalWindowDimBg] = new Vector4(1.00f, 0.00f, 0.00f, 0.35f);

        style.WindowPadding = new Vector2(8.00f, 8.00f);
        style.FramePadding = new Vector2(5.00f, 2.00f);
        style.CellPadding = new Vector2(6.00f, 6.00f);
        style.ItemSpacing = new Vector2(6.00f, 6.00f);
        style.ItemInnerSpacing = new Vector2(6.00f, 6.00f);
        style.TouchExtraPadding = new Vector2(0.00f, 0.00f);
        style.IndentSpacing = 25;
        style.ScrollbarSize = 15;
        style.GrabMinSize = 10;
        style.WindowBorderSize = 1;
        style.ChildBorderSize = 1;
        style.PopupBorderSize = 1;
        style.FrameBorderSize = 1;
        style.TabBorderSize = 1;
        style.WindowRounding = 7;
        style.ChildRounding = 4;
        style.FrameRounding = 3;
        style.PopupRounding = 4;
        style.ScrollbarRounding = 9;
        style.GrabRounding = 3;
        style.LogSliderDeadzone = 4;
        style.TabRounding = 4;
    }
}
