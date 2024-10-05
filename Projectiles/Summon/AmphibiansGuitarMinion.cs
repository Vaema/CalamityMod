using CalamityMod.Buffs.Summon;
using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Projectiles.Summon
{
    public class AmphibiansGuitarMinion : BaseMinionProjectile
    {
        public override int AssociatedProjectileTypeID => ProjectileType<AmphibiansGuitarMinion>();

        public override int AssociatedBuffTypeID => BuffType<AmphibiansGuitarBuff>();

        public override ref bool AssociatedMinionBool => ref ModdedOwner.AmphibiansGuitarBool;

        /// <summary>
        /// A property that states which guitar sprite is using from the spritesheet.<br/>
        /// First guitar goes from 0 and the last to 7.
        /// </summary>
        private int GuitarSprite
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = MathHelper.Clamp(value, 0, 7);
        }

        private float IntendedRotationAngle => MathHelper.TwoPi / (Owner == null ? 1f : MathHelper.Clamp(Owner.ownedProjectileCounts[Type], 1f, 8f)) * Projectile.ai[0] + Main.GlobalTimeWrappedHourly * 1.8f;

        private Vector2 RotationPosition => Target == null ? Owner.MountedCenter : Target.Center;

        private ref float ShootTimer => ref Projectile.ai[1];

        public override void SetDefaults()
        {
            base.SetDefaults();
            (Projectile.width, Projectile.height) = (92, 92);
        }

        public override void MinionAI()
        {
            if (Projectile.ai[2] == 0f)
            {
                ShootTimer = Main.rand.NextFloat(60);
                Projectile.ai[2]++;
                Projectile.netUpdate = true;
                Projectile.netSpam = 0;
            }

            Vector2 intendedPosition = RotationPosition - Vector2.UnitY.RotatedBy(IntendedRotationAngle) * (Target == null ? 100f : (Target.Size.Length() / 2f) + 100f);
            Projectile.Center = Vector2.Lerp(Projectile.Center, intendedPosition, Utils.Remap(Projectile.DistanceSQ(intendedPosition), 6400f, 0f, 0.1f, 0.4f));

            if (Target != null)
            {
                if (ShootTimer > 60f && Main.myPlayer == Projectile.owner)
                {
                    Projectile.NewProjectileDirect(
                        Projectile.GetSource_FromThis(),
                        Projectile.Center,
                        CalamityUtils.CalculatePredictiveAimToTarget(Projectile.Center, Target, 12f),
                        ProjectileType<AmphibiansGuitarProjectile>(),
                        Projectile.damage,
                        Projectile.knockBack,
                        Projectile.owner,
                        ai0: (Main.rand.NextBool(2) && Owner.ownedProjectileCounts[Type] == 8).ToInt());
                    ShootTimer = 0f;
                    Projectile.netUpdate = true;
                    Projectile.netSpam = 0;
                }

                ShootTimer += Main.rand.NextBool(60) ? 2f : 1f;
            }
        }

        public override bool? CanDamage() => false;

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Rectangle frame = texture.Frame(horizontalFrames: 8, frameX: GuitarSprite);
            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                frame,
                Color.White,
                IntendedRotationAngle,
                frame.Size() * 0.5f,
                Projectile.scale,
                SpriteEffects.None);
            return false;
        }
    }
}
