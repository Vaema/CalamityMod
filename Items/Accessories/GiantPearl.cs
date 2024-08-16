using CalamityMod.CalPlayer;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    [LegacyName("AmidiasPendant")]
    public class GiantPearl : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";
        public override void SetDefaults()
        {
            Item.width = 42;
            Item.height = 32;
            Item.value = CalamityGlobalItem.RarityGreenBuyPrice;
            Item.rare = ItemRarityID.Green;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            CalamityPlayer modPlayer = player.Calamity();
            modPlayer.giantPearl = true;
            Lighting.AddLight((int)player.Center.X / 16, (int)player.Center.Y / 16, 0.45f, 0.8f, 0.8f);

            // Draw the aura visual
            float npcDistCompare = 1000f;
            foreach (NPC n in Main.ActiveNPCs)
            {
                if (n.friendly || n.dontTakeDamage)
                    continue;

                float currentNPCDist = Vector2.Distance(n.Center, player.Center);
                if (currentNPCDist < npcDistCompare)
                    npcDistCompare = currentNPCDist;
            }
            float opacity = Utils.Remap(npcDistCompare, 120f, 640f, 1f, 0f);

            if (opacity > 0.7f)
            {
                for (int d = 0; d < 3; d++)
                {
                    Vector2 bubbleSpawn = player.Center + Vector2.UnitX.RotatedByRandom(MathHelper.Pi) * 120f;
                    GenericBubbleParticle bubble = new(bubbleSpawn, Main.rand.NextVector2CircularEdge(0.75f, 0.75f), 0.3f, Main.rand.NextFloat(MathHelper.TwoPi), 8);
                    GeneralParticleHandler.SpawnParticle(bubble);
                }
            }

            CustomPulse auraVisual = new(player.Center, Vector2.Zero, new Color(75, 164, 191), "CalamityMod/Particles/HighResFoggyCircleHardEdge", Vector2.One, 0f, 0.12f, 0.1175f, 3, true, opacity);
            GeneralParticleHandler.SpawnParticle(auraVisual);
        }
    }
}
