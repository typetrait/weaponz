#version 460 core

#define MAX_SCENE_LIGHTS 256

const int LightTypePoint = 0;
const int LightTypeDirectional = 1;

layout (location = 0) in vec3 fsin_Normal;
layout (location = 1) in vec4 fsin_Position;
layout (location = 2) in vec4 fsin_LightSpacePosition;

layout (location = 0) out vec4 fsout_Color;

struct PhongMaterial
{
    vec3 ambientColor;
    vec3 diffuseColor;
    vec3 specularColor;
    float shininess;
};

struct Light
{
    vec4 position;
    vec4 color;
    float type;
};

layout (set = 0, binding = 4) uniform texture2D ShadowMapTexture;
layout (set = 0, binding = 5) uniform sampler ShadowMapSampler;

layout (set = 0, binding = 6) uniform LightingBuffer
{
    vec4 CameraPosition;
    Light lights[MAX_SCENE_LIGHTS];
    int lightCount;
};

void main()
{
    vec3 objectColor = vec3(0.8f, 0.0f, 0.5f);

    vec3 resultColor = vec3(0.0f);

    vec3 normal = normalize(fsin_Normal);
    vec3 viewDirection = normalize(CameraPosition.xyz - fsin_Position.xyz);

    float ambientStrength = 0.1f;
    vec3 ambientColor = ambientStrength * vec3(1.0f);

    for (int i = 0; i < lightCount; i++)
    {
        Light light = lights[i];

        vec3 lightColor = light.color.xyz;
        
        vec3 lightDirection = vec3(0.0f);

        if (light.type == LightTypePoint)
        {
            lightDirection = normalize(light.position.xyz - fsin_Position.xyz);
        }
        else if (light.type == LightTypeDirectional)
        {
            lightDirection = normalize(light.position.xyz); // position is direction in directional lights
        }

        float diffuseIntensity = max(dot(lightDirection, normal), 0.0f);
        vec3 diffuseColor = lightColor * diffuseIntensity;

        vec3 reflectDirection = reflect(-lightDirection, normal);

        float specularIntensity = pow(max(dot(viewDirection, reflectDirection), 0.0), 128);
        vec3 specularColor = 0.5f * specularIntensity * lightColor;

        resultColor += (diffuseColor + specularColor) * objectColor;
    }

    resultColor += ambientColor * objectColor;

    resultColor = clamp(resultColor, 0.0f, 1.0f);

    fsout_Color = vec4(resultColor, 1.0);
}