using System;
using System.Linq;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Projectiles.Healing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee.Yoyos
{
    public class LaceratorYoyo : ModProjectile
    {
        public override LocalizedText DisplayName => CalamityUtils.GetItemName<Lacerator>();
        public const int MaxUpdates = 3;

        private static int CircleRes = 60;
        private static int VertRes = 4;
        private static int SpikeCount = 8;

        private float circleProgress = 0;
        private float verticalProgress = 1;
        private bool goingUp = false;
        public float chargeProgress = 0;

        private bool sawHit = false;
        private bool spawnedBlood = false;

        private int sawDir = 0;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.YoyosLifeTimeMultiplier[Type] = -1f;
            ProjectileID.Sets.YoyosMaximumRange[Type] = Lacerator.Reach;
            ProjectileID.Sets.YoyosTopSpeed[Type] = Lacerator.Speed / MaxUpdates;

            ProjectileID.Sets.TrailCacheLength[Type] = 8;
            ProjectileID.Sets.TrailingMode[Type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = ProjAIStyleID.Yoyo;
            Projectile.width = Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.penetrate = -1;
            Projectile.MaxUpdates = MaxUpdates;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 6 * MaxUpdates;
            
        }

        public override void AI()
        {
            if (sawDir == 0)
                sawDir = Projectile.velocity.X < 0 ? -1 : 1;
            if (chargeProgress > 1)
                chargeProgress = 1;
            if (chargeProgress > 0)
            {
                if (Main.player[Projectile.owner].miscCounter % 5 == 0 && Projectile.FinalExtraUpdate() && chargeProgress > 0.05f)
                {
                    spawnedBlood = false;
                    Projectile.position = Projectile.Center;
                    Projectile.width = 196;
                    Projectile.height = 196;
                    Projectile.Center = Projectile.position;
                    Projectile.originalDamage = Projectile.damage;
                    Projectile.damage = (int)(Projectile.damage * MathHelper.Lerp(0, 1f, chargeProgress));
                    sawHit = true;
                    Projectile.usesIDStaticNPCImmunity = true;
                    Projectile.aiStyle = -1;
                    Projectile.Damage();
                    Projectile.aiStyle = ProjAIStyleID.Yoyo;
                    Projectile.usesIDStaticNPCImmunity = false;
                    sawHit = false;
                    Projectile.damage = Projectile.originalDamage;
                    Projectile.position = Projectile.Center;
                    Projectile.width = 16;
                    Projectile.height = 16;
                    Projectile.Center = Projectile.position;
                }
                chargeProgress -= 0.003f/Projectile.extraUpdates;
                if (chargeProgress < 0)
                    chargeProgress = 0;
            }

            if ((Projectile.position - Main.player[Projectile.owner].position).Length() > 3200f) //200 blocks
                Projectile.Kill();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (sawHit)
            {
                target.AddBuff(ModContent.BuffType<BurningBlood>(), 60);
                Vector2 bloodpos = Projectile.Center + Projectile.DirectionTo(target.Center) * 84;
                if (!spawnedBlood && Main.rand.NextBool() && chargeProgress > 0.25f && target.Hitbox.Contains(bloodpos.ToPoint()))
                {
                    Projectile.NewProjectile(Projectile.GetSource_OnHit(target), bloodpos, Projectile.DirectionTo(target.Center).RotatedBy(sawDir * MathHelper.PiOver2 * 0.9f).RotatedByRandom(0.1f) * Main.rand.NextFloat(3f, 5f), ModContent.ProjectileType<BloodstoneHealOrb>(), 1, 0f, Projectile.owner);
                    spawnedBlood = true;
                }
                return;
            }
            var baseYoyo = Main.projectile.First(x => x.active && x.type == ModContent.ProjectileType<LaceratorYoyo>() && x.owner == Projectile.owner);
            baseYoyo.ModProjectile<LaceratorYoyo>().chargeProgress += (Main.player[Projectile.owner].yoyoGlove ? 0.05f : 0.1f);
            target.AddBuff(ModContent.BuffType<Laceration>(), 180);

            if (Main.player[Projectile.owner].moonLeech)
                return;

            Player player = Main.player[Projectile.owner];
            player.lifeRegenTime += 2;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info) => target.AddBuff(ModContent.BuffType<Laceration>(), 180);

        public override bool PreDraw(ref Color lightColor)
        {
            if (chargeProgress > 0)
            {
                
                var owner = Main.player[Projectile.owner];
                Texture2D texture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Melee/LaceratorSaw").Value;
                float rot = MathHelper.TwoPi * 30 * sawDir * (owner.miscCounter / 300f);
                 Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Color.Red * MathF.Pow(chargeProgress,0.5f)*0.5f, rot, texture.Size() * 0.5f, 1, sawDir == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);

                /*circleProgress = 0;
                verticalProgress = 1;
                goingUp = false;
                Texture2D lightTexture = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/SmallGreyscaleCircle").Value;
                for (int i = 0; i < (CircleRes + VertRes * (SpikeCount+2))*Projectile.MaxUpdates; i++)
                {
                    if (!goingUp)
                    {
                        circleProgress += 1f / (CircleRes * Projectile.MaxUpdates);
                        verticalProgress -= 1f / (CircleRes * Projectile.MaxUpdates) * SpikeCount;
                        if (verticalProgress <= 0)
                        {
                            goingUp = true;
                            //verticalProgress = 0;
                            circleProgress = MathF.Floor(circleProgress * SpikeCount) / SpikeCount;
                        }
                    }
                    else
                    {
                        verticalProgress += 1f / (VertRes * Projectile.MaxUpdates);
                        if (verticalProgress >= 1)
                        {
                            goingUp = false;
                            //verticalProgress = 1;
                        }
                    }

                    Color color = new Color(131,0,0);
                    color.A = 255;
                    color *= 0.75f;
                    var center = Projectile.Center;
                    var owner = Main.player[Projectile.owner];
                    Vector2 position = center + new Vector2(0, 64 + 32 * verticalProgress).RotatedBy((MathHelper.TwoPi * sawDir * circleProgress - (MathHelper.TwoPi * 3) * sawDir * (owner.miscCounter / 300f)));
                    Vector2 drawPosition = position + lightTexture.Size() * 0.5f - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY) + new Vector2(-32.5f, -32.5f); //Last vector is to offset the circle so that it is displayed where the hitbox actually is, instead of a bit down and to the right.
                    Color outerColor = color;
                    Color innerColor = color * 0.5f;
                    innerColor.A = 0;
                    float intensity = 0.9f + 0.15f * (float)Math.Cos(Main.GlobalTimeWrappedHourly * MathHelper.TwoPi);
                    intensity *= MathHelper.Lerp(0.15f, 1f, chargeProgress);
                    Vector2 outerScale = new Vector2(1f) * Projectile.scale * intensity;
                    Vector2 innerScale = new Vector2(1f) * Projectile.scale * intensity * 0.7f;
                    outerColor *= intensity;
                    innerColor *= intensity;
                    Main.EntitySpriteDraw(lightTexture, drawPosition, null, outerColor, 0f, lightTexture.Size() * 0.5f, outerScale * 0.25f, SpriteEffects.None, 0);
                    Main.EntitySpriteDraw(lightTexture, drawPosition, null, innerColor, 0f, lightTexture.Size() * 0.5f, innerScale * 0.25f, SpriteEffects.None, 0);
                }*/
            }
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 1);
            return false;
        }
    }
}
