namespace WeaponZ.Game.SceneGraph;

public class SceneGraphImpl
{
    public ISceneObject Root { get; }

    public SceneGraphImpl(ISceneObject root)
    {
        Root = root;
    }

    public void AppendTo(ISceneObject parent, ISceneObject sceneObject)
    {
        parent.Children.Add(sceneObject);
    }
}
