using Veldrid;

namespace WeaponZ.Game;

public class Keyboard
{
    private IDictionary<Key, bool> _pressedStates;

    public Keyboard()
    {
        _pressedStates = new Dictionary<Key, bool>();
    }

    public void UpdateFromSnapshot(InputSnapshot inputSnapshot)
    {
        foreach (var keyEvent in inputSnapshot.KeyEvents)
        {
            Key key = keyEvent.Key;

            if (keyEvent.Down)
            {
                _pressedStates[key] = true;
            }
            else if (_pressedStates[key])
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
