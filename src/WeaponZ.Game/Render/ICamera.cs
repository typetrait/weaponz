using System.Numerics;

namespace WeaponZ.Game.Render;

public interface ICamera
{
    public Vector3 Position { get; set; }
    public Matrix4x4 Projection { get; }
    public Matrix4x4 View { get; }

    public Vector3 Up { get; set; }
    public Vector3 Right { get; set;  }
    public Vector3 Forward { get; set;  }

    void UpdateViewMatrix();
}
