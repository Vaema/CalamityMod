using System.Collections.Generic;
using System.Linq;
using CalamityMod.DataStructures;
using CalamityMod.Enums;
using CalamityMod.Utilities.Daybreak;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace CalamityMod.Graphics.Renderers.CalamityRenderers
{
    public class DyeableShadersRenderer : BaseRenderer
    {
        #region Fields/Properties
        public static Dictionary<IDyeableShaderRenderer, ManagedRenderTarget> Targets
        {
            get;
            private set;
        }

        public static Dictionary<IDyeableShaderRenderer, int> Dyes
        {
            get;
            private set;
        }

        private static List<IDyeableShaderRenderer> RenderersToDrawThisFrame;

        public override GeneralDrawLayer Layer => GeneralDrawLayer.AfterPlayers;

        // This ignores MainTarget, and handles its own targets, so always call the draw methods.
        public override bool ShouldDraw => true;
        #endregion

        #region Loading
        public override void Load()
        {
            Targets = [];
            Dyes = [];
            RenderersToDrawThisFrame = [];
        }

        public override void Unload()
        {
            Targets?.Clear();
            Targets = null;

            Dyes?.Clear();
            Dyes = null;

            RenderersToDrawThisFrame?.Clear();
            RenderersToDrawThisFrame = null;
        }
        #endregion

        #region Hooks
        [Autoload(Side = ModSide.Client)]
        private class ItemUpdateHooks : GlobalItem
        {
            public override bool InstancePerEntity => false;

            public override void UpdateItemDye(Item item, Player player, int dye, bool hideVisual)
            {
                if (item.ModItem is not IDyeableShaderRenderer drawer)
                    return;

                // Store the dye and player in the slot.
                drawer.OwnerPlayer = player?.whoAmI ?? Main.maxPlayers;
                Dyes[drawer] = dye;
            }

            public override void UpdateVanity(Item item, Player player)
            {
                CheckIfEquipIsValid(item, false);
            }

            public override void UpdateAccessory(Item item, Player player, bool hideVisual)
            {
                CheckIfEquipIsValid(item, hideVisual);
            }

            public override void UpdateArmorSet(Player player, string set)
            {
                // Check each armor piece in the same manner as tMod.
                // If the entire set is equipped, and it is a renderer, it will be marked as valid.
                Item head = player.armor[0];
                Item body = player.armor[1];
                Item legs = player.armor[2];

                if (head.ModItem != null && head.ModItem.IsArmorSet(head, body, legs) && head.ModItem is IDyeableShaderRenderer renderer)
                    MarkAsValid(renderer);


                if (body.ModItem != null && body.ModItem.IsArmorSet(head, body, legs) && body.ModItem is IDyeableShaderRenderer renderer2)
                    MarkAsValid(renderer2);


                if (legs.ModItem != null && legs.ModItem.IsArmorSet(head, body, legs) && legs.ModItem is IDyeableShaderRenderer renderer3)
                    MarkAsValid(renderer3);
            }
        }

        private static void CheckIfEquipIsValid(Item item, bool hideVisual)
        {
            if (Main.dedServ)
                return;

            // Difficulty mode checks.
            if ((item.expertOnly && !Main.expertMode) || (item.masterOnly && !Main.masterMode))
                return;

            // Item exists, is visible and is a mod item checks.
            if (item.IsAir || hideVisual || item.ModItem == null)
                return;

            // Item uses the interface check.
            if (item.ModItem is not IDyeableShaderRenderer dyeableShaderRenderer)
                return;

            MarkAsValid(dyeableShaderRenderer);
        }

        private static void MarkAsValid(IDyeableShaderRenderer renderer)
        {
            if (Main.dedServ)
                return;

            Main.QueueMainThreadAction(() =>
            {
                // If it doesn't have a dictonary entry, create one.
                if (!Targets.ContainsKey(renderer))
                    Main.QueueMainThreadAction(() => Targets[renderer] = new(true, ManagedRenderTarget.CreateScreenSizedTarget));
            });

            // Mark this item as drawable this frame.
            RenderersToDrawThisFrame.AddWithCondition(renderer, renderer.ShouldDrawDyeableShader);
        }
        #endregion

        #region Updates/Drawing
        // Clear the list at the beginning of each update, to ensure its only populated by correct ones.
        public override void PreUpdate()
        {
            RenderersToDrawThisFrame?.Clear();
        }

        public override void DrawToTarget(SpriteBatch spriteBatch)
        {
            if (Main.dedServ)
                return;

            // Leave if nothing to draw.
            if (RenderersToDrawThisFrame.Count <= 0)
                return;

            // Sort the list by draw order.
            RenderersToDrawThisFrame = RenderersToDrawThisFrame.OrderByDescending(renderer => renderer.RenderDepth).ToList();

            foreach (var renderer in RenderersToDrawThisFrame)
            {
                if (!Targets.TryGetValue(renderer, out var target))
                    continue;

                // Swap to the assosiated target and call the interface method.
                target.SwapTo();
                renderer.DrawDyeableShader(spriteBatch);
            }
        }

        public override void DrawTarget(SpriteBatch spriteBatch)
        {
            if (Main.dedServ)
                return;

            // Leave if nothing to draw.
            if (RenderersToDrawThisFrame.Count <= 0)
                return;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

            foreach (var renderer in RenderersToDrawThisFrame)
            {
                if (!Targets.TryGetValue(renderer, out var target))
                    continue;

                // If it is dyeable, and a dye exists, apply it. This has null safety, as dyeShader can be null here.
                if (renderer.ShaderIsDyeable && Dyes.TryGetValue(renderer, out var dyeID) && dyeID > 0)
                    GameShaders.Armor.Apply(dyeID, null, new(target, Vector2.Zero, new Rectangle(0, 0, target.Width, target.Height), Color.White));

                // Draw the assosiated target that has been drawn to.
                spriteBatch.Draw(target, Vector2.Zero, Color.White with { A = 0 });
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer);
        }
        #endregion
    }
}
