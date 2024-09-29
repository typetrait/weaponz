namespace WeaponZ.Game.Scene;

public interface ISceneObject
{
    string DisplayName { get; }
    Transform Transform { get; }
    IList<ISceneObject> Children { get; }

    SceneObjectKind Kind { get; }

    ISceneObject? Parent { get; set; }

    Transform GlobalTransform { get; set; }

    void UpdateGlobalTransform()
    {
        if (Parent is null)
        {
            GlobalTransform = Transform;
        }
        else
        {
            GlobalTransform = new Transform
            {
                Scale = Transform.Scale, // t.Scale = Transform.Scale * Parent.Transform.Scale;
                Position = Transform.Position + Parent.GlobalTransform.Position,
                Rotation = Transform.Rotation + Parent.GlobalTransform.Rotation,
            };
        }
    }
}

public enum SceneObjectKind
{
    Pawn,
    Group,
    Light,
    Camera,
}
