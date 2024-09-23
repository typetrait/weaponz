using Assimp;

namespace WeaponZ.Game.Models;

public class AssimpModelLoader
{
    private readonly AssimpContext _importer;

    public AssimpModelLoader()
    {
        _importer = new AssimpContext();
    }

    public Model? Load(string filePath)
    {
        var scene = _importer.ImportFile(
            filePath,
            PostProcessSteps.Triangulate | PostProcessSteps.FlipUVs | PostProcessSteps.GenerateNormals // PostProcessSteps.GenerateSmoothNormals
        );

        if (scene is null || scene.RootNode is null)
        {
            return null;
        }

        // Getting vertices and indices from the model is an expensive operation, we need this in memory.
        // Quick workaround
        var temp = new AssimpModel { Scene = scene };

        var model = new Model() { Vertices = temp.GetVertices(), Indices = temp.GetIndices() };

        return model;
    }
}
