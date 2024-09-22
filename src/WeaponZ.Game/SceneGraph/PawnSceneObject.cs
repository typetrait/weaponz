namespace WeaponZ.Game.SceneGraph;

public class PawnSceneObject : ISceneObject
{
    public string DisplayName { get; }

    public Transform Transform { get; }

    public IList<ISceneObject> Children { get; }

    public ModelBuffer ModelBuffer { get; }

    public SceneObjectKind Kind => SceneObjectKind.Pawn;

    public PawnSceneObject(string displayName, Transform transform, ModelBuffer modelBuffer)
    {
        DisplayName = displayName;
        Transform = transform;
        ModelBuffer = modelBuffer;

        Children = [];
    }
}
