// Judgement Cut–style horizontal slash
// HLSL for use with SanePrimitiveRenderer's custom Effect path. This shader needs to be compensated for before use.
// Specifically SanePrimitiveRenderer. This won't work with the regular PrimitiveRenderer.
// You have been warned.

matrix uTransformMatrix;
float uTime;

float3 uColor;
float uBrightness;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(in VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = mul(input.Position, uTransformMatrix);
    output.Color = input.Color;
    output.TextureCoordinates = input.TextureCoordinates;
    return output;
}

float hash21(float2 p)
{
    p = frac(p * float2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return frac(p.x * p.y);
}

float noise(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    float2 u = f * f * (3.0 - 2.0 * f);

    return lerp(
        lerp(hash21(i), hash21(i + float2(1.0, 0.0)), u.x),
        lerp(hash21(i + float2(0.0, 1.0)), hash21(i + float2(1.0, 1.0)), u.x),
        u.y
    );
}

float slashMask(float2 uv, float thickness, float feather)
{
    float d = abs(uv.y);
    return smoothstep(thickness + feather, thickness, d);
}

float3 chromaSlash(float2 uv, float time)
{
    float thickness = 0.02;
    float feather   = 0.08;

    float widthPulse = 1.0 + 0.35 * sin(uv.x * 18.0 - time * 14.0);
    float localThickness = thickness * widthPulse;

    float distortA = noise(float2(uv.x * 8.0 - time * 1.3, time * 6.0));
    float distortB = noise(float2(uv.x * 16.0 + time * 0.9, time * 11.0));
    float distort = (distortA - 0.5) * 0.06 + (distortB - 0.5) * 0.035;
    uv.y += distort * slashMask(uv, localThickness * 2.0, feather);

    float aberr = 0.015;
    float r = slashMask(uv + float2( aberr, 0.0), localThickness, feather);
    float g = slashMask(uv,                        localThickness, feather);
    float b = slashMask(uv + float2(-aberr, 0.0), localThickness, feather);

    float3 col = float3(r, g, b);

    float core = slashMask(uv, localThickness * 0.38, feather * 0.22);
    col += core * float3(1.4, 1.4, 1.5);

    float glow = slashMask(uv, localThickness * 3.8, feather * 2.4);
    col += glow * uColor * 0.7;

    float veinNoise = noise(float2(uv.x * 22.0 - time * 3.0, uv.y * 48.0 + time * 10.0));
    float veins = smoothstep(0.7, 0.98, veinNoise) * glow;
    col += veins * uColor * 0.9;

    float shock = exp(-abs(uv.y * 13.0 + sin(uv.x * 11.0 + time * 9.0) * 1.7));
    col += shock * glow * uColor * 0.25;

    return col;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float2 uv = input.TextureCoordinates * 2.0 - 1.0;

    float time = uTime;
    float3 col = chromaSlash(uv, time);

    float2 bloomUV = float2(uv.x * 0.85, uv.y * 2.4);
    float centerBloom = exp(-dot(bloomUV, bloomUV) * 3.8);
    float bloomNoise = noise(float2(uv.x * 10.0 + time * 0.6, uv.y * 5.0));
    centerBloom *= lerp(0.92, 1.08, bloomNoise);
    centerBloom = saturate(centerBloom);

    col += centerBloom * float3(0.18, 0.2, 0.25);

    float sweepPos = frac(time * 1.25) * 2.0 - 1.0;
    float sweep = exp(-abs(uv.x - sweepPos) * 18.0) * slashMask(uv, 0.12, 0.18);
    col += sweep * lerp(float3(1.0, 1.0, 1.0), uColor, 0.2) * 0.55;

    float sparkNoise = noise(float2(uv.x * 30.0 + time * 7.0, uv.y * 7.0 - time * 2.0));
    float sparkMask = smoothstep(0.92, 0.995, sparkNoise) * slashMask(uv, 0.20, 0.24);
    col += sparkMask * lerp(float3(1.0, 1.0, 1.0), uColor, 0.25) * 0.45;

    float chromaPulse = 0.5 + 0.5 * sin(time * 6.0 + uv.x * 8.0);
    float3 pulseTint = lerp(float3(0.9, 0.9, 0.9), float3(1.1, 1.1, 1.1), chromaPulse);
    col *= pulseTint;

    float flicker = 0.85 + 0.15 * sin(time * 40.0);
    col *= flicker;

    float edgeFade = smoothstep(1.0, 0.3, abs(uv.x));
    col *= edgeFade;

    float3 rawCol = col;

    col *= input.Color.rgb;
    col *= uBrightness;

    float structureAlpha = max(rawCol.r, max(rawCol.g, rawCol.b));
    float alpha = structureAlpha * input.Color.a;
    return float4(col, alpha);
}

technique Technique1
{
    pass SlashPass
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
