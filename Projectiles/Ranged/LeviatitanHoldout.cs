using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using CalamityMod.Projectiles.Melee;
using CalamityMod.Projectiles.Ranged;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static CalamityMod.Items.Weapons.Ranged.Leviatitan;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Items.Weapons.Ranged
{
    public class LeviatitanHoldout : BaseGunHoldoutProjectile
    {
        public override int AssociatedItemID => ItemType<Leviatitan>();
        public override string Texture => "CalamityMod/Items/Weapons/Ranged/Leviatitan";

        
        public override float MaxOffsetLengthFromArm => 38f;
        public override float RecoilResolveSpeed => 0.1f;
        public override float OffsetXUpwards => -10f;
        public override float OffsetXDownwards => 2f;
        public override float BaseOffsetY => -10f;
        public override float OffsetYUpwards => 10f;
        public override float OffsetYDownwards => 5f;
        public ref float ShootingTimer => ref Projectile.ai[0];
        public ref float ShotsFired => ref Projectile.ai[1];
        public ref float ShootTimer => ref Projectile.ai[2];
        public int shotColor = 1;
        public bool HasfiredMeteor = false;
        public int time = 0;

        public override void HoldoutAI()
        {
            // The center of the player, taking into account if they have a mount or not.
            Vector2 mountedCenter = Owner.MountedCenter;

            // The vector between the player and the mouse.
            Vector2 ownerToMouse = Owner.Calamity().mouseWorld - mountedCenter;

            if (ShootingTimer >= HeldItem.useAnimation)
            {
                HasfiredMeteor = false;
                // We use the velocity of this projectile as its direction vector.
                Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.Zero);

                Vector2 shootVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * 10;
                Vector2 smokeVel = Projectile.velocity.SafeNormalize(Vector2.UnitY) * 5;

                if (Owner.Calamity().mouseRight)
                {
                    Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), GunTipPosition, shootVelocity, ProjectileType<CometQuasherMeteor>(), Projectile.damage * 4, Projectile.knockBack * 3, Projectile.owner, ownerToMouse.Length());
                    if (!Main.dedServ)
                    {
                        // By decreasing the offset length of the gun from the arms, we give an effect of recoil.
                        OffsetLengthFromArm = 9f;

                        int smokeAmount = Main.rand.Next(8, 12 + 1);
                        for (int i = 0; i < smokeAmount; i++)
                        {
                            Particle smoke = new HeavySmokeParticle(GunTipPosition, smokeVel.RotatedByRandom(0.4f) * Main.rand.NextFloat(1.2f, 2f), Color.White, Main.rand.Next(40, 60 + 1), Main.rand.NextFloat(0.2f, 0.4f), 0.3f, Main.rand.NextFloat(-0.2f, 0.2f), Main.rand.NextBool(), required: true);
                            GeneralParticleHandler.SpawnParticle(smoke);

                        }
                        SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Item/LeviatitanRoar") { Volume = 0.5f }, GunTipPosition);
                        SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Item/MagicRockSound") with { Pitch = -0.7f, Volume = 0.8f }, Projectile.Center);
                    }
                    HasfiredMeteor = true;
                }
                else
                {
                    // Fire the projectiles
                    Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), GunTipPosition, shootVelocity.RotatedBy(MathHelper.ToRadians(2f * shotColor)), ProjectileType<AquaBlastToxic>(), Projectile.damage, Projectile.knockBack, Projectile.owner, ownerToMouse.Length());
                    Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), GunTipPosition, shootVelocity.RotatedBy(MathHelper.ToRadians(-2f * shotColor)), ProjectileType<AquaBlast>(), Projectile.damage, Projectile.knockBack, Projectile.owner, ownerToMouse.Length());
                    // Swap which is the top and bottom with every shot
                    shotColor = (shotColor == 1 ? -1 : 1);

                    // Inside here go all the things that dedicated servers shouldn't spend resources on.
                    // Like visuals and sounds.
                    if (!Main.dedServ)
                    {
                        // By decreasing the offset length of the gun from the arms, we give an effect of recoil.
                        OffsetLengthFromArm = 25f;

                        int smokeAmount = Main.rand.Next(8, 12 + 1);
                        for (int i = 0; i < smokeAmount; i++)
                        {
                            Particle smoke = new HeavySmokeParticle(GunTipPosition, smokeVel.RotatedByRandom(0.4f) * Main.rand.NextFloat(0.2f, 1f), Color.White, Main.rand.Next(40, 60 + 1), Main.rand.NextFloat(0.2f, 0.4f), 0.3f, Main.rand.NextFloat(-0.2f, 0.2f), Main.rand.NextBool(), required: true);
                            GeneralParticleHandler.SpawnParticle(smoke);

                        }
                        SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Item/FlakKrakenShoot") { Pitch = 0.9f, Volume = 0.5f }, GunTipPosition);
                        SoundEngine.PlaySound(SoundID.Item108 with { Pitch = -0.5f, Volume = 0.8f }, Projectile.Center);

                    }
                }
                
                ShootingTimer = 0f;
                ShotsFired++;

                if (ShotsFired == 6)
                {
                    int monsterCount;
                    monsterCount = 2;
                    for (int a = 0; a < monsterCount; a++)
                    {
                        float projSpeed = 5;
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Owner.Center + Main.rand.NextVector2Circular(30, 30), (Owner.Calamity().mouseWorld - Owner.MountedCenter).SafeNormalize(Vector2.Zero) * projSpeed, ModContent.ProjectileType<LeviatitanAberration>(), (int)(Projectile.damage * 1.2f), Projectile.knockBack);
                    }
                    SoundEngine.PlaySound(SoundID.Zombie38 with { Volume = SoundID.Zombie38.Volume * 0.5f }, mountedCenter);
                }
                if (ShotsFired >= 6)
                    ShotsFired = 0;
            }
            if (!HasfiredMeteor || time % 3 == 0)
                ShootingTimer++;
            time++;
        }
        public override void OnSpawn(IEntitySource source)
        {
            base.OnSpawn(source);
        }
    }
}
