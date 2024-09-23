using System.Numerics;

namespace WeaponZ.Game.Models;

public static class PrimitiveModelFactory
{
    public static Model CreateTriangle()
    {
        var triangle = new Model
        {
            Vertices =
            [
                new(new Vector3(-0.5f, -0.5f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f)),
                new(new Vector3(0.5f, -0.5f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f)),
                new(new Vector3(0.0f, 0.5f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f)),
            ],
            Indices = [0, 1, 2],
        };

        return triangle;
    }

    public static Model CreateCube()
    {
        var size = 1;
        // csharpier-ignore
        var cube = new Model
        {
            Vertices =
            [
                // Front face
                new(new Vector3(-size / 2.0f, -size / 2.0f, size / 2.0f), new Vector3(0.0f, 0.0f, 1.0f)),
                new(new Vector3(size / 2.0f, -size / 2.0f, size / 2.0f), new Vector3(0.0f, 0.0f, 1.0f)),
                new(new Vector3(size / 2.0f, size / 2.0f, size / 2.0f), new Vector3(0.0f, 0.0f, 1.0f)),
                new(new Vector3(-size / 2.0f, size / 2.0f, size / 2.0f), new Vector3(0.0f, 0.0f, 1.0f)),
                // Back face
                new(new Vector3(-size / 2.0f, -size / 2.0f, -size / 2.0f), new Vector3(0.0f, 0.0f, -1.0f)),
                new(new Vector3(size / 2.0f, -size / 2.0f, -size / 2.0f), new Vector3(0.0f, 0.0f, -1.0f)),
                new(new Vector3(size / 2.0f, size / 2.0f, -size / 2.0f), new Vector3(0.0f, 0.0f, -1.0f)),
                new(new Vector3(-size / 2.0f, size / 2.0f, -size / 2.0f), new Vector3(0.0f, 0.0f, -1.0f)),
                // Top face
                new(new Vector3(-size / 2.0f, size / 2.0f, size / 2.0f), new Vector3(0.0f, 1.0f, 0.0f)),
                new(new Vector3(size / 2.0f, size / 2.0f, size / 2.0f), new Vector3(0.0f, 1.0f, 0.0f)),
                new(new Vector3(size / 2.0f, size / 2.0f, -size / 2.0f), new Vector3(0.0f, 1.0f, 0.0f)),
                new(new Vector3(-size / 2.0f, size / 2.0f, -size / 2.0f), new Vector3(0.0f, 1.0f, 0.0f)),
                // Bottom face
                new(new Vector3(-size / 2.0f, -size / 2.0f, size / 2.0f), new Vector3(0.0f, -1.0f, 0.0f)),
                new(new Vector3(size / 2.0f, -size / 2.0f, size / 2.0f), new Vector3(0.0f, -1.0f, 0.0f)),
                new(new Vector3(size / 2.0f, -size / 2.0f, -size / 2.0f), new Vector3(0.0f, -1.0f, 0.0f)),
                new(new Vector3(-size / 2.0f, -size / 2.0f, -size / 2.0f), new Vector3(0.0f, -1.0f, 0.0f)),
                // Right face
                new(new Vector3(size / 2.0f, -size / 2.0f, size / 2.0f), new Vector3(1.0f, 0.0f, 0.0f)),
                new(new Vector3(size / 2.0f, -size / 2.0f, -size / 2.0f), new Vector3(1.0f, 0.0f, 0.0f)),
                new(new Vector3(size / 2.0f, size / 2.0f, -size / 2.0f), new Vector3(1.0f, 0.0f, 0.0f)),
                new(new Vector3(size / 2.0f, size / 2.0f, size / 2.0f), new Vector3(1.0f, 0.0f, 0.0f)),
                // Left face
                new(new Vector3(-size / 2.0f, -size / 2.0f, -size / 2.0f), new Vector3(-1.0f, 0.0f, 0.0f)),
                new(new Vector3(-size / 2.0f, -size / 2.0f, size / 2.0f), new Vector3(-1.0f, 0.0f, 0.0f)),
                new(new Vector3(-size / 2.0f, size / 2.0f, size / 2.0f), new Vector3(-1.0f, 0.0f, 0.0f)),
                new(new Vector3(-size / 2.0f, size / 2.0f, -size / 2.0f), new Vector3(-1.0f, 0.0f, 0.0f)),
            ],

            Indices =
            [
                // Front face
                0, 1, 2,
                2, 3, 0,
                // Back face
                5, 4, 6,
                7, 6, 4,
                // Top face
                8, 9, 10,
                10, 11, 8,
                // Bottom face
                15, 14, 12,
                13, 12, 14,
                // Right face
                16, 17, 18,
                18, 19, 16,
                // Left face
                20, 21, 22,
                22, 23, 20,
            ],
        };

        return cube;
    }
}
