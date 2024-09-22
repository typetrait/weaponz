using System.Numerics;

namespace WeaponZ.Game;

public class Transform
{
    public Vector3 Position;
    public Vector3 Rotation;
    public Vector3 Scale;

    public Matrix4x4 Matrix => CalculateTransformationMatrix();

    public Transform()
    {
        Position = Vector3.Zero;
        Rotation = Vector3.Zero;
        Scale = Vector3.Zero;
    }

    public void TranslateX(float x)
    {
        Position.X += x;
    }

    public void TranslateY(float y)
    {
        Position.Y += y;
    }

    public void TranslateZ(float z)
    {
        Position.Z += z;
    }

    public void RotateX(float x)
    {
        Rotation.X += x;
    }

    public void RotateY(float y)
    {
        Rotation.Y += y;
    }

    public void RotateZ(float z)
    {
        Rotation.Z += z;
    }

    public void ScaleX(float x)
    {
        Scale.X += x;
    }

    public void ScaleY(float y)
    {
        Scale.Y += y;
    }

    public void ScaleZ(float z)
    {
        Scale.Z += z;
    }

    private Matrix4x4 CalculateTransformationMatrix()
    {
        var transform = Matrix4x4.Identity;

        transform = 
            Matrix4x4.CreateFromYawPitchRoll(Rotation.X, Rotation.Y, Rotation.Z)
            * Matrix4x4.CreateTranslation(Position)
            * Matrix4x4.CreateScale(Scale);

        return transform;
    }
}
