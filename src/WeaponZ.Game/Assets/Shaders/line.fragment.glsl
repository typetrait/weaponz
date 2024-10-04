#version 460 core

layout (location = 0) out vec4 fsout_Color;

layout (set = 0, binding = 2) uniform ColorBuffer
{
    vec4 Color;
};

void main()
{
    fsout_Color = Color;
}