using System.Numerics;

using Veldrid;

namespace WeaponZ.Game.Render;

public struct Vertex(Vector3 position, Vector3 normal) : IVertex
{
    public Vector3 Position = position;
    public Vector3 Normal = normal;

    // 3 floats for position = 12 bytes
    // 3 floats for normal = 12 bytes
    // Total = 24 bytes
    public static uint SizeInBytes => 24;

    public static VertexLayoutDescription GetLayout()
    {
        return new VertexLayoutDescription(
            new VertexElementDescription(
                "Position",
                VertexElementFormat.Float3,
                VertexElementSemantic.TextureCoordinate
            ),
            new VertexElementDescription(
                "Normal",
                VertexElementFormat.Float3,
                VertexElementSemantic.TextureCoordinate
            )
        );
    }
}

public struct VertexPositionColor(Vector3 position, Vector3 color) : IVertex
{
    public Vector3 Position = position;
    public Vector3 Color = color;

    // 3 floats for position = 12 bytes
    // 3 floats for color = 12 bytes
    // Total = 24 bytes
    public static uint SizeInBytes => 24;

    public static VertexLayoutDescription GetLayout()
    {
        return new VertexLayoutDescription(
            new VertexElementDescription(
                "Position",
                VertexElementFormat.Float3,
                VertexElementSemantic.TextureCoordinate
            ),
            new VertexElementDescription(
                "Color",
                VertexElementFormat.Float3,
                VertexElementSemantic.TextureCoordinate
            )
        );
    }
}
