using System.Numerics;

namespace WeaponZ.Game.SceneGraph;

public class PawnSceneObject : ISceneObject
{
    public IList<ISceneObject> Children { get; }
    public ISceneObject? Parent { get; set; }
    public Transform GlobalTransform { get; set; }
    public SceneObjectKind Kind => SceneObjectKind.Pawn;
    public Transform Transform { get; }
    public string DisplayName { get; }

    public ModelBuffer ModelBuffer { get; }

    public PawnSceneObject(string displayName, Transform transform, ModelBuffer modelBuffer)
    {
        Children = [];
        DisplayName = displayName;
        Transform = transform;
        GlobalTransform = Transform;
        ModelBuffer = modelBuffer;
    }
}
