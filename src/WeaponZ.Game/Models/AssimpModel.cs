using System.Numerics;
using Assimp;
using WeaponZ.Game.Render;

namespace WeaponZ.Game.Models;

public class AssimpModel
{
    public required Scene Scene { get; init; }

    public Vertex[] GetVertices()
    {
        Console.WriteLine(Scene.Meshes[0].Vertices.Count);

        var vertices = new List<Vertex>();

        for (int i = 0; i < Scene.Meshes[0].Vertices.Count; i++)
        {
            var vertex = Scene.Meshes[0].Vertices[i];
            var normal = Scene.Meshes[0].Normals[i];

            vertices.Add(
                new Vertex(new Vector3(vertex.X, vertex.Y, vertex.Z), new Vector3(normal.X, normal.Y, normal.Z))
            );
        }

        return [.. vertices];
    }

    public uint[] GetIndices()
    {
        return Scene.Meshes[0].Faces.SelectMany(f => f.Indices).Select(i => (uint)i).ToArray();
    }

    public uint GetVertexCount()
    {
        return (uint)GetVertices().Length;
    }

    public uint GetIndexCount()
    {
        return (uint)GetIndices().Length;
    }
}
