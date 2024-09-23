using System.Numerics;

namespace WeaponZ.Game.Render;

public struct Vertex(Vector3 position, Vector3 normal) : IVertex
{
    public Vector3 Position = position;
    public Vector3 Normal = normal;

    // 3 floats for position = 12 bytes
    // 3 floats for normal = 12 bytes
    // Total = 24 bytes
    public static uint SizeInBytes => 24;
}
