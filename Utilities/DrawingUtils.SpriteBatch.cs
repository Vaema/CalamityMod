using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace CalamityMod
{
    public static partial class CalamityUtils
    {


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
            return spriteBatch.beginCalled;
        }

        [Obsolete("Please don't use this if possible.")]
        internal static void SafeAction(this SpriteBatch spriteBatch, Action action)
        {
            // we need to get the stack trace here since the crash never happens here directly,
            CalamityMod.Log.Error(Environment.StackTrace.ToString());
            action?.Invoke();
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
