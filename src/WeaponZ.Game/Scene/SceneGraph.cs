using System.Numerics;

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

    public void Update(ISceneObject root, TimeSpan deltaTime)
    {
        if (root.Children is not null)
        {
            foreach (ISceneObject child in root.Children)
            {
                Update(child, deltaTime);
            }
        }
    }

    public static IEnumerable<ISceneObject> FindAllByKind(ISceneObject root, SceneObjectKind kind)
    {
        if (root.Kind == kind)
        {
            yield return root;
        }

        if (root.Children is not null)
        {
            foreach (ISceneObject child in root.Children)
            {
                foreach (ISceneObject descendant in FindAllByKind(child, kind))
                {
                    yield return descendant;
                }
            }
        }
    }

    public void CreateLight(ISceneObject parent)
    {
        AppendTo(
            parent,
            new LightSceneObject(
                $"Light {FindAllByKind(Root, SceneObjectKind.Light).Count() + 1}", // TODO: Obviously horrendous, fix this and every other instance of this later
                new Transform(),
                new Vector3(1.0f)
            )
        );
    }
}
