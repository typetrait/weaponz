using System.Numerics;

using Veldrid;
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

    private const Key DoSomethingKeyBind = Key.F;

    public CameraSceneObject(string displayName, Transform transform, OrthographicCamera camera, IInputContext inputContext)
    {
        Children = [];
        DisplayName = displayName;
        Transform = transform;
        GlobalTransform = Transform;

        Camera = camera;
        _inputContext = inputContext;

        _inputContext.KeyPressed += OnKeyPressed;
        _inputContext.MouseButtonPressed += OnMouseButtonPressed;
    }

    public void Update(TimeSpan deltaTime)
    {
        // Camera.Position = GlobalTransform.Position;
    }

    private void OnKeyPressed(object? sender, KeyboardEventArgs e)
    {
        if (!e.IsRepeatingEvent)
        {
            if (e.Key is DoSomethingKeyBind)
            {
                Console.WriteLine("Did something...");
            }
        }
    }

    private void OnMouseButtonPressed(object? sender, MouseButtonEventArgs e)
    {
        if (e.Button is MouseButton.Left)
        {
            Console.WriteLine($"Mouse clicked at: X = {e.X}; Y = {e.Y}");
        }
    }
}
