using CalamityMod.Systems.Collections;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Renderers;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.ILEditing
{
    public partial class ILChanges
    {
        #region Fixing Vanilla Not Accounting For Spritebatch Modification in Held Projectiles
        private static bool HasLoggedHeldProjectileBlendStateCatch = false;
        private void FixHeldProjectileBlendState(On_PlayerDrawLayers.orig_DrawHeldProj orig, PlayerDrawSet drawinfo, Projectile proj)
        {
            orig(drawinfo, proj);

            // Vanilla uses a worse quality sampler state for mounts when moving for some reason. Really couldn't say why.
            var sampler = (drawinfo.drawPlayer.mount.Active && drawinfo.drawPlayer.fullRotation != 0f) ? LegacyPlayerRenderer.MountedSamplerState : Main.DefaultSamplerState;

            try
            {
                // Restart the spritebatch, to ensure that modifications made to it are properly restored.
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, sampler, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
            catch
            {
                if (!HasLoggedHeldProjectileBlendStateCatch)
                    LogFailure("FixHeldProjectileBlendState", "The spritebatch was not left properly by another mod! The game will now most likely crash.");

                HasLoggedHeldProjectileBlendStateCatch = true;
            }
        }
        #endregion

        #region Fix Vanilla Not Accounting For Multiple Bobbers When Fishing With Truffle Worm
        private void FixTruffleWormFishing(ILContext il)
        {
            var cursor = new ILCursor(il);

            // Initialize a flag variable whether truffle worm was used.
            il.Method.Body.Variables.Add(new VariableDefinition(il.Module.TypeSystem.Boolean));
            byte truffleWormUsed = (byte)(il.Method.Body.Variables.Count - 1);
            cursor.Emit(OpCodes.Ldc_I4_0);
            cursor.Emit(OpCodes.Stloc_S, truffleWormUsed);

            // Move after beq.s, which is before Player.ItemCheck_CheckFishingBobber_PickAndConsumeBait gets called
            if (!cursor.TryGotoNext(MoveType.After, i => i.Match(OpCodes.Beq_S)))
            {
                LogFailure("FixTruffleWormFishing", "Could not locate beq.s before Player.ItemCheck_CheckFishingBobber_PickAndConsumeBait.");
                return;
            }

            // Skip if truffle worm was already used.
            var loopEnd = il.DefineLabel();
            cursor.Emit(OpCodes.Ldloc_S, truffleWormUsed);
            cursor.Emit(OpCodes.Brtrue_S, loopEnd);

            // Find the call to Player.ItemCheck_CheckFishingBobber_PickAndConsumeBait.
            if (!cursor.TryGotoNext(MoveType.After, i => i.MatchCall<Player>("ItemCheck_CheckFishingBobber_PickAndConsumeBait")))
            {
                LogFailure("FixTruffleWormFishing", "Could not locate the call to Player.ItemCheck_CheckFishingBobber_PickAndConsumeBait.");
                return;
            }

            // Retrive baitTypeUsed, compare with truffle worm, and save it.
            cursor.Emit(OpCodes.Ldloc_S, (byte)4);
            cursor.Emit(OpCodes.Ldc_I4, ItemID.TruffleWorm);
            cursor.Emit(OpCodes.Ceq);
            cursor.Emit(OpCodes.Stloc_S, truffleWormUsed);

            // Move before next ldloc.0, which is the end of the loop
            if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchLdloc0()))
            {
                LogFailure("FixTruffleWormFishing", "Could not find the end of the loop.");
                return;
            }

            cursor.MarkLabel(loopEnd);
        }
        #endregion

        private void EnsureCheckDeadOnSegments(ILContext il)
        {
            var cursor = new ILCursor(il);
            if (!cursor.TryGotoNext(MoveType.After,
                i => i.MatchLdfld<NPC>(nameof(NPC.realLife)),
                i => i.MatchLdelemRef(),
                i => i.MatchCallOrCallvirt<NPC>(nameof(NPC.checkDead))
                ))
            {
                LogFailure("EnsureCheckDeadOnSegments", "Could not locate the checkDead instruction sets");
                return;
            }

            cursor.EmitLdarg0();
            cursor.EmitDelegate((NPC npc) =>
            {
                if (npc.life <= 0 && CalamityNPCSets.DoCheckDeadRegardlessRealLife[npc.type])
                {
                    NPCLoader.CheckDead(npc);
                }
            });
        }
    }
}
