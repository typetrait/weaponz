using Veldrid;

namespace WeaponZ.Game.Input;

public interface IKeyboardInputContext
{
    event EventHandler<KeyboardEventArgs>? KeyPressed;
    event EventHandler<KeyboardEventArgs>? KeyReleased;
}

public interface IMouseInputContext
{
    event EventHandler<MouseButtonEventArgs>? MouseButtonPressed;
    event EventHandler<MouseButtonEventArgs>? MouseButtonReleased;
    event EventHandler<MouseEventArgs>? MouseMoved;
}

public interface IInputContext : IKeyboardInputContext, IMouseInputContext
{
}

public class MouseEventArgs(float x, float y) : EventArgs
{
    public readonly float X = x;
    public readonly float Y = y;
}

public class MouseButtonEventArgs(float x, float y, MouseButton button) : MouseEventArgs(x, y)
{
    public readonly MouseButton Button = button;
}

public class KeyboardEventArgs(Key key, bool isRepeatingEvent) : EventArgs
{
    public readonly Key Key = key;
    public readonly bool IsRepeatingEvent = isRepeatingEvent;
}
