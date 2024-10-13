using System.Numerics;
using WeaponZ.Game.Render;

namespace WeaponZ.Game.Scene;

public class LightSceneObject : ISceneObject
{
    public string DisplayName { get; }
    public Transform Transform { get; }
    public IList<ISceneObject> Children { get; }
    public SceneObjectKind Kind => SceneObjectKind.Light;
    public ISceneObject? Parent { get; set; }
    public Transform GlobalTransform { get; set; }

    public Light Light { get; set; }

    public LightSceneObject(string displayName, Transform transform, Vector3 color)
    {
        Children = [];
        DisplayName = displayName;
        Transform = transform;
        GlobalTransform = Transform;

        Light = new Light(
            LightType.Point,
            transform.Position,
            color
        );
    }

    public void Update(TimeSpan deltaTime)
    {
        Light = new Light(
            LightType.Point,
            GlobalTransform.Position,
            new Vector3(
                Light.Color.X,
                Light.Color.Y,
                Light.Color.Z
            )
        );
    }
}
