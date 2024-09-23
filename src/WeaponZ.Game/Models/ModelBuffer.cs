using Veldrid;

namespace WeaponZ.Game.Models;

public class ModelBuffer(DeviceBuffer vertexBuffer, DeviceBuffer indexBuffer, DeviceBuffer uniformBuffer, Model model)
{
    public readonly DeviceBuffer VertexBuffer = vertexBuffer;
    public readonly DeviceBuffer IndexBuffer = indexBuffer;
    public readonly DeviceBuffer UniformBuffer = uniformBuffer;
    public readonly Model Model = model;
}
