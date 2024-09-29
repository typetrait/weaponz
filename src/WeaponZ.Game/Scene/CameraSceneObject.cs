using WeaponZ.Game.Input;
using WeaponZ.Game.Render;

namespace WeaponZ.Game.Scene;

public class CameraSceneObject : ISceneObject
{
    public string DisplayName { get; }
    public Transform Transform { get; }
    public IList<ISceneObject> Children { get; }
    public SceneObjectKind Kind => SceneObjectKind.Camera;
    public ISceneObject? Parent { get; set; }
    public Transform GlobalTransform { get; set; }

    public OrthographicCamera Camera { get; }

    private readonly IInputContext _inputContext;

    public CameraSceneObject(string displayName, Transform transform, OrthographicCamera camera, IInputContext inputContext)
    {
        Children = [];
        DisplayName = displayName;
        Transform = transform;
        GlobalTransform = Transform;

        Camera = camera;
        _inputContext = inputContext;

        _inputContext.MouseButtonPressed += OnMouseButtonPressed;
    }

    private void OnMouseButtonPressed(object? sender, MouseButtonEventArgs e)
    {
        if (e.Button is Veldrid.MouseButton.Left)
        {
            Console.WriteLine($"Mouse clicked at: X = {e.X}; Y = {e.Y}");
        }
    }
}
