using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Magic
{
    public class SHPS : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public ref float Timer => ref Projectile.ai[1];
        public ref float State => ref Projectile.ai[2]; // 0 = Idle; 1 = Homing to enemy
        public const float HomingRange = 560f;

        public NPC Target;
        public float RandomAnglingStrength = 0f;

        public override void SetStaticDefaults() => ProjectileID.Sets.CultistIsResistantTo[Type] = true;
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 180;
        }

        // Can only deal damage after it starts homing
        public override bool? CanDamage() => Timer >= 25f;

        public override void AI()
        {
            Timer++;

            // Randomly changes the strength of idle turning
            if (Timer % 30 == 1f)
                RandomAnglingStrength = Main.rand.NextFloat(-0.16f, 0.16f);

            // Determine behavior
            if (Timer < 25) // Don't try to home at the start
                State = 0f;
            else
            {
                float npcDistCheck = HomingRange;
                int index = -1;
                foreach (NPC n in Main.ActiveNPCs)
                {
                    if (!n.CanBeChasedBy(Projectile))
                        continue;

                    float currentNPCDist = Vector2.Distance(n.Center, Projectile.Center);
                    if (currentNPCDist < npcDistCheck)
                    {
                        npcDistCheck = currentNPCDist;
                        index = n.whoAmI;
                    }
                }

                if (index == -1)
                    State = 0f;
                else
                {
                    Target = Main.npc[index];
                    State = 1f;
                }
            }

            // Actual behavior
            if (State == 0f)
            {
                Projectile.extraUpdates = 0;
                Projectile.velocity = Projectile.velocity.RotatedBy(RandomAnglingStrength);
            }
            else if (State == 1f)
            {
                Projectile.extraUpdates = 1;
                float speed = Projectile.velocity.Length();
                Projectile.velocity = Projectile.velocity.ToRotation().AngleTowards(Projectile.SafeDirectionTo(Target.Center).ToRotation(), 0.15f).ToRotationVector2() * speed;
            }

            // Appearance
            if (Timer % 1 == 0f)
            {
                SquareParticle trail = new(Projectile.Center, Vector2.Zero, false, 25, 3f, SHPB.FindColorForSoul((int)Projectile.ai[0]));
                GeneralParticleHandler.SpawnParticle(trail);
            }
            SquareParticle mainSpot = new(Projectile.Center, Vector2.Zero, false, 2, 1.4f, Color.White);
            GeneralParticleHandler.SpawnParticle(mainSpot);
        }
    }
}
