using Veldrid;

namespace WeaponZ.Game.Input;

public class Mouse
{
    public float X { get; private set; }
    public float Y { get; private set; }

    private readonly Dictionary<MouseButton, bool> _pressedStates;

    public Mouse()
    {
        _pressedStates = [];
    }

    public void UpdateFromSnapshot(InputSnapshot inputSnapshot)
    {
        X = inputSnapshot.MousePosition.X;
        Y = inputSnapshot.MousePosition.Y;

        foreach (var mouseEvent in inputSnapshot.MouseEvents)
        {
            var button = mouseEvent.MouseButton;
            _pressedStates.TryGetValue(button, out var isDown);

            if (mouseEvent.Down)
            {
                _pressedStates[button] = true;
            }
            else if (isDown)
            {
                _pressedStates[button] = false;
            }
        }
    }

    public bool IsButtonDown(MouseButton button)
    {
        _pressedStates.TryGetValue(button, out var isDown);
        return isDown;
    }

    public bool IsButtonUp(MouseButton button)
    {
        _pressedStates.TryGetValue(button, out var isDown);
        return !isDown;
    }
}
