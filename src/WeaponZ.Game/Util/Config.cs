namespace WeaponZ.Game.Util;

public static class Config
{
    // Assets
    public static readonly string AssetsPath = Environment.GetEnvironmentVariable("ASSETS_PATH") ?? ".";

    // Game
    public const uint TargetFps = 144;
}
