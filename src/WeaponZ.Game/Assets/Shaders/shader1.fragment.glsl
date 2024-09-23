#version 460 core

layout (location = 0) in vec3 fsin_Normal;
layout (location = 1) in vec4 fsin_Position;

layout (location = 0) out vec4 fsout_Color;

layout (set = 0, binding = 3) uniform LightingBuffer
{
    vec4 CameraPosition;
    vec4 LightPosition;
};

void main()
{
    vec3 objectColor = vec3(0.8f, 0.0f, 0.5f);

    vec3 lightColor = vec3(1.0f, 1.0f, 1.0f);

    //vec3 lightDirection = normalize(vec3(0.0f, 0.0f, 0.1f) - fsin_Position.xyz);
    vec3 lightDirection = normalize(LightPosition.xyz - fsin_Position.xyz);
    vec3 normal = normalize(fsin_Normal);

    float diffuseIntensity = max(dot(lightDirection, normal), 0.0f);
    vec3 diffuseColor = lightColor * diffuseIntensity;

    vec3 viewDirection = normalize(CameraPosition.xyz - fsin_Position.xyz);
    vec3 reflectDirection = reflect(-lightDirection, normal);

    float specularIntensity = pow(max(dot(viewDirection, reflectDirection), 0.0), 128);
    vec3 specularColor = 0.5f * specularIntensity * lightColor;

    float ambientStrength = 0.1f;
    vec3 ambientColor = ambientStrength * lightColor;

    vec3 result = (ambientColor + diffuseColor + specularColor) * objectColor;
    fsout_Color = vec4(result, 1.0);
}