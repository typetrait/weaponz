using System.Numerics;

using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

using WeaponZ.Game.Editor;
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
    private PerspectiveCamera? _perspectiveCamera;
    private CameraSceneObject? _mainCamera;

    // Input
    private KeyboardState? _keyboardState;
    private MouseState? _mouseState;

    public event EventHandler<MouseButtonEventArgs>? MouseButtonPressed;
    public event EventHandler<MouseButtonEventArgs>? MouseButtonReleased;
    public event EventHandler<MouseEventArgs>? MouseMoved;
    public event EventHandler<KeyboardEventArgs>? KeyPressed;
    public event EventHandler<KeyboardEventArgs>? KeyReleased;

    // Scene
    private SceneGraph? _sceneGraph;

    // Editor Debug Layer
    private EditorDebugLayer? _editorDebugLayer;

    // Window
    private Sdl2Window? _window;

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

        _window = VeldridStartup.CreateWindow(ref windowCreateInfo);

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
        _graphicsDevice = VeldridStartup.CreateGraphicsDevice(_window, graphicsDeviceOptions);

        // OpenGL graphics device
        // _graphicsDevice = VeldridStartup.CreateDefaultOpenGLGraphicsDevice(
        //     graphicsDeviceOptions,
        //     window,
        //     GraphicsBackend.OpenGL
        // );

        // Vulkan graphics device
        // _graphicsDevice = VeldridStartup.CreateVulkanGraphicsDevice(graphicsDeviceOptions, window);

        _renderer = new Renderer(_graphicsDevice);

        // ImGui
        _imGuiRenderer = new ImGuiRenderer(_graphicsDevice, _renderer.PipelineOutput, _window.Width, _window.Height);

        // Camera
        _perspectiveCamera = new PerspectiveCamera(
            windowCreateInfo.WindowWidth,
            windowCreateInfo.WindowHeight,
            0.1f,
            100.0f,
            new Vector3(0.0f, 0.0f, 3.0f)
        );
        _mainCamera = new CameraSceneObject("Default Camera", new Transform(), _perspectiveCamera, this);

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
        var transform = new Transform() { Scale = new Vector3(0.002f) };
        var transform2 = new Transform() { Scale = new Vector3(0.001f) };
        transform2.TranslateY(1.0f);

        var models = new SampleModels();
        var modelBufferFactory = new ModelBufferFactory(_graphicsDevice.ResourceFactory);
        var bunnyModelBuffer = modelBufferFactory.CreateModelBuffer<Vertex>(models.Bunny);

        var floorModel = modelBufferFactory.CreateModelBuffer<Vertex>(models.Quad);

       // var spongaModelBuffer = modelBufferFactory.CreateModelBuffer<Vertex>(models.Sponga);

        // Scene Objects
        var bunnyProp = new PawnSceneObject("Bunny", transform, bunnyModelBuffer);
        var bunnyProp2 = new PawnSceneObject("Bunny 2", transform2, bunnyModelBuffer);

        var floor = new PawnSceneObject("Floor", new Transform(), floorModel);
        floor.Transform.RotateY((float)MathUtils.DegreesToRadians(-90.0f));
        floor.Transform.TranslateY(-0.480f);
        floor.Transform.ScaleX(50.0f);
        floor.Transform.ScaleY(50.0f);

        //var sponga = new PawnSceneObject("Sponga", new Transform(), spongaModelBuffer);

        _sceneGraph.AppendTo(_sceneGraph.Root, bunnyProp);
        _sceneGraph.AppendTo(_sceneGraph.Root.Children[0], bunnyProp2);

        _sceneGraph.AppendTo(_sceneGraph.Root, floor);

        //_sceneGraph.AppendTo(_sceneGraph.Root, sponga);

        _sceneGraph.AppendTo(_sceneGraph.Root, _mainCamera);

        var light1 = new LightSceneObject("Light 1", new Transform() { Position = new Vector3(1.4f, 1.0f, 2.0f) }, new Vector3(1.0f));
        _sceneGraph.AppendTo(_sceneGraph.Root, light1);

        var light2 = new LightSceneObject("Light 2", new Transform() { Position = new Vector3(-2.6f, -3.0f, 1.0f) }, new Vector3(0.2f, 0.1f, 0.5f));
        _sceneGraph.AppendTo(_sceneGraph.Root, light2);

        _keyboardState.KeyPressed += (s, e) => KeyPressed?.Invoke(s, e);
        _keyboardState.KeyReleased += (s, e) => KeyReleased?.Invoke(s, e);

        _mouseState.MouseButtonPressed += (s, e) => MouseButtonPressed?.Invoke(s, e);
        _mouseState.MouseButtonReleased += (s, e) => MouseButtonReleased?.Invoke(s, e);
        _mouseState.MouseMoved += (s, e) => MouseMoved?.Invoke(s, e);

        _editorDebugLayer = new EditorDebugLayer(_imGuiRenderer, this, _sceneGraph);

        // Main loop
        while (_window.Exists)
        {
            var inputSnapshot = _window.PumpEvents();
            _keyboardState.UpdateFromSnapshot(inputSnapshot);
            _mouseState.UpdateFromSnapshot(inputSnapshot, _window.MouseDelta.X, _window.MouseDelta.Y);

            _editorDebugLayer.Update(deltaTime, inputSnapshot);
            _sceneGraph.Update(_sceneGraph.Root, deltaTime);

            Draw(deltaTime);

            frameTimer.SleepUntilTargetFrameTime(Config.TargetFps);
            deltaTime = frameTimer.Restart();
        }
    }

    public void Draw(TimeSpan deltaTime)
    {
        // Validate resources
        if (_graphicsDevice is null
            || _perspectiveCamera is null
            || _mainCamera is null
            || _keyboardState is null
            || _mouseState is null
            || _sceneGraph is null
            || _imGuiRenderer is null
            || _editorDebugLayer is null
            || _renderer is null
        )
        {
            throw new InvalidOperationException("Failed to initialize resources.");
        }

        _renderer.BeginFrame(_mainCamera, _sceneGraph);
        _renderer.DrawSceneGraphNode(_sceneGraph.Root);
        _renderer.EndFrame();

        // Debug
        _renderer.BeginDebugFrame(_mainCamera);

        //foreach (var pawn in SceneGraph.FindAllByKind(_sceneGraph.Root, SceneObjectKind.Pawn))
        //{
        //    _renderer.DrawLine(
        //        _perspectiveCamera.Position + _perspectiveCamera.Forward,
        //        pawn.GlobalTransform.Position,
        //        new Vector3(1.0f, 0.0f, 0.0f)
        //    );
        //}

        foreach (var light in SceneGraph.FindAllByKind(_sceneGraph.Root, SceneObjectKind.Light))
        {
            if (light is LightSceneObject lso && lso is not null)
            {
                _renderer.DrawLine(
                    new Vector3(light.GlobalTransform.Position.X - 0.15f, light.GlobalTransform.Position.Y, light.GlobalTransform.Position.Z),
                    new Vector3(light.GlobalTransform.Position.X + 0.15f, light.GlobalTransform.Position.Y, light.GlobalTransform.Position.Z),
                    new Vector3(1.0f, 1.0f, 1.0f)
                );

                _renderer.DrawLine(
                    new Vector3(light.GlobalTransform.Position.X, light.GlobalTransform.Position.Y - 0.15f, light.GlobalTransform.Position.Z),
                    new Vector3(light.GlobalTransform.Position.X, light.GlobalTransform.Position.Y + 0.15f, light.GlobalTransform.Position.Z),
                    new Vector3(1.0f, 1.0f, 1.0f)
                );

                _renderer.DrawLine(
                    new Vector3(light.GlobalTransform.Position.X, light.GlobalTransform.Position.Y, light.GlobalTransform.Position.Z - 0.15f),
                    new Vector3(light.GlobalTransform.Position.X, light.GlobalTransform.Position.Y, light.GlobalTransform.Position.Z + 0.15f),
                    new Vector3(1.0f, 1.0f, 1.0f)
                );
            }
        }

        _editorDebugLayer.Draw(_graphicsDevice, _renderer);

        _renderer.EndDebugFrame();

        _graphicsDevice.SwapBuffers();
    }

    public void SetMouseGrab(bool shouldGrab)
    {
        if (_window is null)
        {
            throw new InvalidOperationException($"Can't set grab state. {nameof(_window)} is null.");
        }

        Sdl2Native.SDL_SetWindowGrab(_window.SdlWindowHandle, shouldGrab);
        Sdl2Native.SDL_ShowCursor(shouldGrab ? Sdl2Native.SDL_DISABLE : Sdl2Native.SDL_ENABLE);
    }

    public void UpdateWarpedCursorPosition(Vector2 cursorPosition)
    {
        if (_window is null)
        {
            throw new InvalidOperationException($"Can't warp cursor position. {nameof(_window)} is null.");
        }

        Sdl2Native.SDL_WarpMouseInWindow(_window.SdlWindowHandle, (int)cursorPosition.X, (int)cursorPosition.Y);
    }

    public bool IsKeyUp(Key key)
    {
        return _keyboardState is not null && _keyboardState.IsKeyUp(key);
    }

    public bool IsKeyDown(Key key)
    {
        return _keyboardState is not null && _keyboardState.IsKeyDown(key);
    }
}
