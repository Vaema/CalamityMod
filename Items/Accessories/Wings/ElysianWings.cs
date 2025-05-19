using System.Collections.Generic;
using CalamityMod.CalPlayer;
using CalamityMod.Items.Accessories.Vanity;
using CalamityMod.Particles;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories.Wings
{
    [AutoloadEquip(EquipType.Wings)]
    public class ElysianWings : BaseWings
    {
        public override float BonusAscentWhileFalling => 1f;
        public override float BonusAscentWhileRising => 0.17f;
        public override float RisingSpeedThreshold => 1.2f;
        public override float MaxAscentSpeed => 3f;
        public override float BaseAscent => 0.15f;

        public override void SetStaticDefaults() => ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new WingStats(240, 10f, 3f);

        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.width = 48;
            Item.height = 50;
            Item.value = CalamityGlobalItem.RarityTurquoiseBuyPrice;
            Item.rare = ModContent.RarityType<Turquoise>();
        }

        public override void UpdateVanity(Player player) => DrawWingEffects(player);

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            if (!hideVisual)
                DrawWingEffects(player);
        }
        private void DrawWingEffects(Player player)
        {
            float rate = Main.GlobalTimeWrappedHourly * 2;
            List<Color> eColors = new List<Color>()
            {
                Color.Gold,
                Color.Khaki
            };

            int colorIndex = (int)(rate / 2 % eColors.Count);
            Color currentColor = eColors[colorIndex];
            Color nextColor = eColors[(colorIndex + 1) % eColors.Count];
            Color usedColor = Color.Lerp(currentColor, nextColor, rate % 2f > 1f ? 1f : rate % 1f);


            Vector2 spawnPos = player.Center + new Vector2(-25 * player.direction, 0);
            Lighting.AddLight(spawnPos, usedColor.ToVector3() * 1.2f);

            if (player.wingTime > 0f && player.jump == 0 && player.velocity.Y != 0f)
            {
                spawnPos = player.Center + new Vector2(-25 * player.direction, 0) + Main.rand.NextVector2Circular(20, 20);
                Vector2 spawnPos2 = player.Center + new Vector2(15 * player.direction, 0) + Main.rand.NextVector2Circular(20, 20);

                float partScale = Main.rand.NextFloat(0.3f, 0.8f);
                Vector2 partVel = new Vector2(0, 5).RotatedBy(0.5f * player.direction).RotatedByRandom(0.5f) * Main.rand.NextFloat(0.5f, 0.8f);

                Particle smoke = new CustomSpark(spawnPos, partVel, "CalamityMod/Particles/SmallBloom", false, 13, partScale * 0.25f, usedColor * 0.4f, Vector2.One, true, false, 0, false, false);
                GeneralParticleHandler.SpawnParticle(smoke);

                if (Main.rand.NextBool((player.controlJump ? 2 : 4)))
                {
                    Particle spark3 = new CustomSpark(spawnPos, partVel, "CalamityMod/Particles/ProvidenceMarkParticle", false, 19, partScale, Main.rand.NextBool(4) ? Color.Khaki : Color.Goldenrod, new Vector2(1.3f, 0.5f), true, false, 0, false, false, Main.rand.NextFloat(0.1f, 0.2f));
                    GeneralParticleHandler.SpawnParticle(spark3);
                }

                if (Main.rand.NextBool())
                {
                    Particle smoke2 = new CustomSpark(spawnPos2, partVel, "CalamityMod/Particles/SmallBloom", false, 13, partScale * 0.15f, usedColor * 0.4f, Vector2.One, true, false, 0, false, false);
                    GeneralParticleHandler.SpawnParticle(smoke2);
                    if (Main.rand.NextBool((player.controlJump ? 2 : 4)))
                    {
                        Particle spark3 = new CustomSpark(spawnPos2, partVel, "CalamityMod/Particles/ProvidenceMarkParticle", false, 19, partScale * 0.7f, Main.rand.NextBool(4) ? Color.Khaki : Color.Goldenrod, new Vector2(1.3f, 0.5f), true, false, 0, false, false, Main.rand.NextFloat(0.1f, 0.2f));
                        GeneralParticleHandler.SpawnParticle(spark3);
                    }
                }

            }
        }
    }
}
