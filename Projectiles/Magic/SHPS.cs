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
        public ref float Pickup => ref Projectile.ai[2]; // 0 if attacking soul, 1 if pickup soul
        private float AIState = 0f; // 0 = Idle, 1 = Homing to enemy, 2 = Getting sucked to player
        private const float HomingRange = 560f;

        private NPC Target;
        private Projectile ToSuckTowards;
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
        public override bool? CanDamage() => Timer >= 25f && Pickup == 0f ? null : false;

        public override void AI()
        {
            Timer++;

            // Determine behavior
            if (Timer >= 25) // Always remains idle for a short delay
            {
                if (Pickup == 0f) // Only run homing code if not a pickup
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
                        AIState = 0f;
                    else
                    {
                        Target = Main.npc[index];
                        AIState = 1f;
                    }
                }
                else // Only run suction code if a pickup
                {
                    foreach (Projectile p in Main.ActiveProjectiles)
                    {
                        if (p.type == ModContent.ProjectileType<SHPV>() && p.Colliding(p.Hitbox, Projectile.Hitbox))
                        {
                            AIState = 2f;
                            ToSuckTowards = p;
                            break;
                        }
                        else
                            AIState = 0f;
                    }
                }
            }

            // Actual behavior
            switch (AIState)
            {
                case 0f:
                    Projectile.extraUpdates = 0;
                    // Randomly changes the strength of idle turning
                    if (Timer % 30 == 1f)
                        RandomAnglingStrength = Main.rand.NextFloat(-0.16f, 0.16f);
                    Projectile.velocity = Projectile.velocity.RotatedBy(RandomAnglingStrength);
                    if (Projectile.velocity.Length() > 2.75f && Pickup == 1f)
                        Projectile.velocity *= 0.96f;
                    break;
                case 1f:
                    Projectile.extraUpdates = 1;
                    float speed = Projectile.velocity.Length();
                    Projectile.velocity = Projectile.velocity.ToRotation().AngleTowards(Projectile.SafeDirectionTo(Target.Center).ToRotation(), 0.15f).ToRotationVector2() * speed;
                    break;
                case 2f:
                    Projectile.velocity = (Projectile.velocity * 15f + Utils.DirectionTo(Projectile.Center, ToSuckTowards.ModProjectile<SHPV>().TipPosition + ToSuckTowards.velocity) * 25f) / 16f;
                    if (Vector2.Distance(Projectile.Center, ToSuckTowards.ModProjectile<SHPV>().TipPosition) < 70f)
                    {
                        ToSuckTowards.ModProjectile<SHPV>().SoulColors.Add(Projectile.ai[0]);
                        Projectile.Kill();
                    }
                    break;
            }

            // Appearance
            if (Timer % 1 == 0f)
            {
                SquareParticle trail = new(Projectile.Center, Vector2.Zero, false, 25, 3f, SHPB.FindColorForSoul((int)Projectile.ai[0]), Pickup == 1f ? MathHelper.PiOver4 : 0f);
                GeneralParticleHandler.SpawnParticle(trail);
            }
            SquareParticle mainSpot = new(Projectile.Center, Vector2.Zero, false, 2, 1.4f, Color.White, Pickup == 1f ? MathHelper.PiOver4 : 0f);
            GeneralParticleHandler.SpawnParticle(mainSpot);
        }
    }
}
