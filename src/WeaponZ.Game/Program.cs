using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace WeaponZ.Game;

public class Program
{
    private static GraphicsDevice? _graphicsDevice;

    public static void Main(string[] args)
    {
        Console.WriteLine("Hello, WeaponZ!");

        WindowCreateInfo windowCI = new WindowCreateInfo()
        {
            X = 100,
            Y = 100,
            WindowWidth = 800,
            WindowHeight = 600,
            WindowTitle = "WeaponZ"
        };

        Sdl2Window window = VeldridStartup.CreateWindow(ref windowCI);

        GraphicsDeviceOptions options = new()
        {
            PreferStandardClipSpaceYDirection = true,
            PreferDepthRangeZeroToOne = true
        };

        _graphicsDevice = VeldridStartup.CreateGraphicsDevice(window, options);

        while (window.Exists)
        {
            window.PumpEvents();
        }
    }
}
