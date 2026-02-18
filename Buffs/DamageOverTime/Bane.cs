using System;
using CalamityMod.DataStructures;
using CalamityMod.Dusts;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.DamageOverTime
{
    public class Bane : ModBuff
    {
        public static DebuffData debuffData = new DebuffData()
        {
            EnemyLostRegen = 90,
            NPCLifeRegenMethod = BaneNPCLifeRegen
        };
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.LongerExpertDebuff[Type] = true;
            BuffDatasets.DebuffDataset[Type] = debuffData;
        }
        public static void BaneNPCLifeRegen(NPC npc, int buffType, ref int buffIndex, ref int damage)
        {
            var cnpc = npc.Calamity();
            bool effected = (cnpc.abaddonEffected || cnpc.voidOfExtinctionEffected);
            bool strong = cnpc.voidOfExtinctionEffected;
            int baseDoTValue = (int)debuffData.EnemyLostRegen;
            if (effected)
            {
                foreach (var player in Main.ActivePlayers)
                {
                    {
                        float improvedDamage = baseDoTValue * player.Calamity().abaddonDebuffDamage;
                        if (improvedDamage > baseDoTValue && (player.Calamity().abaddon || player.Calamity().voidOfExtinction))
                        {
                            baseDoTValue = (int)improvedDamage;
                        }
                    }
                }
            }
            cnpc.ApplyDPSDebuff(baseDoTValue, baseDoTValue / 20, ref npc.lifeRegen, ref damage);
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.Calamity().bane = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.Calamity().bane = true;
        }

        internal static void DrawEffects(PlayerDrawSet drawInfo, bool hasDebuffResistance = false)
        {
            // This shouldn't be on players, like ever
            Player Player = drawInfo.drawPlayer;
            Vector3 light = Color.Lavender.ToVector3();
            Lighting.AddLight(Player.Center, light * 2);

            if (Main.rand.NextBool(6))
            {
                Dust dust = Dust.NewDustPerfect(Player.Calamity().RandomDebuffVisualSpot, ModContent.DustType<LightDust>(), new Vector2(0, Main.rand.NextFloat(-3f, -5f)) + Player.velocity, 0, default, hasDebuffResistance ? 1.1f : 1.6f);
                dust.noGravity = true;
                dust.color = Color.PaleTurquoise;
            }
        }

        internal static void DrawEffects(NPC npc, ref Color drawColor)
        {
            Texture2D bloom = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomRingAngled").Value;
            Texture2D center = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle").Value;
            Vector2 drawPosition = npc.Center - Main.screenPosition;
            Color color = Color.Turquoise;
            Color color2 = Color.Orange;
            float maxSizeMult = 10f;
            float widthMult = MathF.Pow(Utils.Remap(npc.width, 10, 500, 1f, maxSizeMult), 0.8f);
            float sine = (float)Math.Sin((Main.GlobalTimeWrappedHourly * 9.0f) / MathHelper.Pi);
            float sine2 = (float)Math.Sin((Main.GlobalTimeWrappedHourly * 11.0f) / MathHelper.Pi);
            float randSize = Main.rand.NextFloat(0.82f, 1.08f);

            for (int i = 0; i < 2; i++)
            {
                float sizeMult = (i == 0 ? 1 : 0.8f);
                float colorMult = (i == 0 ? 1 : 0.7f);
                Main.EntitySpriteDraw(bloom, drawPosition + (Vector2.UnitY * npc.height / 2f) + Vector2.UnitY * ((1 - sine) * 2.3f), null, Color.Lerp(color, Color.White, i * 0.7f) with { A = 0 } * colorMult, 0, bloom.Size() / 2, new Vector2((sine * 0.09f + 1.2f) * widthMult, 0.9f) * 0.18f * MathF.Pow(randSize, 0.3f), SpriteEffects.None);
                Main.EntitySpriteDraw(center, drawPosition + (Vector2.UnitY * npc.height / 2f) + Vector2.UnitY * ((1 - sine2) * 3.3f), null, Color.Lerp(color2, Color.White, i * 0.7f) with { A = 0 } * colorMult, 0, center.Size() / 2, new Vector2((sine2 * 0.09f + 1.2f) * widthMult, 0.6f) * 0.18f * randSize, SpriteEffects.None);
            }

            if (Main.rand.NextBool(13))
            {
                bool start = Main.rand.NextBool();
                Vector2 baneVel = Vector2.UnitY * Main.rand.NextFloat(-0.5f, -1.5f) * widthMult * (Main.rand.NextBool(5) ? 2 : 1) + npc.velocity * 0.35f;
                Vector2 banePos = npc.Center + Main.rand.NextVector2Circular(3 * widthMult, 8) + (Vector2.UnitY * npc.height / 3f) + Vector2.UnitY * ((1 - sine) * 2.3f);
                Particle baneRunes = new BaneParticle(banePos, baneVel, false, 55, 1f * MathF.Pow(widthMult, 0.45f), start ? color : color2, start ? color2 : color, Vector2.One, true, Main.rand.NextBool(3) ? Main.rand.NextFloat(-0.2f, 0.2f) : 0, 0.95f, MathHelper.PiOver2 * Main.rand.Next(4 + 1), sineRate: Main.rand.NextFloat(0.2f, 1f), sineIntensity: Main.rand.Next(14, 19 + 1) * widthMult);
                GeneralParticleHandler.SpawnParticle(baneRunes);
                if (Main.rand.NextBool())
                    baneRunes.DrawLayer = Enums.GeneralDrawLayer.BeforeNPCs;
            }
            Lighting.AddLight(npc.Center, color.ToVector3() * 0.6f);
        }
    }
}
