using System.Numerics;
using System.Runtime.InteropServices;

namespace WeaponZ.Game.Render;

public interface ILight
{
    LightType Type { get; }
    Vector4 Color { get; }
}

public enum LightType
{
    Point,
    Directional
}

[StructLayout(LayoutKind.Sequential)]
public struct PointLight(Vector3 position, Vector3 color) : ILight
{
    public readonly LightType Type => LightType.Point;
    public Vector4 Color { get; } = new(color, 1.0f);


    public Vector4 Position = new(position, 1.0f);
}

[StructLayout(LayoutKind.Sequential)]
public struct DirectionalLight(Vector3 direction, Vector3 color) : ILight
{
    public readonly LightType Type => LightType.Directional;
    public Vector4 Color { get; } = new(color, 1.0f);


    public Vector4 Direction = new(direction, 1.0f);
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct LightingBuffer
{
    public Vector4 CameraPosition;

    public fixed float Lights[256 * 12]; // 256 Lights * (1 type int + 4 Position floats + 4 Color floats)

    public int LightCount;

    public LightingBuffer(Vector4 cameraPosition, ILight[] lights)
    {
        CameraPosition = cameraPosition;
        LightCount = Math.Min(lights.Length, 256);

        // Copy light data into the fixed-size array
        fixed (float* pLights = Lights)
        {
            for (int i = 0; i < LightCount; i++)
            {
                int offset = i * 12;

                ILight light = lights[i];

                if (light.Type is LightType.Point && light is PointLight p)
                {
                    // Position (4 floats)
                    pLights[offset] = p.Position.X;
                    pLights[offset + 1] = p.Position.Y;
                    pLights[offset + 2] = p.Position.Z;
                    pLights[offset + 3] = p.Position.W;

                    // Color (4 floats)
                    pLights[offset + 4] = p.Color.X;
                    pLights[offset + 5] = p.Color.Y;
                    pLights[offset + 6] = p.Color.Z;
                    pLights[offset + 7] = p.Color.W;
                }
                else if (light.Type is LightType.Directional && light is DirectionalLight d)
                {
                    // Position (4 floats)
                    pLights[offset] = d.Direction.X;
                    pLights[offset + 1] = d.Direction.Y;
                    pLights[offset + 2] = d.Direction.Z;
                    pLights[offset + 3] = d.Direction.W;

                    // Color (4 floats)
                    pLights[offset + 4] = d.Color.X;
                    pLights[offset + 5] = d.Color.Y;
                    pLights[offset + 6] = d.Color.Z;
                    pLights[offset + 7] = d.Color.W;
                }

                pLights[offset + 8] = (int)lights[i].Type;
            }
        }
    }

}
