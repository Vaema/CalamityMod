sampler noiseTexture1 : register(s1);
sampler noiseTexture2 : register(s2);

float time;
float4 secondaryColor;
matrix uWorldViewProjection;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float3 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float3 TextureCoordinates : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;
    float4 pos = mul(input.Position, uWorldViewProjection);
    output.Position = pos;
    output.Color = input.Color;
    output.TextureCoordinates = input.TextureCoordinates;

    return output;
}

float InverseLerp(float from, float to, float x)
{
    return saturate((x - from) / (to - from));
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float4 color = input.Color;
    float2 coords = input.TextureCoordinates;
    
    // Account for texture distortion artifacts.
    coords.y = (coords.y - 0.5) / input.TextureCoordinates.z + 0.5;
    
     // Read the fade map as a streak.
    float bloomFadeout = pow(sin(coords.y * 3.141), 4);
    float4 noiseColor = tex2D(noiseTexture1, float2(frac(coords.x * 5 - time * 3.5), coords.y - time * 1.5));
    float opacity = (0.5 + noiseColor.g) * bloomFadeout;
    
    // Calculate secondary noise colors.
    float noiseFade2 = tex2D(noiseTexture2, float2(frac(coords.x * 2.5 - time * 2.75), coords.y)).r;
    float4 noiseColor2 = InverseLerp(0.4, 0.5, noiseFade2 * bloomFadeout) * secondaryColor;
    
    // Fade out at the ends of the streak.
    if (coords.x < 0.018)
        opacity *= pow(coords.x / 0.018, 6);
    if (coords.x > 0.95)
        opacity *= pow(1 - (coords.x - 0.95) / 0.05, 6);
    
    return color * 1.4 * opacity + noiseColor2 * opacity;
}

technique Technique1
{
    pass LaserPass
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
