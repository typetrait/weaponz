using System.Numerics;

using Veldrid;

namespace WeaponZ.Game.Input;

public interface IKeyboardInputContext
{
    event EventHandler<KeyboardEventArgs>? KeyPressed;
    event EventHandler<KeyboardEventArgs>? KeyReleased;

    bool IsKeyUp(Key key);
    bool IsKeyDown(Key key);
}

public interface IMouseInputContext
{
    event EventHandler<MouseButtonEventArgs>? MouseButtonPressed;
    event EventHandler<MouseButtonEventArgs>? MouseButtonReleased;
    event EventHandler<MouseEventArgs>? MouseMoved;
}

public interface IInputContext : IKeyboardInputContext, IMouseInputContext
{
    void UpdateWarpedCursorPosition(Vector2 cursorPosition);
    void SetMouseGrab(bool shouldGrab);
}

public class MouseEventArgs(float x, float y, float deltaX, float deltaY) : EventArgs
{
    public readonly float X = x;
    public readonly float Y = y;

    public readonly float DeltaX = deltaX;
    public readonly float DeltaY = deltaY;
}

public class MouseButtonEventArgs(float x, float y, float deltaX, float deltaY, MouseButton button) : MouseEventArgs(x, y, deltaX, deltaY)
{
    public readonly MouseButton Button = button;
}

public class KeyboardEventArgs(Key key, bool isRepeatingEvent) : EventArgs
{
    public readonly Key Key = key;
    public readonly bool IsRepeatingEvent = isRepeatingEvent;
}
