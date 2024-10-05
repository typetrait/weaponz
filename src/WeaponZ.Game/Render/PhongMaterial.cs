using System.Numerics;

namespace WeaponZ.Game.Render;

public class PhongMaterial(Vector3 ambient, Vector3 diffuse, Vector3 specular, float shininess)
{
    public Vector3 AmbientColor { get; init; } = ambient;
    public Vector3 DiffuseColor { get; init; } = diffuse;
    public Vector3 SpecularColor { get; init; } = specular;
    public float Shininess { get; init; } = shininess;
}
