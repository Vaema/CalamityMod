using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
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

        public override void HoldoutAI()
        {
            // The center of the player, taking into account if they have a mount or not.
            Vector2 mountedCenter = Owner.MountedCenter;

            // The vector between the player and the mouse.
            Vector2 ownerToMouse = Owner.Calamity().mouseWorld - mountedCenter;


            if (ShootingTimer >= HeldItem.useAnimation)
            {
                // We use the velocity of this projectile as its direction vector.
                Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.Zero);

                Owner.PickAmmo(Owner.ActiveItem(), out _, out float itemShootSpeed, out int itemDamage, out float itemKnockback, out int rocketTypeShot);

                Vector2 smokeVel = Projectile.velocity.SafeNormalize(Vector2.UnitY) * 5;

                if (Main.rand.NextBool(3))
                    Projectile.NewProjectileDirect(
                    Projectile.GetSource_FromThis(),
                    GunTipPosition,
                    direction * itemShootSpeed,
                    ProjectileType<AquaBlastToxic>(),
                    itemDamage,
                    itemKnockback,
                    Projectile.owner,
                    ownerToMouse.Length());
                else
                    Projectile.NewProjectileDirect(
                    Projectile.GetSource_FromThis(),
                    GunTipPosition,
                    direction * itemShootSpeed,
                    ProjectileType<AquaBlast>(),
                    itemDamage,
                    itemKnockback,
                    Projectile.owner,
                    ownerToMouse.Length());

                // Inside here go all the things that dedicated servers shouldn't spend resources on.
                // Like visuals and sounds.
                if (!Main.dedServ)
                {
                    // By decreasing the offset length of the gun from the arms, we give an effect of recoil.
                    OffsetLengthFromArm = 18f;

                    int smokeAmount = Main.rand.Next(8, 12 + 1);
                    for (int i = 0; i < smokeAmount; i++)
                    {
                        Particle smoke = new HeavySmokeParticle(GunTipPosition, smokeVel.RotatedByRandom(0.4f) * Main.rand.NextFloat(0.2f, 1f), Color.White, Main.rand.Next(40, 60 + 1), Main.rand.NextFloat(0.2f, 0.4f), 0.5f, Main.rand.NextFloat(-0.2f, 0.2f), Main.rand.NextBool(), required: true);
                        GeneralParticleHandler.SpawnParticle(smoke);

                    }
                    SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Item/FlakKrakenShoot") { Pitch = 0.65f, Volume = 0.5f }, GunTipPosition);
                }
                ShootingTimer = 0f;
                ShotsFired++;
                if (ShotsFired == 7)
                {
                    int monsterCount;
                    monsterCount = 2;
                    for (int a = 0; a < monsterCount; a++)
                    {
                        float projSpeed = itemShootSpeed;
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Owner.Center + Main.rand.NextVector2Circular(30, 30), (Owner.Calamity().mouseWorld - Owner.MountedCenter).SafeNormalize(Vector2.Zero) * projSpeed, ModContent.ProjectileType<LeviatitanAberration>(), (int)(itemDamage * 1.5f), itemKnockback);
                        SoundEngine.PlaySound(SoundID.Zombie38 with { Volume = SoundID.Zombie38.Volume * 0.5f }, mountedCenter);
                    }
                }
                if (ShotsFired >= 8)
                    ShotsFired = 0;
            }
            ShootingTimer++;
        }
        public override void OnSpawn(IEntitySource source)
        {
            base.OnSpawn(source);
            SoundEngine.PlaySound(SoundID.Item108 with { Volume = 0.7f }, Projectile.Center);
        }
    }
}
