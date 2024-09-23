using System.Numerics;

namespace WeaponZ.Game.Util;

public static class Debug
{
    public static void LogMatrix4x4(Matrix4x4 matrix)
    {
        Console.WriteLine($"{matrix.M11}, {matrix.M12}, {matrix.M13}, {matrix.M14}");
        Console.WriteLine($"{matrix.M21}, {matrix.M22}, {matrix.M23}, {matrix.M24}");
        Console.WriteLine($"{matrix.M31}, {matrix.M32}, {matrix.M33}, {matrix.M34}");
        Console.WriteLine($"{matrix.M41}, {matrix.M42}, {matrix.M43}, {matrix.M44}");
    }
}
