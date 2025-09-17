/* Originally written for NIGHTSHADE for use as the liquid shader in the orb
 * health and mana UIs:
 * https://github.com/gold-meridian/nightshade-mod/blob/df037a103cb25dac8d36622515b3793f2b10ac3f/src/Nightshade/Assets/Shaders/UI/OrbFluidFillShader.hlsl
 *
 * Originally authored by Blockaroz and adapted by Tomat.
 */

#include "../pixelation.h"

sampler uImage0 : register(s0);

texture uTexture0;
sampler tex0 = sampler_state
{
    texture = <uTexture0>;
    magfilter = POINT;
    minfilter = POINT;
    mipfilter = POINT;
    AddressU = wrap;
    AddressV = wrap;
};

texture uBubbleTexture;
sampler bubbleTex = sampler_state
{
    texture = <uBubbleTexture>;
    magfilter = POINT;
    minfilter = POINT;
    mipfilter = POINT;
    AddressU = wrap;
    AddressV = wrap;
};

float2 uImageSize;
float uTime;
float uFillAmount;
float4 uFillColor;
float4 uEdgeColor;
float4 uLineColor;
float uWaveStrength;
float uWaveOffset;
float uSubtract;

float4 main(float2 uv : TEXCOORD0, float4 baseColor : COLOR0) : COLOR0
{
    float4 image = tex2D(uImage0, uv);
    
    uv = normalize_with_pixelation(uv * uImageSize, 2, uImageSize);
    float2 center = uv * 2 - 1;
    float distFromCenter = length(center);

    float2 uvForNoise = uv * uImageSize / 128;

    float4 bubbles = tex2D(bubbleTex, uvForNoise + float2(uTime / 10, uTime / 6));
    float4 bubbles2 = tex2D(bubbleTex, uvForNoise + float2((bubbles.r - 0.5) * 0.5, uTime / 10));
    float4 noise = tex2D(tex0, uvForNoise + float2(uTime / 6 + (bubbles.r - 0.5) * 0.05, (bubbles2.r - 0.5) * 0.1));
    uv.y = lerp(uv.y, uv.y, 0.15);
    
    float fillAmount = 1.0f - uFillAmount;
    float bumpLine = 0.2f * (smoothstep(0.9f, 1.0f, uFillAmount) - smoothstep(0.1f, 0.0f, uFillAmount));
    float height = uv.y + (sin(3.1415 * ((uv.x * uImageSize.x / 100 + uWaveOffset) * 1.5 - uTime)) + length(bubbles.rgb)) * uWaveStrength / uImageSize.y + bumpLine - (fillAmount + 2.0f / uImageSize.y);
    float edgeGlow = pow(distFromCenter * 0.8 + (uv.y + height - 0.33f) * 0.2, (1 + length(noise.rgb)) * uSubtract) * (distFromCenter * 1.5 + 1 - length(noise.rgb));
    
    float topCut = smoothstep(-1.5 / uImageSize.y, 0.5 / uImageSize.y, height);
    float topLine = smoothstep(1.8 / uImageSize.y, 0, height) * 1.5 * sqrt(1 - distFromCenter) - (bubbles2 - bubbles) * (1 - height) * 0.2;

    return ((lerp(uFillColor - 0.3 * topCut * uSubtract, uEdgeColor * 0.9, clamp(edgeGlow * length(image.rgb) / 2, 0, 1.3)) + topLine * 2 * uLineColor) * topCut) * image;
}

technique Technique1
{
    pass WaterShader
    {
        PixelShader = compile ps_3_0 main();
    }
}