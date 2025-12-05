using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace CalamityMod
{
    public static partial class CalamityUtils
    {
        // Cached for efficiency purposes.
        private const BindingFlags Bind_Private_Instance = BindingFlags.NonPublic | BindingFlags.Instance;
        private static readonly FastField<SpriteBatch, bool> Fld_BeginCalled = new("beginCalled", Bind_Private_Instance);
        private static readonly FastField<SpriteBatch, SpriteSortMode> Fld_SortMode = new("sortMode", Bind_Private_Instance);
        private static readonly FastField<SpriteBatch, BlendState> Fld_BlendState = new("blendState", Bind_Private_Instance);
        private static readonly FastField<SpriteBatch, SamplerState> Fld_SamplerState = new("samplerState", Bind_Private_Instance);
        private static readonly FastField<SpriteBatch, DepthStencilState> Fld_DepthStencilState = new("depthStencilState", Bind_Private_Instance);
        private static readonly FastField<SpriteBatch, RasterizerState> Fld_RasterizerState = new("rasterizerState", Bind_Private_Instance);
        private static readonly FastField<SpriteBatch, Matrix> Fld_TransformMatrix = new("transformMatrix", Bind_Private_Instance);
        private static readonly FastField<SpriteBatch, Effect> Fld_CustomEffect = new("customEffect", Bind_Private_Instance);

        /// <summary>
        /// Sets a <see cref="SpriteBatch"/>'s <see cref="BlendState"/> arbitrarily.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch.</param>
        /// <param name="blendState">The blend state to use.</param>
        public static void SetBlendState(this SpriteBatch spriteBatch, BlendState blendState)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, blendState, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Determines if a <see cref="SpriteBatch"/> is in a lock due to a <see cref="SpriteBatch.Begin"/> call.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to check.</param>
        public static bool HasBeginBeenCalled(this SpriteBatch spriteBatch)
        {
            return Fld_BeginCalled.Get(spriteBatch);
        }

        public static void SafeAction(this SpriteBatch spriteBatch, Action action)
        {
            if (spriteBatch is null)
                return;

            if (spriteBatch.HasBeginBeenCalled())
            {
                var oldSort = Fld_SortMode.Get(spriteBatch);
                var oldBlend = Fld_BlendState.Get(spriteBatch);
                var oldSampler = Fld_SamplerState.Get(spriteBatch);
                var oldDepths = Fld_DepthStencilState.Get(spriteBatch);
                var oldRaster = Fld_RasterizerState.Get(spriteBatch);
                var oldEffect = Fld_CustomEffect.Get(spriteBatch);
                var oldMtx = Fld_TransformMatrix.Get(spriteBatch);
                try
                {
                    action?.Invoke();
                }
                finally
                {
                    // If something has started in this block, restore the state to previous one
                    spriteBatch.TryEnd();
                    spriteBatch.TryBegin(oldSort, oldBlend, oldSampler, oldDepths, oldRaster, oldEffect, oldMtx);
                }
            }
            else
            {
                try
                {
                    action?.Invoke();
                }
                finally
                {
                    // Initial State was off, turn off the batching
                    spriteBatch.TryEnd(); 
                }
            }
        }

        /// <summary>
        /// Starts SpriteBatch then Re-Begin batch with old settings when it's all done
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="sortMode"></param>
        /// <param name="settings"></param>
        /// <param name="effect"></param>
        /// <param name="transformMatrix"></param>
        /// <param name="batchCallback"></param>
        public static void SafeBegin(this SpriteBatch spriteBatch, SpriteSortMode sortMode,
            BatchSetting settings,
            Effect effect,
            Matrix transformMatrix,
            Action batchCallback
            )
        {
            if (spriteBatch is null)
                return;

            spriteBatch.SafeAction(() =>
            {
                spriteBatch.TryEnd();
                var rasterizer = settings.rasterizerState ?? Main.Rasterizer;
                spriteBatch.TryBegin(sortMode, settings.blendState, settings.samplerState, settings.depthStencilState, rasterizer, effect, transformMatrix);
                batchCallback?.Invoke();
            });
        }

        public static bool TryBegin(this SpriteBatch spriteBatch, SpriteSortMode sortMode,
            BlendState blendState,
            SamplerState samplerState,
            DepthStencilState depthStencilState,
            RasterizerState rasterizerState,
            Effect effect,
            Matrix transformMatrix)
        {
            if (spriteBatch.HasBeginBeenCalled())
            {
                return false;
            }
            else
            {
                spriteBatch.Begin(sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, transformMatrix);
                return true;
            }
        }

        public static bool TryEnd(this SpriteBatch spriteBatch)
        {
            if (!spriteBatch.HasBeginBeenCalled())
            {
                return false;
            }
            else
            {
                spriteBatch.End();
                return true;
            }
        }
    }
}
