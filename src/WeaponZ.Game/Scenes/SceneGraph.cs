namespace WeaponZ.Game.Scenes;

public class SceneGraph
{
    public ISceneObject Root { get; }

    public SceneGraph(ISceneObject root)
    {
        Root = root;
    }

    public void AppendTo(ISceneObject parent, ISceneObject sceneObject)
    {
        sceneObject.Parent = parent;
        parent.Children.Add(sceneObject);
    }
}
