// Judgement Cut–style horizontal slash
// HLSL for use with SanePrimitiveRenderer's custom Effect path
// Specifically SanePrimitiveRenderer. This won't work with the regular one.
// Sorry!

matrix uTransformMatrix;
float uTime;

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

    float distort = noise(float2(uv.x * 8.0, time * 6.0)) * 0.05;
    uv.y += distort * slashMask(uv, thickness * 2.0, feather);

    float aberr = 0.015;
    float r = slashMask(uv + float2( aberr, 0.0), thickness, feather);
    float g = slashMask(uv,                        thickness, feather);
    float b = slashMask(uv + float2(-aberr, 0.0), thickness, feather);

    float3 col = float3(r, g, b);

    float core = slashMask(uv, thickness * 0.4, feather * 0.2);
    col += core * float3(1.2, 1.3, 1.5);

    float glow = slashMask(uv, thickness * 3.5, feather * 2.0);
    col += glow * float3(0.3, 0.6, 1.0);

    return col;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float2 uv = input.TextureCoordinates * 2.0 - 1.0;

    float time = uTime;
    float3 col = chromaSlash(uv, time);

    // temp
    float fillMask = smoothstep(1.0, 0.6, abs(uv.y));
    float fillNoise = noise(float2(uv.x * 6.0 + time * 0.8, uv.y * 3.0));
    fillMask *= lerp(0.7, 1.1, fillNoise);
    fillMask = pow(saturate(fillMask), 1.8);
    float fillEdgeFade = smoothstep(1.0, 0.25, abs(uv.y);
    col += fillMask * fillEdgeFade * float3(0.12, 0.25, 0.55);

    float flicker = 0.85 + 0.15 * sin(time * 40.0);
    col *= flicker;

    float edgeFade = smoothstep(1.0, 0.3, abs(uv.x));
    col *= edgeFade;

    col *= input.Color.rgb;

    float alpha = max(col.r, max(col.g, col.b)) * input.Color.a;
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
