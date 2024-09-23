#version 460 core

layout (location = 0) in vec3 Position;
layout (location = 1) in vec3 Normal;

layout (location = 0) out vec3 fsout_Normal;
layout (location = 1) out vec4 fsout_Position;

layout (set = 0, binding = 0) uniform ProjectionBuffer
{
    mat4 Projection;
};

layout (set = 0, binding = 1) uniform ViewBuffer
{
    mat4 View;
};

layout (set = 0, binding = 2) uniform ModelBuffer
{
    mat4 Model;
};

void main()
{
    fsout_Normal = mat3(transpose(inverse(Model))) * Normal;
    fsout_Position = Model * vec4(Position, 1.0);
    gl_Position = Projection * View * Model * vec4(Position, 1.0);
}