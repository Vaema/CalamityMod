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
            spriteBatch.Begin(SpriteSortMode.Immediate, blendState, Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Determines if a <see cref="SpriteBatch"/> is in a lock due to a <see cref="SpriteBatch.Begin"/> call.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to check.</param>
        public static bool HasBeginBeenCalled(this SpriteBatch spriteBatch)
        {
            return spriteBatch.beginCalled;
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

            spriteBatch.GetParameters(out var parameters);
            spriteBatch.End();
            
            spriteBatch.Begin(sortMode, settings.blendState, settings.samplerState, settings.depthStencilState, settings.rasterizerState ?? Main.Rasterizer, effect, transformMatrix);
            batchCallback?.Invoke();
            spriteBatch.Restart(parameters);
        }

        [Obsolete("This is violative of spritebatch's control flow and will eventually be removed")]
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
                spriteBatch.Begin(sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect,
                    transformMatrix);
                return true;
            }
        }

        [Obsolete("This is violative of spritebatch's control flow and will eventually be removed")]
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

        internal readonly record struct SpritebatchParameters(
            BlendState BlendState,
            SpriteSortMode SortMode,
            DepthStencilState DepthStencilState,
            SamplerState SamplerState, 
            RasterizerState RasterizerState,
            Effect Effect,
            Matrix TransformMatrix
        );

        internal static void End(this SpriteBatch spriteBatch, out SpritebatchParameters parameters)
        {
            spriteBatch.GetParameters(out parameters);
            
            spriteBatch.End();
        }

        internal static void GetParameters(this SpriteBatch spriteBatch, out SpritebatchParameters parameters)
        {
            parameters = new SpritebatchParameters(
                spriteBatch.blendState,
                spriteBatch.sortMode,
                spriteBatch.depthStencilState,
                spriteBatch.samplerState,
                spriteBatch.rasterizerState,
                spriteBatch.customEffect,
                spriteBatch.transformMatrix
            );
        }
        
        internal static void Begin(this SpriteBatch spriteBatch, in SpritebatchParameters parameters)
        {
            spriteBatch.Begin(parameters.SortMode,
                parameters.BlendState, 
                parameters.SamplerState, 
                parameters.DepthStencilState, 
                parameters.RasterizerState,
                parameters.Effect,
                parameters.TransformMatrix);
        }

        internal static void Restart(this SpriteBatch spriteBatch)
        {
            spriteBatch.End(out var sp);
            spriteBatch.Begin(in sp);
        }
        
        internal static void Restart(this SpriteBatch spriteBatch, in SpritebatchParameters parameters)
        {
            spriteBatch.End();
            spriteBatch.Begin(parameters);
        }

        internal class SpritebatchScope : IDisposable
        {
            private readonly SpritebatchParameters _parameters;
            private readonly SpriteBatch _sb;
            /// <summary>
            /// Takes in a spritebatch and gets <see cref="SpritebatchParameters"/> from it without ending or otherwise mutating it.
            /// </summary>
            /// <param name="sb"></param>
            public SpritebatchScope(SpriteBatch sb)
            {
                _sb = sb;
                _sb.GetParameters(out _parameters);
            }
            /// <summary>
            /// Takes in a spritebatch and gets <see cref="SpritebatchParameters"/> from it before restarting it with the input <see cref="SpritebatchParameters"/>.
            /// </summary>
            /// <param name="sb"></param>
            /// <param name="parameters"></param>
            public SpritebatchScope(SpriteBatch sb, SpritebatchParameters parameters)
            {
                _sb = sb;
                _sb.GetParameters(out _parameters);
                _sb.Restart(parameters);
            }
            /// <summary>
            /// SafeBegin equivalent; takes in a <see cref="SpriteBatch"/>, gets <see cref="SpritebatchParameters"/> from it, and then uses the <see cref="SpriteBatch"/> and other parameters to start a new <see cref="SpriteBatch"/>.
            /// </summary>
            /// <param name="sb"></param>
            /// <param name="sortMode"></param>
            /// <param name="settings"></param>
            /// <param name="effect"></param>
            /// <param name="transformMatrix"></param>
            public SpritebatchScope(SpriteBatch sb, SpriteSortMode sortMode, BatchSetting settings, Effect effect, Matrix transformMatrix, bool end = false)
            {
                _sb = sb;
                _sb.GetParameters(out _parameters);
                if (end) {
                    _sb.End();
                }
                _sb.Begin(sortMode, settings.blendState, settings.samplerState, settings.depthStencilState, settings.rasterizerState ?? Main.Rasterizer, effect, transformMatrix);
            }

            public void Dispose()
            {
                _sb.Restart(_parameters);
            }
        }
    }
}
