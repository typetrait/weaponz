
namespace WeaponZ.Game.Scene;

public class LightSceneObject : ISceneObject
{
    public string DisplayName { get; }
    public Transform Transform {  get; }
    public IList<ISceneObject> Children { get; }
    public SceneObjectKind Kind => SceneObjectKind.Light;
    public ISceneObject? Parent { get; set; }
    public Transform GlobalTransform { get; set; }

    public LightSceneObject(string displayName, Transform transform)
    {
        Children = [];
        DisplayName = displayName;
        Transform = transform;
        GlobalTransform = Transform;
    }
}
