using System.Numerics;

namespace WeaponZ.Game.Render;

public struct LightingBuffer(Vector4 cameraPosition, Vector4 lightPosition)
{
    public Vector4 CameraPosition = cameraPosition;
    public Vector4 LightPosition = lightPosition;
}
