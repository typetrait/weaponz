using Veldrid;
using Veldrid.SPIRV;
using WeaponZ.Game.Util;

namespace WeaponZ.Game.Render;

public class SampleShaders
{
    public Shader[] ShaderGroup1;

    public SampleShaders(ResourceFactory resourceFactory)
    {
        var shader1VertexShaderPath = $"{Config.AssetsPath}/Shaders/shader1.vertex.glsl";
        var shader1FragmentShaderPath = $"{Config.AssetsPath}/Shaders/shader1.fragment.glsl";

        var shader1VertexShader = ShaderLoader.Load(shader1VertexShaderPath, ShaderStages.Vertex);
        var shader1FragmentShader = ShaderLoader.Load(shader1FragmentShaderPath, ShaderStages.Fragment);

        ShaderGroup1 = resourceFactory.CreateFromSpirv(shader1VertexShader, shader1FragmentShader);
    }
}
