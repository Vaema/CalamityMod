using CalamityMod.BiomeManagers;
using CalamityMod.Items.Placeables.Banners;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
using Terraria.DataStructures;
using CalamityMod.Particles;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;

namespace CalamityMod.NPCs.SunkenSea
{
    public class HauntedChum : ModNPC
    {
        public static Texture2D jawTexture;
        public override void SetStaticDefaults()
        {
            if (!Main.dedServ)
            {
                jawTexture = ModContent.Request<Texture2D>("CalamityMod/NPCs/SunkenSea/HauntedChumMouth", AssetRequestMode.ImmediateLoad).Value;
            }
        }
        public override void SetDefaults()
        {
            NPC.width = 36;
            NPC.height = 50;
            NPC.defense = 5;
            NPC.lifeMax = 60;
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.HitSound = SoundID.DD2_SkeletonHurt;
            NPC.DeathSound = SoundID.DD2_SkeletonDeath;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            SpawnModBiomes = new int[1] { ModContent.GetInstance<SunkenSeaBiome>().Type };
        }

        public override void AI()
        {
            int aggroRange = 600;
            if (NPC.direction == 0)
            {
                NPC.TargetClosest();
            }
            Player target = Main.player[NPC.target];
            NPC body = Main.npc[(int)NPC.ai[3]];
            if (body == null || !body.active || body.type != ModContent.NPCType<FesteringRemains>())
            {
                NPC.StrikeInstantKill();
            }
            switch (NPC.ai[0])
            {
                case 0:
                    NPC.velocity.Y = -1;
                    NPC.ai[1]++;
                    if (NPC.ai[1] > 30)
                    {
                        NPC.ai[0] = 1;
                        NPC.ai[1] = 0;
                    }
                    break;
                case 1:
                    if (target != null && target.active && target.Distance(body.Center) < aggroRange)
                    {
                        NPC.ai[0] = 2;
                        NPC.ai[1] = 0;
                        NPC.TargetClosest();
                        Vector2 direction = NPC.DirectionTo(target.Center);
                        direction.SafeNormalize(Vector2.Zero);
                        NPC.velocity = direction * 7;
                    }
                    NPC.TargetClosest();
                    NPC.localAI[0] = MathHelper.Lerp(NPC.localAI[0], -MathHelper.PiOver4, 0.05f);
                    if (NPC.velocity.Length() < 0.25f)
                    {
                        if (NPC.Distance(body.Center) > 320)
                        {
                            Vector2 direction = NPC.DirectionTo(body.Center);
                            direction.SafeNormalize(Vector2.Zero);
                            NPC.velocity = direction * 4;
                        }
                        else
                        {
                            Vector2 direction = Main.rand.NextVector2Circular(30, 30);
                            direction = direction.SafeNormalize(Vector2.Zero);
                            NPC.velocity = direction * 4;
                        }
                    }
                    NPC.velocity *= 0.98f;
                    NPC.direction = NPC.velocity.X > 0 ? 1 : -1;
                    break;
                case 2:
                    if (target == null || !target.active || target.Distance(body.Center) >= aggroRange)
                    {
                        NPC.ai[0] = 1;
                        NPC.ai[1] = 0;
                    }
                    else
                    {
                        NPC.ai[1]++;
                        float dist = target.Distance(NPC.Center);
                        if (dist < 32 || NPC.ai[1] > 50)
                        {
                            NPC.localAI[0] = MathHelper.Lerp(NPC.localAI[0], -MathHelper.PiOver4, 0.3f);
                        }
                        else
                        {
                            NPC.localAI[0] = MathHelper.Lerp(NPC.localAI[0], 0, 0.1f);
                        }
                        if (NPC.ai[1] > Main.rand.Next(55, 76))
                        {
                            NPC.TargetClosest();
                            Vector2 direction = NPC.DirectionTo(target.Center);
                            direction.SafeNormalize(Vector2.Zero);
                            NPC.velocity = direction * 7;
                            NPC.ai[1] = 0;
                        }
                        else
                        {
                            NPC.velocity.X *= 0.99f;
                            NPC.velocity.Y *= 0.999f;
                        }
                    }
                    NPC.direction = NPC.velocity.X > 0 ? 1 : -1;
                    break;
            }
            NPC.spriteDirection = NPC.direction;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Sandnado, hit.HitDirection, -1f, 0, default, 1f);
            }
        }

        public override bool CheckActive()
        {
            return !(Main.npc[(int)NPC.ai[3]].active && Main.npc[(int)NPC.ai[3]].type == ModContent.NPCType<HauntedChum>());
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (NPC.spriteDirection == 1)
                spriteEffects = SpriteEffects.FlipHorizontally;

            Texture2D texture = TextureAssets.Npc[NPC.type].Value;
            Vector2 origin = new Vector2(texture.Width / 2, texture.Height / 2);
            Vector2 jawOrigin = new Vector2(NPC.spriteDirection == 1? jawTexture.Width - 22 : 22, 4);
            Vector2 npcOffset = NPC.Center - screenPos;
            npcOffset -= new Vector2(texture.Width, texture.Height) * NPC.scale / 2f;
            npcOffset += origin * NPC.scale + new Vector2(0f, NPC.gfxOffY);
            spriteBatch.Draw(jawTexture, npcOffset + new Vector2(16 * -NPC.spriteDirection, 4), null, NPC.GetAlpha(drawColor), NPC.localAI[0] * NPC.spriteDirection, jawOrigin, NPC.scale, spriteEffects, 0f);
            spriteBatch.Draw(texture, npcOffset, null, NPC.GetAlpha(drawColor), NPC.rotation, origin, NPC.scale, spriteEffects, 0f);

            return false;
        }
    }
}
