using Veldrid;

namespace WeaponZ.Game.Input;

public class KeyboardState
{
    private readonly Dictionary<Key, bool> _pressedStates;

    public KeyboardState()
    {
        _pressedStates = [];
    }

    public void UpdateFromSnapshot(InputSnapshot inputSnapshot)
    {
        foreach (var keyEvent in inputSnapshot.KeyEvents)
        {
            Key key = keyEvent.Key;
            _pressedStates.TryGetValue(key, out var isDown);

            if (keyEvent.Down)
            {
                _pressedStates[key] = true;
            }
            else if (isDown)
            {
                _pressedStates[key] = false;
            }
        }
    }

    public bool IsKeyDown(Key key)
    {
        _pressedStates.TryGetValue(key, out var isDown);
        return isDown;
    }

    public bool IsKeyUp(Key key)
    {
        _pressedStates.TryGetValue(key, out var isDown);
        return !isDown;
    }
}
