using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Particles;
using static Terraria.Player;
using Microsoft.Xna.Framework.Graphics;
using static System.Net.Mime.MediaTypeNames;
using CalamityMod.Projectiles.Melee;

namespace CalamityMod.Projectiles.DraedonsArsenal
{
    public class StarburstShivHoldout : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Misc";

        public override string Texture => "CalamityMod/Items/Weapons/Melee/StarburstShiv";
        public ref float attackTimer => ref Projectile.ai[0];
        public Player Owner => Main.player[Projectile.owner];

        // Sprite visuals
        public float scaleFx = 0.6f; // Default that looks reasonable size-wise, changes on some attacks

        public Vector2 innateOffset = new(23f, -5f);
        public Vector2 handPos;

        public int primaryStabfireRate => 2;
        public int time = 0;

        public float bladeRot = 0;
        public bool pressedRight = false;

        // Internal control
        private int stabTimer;

        public override bool? CanDamage() => false;

        public override void SetDefaults()
        {
            Projectile.width = 74;
            Projectile.height = 94;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 5;
            Projectile.extraUpdates = 1;
        }
        public void Positioning(Vector2 toMouse) // Hand and holdout positioning
        {

            Owner.ChangeDir(Math.Sign(toMouse.X));

            // Calculate the angle for the arm holding the weapon
            float baseArmRotation = toMouse.ToRotation();
            float compositeArmRotation = baseArmRotation + bladeRot - MathHelper.PiOver2;

            // Set the front composite arm to point towards the correct direction.
            Owner.SetCompositeArmFront(true, CompositeArmStretchAmount.Full, compositeArmRotation);
            Owner.SetCompositeArmBack(true, CompositeArmStretchAmount.Full, 0f);

            Vector2 actualInnateOffset = innateOffset;
            if (Owner.direction == -1)
            {
                actualInnateOffset.X += 1f;
                actualInnateOffset.Y += 10f;

            }

            handPos = Owner.GetFrontHandPosition(CompositeArmStretchAmount.Full, compositeArmRotation) + actualInnateOffset.RotatedBy(baseArmRotation);

            // Set properties based on the mouse direction
            Projectile.velocity = toMouse;
            Projectile.rotation = toMouse.ToRotation() + bladeRot;
            Projectile.Center = handPos;

            // Same for the owner
            Owner.heldProj = Projectile.whoAmI;
            Owner.itemTime = Owner.itemAnimation = 2;

            // Item rotation for the player's sprite display
            Owner.itemRotation = Projectile.rotation;
            if (Owner.direction != 1)
            {
                Owner.itemRotation -= MathHelper.Pi;
            }

            Owner.itemRotation = MathHelper.WrapAngle(Owner.itemRotation);
        }


        public override void AI()
        {
            Projectile.scale = 1f;

            if (Owner.channel)
            {
                Projectile.timeLeft = 2;
            }
            else
            {
                Projectile.Kill();
            }

            Vector2 toMouse = Utils.DirectionTo(Owner.Center, Owner.ClampedMouseWorld());
            Positioning(toMouse);

            if (Owner.altFunctionUse == 0)
            {
                UsePrimary(toMouse);

                Vector2 currOffset = Projectile.Center + new Vector2(Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-1f, 7f));
                Projectile.Center = currOffset;
            }

            else if (Owner.altFunctionUse == 2) 
            {
                 // Dash functionality
            }
        }

        // -- BASIC M1 --
        private void UsePrimary(Vector2 toMouse)
        {
            stabTimer++;
            if (stabTimer % primaryStabfireRate != 0)
                return;

            float offset = Main.rand.NextFloat(-MathHelper.ToRadians(6f), MathHelper.ToRadians(6f));
            Vector2 stabDir = toMouse.RotatedBy(offset);

            // SoundEngine.PlaySound(SoundID.Item1 with { Volume = 0.8f, Pitch = 0.1f, MaxInstances = 6, PitchVariance = 0.1f }, Owner.Center);

            Vector2 stabOrigin = Owner.MountedCenter;
            Vector2 stabTip = stabOrigin + stabDir * 100f;

            for (int i = 0; i < 4; i++)
            {
                Vector2 spawnPos = stabTip + Main.rand.NextVector2Circular(18f, 12f);

                Vector2 vel = stabDir * Main.rand.NextFloat(5f, 19f);
                Color color = Color.Lerp(Color.AliceBlue, Color.OrangeRed, Main.rand.NextFloat(1f));
                Particle spark = new GlowSparkParticle(spawnPos, vel, false, Main.rand.Next(5, 8), Main.rand.NextFloat(0.02f, 0.07f), color * 0.55f, new Vector2(0.5f, 1.3f), true, false);
                GeneralParticleHandler.SpawnParticle(spark);
            }


            // Drawing randomness
            bladeRot = Main.rand.NextFloat(-0.25f, 0.5f) * Owner.direction;
            Projectile.scale *= Main.rand.NextFloat(0.88f, 1.04f);
            if (Main.rand.NextBool())
            {
                Owner.SetCompositeArmFront(true, Main.rand.NextBool() ? CompositeArmStretchAmount.ThreeQuarters : CompositeArmStretchAmount.Quarter, Owner.itemRotation - (Owner.direction == 1 ? MathHelper.PiOver2 : MathHelper.TwoPi * 0.75f) + Main.rand.NextFloat(-0.22f, 0.22f));
            }

            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Owner.Center + toMouse * 20 * (float)Math.Pow(scaleFx, 4), toMouse * 25, ModContent.ProjectileType<StarburstShivM1Hitbox>(), Projectile.damage, 0, Projectile.owner);

        }

        // -- DASH --
        private void UseSecondary(Vector2 toMouse)
        {

        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Un-jand the rotation that the sprite is drawn to actually point toward the mouse cursor
            float drawRotation = Projectile.rotation + MathHelper.PiOver4;
            if (Owner.direction == -1)
            {
                drawRotation += MathHelper.PiOver2;
            }
        
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            Rectangle frame = new Rectangle(0, Projectile.frame * frameHeight, texture.Width, frameHeight);
            Vector2 origin = new Vector2(texture.Width / 2f, frameHeight / 2f);

            SpriteEffects spriteEffects = Owner.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, lightColor, drawRotation, origin, Projectile.scale * scaleFx, spriteEffects, 0);
            return false;
        }
    }
}
