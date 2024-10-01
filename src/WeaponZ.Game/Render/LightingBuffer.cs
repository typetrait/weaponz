using System.Numerics;
using System.Runtime.InteropServices;

namespace WeaponZ.Game.Render;

[StructLayout(LayoutKind.Sequential)]
public struct PointLight(Vector3 position, Vector3 color)
{
    public Vector4 Position = new(position, 1.0f);
    public Vector4 Color = new(color, 1.0f);
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct LightingBuffer
{
    public Vector4 CameraPosition;

    public fixed float PointLights[256 * 8]; // 256 PointLights * (4 Position floats + 4 Color floats)

    public int PointLightCount;

    public LightingBuffer(Vector4 cameraPosition, PointLight[] pointLights)
    {
        CameraPosition = cameraPosition;
        PointLightCount = Math.Min(pointLights.Length, 256);

        // Copy point light data into the fixed-size array
        fixed (float* pLights = PointLights)
        {
            for (int i = 0; i < PointLightCount; i++)
            {
                int offset = i * 8;

                // Position (4 floats)
                pLights[offset] = pointLights[i].Position.X;
                pLights[offset + 1] = pointLights[i].Position.Y;
                pLights[offset + 2] = pointLights[i].Position.Z;
                pLights[offset + 3] = pointLights[i].Position.W;

                // Color (4 floats)
                pLights[offset + 4] = pointLights[i].Color.X;
                pLights[offset + 5] = pointLights[i].Color.Y;
                pLights[offset + 6] = pointLights[i].Color.Z;
                pLights[offset + 7] = pointLights[i].Color.W;
            }
        }
    }
}
