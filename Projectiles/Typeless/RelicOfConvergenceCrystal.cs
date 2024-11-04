using System;
using CalamityMod.Dusts;
using CalamityMod.Enums;
using CalamityMod.Items;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static CalamityMod.Systems.LavaRenderingSystem;

namespace CalamityMod.Projectiles.Typeless
{
    public class RelicOfConvergenceCrystal : ModProjectile
    {
        public override LocalizedText DisplayName => CalamityUtils.GetItemName<RelicOfConvergence>();
        public int SoundInterval = 25;
        public int TotalCrystalsToDraw = 3;
        public int CrystalsDrawTime = 50;
        public float MaxCrystalOffsetRadius = 80f;
        public float MaxDustOffsetRadius = 70f;

        public ref float time => ref Projectile.ai[0];
        public float completion = 0;
        public float fade = 0;
        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 46;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 180;
        }

        public override void AI()
        {
            completion = Utils.GetLerpValue(180, 0, Projectile.timeLeft, true);
            fade = MathHelper.Lerp(fade, 0, 0.04f);

            Player player = Main.player[Projectile.owner];
            if (!player.channel)
            {
                Projectile.Kill();
                return;
            }

            player.GetDamage<SummonDamageClass>() -= 1f; // This is a summoner moment and a half

            UpdatePlayerVisuals(player);

            // Make a constant "magical" sound.
            if (Projectile.soundDelay <= 0)
            {
                SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact with { Volume = 0.5f * (time >= CrystalsDrawTime ? 1 : 2), Pitch = 0.5f * completion }, Projectile.Center);

                if (time >= CrystalsDrawTime)
                {
                    SoundStyle h = new("CalamityMod/Sounds/Item/NullHit");
                    SoundEngine.PlaySound(h with { Volume = 0.4f, Pitch = -0.3f + 0.7f * completion }, Projectile.Center);

                    float numberOfDusts = 10f;
                    for (int i = 0; i < numberOfDusts; i++)
                    {
                        Particle energy = new VelChangingSpark(Projectile.Center, Vector2.One.RotatedByRandom(100) * Main.rand.NextFloat(9f, 18f), Utils.DirectionFrom(player.Calamity().mouseWorld, Projectile.Center) * 35, "CalamityMod/Particles/BloomCircle", 25, Main.rand.NextFloat(0.1f, 0.35f) * completion, Color.Lerp(Color.Orange, Color.Orchid, completion), new Vector2(1f, 1f), lerpRate: 0.04f, shrinkSpeed: 0.15f);
                        GeneralParticleHandler.SpawnParticle(energy);
                    }
                }

                Projectile.soundDelay = (int)(SoundInterval * (time >= CrystalsDrawTime ? 1 - 0.8f * completion : 0.5f));
                fade = 1;
            }

            // Make a sound when fully charged.
            if (time == CrystalsDrawTime)
            {
                //SoundEngine.PlaySound(SoundID.DD2_DarkMageHealImpact, Projectile.Center);
                //SoundStyle h = new("CalamityMod/Sounds/Item/MeldShoot");
                //SoundEngine.PlaySound(h with { Volume = 0.4f, PitchVariance = 0.7f * completion }, Projectile.Center);
            }
            // Create a circle of dust. The circle expands outward at first until it reaches its "destination" radius.
            // Once the circle is at its maximum size, some of the dust moves inward.
            if (time >= CrystalsDrawTime)
            {
                GeneratePassiveDust(player);

                Lighting.AddLight(Projectile.Center, Color.Lerp(Color.Orange, Color.Orchid, completion).ToVector3() * (2.5f * (completion - 0.375f) + fade));
            }
            if (Projectile.timeLeft == 1)
            {
                int playerCount = 0;
                foreach (Player fella in Main.ActivePlayers)
                {
                    if (Utils.Distance(fella.Center, player.Calamity().mouseWorld) < 138)
                        playerCount++;
                }
                for (int index = 0; index < Main.player.Length; index++)
                {
                    Player fella = Main.player[index];
                    if (Utils.Distance(fella.Center, player.Calamity().mouseWorld) < 138)
                    {
                        fella.HealPlayer((int)(RelicOfConvergence.HealValue * (fella != player ? 1.5f : 1)), HealTextType.Broadcast);

                        SoundStyle heal = new("CalamityMod/Sounds/Custom/ProfanedGuardians/GuardianHeal");
                        SoundEngine.PlaySound(heal with { Volume = 1 / playerCount, MaxInstances = -1 }, fella.Center);

                        for (int i = 0; i < 5; i++) 
                        {
                            Particle spark = new CustomSpark(fella.Center + Main.rand.NextVector2Circular(15, 15), (-Vector2.UnitY * Main.rand.NextFloat(0.2f, 3f)), "CalamityMod/Particles/HealingPlus", false, Main.rand.Next(35, 50 + 1), Main.rand.NextFloat(1.1f, 1.9f), Color.Lerp(Color.Orchid, Color.White, i * 0.1f), Vector2.One, true, true, 0, false, false, 0.1f);
                            GeneralParticleHandler.SpawnParticle(spark);
                        }
                    }
                }
            }
            time++;
        }

        public void UpdatePlayerVisuals(Player player)
        {
            Vector2 vel = Utils.DirectionTo(player.Center, player.Calamity().mouseWorld);
            float rot = vel.ToRotation() + (player.direction == -1 ? MathHelper.ToRadians(270f) : MathHelper.ToRadians(-90f));

            player.direction = Math.Sign(vel.X);

            Projectile.Center = player.Center + vel * 15f;

            // The crystal is a holdout projectile, so change the player's variables to reflect that
            player.heldProj = Projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;

            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, rot);
            player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, rot);
        }

        public void GeneratePassiveDust(Player player)
        {
            float radius = 45f;
            radius = MathHelper.Lerp(0f, 200f, completion - 0.375f);

            for (float angle = 0f; angle <= MathHelper.TwoPi; angle += MathHelper.ToRadians(Main.rand.NextFloat(6f, 8f)))
            {
                Vector2 drawPos = player.Calamity().mouseWorld + angle.ToRotationVector2() * radius;
                Color useColor = Color.Lerp(Color.Orange, Color.Orchid, completion) * (completion - 0.25f);
                float particleScale = 0.01f + fade * 0.08f + completion * 0.08f;
                Particle aura = new CustomSpark(drawPos, Utils.DirectionTo(player.Calamity().mouseWorld, drawPos), "CalamityMod/Particles/SmallBloom", false, 4, particleScale, useColor, new Vector2(0.5f + completion, (2f - completion) * 7 - completion * 7));
                GeneralParticleHandler.SpawnParticle(aura);

                if (Main.rand.NextBool(70))
                {
                    Dust dust2 = Dust.NewDustPerfect(Projectile.Center + angle.ToRotationVector2() * radius, ModContent.DustType<LightDust>());
                    dust2.position = player.Calamity().mouseWorld + angle.ToRotationVector2() * radius;
                    dust2.scale = Main.rand.NextFloat(1.4f, 1.9f) * completion;
                    dust2.noGravity = false;
                    dust2.velocity = new Vector2(0, Main.rand.NextFloat(1, 5));
                    dust2.color = useColor;
                }
                if (Projectile.timeLeft == 1)
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center + angle.ToRotationVector2() * radius, ModContent.DustType<LightDust>());
                    dust.position = drawPos;
                    dust.scale = Main.rand.NextFloat(1.6f, 1.9f);
                    dust.noGravity = !Main.rand.NextBool(5);
                    dust.velocity = Utils.DirectionTo(player.Calamity().mouseWorld, drawPos) * Main.rand.NextFloat(2f, 4f);
                    dust.color = Color.Orchid;
                    dust.noLightEmittence = true;
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float opacity = time / CrystalsDrawTime;
            Texture2D crystalTexture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            for (int i = 0; i < TotalCrystalsToDraw; i++)
            {
                float angle = MathHelper.TwoPi / TotalCrystalsToDraw * i + time / 10f;
                float radius = MathHelper.Lerp(MaxCrystalOffsetRadius, 0f, time / CrystalsDrawTime);
                Vector2 drawPositionOffset = angle.ToRotationVector2() * radius;
                Vector2 drawPosition = (time >= CrystalsDrawTime ? Projectile.Center : Projectile.Center + drawPositionOffset + Main.rand.NextVector2Circular(12, 12));

                Projectile.DrawProjectileWithBackglow(Color.Lerp(Color.Orchid, Color.Goldenrod, fade) with { A = 0 } * completion * 0.5f, Color.Lerp(Color.White, Color.White with { A = 0 }, fade * 0.5f) * MathHelper.Clamp(completion * 1.5f, time >= CrystalsDrawTime ? 0.8f : 0f, 1), 4f * completion + (fade * 3), crystalTexture, xPos: drawPosition.X, yPos: drawPosition.Y);

                /*
                Main.EntitySpriteDraw(crystalTexture,
                                 drawPosition - Main.screenPosition,
                                 null,
                                 Color.White * opacity,
                                 Projectile.rotation,
                                 Projectile.Size * 0.5f,
                                 Projectile.scale,
                                 SpriteEffects.None,
                                 0);
                */
            }
            return false;
        }
    }
}
