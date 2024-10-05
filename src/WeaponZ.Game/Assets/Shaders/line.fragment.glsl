#version 460 core

layout (location = 0) in vec3 fsin_Color;

layout (location = 0) out vec4 fsout_Color;

layout (set = 0, binding = 2) uniform ColorBuffer
{
    vec3 Color;
};

void main()
{
    fsout_Color = vec4(fsin_Color, 1.0f);
}