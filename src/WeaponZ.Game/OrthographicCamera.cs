using System.Numerics;

namespace WeaponZ.Game;

public class OrthographicCamera
{
    public float ZNear { get; private set; }
    public float ZFar { get; private set; }
    public Vector3 Position { get; private set; }

    public Matrix4x4 Projection { get; private set; }
    public Matrix4x4 View { get; private set; }

    // Used by uniform buffers
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

    public void Update(float dt) { }
}

public struct ViewProjectionMatrix(Matrix4x4 view, Matrix4x4 projection)
{
    public Matrix4x4 View = view;
    public Matrix4x4 Projecion = projection;

    public const uint SizeInBytes = 128;
}
