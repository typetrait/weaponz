using WeaponZ.Game.Render;

namespace WeaponZ.Game.Models;

public class Model
{
    public required Vertex[] Vertices { get; init; }
    public required uint[] Indices { get; init; }

    public Vertex[] GetVertices()
    {
        return Vertices;
    }

    public uint[] GetIndices()
    {
        return Indices;
    }

    public uint GetVertexCount()
    {
        return (uint)Vertices.Length;
    }

    public uint GetIndexCount()
    {
        return (uint)Indices.Length;
    }
}
