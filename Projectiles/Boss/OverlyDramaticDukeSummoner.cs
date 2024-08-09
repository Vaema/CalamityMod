using System;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Dusts;
using CalamityMod.Events;
using CalamityMod.NPCs.OldDuke;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Boss
{
    public class OverlyDramaticDukeSummoner : ModProjectile, ILocalizedModType
    {
        Vector2 cen;

        public new string LocalizationCategory => "Projectiles.Boss";
        public override string Texture => "CalamityMod/Projectiles/Boss/OldDukeVortex";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 10000;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 408;
            Projectile.scale = 0.004f;
            Projectile.hostile = true;
            Projectile.alpha = 255;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 1800;
        }
        private static void ExpandVertically(int startX, int startY, out int topY, out int bottomY, int maxExpandUp = 100, int maxExpandDown = 100)
        {
            topY = startY;
            bottomY = startY;
            if (!WorldGen.InWorld(startX, startY, 10))
            {
                return;
            }
            int yUp = 0;
            while (yUp < maxExpandUp && topY > 0 && topY >= 10 && Main.tile[startX, topY] != null)
            {
                topY--;
                yUp++;
            }
            int yDown = 0;
            while (yDown < maxExpandDown && bottomY < Main.maxTilesY - 10 && bottomY <= Main.maxTilesY - 10)
            {
                if (Main.tile[startX, bottomY] == null)
                {
                    return;
                }
                bottomY++;
                yDown++;
            }
        }

        public override void AI()
        {
            if (Projectile.ai[0] == 0)
                cen = Projectile.Center;

            Projectile.rotation -= 0.15f * (float)(1D - (Projectile.alpha / 255D)) * (Projectile.ai[0] / 660f);
            Projectile.ai[0]++;

            Projectile.ai[1]++;

            Vector2 vec = new Vector2(408, 408) * Projectile.scale;

            Projectile.position = cen - new Vector2((float)Math.Sqrt(vec.X), (float)Math.Sqrt(vec.Y));

            float totalTilesToExpand = 1600f * Projectile.scale / 16;

            Point centerAsTileCoords = Projectile.Center.ToTileCoordinates();
            Vector2 topVector = Projectile.Top;
            Vector2 bottomVector = Projectile.Bottom;
            Vector2 centerVector = Vector2.Lerp(topVector, bottomVector, 0.5f);
            Projectile.width = (int)(208 * Projectile.scale);

            Vector2 ProjectileSpawnPosition = cen;

            if (Projectile.ai[0] < 90f)
            {
                Projectile.alpha = (int)MathHelper.Lerp(255f, 0f, Projectile.ai[0] / 90f);
            }
            if (Projectile.ai[0] < 660f)
            {
                Projectile.scale = MathHelper.Lerp(0.004f, 1.6f, Projectile.ai[0] / 660f);
            }

            if (Projectile.ai[0] % 10 == 1 && Projectile.ai[0] < 600f)
            {
                SoundStyle style = SoundID.DD2_BetsyFireballShot;
                style.MaxInstances = 10;
                SoundEngine.PlaySound(style.WithPitchOffset(-0.5f + (Projectile.ai[0] / 660f * 0.5f)).WithVolumeScale(Projectile.ai[0] / 660f), cen);

                GeneralParticleHandler.SpawnParticle(new CustomPulse(cen, Vector2.Zero, new Color(55, 195, 0, 20), "CalamityMod/Particles/DustyCircleHardEdge", Vector2.One, Main.rand.NextFloat(MathHelper.TwoPi), Projectile.scale * 0.9f, Projectile.scale * 0.4f, 40));
            }

            // Spray gore and acid everywhere
            if (Projectile.ai[0] < 480f && Projectile.ai[0] > 90f)
            {
                if (Projectile.ai[0] % 10f == 9f)
                {
                    Vector2 velocity = new Vector2(0f, -18f).RotatedByRandom(0.7f);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), ProjectileSpawnPosition, velocity,
                        ModContent.ProjectileType<OldDukeSummonDrop>(), 65, 2f);
                }
                if (Projectile.ai[0] % 35f == 34f)
                {
                    Vector2 velocity = new Vector2(Main.rand.NextFloat(-3f, 3f), -7f - Main.rand.NextFloat(4f, 12f)).RotatedByRandom(0.5f);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), ProjectileSpawnPosition, velocity,
                        ModContent.ProjectileType<OldDukeGore>(), 65, 2f);
                }
            }

            // Fade out and die
            if (Projectile.ai[0] >= 600f)
            {
                Projectile.alpha = (int)MathHelper.Lerp(0f, 255f, (Projectile.ai[0] - 600f) / 120f);

                bool canSpawnBoomer = false;
                foreach (Player player in Main.ActivePlayers)
                {
                    if (!player.dead && Projectile.Distance(player.Center) < 12000f)
                    {
                        canSpawnBoomer = true;
                        break;
                    }
                }

                // Summon the boomer duke
                if (Projectile.ai[0] == 660f)
                {
                    if (canSpawnBoomer)
                    {
                        SoundEngine.PlaySound(SoundID.DD2_BetsyFlameBreath.WithPitchOffset(-0.5f), cen);
                        SoundEngine.PlaySound(OldDuke.DashSoundP3, cen);

                        for (float i = 0; i <= 5; i++)
                        {
                            GeneralParticleHandler.SpawnParticle(new CustomPulse(cen, Vector2.Zero, new Color(55, 255, 0), "CalamityMod/Particles/DustyCircleHardEdge", Vector2.One, Main.rand.NextFloat(MathHelper.TwoPi), 0.1f, i * 0.2f, 40));
                        }
                        
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            int boomer = NPC.NewNPC(Projectile.GetSource_FromThis(), (int)ProjectileSpawnPosition.X, (int)ProjectileSpawnPosition.Y, ModContent.NPCType<OldDuke>());
                            string boomerName = Main.npc[boomer].TypeName;

                            if (Main.netMode == NetmodeID.SinglePlayer)
                            {
                                Main.NewText(Language.GetTextValue("Announcement.HasAwoken", boomerName), new Color(175, 75, 255));
                                return;
                            }

                            if (Main.netMode == NetmodeID.Server)
                            {
                                ChatHelper.BroadcastChatMessage(NetworkText.FromKey("Announcement.HasAwoken", new object[]
                                {
                                    Main.npc[boomer].GetTypeNetName()
                                }), new Color(175, 75, 255));
                                return;
                            }

                            CalamityUtils.BossAwakenMessage(boomer);

                            Main.npc[boomer].velocity = Vector2.UnitY * -12f;
                            Main.npc[boomer].alpha = 255;
                            Main.npc[boomer].Calamity().newAI[3] = 1f; // To signal that Old Duke should not deccelerate as it normally would
                            Main.npc[boomer].netUpdate = true;
                            AcidRainEvent.HasTriedToSummonOldDuke = true;
                            AcidRainEvent.OldDukeHasBeenEncountered = true;
                            AcidRainEvent.UpdateInvasion(false);
                        }
                    }
                    else
                    {
                        AcidRainEvent.AccumulatedKillPoints = 0;
                        AcidRainEvent.HasTriedToSummonOldDuke = false;
                        AcidRainEvent.UpdateInvasion(false);
                    }
                }
            }
            if (Projectile.ai[0] >= 720f)
            {
                Projectile.Kill();
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> Tex = ModContent.Request<Texture2D>(Texture);

            float sc = MathHelper.Lerp(1, 0, Projectile.localAI[2]);

            float alphaLerp = MathHelper.Lerp(1f, 0f, (float)Projectile.alpha / 255f);

            Main.EntitySpriteDraw(Tex.Value, cen - Main.screenPosition, Tex.Frame(), new Color(0f, 0f, 0f, 0.4f).MultiplyRGBA(new Color(alphaLerp, alphaLerp, alphaLerp, alphaLerp)), -Projectile.rotation / 2 * (4 + 1), Tex.Frame().Center(), 1.61f * Projectile.scale * sc, SpriteEffects.None);

            for (int i = 2; i >= 0; i--)
            {
                float lerp = (float)i / 3f;

                Main.EntitySpriteDraw(Tex.Value, cen - Main.screenPosition, Tex.Frame(), Color.Lerp(new Color(5, 155, 95, 100), new Color(255, 255, 255, 55), lerp).MultiplyRGBA(new Color(alphaLerp, alphaLerp, alphaLerp, alphaLerp)), -Projectile.rotation / 2 * (i + 1), Tex.Frame().Center(), MathHelper.Lerp(1f, 1.7f, lerp) * Projectile.scale * sc, SpriteEffects.None);
            }
            return false;
        }

        public override bool CanHitPlayer(Player target) => Projectile.ai[0] >= 90f;

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, 240f * Projectile.scale, targetHitbox);

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (info.Damage <= 0)
                return;

            target.AddBuff(ModContent.BuffType<Irradiated>(), 420);
        }
    }
}
