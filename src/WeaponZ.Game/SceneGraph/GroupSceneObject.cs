
namespace WeaponZ.Game.SceneGraph;

public class GroupSceneObject : ISceneObject
{
    public string DisplayName { get; }

    public Transform Transform { get; }

    public IList<ISceneObject> Children { get; }

    public SceneObjectKind Kind => SceneObjectKind.Group;

    public GroupSceneObject(string displayName)
    {
        DisplayName = displayName;
        Transform = new Transform();
        Children = [];
    }
}
