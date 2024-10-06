
using Veldrid;

namespace WeaponZ.Game.Input;

public class MouseState: IMouseInputContext
{
    public float X { get; private set; }
    public float Y { get; private set; }

    private readonly Dictionary<MouseButton, bool> _pressedStates;

    public event EventHandler<MouseButtonEventArgs>? MouseButtonPressed;
    public event EventHandler<MouseButtonEventArgs>? MouseButtonReleased;
    public event EventHandler<MouseEventArgs>? MouseMoved;

    public MouseState()
    {
        _pressedStates = [];
    }

    public void UpdateFromSnapshot(InputSnapshot inputSnapshot, float deltaX, float deltaY)
    {
        float newFrameX = inputSnapshot.MousePosition.X;
        float newFrameY = inputSnapshot.MousePosition.Y;

        if (newFrameX != X || newFrameY != Y)
        {
            MouseMoved?.Invoke(this, new MouseEventArgs(newFrameX, newFrameY, deltaX, deltaY));
            X = inputSnapshot.MousePosition.X;
            Y = inputSnapshot.MousePosition.Y;
        }

        foreach (var mouseEvent in inputSnapshot.MouseEvents)
        {
            var button = mouseEvent.MouseButton;
            _pressedStates.TryGetValue(button, out var isDown);

            if (mouseEvent.Down)
            {
                _pressedStates[button] = true;
                MouseButtonPressed?.Invoke(this, new MouseButtonEventArgs(X, Y, deltaX, deltaY, button));
            }
            else if (isDown)
            {
                _pressedStates[button] = false;
                MouseButtonReleased?.Invoke(this, new MouseButtonEventArgs(X, Y, deltaX, deltaY, button));
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
