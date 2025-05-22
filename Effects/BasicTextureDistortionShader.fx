cbuffer DistortionParams : register(b0)
{
    float time;
    float distortionXSpeed;
    float distortionYSpeed;
    float distortionStrength;
    float noiseScale;
};

Texture2D baseTexture : register(t0);
Texture2D distortionTexture : register(t1);
SamplerState linearSampler : register(s0);

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : SV_Target
{
    float2 distortedCoords = coords + float2(time * distortionXSpeed, time * distortionYSpeed);
    distortedCoords *= noiseScale;
    float distortion = distortionTexture.Sample(linearSampler, distortedCoords).r;
    
    float4 returnColor = baseTexture.Sample(linearSampler, coords + distortion * distortionStrength);
    return returnColor * sampleColor.a;
}

technique Technique1
{
    pass DistortionPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
