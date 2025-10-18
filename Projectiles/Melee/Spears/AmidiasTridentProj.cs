using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Projectiles.BaseProjectiles;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using CalamityMod.NPCs;
using ReLogic.Content;
using Microsoft.Xna.Framework.Graphics;
using CalamityMod.Particles;
using Terraria.Audio;
using Terraria.ID;
using System;
using Terraria.DataStructures;

namespace CalamityMod.Projectiles.Melee.Spears
{
    [PierceResistException]
    public class AmidiasTridentProj : BaseCustomUseStyleProjectile
    {
        public override int AssignedItemID => ModContent.ItemType<AmidiasTrident>();
        public override LocalizedText DisplayName => CalamityUtils.GetItemName<AmidiasTrident>();
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 50;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = 2;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.ownerHitCheck = true;
            Projectile.hide = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 7;
        }
        float rot = 0f;
        float glow = 0f;
        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            Vector2 cen = Projectile.Center + new Vector2(45, 0).RotatedBy(rot);
            hitbox = new Rectangle((int)cen.X - 40, (int)cen.Y - 40, 80, 80);
        }
        public override void WhenSpawned()
        {
            Projectile.ai[0] = -20;
            rot = Owner.DirectionTo(Owner.Calamity().mouseWorld).ToRotation();
        }
        public override void UseStyle()
        {
            glow = MathHelper.Lerp(glow, 0f, 0.12f);
            Owner.direction = Math.Sign(Owner.Calamity().mouseWorld.X - Owner.Center.X);

            switch (Owner.altFunctionUse)
            {
                case 2: // Alt attack
                    break;
                

                default: // Main attack
                    if (AnimationProgress < 21)
                    {
                        rot = Owner.DirectionTo(Owner.Calamity().mouseWorld).ToRotation();
                        if (AnimationProgress % 7 < 2)
                        {
                            Projectile.damage = AmidiasTrident.BaseAttackMeleeDamage;
                            if (AnimationProgress % 7 == 1)
                            {
                                SoundEngine.PlaySound(SoundID.Item1, Owner.Center);
                                for (int i = 0; i < 6; i++)
                                {
                                    CustomSpark p = new CustomSpark(Projectile.Center + new Vector2(20, Main.rand.NextFloat(-30, 30)).RotatedBy(rot), Owner.DirectionTo(AbsolutePosition) * 10, "CalamityMod/Particles/ThinEndedLine", false, 5, 1f, Color.CadetBlue.MultiplyRGBA(new(1f, 1f, 1f, 1f)), new Vector2(0.2f, 1.2f));
                                    GeneralParticleHandler.SpawnParticle(p);
                                }
                                if (!Main.dedServ)
                                {
                                    Projectile.NewProjectile(new EntitySource_Parent(Projectile), Projectile.Center + new Vector2(50, 0).RotatedBy(Projectile.rotation + MathHelper.ToRadians(-135f)),
                                        (Projectile.rotation + MathHelper.ToRadians(-135f)).ToRotationVector2() * 30f, ModContent.ProjectileType<AmidiasTridentBoltProj>(), AmidiasTrident.BaseAttackProjectileDamage, 2f, Projectile.owner);
                                }
                            }
                            Projectile.ai[0] = MathHelper.Lerp(Projectile.ai[0], 92, 0.3f);
                            glow = MathHelper.Lerp(glow, 1f, 0.3f);
                        }
                        else
                        {
                            Projectile.damage = 0;
                            if (AnimationProgress % 7 < 4)
                            {
                                Projectile.ai[0] = MathHelper.Lerp(Projectile.ai[0], 10, 0.3f);
                            }
                            else
                            {
                                Projectile.ai[0] = MathHelper.Lerp(Projectile.ai[0], 10, 0.1f);
                            }
                        }
                    }
                    else
                    {
                        rot = rot.AngleLerp(Owner.DirectionTo(Owner.Calamity().mouseWorld).ToRotation(), 0.3f);
                        Projectile.ai[0] = MathHelper.Lerp(Projectile.ai[0], 10, 0.1f);
                    }

                    Projectile.rotation = rot + MathHelper.ToRadians(135f);
                    AbsolutePosition = Owner.Center + new Vector2(Projectile.ai[0], 0).RotatedBy(rot);
                    break;
            }
        }
        public static readonly Asset<Texture2D> StabTexture = ModContent.Request<Texture2D>("CalamityMod/Particles/LargeSpark");
        public static readonly Asset<Texture2D> SwingTexture = ModContent.Request<Texture2D>("CalamityMod/Particles/CircularSmearFire3");
        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> tex = ModContent.Request<Texture2D>("CalamityMod/Particles/GlowBlade");

            for (int i = 0; i < 5; i++)
            {
                int f = (int)(tex.Height() * glow);
                Main.EntitySpriteDraw(tex.Value, Projectile.Center - Main.screenPosition + new Vector2(-60 + (f * 0.03f), 0).RotatedBy(rot) + new Vector2(0f, Owner.gfxOffY), new Rectangle(0, 0, tex.Width(), f), Color.DeepSkyBlue.MultiplyRGBA(new(glow, glow, glow, 0f)), rot + MathHelper.PiOver2, new Vector2(tex.Width() / 2, tex.Height()), new Vector2(0.03f), SpriteEffects.None);
            }

            return base.PreDraw(ref lightColor);
        }
        public override void AI()
        {
            base.AI();

            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation + MathHelper.ToRadians(135f));
            Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation + MathHelper.ToRadians(135f));
        }
    }
}
