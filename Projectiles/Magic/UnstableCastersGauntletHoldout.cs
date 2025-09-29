using CalamityMod.CalPlayer;
using CalamityMod.Dusts;
using CalamityMod.Items.Weapons.Magic;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using CalamityMod.Projectiles.Magic;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Magic
{
    public class UnstableCastersGauntletHoldout : BaseGunHoldoutProjectile
    {
        public override int AssociatedItemID => ModContent.ItemType<UnstableCastersGauntlet>();
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public ref float shootingTimer => ref Projectile.ai[0];

        private float currentRecoilRotation;
        private float RecoilRotationAmount = 0.22f;
        private float RotationResolveSpeed = 0.22f;

        // Normal weapon usetime is 20. This is used throughout the AI to make the holdout's components scale with attack speed.
        public float speedModifier => HeldItem.useTime / 20f;

        public override void SetDefaults()
        {
            Projectile.width = 1;
            Projectile.height = 1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.ownerHitCheck = true;
        }

        public override void HoldoutAI()
        {
            CalamityPlayer calPlayer = Owner.Calamity();

            Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.None, 0f);

            // -- SIGIL M1 ATTACKS --
            if (Owner.ownedProjectileCounts[ModContent.ProjectileType<SigilSet>()] == 1 && Owner.altFunctionUse == 0)
            {
                float sigilFireRate = HeldItem.useTime * 4f;

                if (shootingTimer >= sigilFireRate)
                {
                    // Find all active sigils
                    var activeSigils = new List<Projectile>();
                    for (int i = 0; i < Main.maxProjectiles; i++)
                    {
                        Projectile proj = Main.projectile[i];
                        if (proj.active && proj.owner == Projectile.owner)
                        {
                            if (proj.type == ModContent.ProjectileType<IgnisSigil>() ||
                                proj.type == ModContent.ProjectileType<AquaSigil>() ||
                                proj.type == ModContent.ProjectileType<TerraSigil>() ||
                                proj.type == ModContent.ProjectileType<AerSigil>() ||
                                proj.type == ModContent.ProjectileType<OrdoSigil>() ||
                                proj.type == ModContent.ProjectileType<PerditoSigil>())
                            {
                                // Make sure sigil isn't already consumed
                                if (proj.ai[2] <= 0)
                                {
                                    activeSigils.Add(proj);
                                }
                            }
                        }
                    }

                    // If there are active sigils, select a random one and start the consuming anim
                    if (activeSigils.Count > 0)
                    {
                        int randomIndex = Main.rand.Next(activeSigils.Count);
                        Projectile chosenSigil = activeSigils[randomIndex];

                        // Start fadeout
                        chosenSigil.ai[2] = 1.0f;
                        shootingTimer = 0f;
                    }
                }

                // Particle/dust channeling VFX
                if (Owner.ownedProjectileCounts[ModContent.ProjectileType<SigilSet>()] > 0)
                {
                    Projectile sigilParent = null;
                    int sigilType = ModContent.ProjectileType<SigilSet>();

                    foreach (Projectile p in Main.ActiveProjectiles)
                    {
                        if (p.type == sigilType && p.owner == Projectile.owner)
                        {
                            sigilParent = p;
                            break;
                        }
                    }

                    if (sigilParent != null)
                    {
                        Vector2 randomSpawnOffset = Main.rand.NextVector2Circular(90f, 90f);
                        Vector2 dustSpawnPosition = GunTipPosition + randomSpawnOffset;

                        Vector2 inwardVelocity = (GunTipPosition - dustSpawnPosition).SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(7f, 10f);
                        Dust dust = Dust.NewDustPerfect(dustSpawnPosition, ModContent.DustType<LightDust>(), inwardVelocity, 0, Color.DarkMagenta, 1.2f);
                        dust.noGravity = true;
                        dust.velocity *= 0.5f;
                        dust.fadeIn = 0.5f;
                        dust.scale *= 0.4f;
                        
                        Lighting.AddLight(GunTipPosition, 0.4f * Color.DarkMagenta.ToVector3());
                    }
                }
            }
        


            // -- NEEDLES --
            else
            {
                int needleFireRate = (int)(12 * speedModifier);
                if (shootingTimer >= needleFireRate)
                {
                    if (calPlayer.unstableCastersGauntletVis >= 0.3f)
                    {
                        // Consume vis
                        calPlayer.unstableCastersGauntletVis -= 0.3f;

                        Projectile.velocity *= Main.rand.NextFloat(0.97f, 1.04f);

                        currentRecoilRotation -= RecoilRotationAmount;

                        // SFX
                        SoundEngine.PlaySound(new("CalamityMod/Sounds/Item/UnstableCastersGauntlet/VisNeedleFire") { Volume = 0.4f, PitchVariance = 0.15f }, Projectile.Center);

                        // Pulse VFX
                        Particle shootPulse = new DirectionalPulseRing(GunTipPosition, Projectile.velocity.SafeNormalize(Vector2.UnitY) * 5f, Color.DarkMagenta * 4f, new Vector2(0.4f, 0.8f), Projectile.velocity.ToRotation(), 0.07f, 0.3f, 16);
                        GeneralParticleHandler.SpawnParticle(shootPulse);

                        // Spawn needle
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, (Projectile.velocity * 35f).RotatedBy(Main.rand.NextFloat(-0.07f, 0.07f)), ModContent.ProjectileType<VisNeedle>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                        shootingTimer = 0f;
                    }
                    else
                    {
                        // Cull if there isnt enough vis
                        Projectile.Kill();
                    }
                }
            }


            // -- SIGIL SPAWNING --
            if (Owner.altFunctionUse == 2 && calPlayer.unstableCastersGauntletVis >= 6f)
            {
                float sigilFireRate = HeldItem.useTime * 4f;

                if (Owner.ownedProjectileCounts[ModContent.ProjectileType<SigilSet>()] <= 0 || shootingTimer >= sigilFireRate)
                {
                    calPlayer.unstableCastersGauntletVis -= 6f;

                    SoundEngine.PlaySound(new("CalamityMod/Sounds/Item/MagicRockSound") { Volume = 0.4f, PitchVariance = 0.05f }, Projectile.Center);

                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Owner.Center, Vector2.Zero, ModContent.ProjectileType<SigilSet>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                    shootingTimer = 0f;
                }
            }


            // -- RECOIL -- 
            ExtraFrontArmRotation = currentRecoilRotation;

            // Resolve
            if (currentRecoilRotation != 0f)
                currentRecoilRotation = MathHelper.Lerp(currentRecoilRotation, 0f, RotationResolveSpeed);


            shootingTimer++;
        }
    }
}
