sampler baseTexture : register(s0);
sampler distortionTexture : register(s1);

float time;
float distortionXSpeed;
float distortionYSpeed;
float distortionStrength;
float noiseScale;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 distortedCoords = coords + float2(time * distortionXSpeed, time * distortionYSpeed);
    distortedCoords *= noiseScale;
    float distortion = tex2D(distortionTexture, distortedCoords).r;
    
    float4 returnColor = tex2D(baseTexture, coords + distortion * distortionStrength);
    return returnColor * sampleColor.a;
}

technique Technique1
{
    pass DistortionPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
