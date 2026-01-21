using System;
using CalamityMod.CalPlayer;
using CalamityMod.Dusts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    public class DemonshadeRedDevil : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public int dust = 3;

        public ref float State => ref Projectile.ai[0]; // 0 = attacking, 1 = returning, 2 = initial spawn
        public ref float Timer => ref Projectile.ai[1];

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 5;
            ProjectileID.Sets.MinionSacrificable[Type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 48;
            Projectile.netImportant = true;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 18000;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft *= 5;
        }

        public override void AI()
        {
            bool isMinion = Projectile.type == ModContent.ProjectileType<DemonshadeRedDevil>();
            Player player = Main.player[Projectile.owner];
            CalamityPlayer modPlayer = player.Calamity();
            if (!modPlayer.redDevil)
            {
                Projectile.active = false;
                return;
            }
            if (isMinion)
            {
                if (player.dead)
                {
                    modPlayer.rDevil = false;
                }
                if (modPlayer.rDevil)
                {
                    Projectile.timeLeft = 2;
                }
            }

            dust--;
            if (dust >= 0)
            {
                for (int i = 0; i < 50; i++)
                {
                    int brimDust = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y + 16f), Projectile.width, Projectile.height - 16, (int)CalamityDusts.Brimstone, 0f, 0f, 0, default, 1f);
                    Main.dust[brimDust].velocity *= 2f;
                    Main.dust[brimDust].scale *= 1.15f;
                }
            }

            Projectile.frameCounter++;
            if (Projectile.frameCounter > 8)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame > 4)
                    Projectile.frame = 0;
            }

            float lights = (float)Main.rand.Next(90, 111) * 0.01f;
            lights *= Main.essScale;
            Lighting.AddLight(Projectile.Center, 1f * lights, 0f * lights, 0.15f * lights);

            Projectile.rotation = Projectile.velocity.X * 0.04f;
            if ((double)Math.Abs(Projectile.velocity.X) > 0.2)
            {
                Projectile.spriteDirection = -Projectile.direction;
            }

            if (State == 2f)
            {
                Timer++;
                if (Timer > 60f)
                {
                    Timer = 1f;
                    State = 0f;
                    Projectile.netUpdate = true;
                }
                return;
            }

            Vector2 attackPos = Projectile.position;
            float attackRange = 2000f;
            bool canAttack = false;
            if (player.HasMinionAttackTargetNPC)
            {
                NPC npc = Main.npc[player.MinionAttackTargetNPC];
                if (npc.CanBeChasedBy(Projectile, false))
                {
                    float npcDist = Vector2.Distance(npc.Center, Projectile.Center);
                    if (!canAttack && npcDist < attackRange)
                    {
                        attackRange = npcDist;
                        attackPos = npc.Center;
                        canAttack = true;
                    }
                }
            }
            else
            {
                foreach (NPC nPC2 in Main.ActiveNPCs)
                {
                    if (nPC2.CanBeChasedBy(Projectile, false))
                    {
                        float npcDist = Vector2.Distance(nPC2.Center, Projectile.Center);
                        if ((!canAttack && npcDist < attackRange) && Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, nPC2.position, nPC2.width, nPC2.height))
                        {
                            attackRange = npcDist;
                            attackPos = nPC2.Center;
                            canAttack = true;
                        }
                    }
                }
            }

            float separationAnxietyDist = canAttack ? 3000f: 2000f;
            if (Vector2.Distance(player.Center, Projectile.Center) > separationAnxietyDist)
            {
                State = 1f;
                Projectile.netUpdate = true;
            }
            if (canAttack && State == 0f)
            {
                Vector2 projDirection = attackPos - Projectile.Center;
                float projDist = projDirection.Length();
                projDirection.Normalize();
                if (projDist > 200f)
                {
                    projDirection *= 25f;
                    Projectile.velocity = (Projectile.velocity * 40f + projDirection) / 41f;
                }
                else
                {
                    projDirection *= -9f;
                    Projectile.velocity = (Projectile.velocity * 40f + projDirection) / 41f;
                }
            }
            else
            {
                bool isReturning = State == 1f;
                float returnSpeed = isReturning ? 24f : 15f;

                Vector2 playerDirection = player.Center - Projectile.Center + new Vector2(0f, -30f);
                float playerDist = playerDirection.Length();
                if (playerDist > 200f && returnSpeed < 10f)
                {
                    returnSpeed = 18f;
                }
                if (playerDist < 150f && isReturning && !Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height))
                {
                    State = 0f;
                    Projectile.netUpdate = true;
                }
                if (playerDist > 2000f)
                {
                    Projectile.Center = player.Center;
                    Projectile.netUpdate = true;
                }
                if (playerDist > 70f)
                {
                    playerDirection.Normalize();
                    playerDirection *= returnSpeed;
                    Projectile.velocity = (Projectile.velocity * 40f + playerDirection) / 41f;
                }
                else if (Projectile.velocity == Vector2.Zero)
                {
                    Projectile.velocity = new Vector2(-0.2f, -0.1f);
                }
            }
            if (Timer > 0f)
            {
                Timer += Main.rand.Next(1, 2 + 1);
            }
            if (Timer > 80f)
            {
                Timer = 0f;
                Projectile.netUpdate = true;
            }
            if (State == 0f)
            {
                if (canAttack && Timer == 0f)
                {
                    Timer += 1f;
                    if (Main.myPlayer == Projectile.owner && Collision.CanHitLine(Projectile.Center, Projectile.width, Projectile.height, attackPos, 0, 0))
                    {
                        Vector2 velocity = Vector2.Normalize(attackPos - Projectile.Center) * 24f;
                        int trident = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ProjectileID.UnholyTridentFriendly, Projectile.damage, 0f, Main.myPlayer);
                        if (trident.WithinBounds(Main.maxProjectiles))
                        {
                            Main.projectile[trident].timeLeft = 300;
                            Main.projectile[trident].usesLocalNPCImmunity = true;
                            Main.projectile[trident].localNPCHitCooldown = 10;
                            Main.projectile[trident].DamageType = DamageClass.Generic; //typeless
                        }
                        Projectile.netUpdate = true;
                    }
                }
            }
        }

        public override bool? CanDamage() => false;
    }
}
