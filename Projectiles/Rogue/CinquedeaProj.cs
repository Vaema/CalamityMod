using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class CinquedeaProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Items/Weapons/Rogue/Cinquedea";

        public ref float Timer => ref Projectile.ai[0];
        public ref float Target => ref Projectile.ai[1];
        public static readonly SoundStyle StealthSliceSound = new("CalamityMod/Sounds/Custom/SwiftSlice");

        internal float gravspin = 0f;
        private Vector2 StoredVelocity;
        private Vector2 StickOffset;
        private const int StickTime = 30;
        private int Stick = 0;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 4;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.penetrate = 2;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 10;
        }

        public override void AI()
        {
            var modProj = Projectile.Calamity();
            DrawOriginOffsetY = 11;
            DrawOffsetX = -22;
            gravspin = Projectile.velocity.Y * (0.03f * Projectile.spriteDirection);
            Timer++;

            //Fucking slopes
            if (Timer > 2f)
                Projectile.tileCollide = true;

            // Stealth sticking handle
            if (Stick > 0)
            {
                Projectile.Center = Main.npc[(int)Target].Center + StickOffset;
                Stick--;
                if (Stick == 0)
                {
                    SoundEngine.PlaySound(StealthSliceSound, Projectile.Center);
                    Projectile.velocity = StoredVelocity;
                    Projectile.extraUpdates = 8;
                    Projectile.timeLeft = 90;

                    // Launch particles
                    for (int i = 0; i < 6; i++)
                    {
                        ElectricSpark spark = new(Projectile.Center, -Projectile.velocity.RotatedByRandom(MathHelper.Pi / 6f), Color.Aqua, Color.AliceBlue, 1.2f, 60);
                        GeneralParticleHandler.SpawnParticle(spark);
                    }
                }
            }

            // Stealth visual
            if (modProj.stealthStrike && Stick == 0 && Timer % (Projectile.numHits > 0 ? 1 : 3) == 0)
            {
                ElectricSpark spark = new(Projectile.Center, Main.rand.NextVector2CircularEdge(3f, 3f), Color.Aqua, Color.AliceBlue, 0.9f, 25);
                GeneralParticleHandler.SpawnParticle(spark);
            }

            //Face-forward rotation code
            if (((Timer <= 80 && !modProj.stealthStrike) || modProj.stealthStrike || Projectile.velocity.Y <= 0) && Stick == 0)
            {
                Projectile.spriteDirection = Projectile.direction = (Projectile.velocity.X > 0).ToDirectionInt();
                Projectile.rotation = Projectile.velocity.ToRotation() + (Projectile.spriteDirection == 1 ? 0f : MathHelper.Pi);
                Projectile.rotation += MathHelper.ToRadians(45f) * Projectile.spriteDirection;
            }

            // Gravity code
            if (Timer > 80 && !modProj.stealthStrike)
            {
                Projectile.velocity.Y = Projectile.velocity.Y + 0.15f;
                if (Projectile.velocity.Y > 0)
                    Projectile.rotation += gravspin;

                if (Projectile.velocity.Y > 10f)
                    Projectile.velocity.Y = 10f;
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
            return true;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            return false;
        }

        public override bool? CanDamage() => Stick == 0;

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.Calamity().stealthStrike && Projectile.numHits == 0)
            {
                Target = target.whoAmI;
                Projectile.penetrate = -1;
                Stick = StickTime;
                StickOffset = Projectile.Center - target.Center;
                StoredVelocity = Projectile.velocity;
                Projectile.velocity = Vector2.Zero;
            }
        }
    }
}
