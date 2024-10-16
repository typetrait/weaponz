using System.Numerics;

using ImGuiNET;

using Veldrid;
using WeaponZ.Game.Input;
using WeaponZ.Game.Render;

namespace WeaponZ.Game.Scene;

public class CameraSceneObject : ISceneObject
{
    public string DisplayName { get; }
    public Transform Transform { get; }
    public IList<ISceneObject> Children { get; }
    public SceneObjectKind Kind => SceneObjectKind.Camera;
    public ISceneObject? Parent { get; set; }
    public Transform GlobalTransform { get; set; }
    public ICamera Camera { get; }

    public float BaseSpeed { get; set; } = 3.5f;
    public float SpeedModifier { get; set; } = 2.5f;

    private readonly IInputContext _inputContext;

    private const Key DoSomethingKeyBind = Key.F;

    // Input stuff should be in a controller abstraction
    private Vector2 _dragStartPosition;

    private readonly Dictionary<MouseButton, bool> _draggingInputs = new()
    {
        {MouseButton.Left, false},
        {MouseButton.Middle, false},
        {MouseButton.Right, false},
    };

    private float _yaw = 0f;
    private float _pitch = 0f;

    private const float DragSensitivity = 0.002f;

    public CameraSceneObject(
        string displayName,
        Transform transform,
        ICamera camera,
        IInputContext inputContext
    )
    {
        Children = [];
        DisplayName = displayName;
        Transform = transform;
        GlobalTransform = Transform;

        Camera = camera;
        _inputContext = inputContext;

        _inputContext.KeyPressed += OnKeyPressed;

        _inputContext.MouseButtonPressed += OnMouseButtonPressed;
        _inputContext.MouseButtonReleased += OnMouseButtonReleased;
        _inputContext.MouseMoved += OnMouseMoved;
    }

    public void Update(TimeSpan deltaTime)
    {
        // Camera.Position = GlobalTransform.Position;
        Vector3 translation = Vector3.Zero;
        float speed = BaseSpeed * (float)deltaTime.TotalSeconds;

        if (_inputContext.IsKeyDown(Key.ShiftLeft))
        {
            speed *= SpeedModifier;
        }

        if (_inputContext.IsKeyDown(Key.W))
        {
            translation += Camera.Forward * speed;
        }

        if (_inputContext.IsKeyDown(Key.A))
        {
            translation += -Camera.Right * speed;
        }

        if (_inputContext.IsKeyDown(Key.S))
        {
            translation += -Camera.Forward * speed;
        }

        if (_inputContext.IsKeyDown(Key.D))
        {
            translation += Camera.Right * speed;
        }

        if (_inputContext.IsKeyDown(Key.Space))
        {
            translation += Vector3.UnitY * speed;
        }

        if (_inputContext.IsKeyDown(Key.LControl))
        {
            translation += -Vector3.UnitY * speed;
        }

        Camera.Position += translation;
        Camera.UpdateViewMatrix();
    }

    private void OnKeyPressed(object? sender, KeyboardEventArgs e)
    {
        if (!e.IsRepeatingEvent)
        {
            if (e.Key is DoSomethingKeyBind)
            {
                Console.WriteLine("Did something...");
            }
        }
    }

    private void OnMouseButtonPressed(object? sender, MouseButtonEventArgs e)
    {
        //TODO: Fix this coupling
        if (ImGui.GetIO().WantCaptureMouse) { return; }

        // Reset every other input's dragging state
        foreach (var input in _draggingInputs)
        {
            if (input.Key != e.Button)
            {
                _draggingInputs[input.Key] = false;
            }
        }

        _draggingInputs[e.Button] = true;
        _dragStartPosition = new Vector2(e.X, e.Y);

        _inputContext.SetMouseGrab(true);
    }

    private void OnMouseButtonReleased(object? sender, MouseButtonEventArgs e)
    {
        if (_draggingInputs[e.Button])
        {
            _inputContext.SetMouseGrab(false);
        }

        _draggingInputs[e.Button] = false;
        _dragStartPosition = new Vector2();
    }

    private void OnMouseMoved(object? sender, MouseEventArgs e)
    {
        Vector2 mousePosition = new(e.X, e.Y);

        if (_draggingInputs[MouseButton.Left])
        {
            Vector2 dragDelta = mousePosition - _dragStartPosition;

            _inputContext.UpdateWarpedCursorPosition(_dragStartPosition);

            _yaw += -dragDelta.X * DragSensitivity;
            _pitch += -dragDelta.Y * DragSensitivity;

            _pitch = Math.Clamp(_pitch, -MathF.PI / 2 + 0.01f, MathF.PI / 2 - 0.01f);

            Matrix4x4 rotationMatrix = Matrix4x4.CreateFromYawPitchRoll(_yaw, _pitch, 0f);

            Camera.Forward = Vector3.Normalize(Vector3.Transform(-Vector3.UnitZ, rotationMatrix));
            Camera.Right = Vector3.Normalize(Vector3.Cross(Camera.Forward, Vector3.UnitY));
            Camera.Up = Vector3.Cross(Camera.Right, Camera.Forward);

            Camera.UpdateViewMatrix();
        }
        else if (_draggingInputs[MouseButton.Middle])
        {
            Vector2 dragDelta = mousePosition - _dragStartPosition;

            _inputContext.UpdateWarpedCursorPosition(_dragStartPosition);

            Camera.Position += -Camera.Right * dragDelta.X * DragSensitivity;
            Camera.Position += Camera.Up * dragDelta.Y * DragSensitivity;

            Camera.UpdateViewMatrix();
        }
        else if (_draggingInputs[MouseButton.Right])
        {
            Vector2 dragDelta = mousePosition - _dragStartPosition;

            _inputContext.UpdateWarpedCursorPosition(_dragStartPosition);

            Camera.Position += -Camera.Forward * (-dragDelta.X + dragDelta.Y) * DragSensitivity;

            Camera.UpdateViewMatrix();
        }
    }
}
