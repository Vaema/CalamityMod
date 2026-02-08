using System;
using Microsoft.Xna.Framework.Graphics;

namespace CalamityMod
{
    [Obsolete("Use SpriteBatchParameters/SpriteBatchSnapshot")]
    public class BatchSetting(BlendState blend, SamplerState sampler, DepthStencilState depthStencil, RasterizerState rasterizer)
    {
        public readonly BlendState blendState = blend;
        public readonly SamplerState samplerState = sampler;
        public readonly DepthStencilState depthStencilState = depthStencil;
        public readonly RasterizerState rasterizerState = rasterizer;

        public static readonly BatchSetting AlphaBlend = new(
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            DepthStencilState.None,
            rasterizer: null
        );

        public static readonly BatchSetting Additive = new(
            BlendState.Additive,
            SamplerState.PointClamp,
            DepthStencilState.None,
            rasterizer: null
        );

        public static readonly BatchSetting NonPremultiplied = new(
            BlendState.NonPremultiplied,
            SamplerState.PointClamp,
            DepthStencilState.None,
            rasterizer: null
        );

        public static readonly BatchSetting Opaque = new(
            BlendState.Opaque,
            SamplerState.PointClamp,
            DepthStencilState.None,
            rasterizer: null
        );
    }
}
