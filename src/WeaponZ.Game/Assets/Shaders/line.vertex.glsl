#version 460 core

layout (location = 0) in vec3 Position;

layout (set = 0, binding = 0) uniform ProjectionBuffer
{
    mat4 Projection;
};

layout (set = 0, binding = 1) uniform ViewBuffer
{
    mat4 View;
};

void main()
{
    gl_Position = Projection * View * vec4(Position, 1.0);
}