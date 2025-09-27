using System;
using System.Collections.Generic;
using CalamityMod.Items.Weapons.Rogue;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class GlaiveProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Items/Weapons/Rogue/Glaive";

        private static int Lifetime = 180;
        private static int ReboundTime = 40;
        private List<int> HitNPCs = [];
        public ref float AIState => ref Projectile.ai[0]; // 0 - Going out. 1 - Returning. 2 - Stealth stuck to enemy.

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 3;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 3;
            Projectile.MaxUpdates = 3;
            Projectile.timeLeft = Lifetime;
            DrawOffsetX = -10;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 20;
        }

        public override void AI()
        {
            // Boomerang rotation
            Projectile.rotation += 0.175f * Projectile.direction;

            // Boomerang sound
            if (Projectile.soundDelay == 0)
            {
                Projectile.soundDelay = 8;
                SoundEngine.PlaySound(SoundID.Item7, Projectile.position);
            }

            // Returns after some number of frames in the air
            if (Projectile.timeLeft < Lifetime - ReboundTime)
                AIState = 1f;

            if (AIState == 0f && Projectile.ai[1] >= 0)
            {
                if (Projectile.timeLeft % 10 == 0)
                    Projectile.velocity = Utils.DirectionTo(Projectile.Center, Main.npc[(int)Projectile.ai[1]].Center) * Projectile.velocity.Length();
            }

            if (AIState == 1f)
            {
                Projectile.tileCollide = false;

                float returnSpeed = Glaive.Speed * 1.6f;
                float acceleration = 1.4f;

                if (Projectile.Calamity().stealthStrike)
                {
                    returnSpeed *= Glaive.StealthSpeedMult;
                    acceleration *= Glaive.StealthSpeedMult;
                }

                Player owner = Main.player[Projectile.owner];

                // Delete the projectile if it's excessively far away.
                Vector2 playerCenter = owner.Center;
                float xDist = playerCenter.X - Projectile.Center.X;
                float yDist = playerCenter.Y - Projectile.Center.Y;
                float dist = (float)Math.Sqrt((double)(xDist * xDist + yDist * yDist));
                if (dist > 3000f)
                    Projectile.Kill();

                dist = returnSpeed / dist;
                xDist *= dist;
                yDist *= dist;

                // Home back in on the player.
                if (Projectile.velocity.X < xDist)
                {
                    Projectile.velocity.X += acceleration;
                    if (Projectile.velocity.X < 0f && xDist > 0f)
                        Projectile.velocity.X += acceleration;
                }
                else if (Projectile.velocity.X > xDist)
                {
                    Projectile.velocity.X -= acceleration;
                    if (Projectile.velocity.X > 0f && xDist < 0f)
                        Projectile.velocity.X -= acceleration;
                }
                if (Projectile.velocity.Y < yDist)
                {
                    Projectile.velocity.Y += acceleration;
                    if (Projectile.velocity.Y < 0f && yDist > 0f)
                        Projectile.velocity.Y += acceleration;
                }
                else if (Projectile.velocity.Y > yDist)
                {
                    Projectile.velocity.Y -= acceleration;
                    if (Projectile.velocity.Y > 0f && yDist < 0f)
                        Projectile.velocity.Y -= acceleration;
                }

                // Delete the projectile if it touches its owner.
                if (Main.myPlayer == Projectile.owner)
                    if (Projectile.Hitbox.Intersects(owner.Hitbox))
                        Projectile.Kill();
            }

            if (AIState == 2f)
            {
                Projectile.tileCollide = false;
                Projectile.velocity = Vector2.Zero;
                if (Main.npc[(int)Projectile.ai[1]].active)
                {
                    if (Projectile.FinalExtraUpdate())
                        Projectile.Center += Main.npc[(int)Projectile.ai[1]].velocity;
                }
                else
                    AIState = 1f;

                if (Projectile.penetrate == -1)
                {
                    AIState = 1f;
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 1);
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.Calamity().stealthStrike)
            {
                if (AIState != 2f)
                {
                    HitNPCs.Add(target.whoAmI);
                    float maxDistance = 1000f;
                    float npcDistCompare = 1000f;
                    int index = -1;
                    foreach (NPC n in Main.ActiveNPCs) // Find an NPC to ricochet to
                    {
                        if (!n.CanBeChasedBy(Projectile) || !Projectile.WithinRange(n.Center, maxDistance) || HitNPCs.Contains(n.whoAmI))
                            continue;

                        float currentNPCDist = Vector2.Distance(n.Center, Projectile.Center);
                        if ((currentNPCDist < npcDistCompare) && (Collision.CanHit(Projectile.Center, 1, 1, n.Center, 1, 1)))
                        {
                            npcDistCompare = currentNPCDist;
                            index = n.whoAmI;
                        }
                    }

                    // If you find an NPC, ricochet in their direction and reset iframes for them
                    if (index != -1)
                    {
                        Projectile.ai[1] = index;
                        Projectile.velocity = Utils.DirectionTo(Projectile.Center, Main.npc[index].Center) * Projectile.velocity.Length();
                        Projectile.perIDStaticNPCImmunity[Type][index] = Main.GameUpdateCount; // Resets the iframes
                    }
                    else // If there are no new NPCs to ricochet to, try to go back to an already hit NPC
                    {
                        maxDistance = 1000f;
                        npcDistCompare = 1000f;
                        index = -1;
                        foreach (NPC n in Main.ActiveNPCs)
                        {
                            if (!n.CanBeChasedBy(Projectile) || !Projectile.WithinRange(n.Center, maxDistance) || n.whoAmI == target.whoAmI)
                                continue;

                            float currentNPCDist = Vector2.Distance(n.Center, Projectile.Center);
                            if ((currentNPCDist < npcDistCompare) && (Collision.CanHit(Projectile.Center, 1, 1, n.Center, 1, 1)))
                            {
                                npcDistCompare = currentNPCDist;
                                index = n.whoAmI;
                            }
                        }

                        // If this can find an NPC, ricochet back to them
                        if (index != -1)
                        {
                            Projectile.ai[1] = index;
                            Projectile.velocity = Utils.DirectionTo(Projectile.Center, Main.npc[index].Center) * Projectile.velocity.Length();
                            Projectile.perIDStaticNPCImmunity[Type][index] = Main.GameUpdateCount; // Resets the iframes
                        }
                        else // If you still find no one new, stick to the hit NPC
                        {
                            Projectile.ai[1] = target.whoAmI;
                            AIState = 2f;
                        }
                    }
                }
                Projectile.timeLeft = Lifetime + ReboundTime * 3;
            }
            // After its last hit, starts returning instead of vanishing. Can pierce infinitely on the way back.
            if (Projectile.penetrate == 1)
            {
                Projectile.penetrate = -1;
                AIState = 1f;
            }
        }

        // Make it bounce on tiles.
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // Impacts the terrain even though it bounces off.
            SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
            Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);

            if (Projectile.velocity.X != oldVelocity.X)
            {
                Projectile.velocity.X = -oldVelocity.X;
            }
            if (Projectile.velocity.Y != oldVelocity.Y)
            {
                Projectile.velocity.Y = -oldVelocity.Y;
            }
            AIState = 1f;
            return false;
        }
    }
}
