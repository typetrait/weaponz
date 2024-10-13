using System.Numerics;
using System.Runtime.InteropServices;

namespace WeaponZ.Game.Render;

public enum LightType
{
    Point,
    Directional
}

[StructLayout(LayoutKind.Sequential)]
public struct Light(LightType type, Vector3 position, Vector3 color)
{
    public LightType Type = type;
    public Vector4 Position = new(position, 1.0f);
    public Vector4 Color = new(color, 1.0f);
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct LightingBuffer
{
    public Vector4 CameraPosition;

    public fixed float Lights[256 * 12]; // 256 Lights * (1 type int + 4 Position floats + 4 Color floats)

    public int LightCount;

    public LightingBuffer(Vector4 cameraPosition, Light[] lights)
    {
        CameraPosition = cameraPosition;
        LightCount = Math.Min(lights.Length, 256);

        // Copy light data into the fixed-size array
        fixed (float* pLights = Lights)
        {
            for (int i = 0; i < LightCount; i++)
            {
                int offset = i * 12;

                // Position (4 floats)
                pLights[offset] = lights[i].Position.X;
                pLights[offset + 1] = lights[i].Position.Y;
                pLights[offset + 2] = lights[i].Position.Z;
                pLights[offset + 3] = lights[i].Position.W;

                // Color (4 floats)
                pLights[offset + 4] = lights[i].Color.X;
                pLights[offset + 5] = lights[i].Color.Y;
                pLights[offset + 6] = lights[i].Color.Z;
                pLights[offset + 7] = lights[i].Color.W;

                pLights[offset + 8] = (int)lights[i].Type;
            }
        }
    }
}
