using System.Numerics;

namespace WeaponZ.Game.Render;

public class OrthographicCamera : ICamera
{
    public float ZNear { get; private set; }
    public float ZFar { get; private set; }
    public Vector3 Position { get; set; }

    public Matrix4x4 Projection { get; private set; }
    public Matrix4x4 View { get; private set; }

    public ViewProjectionMatrix ViewProjection { get; private set; }

    public Vector3 Up { get; set; }
    public Vector3 Forward { get; set; }
    public Vector3 Right { get; set; }

    public OrthographicCamera(float width, float height, float zNear, float zFar, Vector3 position)
    {
        ZNear = zNear;
        ZFar = zFar;
        Position = position;

        Projection = Matrix4x4.CreateOrthographicOffCenter(-10.0f, 10.0f, -10.0f, 10.0f, zNear, zFar);

        Up = Vector3.UnitY;
        Forward = -Vector3.UnitZ;

        View = Matrix4x4.CreateLookAt(position, Vector3.Zero, Up);

        ViewProjection = new ViewProjectionMatrix(View, Projection);

        Right = Vector3.Cross(Up, Forward);
        //Up = Vector3.Cross(Forward, Right);
    }

    public void UpdateViewMatrix()
    {
        Forward = Vector3.Normalize(Forward);
        Right = Vector3.Normalize(Vector3.Cross(Forward, Vector3.UnitY));
        Up = Vector3.Cross(Right, Forward);

        View = Matrix4x4.CreateLookAt(Position, Position + Forward, Up);
        ViewProjection = new ViewProjectionMatrix(View, Projection);
    }
}
