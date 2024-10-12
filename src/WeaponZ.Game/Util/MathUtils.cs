namespace WeaponZ.Game.Util;

public static class MathUtils
{
    public static double DegreesToRadians(double value)
    {
        return value * (Math.PI / 180.0f);
    }

    public static double RadiansToDegrees(double value)
    {
        return value * (180.0f / Math.PI);
    }
}
