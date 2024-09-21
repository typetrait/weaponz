namespace WeaponZ.Game;

public static class Config
{
    public static readonly string AssetsPath =
        Environment.GetEnvironmentVariable("ASSETS_PATH") ?? ".";
}
