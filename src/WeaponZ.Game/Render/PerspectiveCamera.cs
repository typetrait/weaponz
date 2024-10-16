﻿using System.Numerics;

namespace WeaponZ.Game.Render;

public class PerspectiveCamera : ICamera
{
    public float ZNear { get; private set; }
    public float ZFar { get; private set; }
    public Vector3 Position { get; set; }

    public Matrix4x4 Projection { get; private set; }
    public Matrix4x4 View { get; private set; }

    public ViewProjectionMatrix ViewProjection { get; private set; }

    public Vector3 Up { get; set; }
    public Vector3 Forward { get; set; }
    public Vector3 Right { get; set; }

    public PerspectiveCamera(float width, float height, float zNear, float zFar, Vector3 position)
    {
        ZNear = zNear;
        ZFar = zFar;
        Position = position;

        Projection = Matrix4x4.CreatePerspectiveFieldOfView(1.0f, width / height, zNear, zFar);

        Up = Vector3.UnitY;
        Forward = -Vector3.UnitZ;

        View = Matrix4x4.CreateLookAt(position, position + Forward, Up);

        ViewProjection = new ViewProjectionMatrix(View, Projection);

        Right = Vector3.Cross(Up, Forward);
        //Up = Vector3.Cross(Forward, Right);
    }

    public void UpdateViewMatrix()
    {
        Forward = Vector3.Normalize(Forward);
        Right = Vector3.Normalize(Vector3.Cross(Forward, Vector3.UnitY));
        Up = Vector3.Cross(Right, Forward);

        View = Matrix4x4.CreateLookAt(Position, Position + Forward, Up);
        ViewProjection = new ViewProjectionMatrix(View, Projection);
    }
}

public struct ViewProjectionMatrix(Matrix4x4 view, Matrix4x4 projection)
{
    public Matrix4x4 View = view;
    public Matrix4x4 Projection = projection;

    public const uint SizeInBytes = 128;
}
