namespace WeaponZ.Game.SceneGraph;

public interface ISceneObject
{
    string DisplayName { get; }
    Transform Transform { get; }
    IList<ISceneObject> Children { get; }

    SceneObjectKind Kind { get; }

    // Scene state maybe? So ingame/editing are different and entities dont do gameplay logic during editing...
}

public enum SceneObjectKind
{
    Pawn,
    Group,
    Light,
    Camera
}
