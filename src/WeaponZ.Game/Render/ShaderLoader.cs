using System.Text;
using Veldrid;

namespace WeaponZ.Game.Render;

public static class ShaderLoader
{
    public static ShaderDescription Load(string filepath, ShaderStages stage)
    {
        var sourceCode = File.ReadAllText(filepath);

        var bytes = Encoding.UTF8.GetBytes(sourceCode);

        var description = new ShaderDescription
        {
            Stage = stage,
            ShaderBytes = bytes,
            EntryPoint = "main",
        };

        return description;
    }
}
