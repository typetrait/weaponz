namespace WeaponZ.Game.Scenes;

public class GroupSceneObject : ISceneObject
{
    public IList<ISceneObject> Children { get; }
    public ISceneObject? Parent { get; set; }
    public Transform GlobalTransform { get; set; }
    public SceneObjectKind Kind => SceneObjectKind.Group;
    public Transform Transform { get; }
    public string DisplayName { get; }

    public GroupSceneObject(string displayName)
    {
        Children = [];
        DisplayName = displayName;
        Transform = new Transform();
        GlobalTransform = Transform;
    }
}
