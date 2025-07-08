sampler baseTexture : register(s0);
sampler distortionTexture : register(s1);

float2 timeOffset;
float2 noiseScaleStrength;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR
{
    float2 distortedCoords = coords * noiseScaleStrength.x + timeOffset;
    float distortion = tex2D(distortionTexture, distortedCoords).r * noiseScaleStrength.y;
    return tex2D(baseTexture, coords + distortion) * sampleColor.a;
}

technique Technique1
{
    pass DistortionPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
