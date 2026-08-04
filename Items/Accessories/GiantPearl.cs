using CalamityMod.CalPlayer;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    public class GiantPearl : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";
        public const float AuraRadius = 120f;
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
            if (Main.myPlayer == player.whoAmI)
            {
                float npcDistCompare = 1000f;
                foreach (NPC n in Main.ActiveNPCs)
                {
                    if (n.friendly || n.dontTakeDamage)
                        continue;

                    float currentNPCDist = Vector2.Distance(n.Center, player.Center);
                    if (currentNPCDist < npcDistCompare)
                        npcDistCompare = currentNPCDist;
                }
                float opacity = Utils.Remap(npcDistCompare, AuraRadius, AuraRadius * 5f, 1f, 0f);

                if (opacity >= 1f)
                {
                    for (int d = 0; d < 2; d++)
                    {
                        WaterGlobParticle water = new(player.Center, Main.rand.NextVector2CircularEdge(13.5f, 13.5f), 0.32f, 0f, 35);
                        GeneralParticleHandler.SpawnParticle(water);
                    }
                }

                SemiCircularSmearFade smearBorder = new(player.Center, player.velocity, new Color(75, 164, 191) * opacity, player.miscCounter * (MathHelper.Pi / 30f), AuraRadius * 0.01125f, Vector2.One, 2, true);
                GeneralParticleHandler.SpawnParticle(smearBorder);
                smearBorder = new(player.Center, player.velocity, new Color(75, 164, 191) * opacity, player.miscCounter * (MathHelper.Pi / 30f) + MathHelper.Pi, AuraRadius * 0.01125f, Vector2.One, 2, true);
                GeneralParticleHandler.SpawnParticle(smearBorder);

                CustomPulse auraVisual = new(player.Center, player.velocity, new Color(75, 164, 191), "CalamityMod/Particles/BloomRing", Vector2.One, 0f, AuraRadius * 0.0105f, AuraRadius * 0.0105f, 2, true, opacity, false);
                GeneralParticleHandler.SpawnParticle(auraVisual);
            }
        }
    }
}
