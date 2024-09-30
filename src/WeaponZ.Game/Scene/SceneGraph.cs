namespace WeaponZ.Game.Scene;

public class SceneGraph
{
    public ISceneObject Root { get; }

    private readonly Dictionary<int, ISceneObject> _sceneObjects;

    public SceneGraph(ISceneObject root)
    {
        Root = root;
        _sceneObjects = [];
    }

    public void AppendTo(ISceneObject parent, ISceneObject sceneObject)
    {
        sceneObject.Parent = parent;
        parent.Children.Add(sceneObject);
        _sceneObjects[_sceneObjects.Count - 1] = sceneObject;
    }

    public IEnumerable<ISceneObject> FindAllByKind(ISceneObject root, SceneObjectKind kind)
    {
        IList<ISceneObject> children = root.Children;

        if (children is not null)
        {
            foreach (ISceneObject child in children)
            {
                if (child.Kind == kind)
                {
                    yield return child;
                }
            }
        }
    }
}
