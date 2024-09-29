using Veldrid;

namespace WeaponZ.Game.Input;

public interface IInputContext
{
    event EventHandler<MouseButtonEventArgs> MouseButtonPressed;
    event EventHandler<MouseButtonEventArgs> MouseButtonReleased;
    event EventHandler<MouseEventArgs> MouseMoved;
    event EventHandler KeyPressed;
    event EventHandler KeyReleased;
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

public class KeyboardEventArgs(Key key) : EventArgs
{
    public readonly Key Key = key;
}
