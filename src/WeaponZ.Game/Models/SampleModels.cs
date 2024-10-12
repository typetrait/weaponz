using WeaponZ.Game.Util;

namespace WeaponZ.Game.Models;

public class SampleModels
{
    private readonly AssimpModelLoader _modelLoader;

    public Model Triangle { get; }
    public Model Quad { get; }
    public Model Cube { get; }
    public Model Bunny { get; }
    public Model Sponga { get; }

    public SampleModels()
    {
        _modelLoader = new AssimpModelLoader();

        Triangle = PrimitiveModelFactory.CreateTriangle();

        Quad = PrimitiveModelFactory.CreateQuad();

        Cube = PrimitiveModelFactory.CreateCube();

        Bunny =
            _modelLoader.Load($"{Config.AssetsPath}/Models/fbx/Bunny.fbx")
            ?? throw new InvalidOperationException("Failed to load model");

        Sponga =
            _modelLoader.Load($"{Config.AssetsPath}/Models/fbx/Sponza.fbx")
            ?? throw new InvalidOperationException("Failed to load model");
    }
}
