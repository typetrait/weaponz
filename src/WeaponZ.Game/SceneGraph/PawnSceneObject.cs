namespace WeaponZ.Game.SceneGraph;

public class PawnSceneObject : ISceneObject
{
    public string DisplayName { get; }

    public Transform Transform { get; }

    public IList<ISceneObject> Children { get; }

    public IModel Model { get; }

    public SceneObjectKind Kind => SceneObjectKind.Pawn;

    public PawnSceneObject(string displayName, Transform transform, IModel model)
    {
        DisplayName = displayName;
        Transform = transform;
        Model = model;

        Children = [];
    }
}
