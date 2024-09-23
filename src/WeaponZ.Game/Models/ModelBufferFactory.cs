using Veldrid;
using WeaponZ.Game.Render;

namespace WeaponZ.Game.Models;

public class ModelBufferFactory(ResourceFactory resourceFactory)
{
    public readonly ResourceFactory ResourceFactory = resourceFactory;

    public ModelBuffer CreateModelBuffer<TVertex>(Model model)
        where TVertex : IVertex
    {
        var vb = ResourceFactory.CreateBuffer(
            new BufferDescription(TVertex.SizeInBytes * model.GetVertexCount(), BufferUsage.VertexBuffer)
        );
        var ib = ResourceFactory.CreateBuffer(
            new BufferDescription(model.GetIndexCount() * sizeof(uint), BufferUsage.IndexBuffer)
        );
        var ub = ResourceFactory.CreateBuffer(
            new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic)
        ); // extract const

        var mb = new ModelBuffer(vb, ib, ub, model);
        return mb;
    }
}
