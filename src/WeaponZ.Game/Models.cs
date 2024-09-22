using System.Numerics;
using Assimp;
using Veldrid;

namespace WeaponZ.Game;

public class SampleModels
{
    private readonly ModelLoader _modelLoader;

    public SampleModels()
    {
        _modelLoader = new ModelLoader();

        Triangle = PrimitiveModelFactory.CreateTriangle();

        Cube = PrimitiveModelFactory.CreateCube();

        Bunny =
            _modelLoader.Load($"{Config.AssetsPath}/fbx/Bunny.fbx")
            ?? throw new InvalidOperationException("Failed to load model");

        Sponga =
            _modelLoader.Load($"{Config.AssetsPath}/fbx/Sponza.fbx")
            ?? throw new InvalidOperationException("Failed to load model");
    }

    public Model Triangle { get; }
    public Model Cube { get; }
    public Model Bunny { get; }
    public Model Sponga { get; }
}

public class ModelLoader
{
    private readonly AssimpContext _importer;

    public ModelLoader()
    {
        _importer = new AssimpContext();
    }

    public Model? Load(string filePath)
    {
        var scene = _importer.ImportFile(
            filePath,
            PostProcessSteps.Triangulate
                | PostProcessSteps.FlipUVs
                | PostProcessSteps.GenerateNormals
        // | PostProcessSteps.GenerateSmoothNormals
        );

        if (scene is null || scene.RootNode is null)
        {
            return null;
        }

        // Getting vertices and indices from the model is an expensive operation, we need this in memory.
        // Quick workaround
        var temp = new AssimpModel { Scene = scene };

        var model = new Model()
        {
            Vertices = temp.GetVertices(),
            Indices = temp.GetIndices(),
        };

        return model;
    }
}

public static class PrimitiveModelFactory
{
    public static Model CreateTriangle()
    {
        var triangle = new Model
        {
            Vertices =
            [
                new(new Vector3(-0.5f, -0.5f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f)),
                new(new Vector3(0.5f, -0.5f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f)),
                new(new Vector3(0.0f, 0.5f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f))
            ],
            Indices = [0, 1, 2]
        };

        return triangle;
    }

    public static Model CreateCube()
    {
        var size = 1;

        var cube = new Model
        {
            Vertices =
            [
                // Front face
                new(
                    new Vector3(-size / 2.0f, -size / 2.0f, size / 2.0f),
                    new Vector3(0.0f, 0.0f, 1.0f)
                ),
                new(
                    new Vector3(size / 2.0f, -size / 2.0f, size / 2.0f),
                    new Vector3(0.0f, 0.0f, 1.0f)
                ),
                new(
                    new Vector3(size / 2.0f, size / 2.0f, size / 2.0f),
                    new Vector3(0.0f, 0.0f, 1.0f)
                ),
                new(
                    new Vector3(-size / 2.0f, size / 2.0f, size / 2.0f),
                    new Vector3(0.0f, 0.0f, 1.0f)
                ),
                // Back face
                new(
                    new Vector3(-size / 2.0f, -size / 2.0f, -size / 2.0f),
                    new Vector3(0.0f, 0.0f, -1.0f)
                ),
                new(
                    new Vector3(size / 2.0f, -size / 2.0f, -size / 2.0f),
                    new Vector3(0.0f, 0.0f, -1.0f)
                ),
                new(
                    new Vector3(size / 2.0f, size / 2.0f, -size / 2.0f),
                    new Vector3(0.0f, 0.0f, -1.0f)
                ),
                new(
                    new Vector3(-size / 2.0f, size / 2.0f, -size / 2.0f),
                    new Vector3(0.0f, 0.0f, -1.0f)
                ),
                // Top face
                new(
                    new Vector3(-size / 2.0f, size / 2.0f, size / 2.0f),
                    new Vector3(0.0f, 1.0f, 0.0f)
                ),
                new(
                    new Vector3(size / 2.0f, size / 2.0f, size / 2.0f),
                    new Vector3(0.0f, 1.0f, 0.0f)
                ),
                new(
                    new Vector3(size / 2.0f, size / 2.0f, -size / 2.0f),
                    new Vector3(0.0f, 1.0f, 0.0f)
                ),
                new(
                    new Vector3(-size / 2.0f, size / 2.0f, -size / 2.0f),
                    new Vector3(0.0f, 1.0f, 0.0f)
                ),
                // Bottom face
                new(
                    new Vector3(-size / 2.0f, -size / 2.0f, size / 2.0f),
                    new Vector3(0.0f, -1.0f, 0.0f)
                ),
                new(
                    new Vector3(size / 2.0f, -size / 2.0f, size / 2.0f),
                    new Vector3(0.0f, -1.0f, 0.0f)
                ),
                new(
                    new Vector3(size / 2.0f, -size / 2.0f, -size / 2.0f),
                    new Vector3(0.0f, -1.0f, 0.0f)
                ),
                new(
                    new Vector3(-size / 2.0f, -size / 2.0f, -size / 2.0f),
                    new Vector3(0.0f, -1.0f, 0.0f)
                ),
                // Right face
                new(
                    new Vector3(size / 2.0f, -size / 2.0f, size / 2.0f),
                    new Vector3(1.0f, 0.0f, 0.0f)
                ),
                new(
                    new Vector3(size / 2.0f, -size / 2.0f, -size / 2.0f),
                    new Vector3(1.0f, 0.0f, 0.0f)
                ),
                new(
                    new Vector3(size / 2.0f, size / 2.0f, -size / 2.0f),
                    new Vector3(1.0f, 0.0f, 0.0f)
                ),
                new(
                    new Vector3(size / 2.0f, size / 2.0f, size / 2.0f),
                    new Vector3(1.0f, 0.0f, 0.0f)
                ),
                // Left face
                new(
                    new Vector3(-size / 2.0f, -size / 2.0f, -size / 2.0f),
                    new Vector3(-1.0f, 0.0f, 0.0f)
                ),
                new(
                    new Vector3(-size / 2.0f, -size / 2.0f, size / 2.0f),
                    new Vector3(-1.0f, 0.0f, 0.0f)
                ),
                new(
                    new Vector3(-size / 2.0f, size / 2.0f, size / 2.0f),
                    new Vector3(-1.0f, 0.0f, 0.0f)
                ),
                new(
                    new Vector3(-size / 2.0f, size / 2.0f, -size / 2.0f),
                    new Vector3(-1.0f, 0.0f, 0.0f)
                )
            ],

            Indices =
            [
                // Front face
                0,
                1,
                2,
                2,
                3,
                0,
                // Back face
                5,
                4,
                6,
                7,
                6,
                4,
                // Top face
                8,
                9,
                10,
                10,
                11,
                8,
                // Bottom face
                15,
                14,
                12,
                13,
                12,
                14,
                // Right face
                16,
                17,
                18,
                18,
                19,
                16,
                // Left face
                20,
                21,
                22,
                22,
                23,
                20
            ]
        };

        return cube;
    }
}

//public interface IModel
//{
//    Vertex[] GetVertices();
//    uint[] GetIndices();

//    uint GetVertexCount();
//    uint GetIndexCount();
//}

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

public class ModelBuffer(DeviceBuffer vertexBuffer, DeviceBuffer indexBuffer, DeviceBuffer uniformBuffer, Model model)
{
    public readonly DeviceBuffer VertexBuffer = vertexBuffer;
    public readonly DeviceBuffer IndexBuffer = indexBuffer;
    public readonly DeviceBuffer UniformBuffer = uniformBuffer;
    public readonly Model Model = model;
}

public class ModelBufferFactory(ResourceFactory resourceFactory)
{
    public readonly ResourceFactory ResourceFactory = resourceFactory;

    public ModelBuffer CreateModelBuffer<TVertex>(Model model) where TVertex : IVertex
    {
        var vb = ResourceFactory.CreateBuffer(new BufferDescription(TVertex.SizeInBytes * model.GetVertexCount(), BufferUsage.VertexBuffer));
        var ib = ResourceFactory.CreateBuffer(new BufferDescription(model.GetIndexCount() * sizeof(uint), BufferUsage.IndexBuffer));
        var ub = ResourceFactory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic)); // extract const

        var mb = new ModelBuffer(vb, ib, ub, model);
        return mb;
    }
}

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
                new Vertex(
                    new Vector3(vertex.X, vertex.Y, vertex.Z),
                    new Vector3(normal.X, normal.Y, normal.Z)
                )
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
