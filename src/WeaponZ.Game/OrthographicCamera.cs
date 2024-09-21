using System.Numerics;
using WeaponZ.Game.Input;

namespace WeaponZ.Game;

public class OrthographicCamera
{
    public float BaseSpeed { get; set; } = 0.0005f;
    public float SpeedModifier { get; set; } = 2.2f;

    public float ZNear { get; private set; }
    public float ZFar { get; private set; }
    public Vector3 Position { get; private set; }

    public Matrix4x4 Projection { get; private set; }
    public Matrix4x4 View { get; private set; }

    public ViewProjectionMatrix ViewProjection { get; private set; }

    public Vector3 Up { get; private set; }
    public Vector3 Forward { get; private set; }
    public Vector3 Right { get; private set; }

    public OrthographicCamera(float width, float height, float zNear, float zFar, Vector3 position)
    {
        ZNear = zNear;
        ZFar = zFar;
        Position = position;

        Projection = Matrix4x4.CreatePerspectiveFieldOfView(1.0f, width / height, zNear, zFar);

        Up = Vector3.UnitY;
        Forward = -Vector3.UnitZ;

        View = Matrix4x4.CreateLookAt(position, position + Forward, Up);

        ViewProjection = new ViewProjectionMatrix(View, Projection);

        Right = Vector3.Cross(Up, Forward);
        Up = Vector3.Cross(Forward, Right);
    }

    public void Update(Keyboard keyboard, Mouse mouse, float dt)
    {
        Vector3 translation = Vector3.Zero;
        float speed = BaseSpeed;

        if (keyboard.IsKeyDown(Veldrid.Key.ShiftLeft))
        {
            speed *= SpeedModifier;
        }

        if (keyboard.IsKeyDown(Veldrid.Key.W))
        {
            translation += Forward * speed;
        }

        if (keyboard.IsKeyDown(Veldrid.Key.A))
        {
            translation += Right * speed;
        }

        if (keyboard.IsKeyDown(Veldrid.Key.S))
        {
            translation += -Forward * speed;
        }

        if (keyboard.IsKeyDown(Veldrid.Key.D))
        {
            translation += -Right * speed;
        }

        if (keyboard.IsKeyDown(Veldrid.Key.Space))
        {
            translation += Vector3.UnitY * speed;
        }

        if (keyboard.IsKeyDown(Veldrid.Key.LControl))
        {
            translation += -Vector3.UnitY * speed;
        }

        if (mouse.IsButtonDown(Veldrid.MouseButton.Left))
        {
            Console.WriteLine($"Mouse clicked at: X = {mouse.X}; Y = {mouse.Y}");
        }

        Position += translation;
        UpdateViewMatrix();
    }

    private void UpdateViewMatrix()
    {
        View = Matrix4x4.CreateLookAt(Position, Position + Forward, Up);
        ViewProjection = new ViewProjectionMatrix(View, Projection);
    }
}

public struct ViewProjectionMatrix(Matrix4x4 view, Matrix4x4 projection)
{
    public Matrix4x4 View = view;
    public Matrix4x4 Projecion = projection;

    public const uint SizeInBytes = 128;
}
