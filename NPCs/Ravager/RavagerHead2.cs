using System;
using CalamityMod.Events;
using CalamityMod.Projectiles.Boss;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.Ravager
{
    public class RavagerHead2 : ModNPC
    {
        public override LocalizedText DisplayName => CalamityUtils.GetText("NPCs.RavagerBody.DisplayName");
        public override void SetStaticDefaults()
        {
            this.HideFromBestiary();
            NPCID.Sets.NeedsExpertScaling[Type] = true;
        }

        public static int HomingDartDamage = 30; // 120
        public static int PostProviDartBuff = 20; // +80 == 200

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.damage = 0; // No contact damage
            NPC.width = 80;
            NPC.height = 80;
            NPC.defense = 40;
            NPC.DR_NERD(0.15f);
            NPC.lifeMax = 20000;
            NPC.knockBackResist = 0f;
            AIType = -1;
            NPC.netAlways = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = RavagerBody.HitSound;
            NPC.DeathSound = RavagerBody.LimbLossSound;
            if (DownedBossSystem.downedProvidence && !BossRushEvent.BossRushActive)
            {
                NPC.defense *= 2;
                NPC.lifeMax *= 3;
            }
            if (BossRushEvent.BossRushActive)
            {
                NPC.lifeMax = 22500;
            }
            NPC.Calamity().VulnerableToSickness = false;
            NPC.Calamity().VulnerableToWater = true;
            NPC.dontTakeDamage = true;
        }

        public override void AI()
        {
            if (CalamityGlobalNPC.scavenger < 0 || !Main.npc[CalamityGlobalNPC.scavenger].active)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    NPC.StrikeInstantKill();

                return;
            }

            Player player = Main.player[Main.npc[CalamityGlobalNPC.scavenger].target];

            bool death = CalamityWorld.death || BossRushEvent.BossRushActive;
            bool provy = DownedBossSystem.downedProvidence && !BossRushEvent.BossRushActive;

            if (NPC.timeLeft < 1800)
                NPC.timeLeft = 1800;

            // Rotation
            float playerXDist = NPC.position.X + (NPC.width / 2) - player.position.X - (player.width / 2);
            float playerYDist = NPC.position.Y + NPC.height - 59f - player.position.Y - (player.height / 2);
            float headRotation = (float)Math.Atan2(playerYDist, playerXDist) + MathHelper.PiOver2;
            if (headRotation < 0f)
                headRotation += MathHelper.TwoPi;
            else if (headRotation > MathHelper.TwoPi)
                headRotation -= MathHelper.TwoPi;

            float headRotateIncrement = 0.1f;
            if (NPC.rotation < headRotation)
            {
                if ((headRotation - NPC.rotation) > MathHelper.Pi)
                    NPC.rotation -= headRotateIncrement;
                else
                    NPC.rotation += headRotateIncrement;
            }
            else if (NPC.rotation > headRotation)
            {
                if ((NPC.rotation - headRotation) > MathHelper.Pi)
                    NPC.rotation += headRotateIncrement;
                else
                    NPC.rotation -= headRotateIncrement;
            }

            if (NPC.rotation > headRotation - headRotateIncrement && NPC.rotation < headRotation + headRotateIncrement)
                NPC.rotation = headRotation;
            if (NPC.rotation < 0f)
                NPC.rotation += MathHelper.TwoPi;
            else if (NPC.rotation > MathHelper.TwoPi)
                NPC.rotation -= MathHelper.TwoPi;
            if (NPC.rotation > headRotation - headRotateIncrement && NPC.rotation < headRotation + headRotateIncrement)
                NPC.rotation = headRotation;

            NPC.ai[1] += 1f;
            bool fireProjectiles = NPC.ai[1] >= 480f;
            if (fireProjectiles && Vector2.Distance(NPC.Center, player.Center) > 80f)
            {
                int type = ModContent.ProjectileType<HomingLaserDart>();
                float projectileVelocity = death ? 8f : 6f;

                if (NPC.ai[1] >= 600f)
                {
                    NPC.ai[0] += 1f;
                    NPC.ai[1] = 0f;

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        SoundEngine.PlaySound(RavagerHead.MissileSound, NPC.Center);
                        type = ModContent.ProjectileType<RavagerNuke>();
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Normalize(player.Center - NPC.Center) * projectileVelocity * 0.25f, type, RavagerHead.NukeDamage + (provy ? RavagerHead.PostProviNukeBuff : 0), 0f, Main.myPlayer, Main.npc[CalamityGlobalNPC.scavenger].target, 0f);
                    }
                }
            }

            float attackMovementVel = 22f;
            float attackMovementAccel = 0.3f;
            if (death)
            {
                attackMovementVel += 4f;
                attackMovementAccel += 0.05f;
            }
            if (provy)
            {
                attackMovementVel *= 1.25f;
                attackMovementAccel *= 1.25f;
            }

            Vector2 npcCenter = NPC.Center;
            float distanceX = NPC.ai[0] % 2f == 0f ? 480f : -480f;
            float distanceY = fireProjectiles ? -320f : 320f;
            float playerXDistAttack = player.Center.X + (fireProjectiles ? distanceX : 0f) - npcCenter.X;
            float playerYDistAttack = player.Center.Y + distanceY - npcCenter.Y;
            float playerDistanceAttack = (float)Math.Sqrt(playerXDistAttack * playerXDistAttack + playerYDistAttack * playerYDistAttack);
            playerDistanceAttack = attackMovementVel / playerDistanceAttack;
            playerXDistAttack *= playerDistanceAttack;
            playerYDistAttack *= playerDistanceAttack;

            if (NPC.velocity.X < playerXDistAttack)
            {
                NPC.velocity.X += attackMovementAccel;
                if (NPC.velocity.X < 0f && playerXDistAttack > 0f)
                    NPC.velocity.X += attackMovementAccel;
            }
            else if (NPC.velocity.X > playerXDistAttack)
            {
                NPC.velocity.X -= attackMovementAccel;
                if (NPC.velocity.X > 0f && playerXDistAttack < 0f)
                    NPC.velocity.X -= attackMovementAccel;
            }
            if (NPC.velocity.Y < playerYDistAttack)
            {
                NPC.velocity.Y += attackMovementAccel;
                if (NPC.velocity.Y < 0f && playerYDistAttack > 0f)
                    NPC.velocity.Y += attackMovementAccel;
            }
            else if (NPC.velocity.Y > playerYDistAttack)
            {
                NPC.velocity.Y -= attackMovementAccel;
                if (NPC.velocity.Y > 0f && playerYDistAttack < 0f)
                    NPC.velocity.Y -= attackMovementAccel;
            }
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 3; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, hit.HitDirection, -1f, 0, default, 1f);
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Torch, hit.HitDirection, -1f, 0, default, 1f);
            }
            if (NPC.life <= 0)
            {
                if (!Main.dedServ)
                {
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("ScavengerHead").Type, 1f);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("ScavengerHead2").Type, 1f);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("ScavengerHead3").Type, 1f);
                }
                for (int k = 0; k < 20; k++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, hit.HitDirection, -1f, 0, default, 1f);
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Torch, hit.HitDirection, -1f, 0, default, 1f);
                }
            }
        }

        public override bool CheckActive() => false;
    }
}
