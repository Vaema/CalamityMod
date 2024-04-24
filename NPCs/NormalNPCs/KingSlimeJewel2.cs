using CalamityMod.CalPlayer;
using CalamityMod.Events;
using CalamityMod.Projectiles.Boss;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.NormalNPCs
{
    public class KingSlimeJewel2 : ModNPC
    {
        public override string Texture => "CalamityMod/NPCs/NormalNPCs/KingSlimeJewel";

        private const int BuffDustGateValue = 60;
        private const float LightTelegraphDuration = 45f;

        public override void SetStaticDefaults()
        {
            NPCID.Sets.NPCBestiaryDrawModifiers bestiaryData = new NPCID.Sets.NPCBestiaryDrawModifiers() { Hide = true };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, bestiaryData);
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.damage = 10;
            NPC.width = 22;
            NPC.height = 22;
            NPC.defense = 5;
            NPC.DR_NERD(0.05f);

            NPC.lifeMax = 140;
            double HPBoost = CalamityConfig.Instance.BossHealthBoost * 0.01;
            NPC.lifeMax += (int)(NPC.lifeMax * HPBoost);

            NPC.knockBackResist = 0.9f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit5;
            NPC.DeathSound = SoundID.NPCDeath15;
            NPC.Calamity().VulnerableToSickness = false;
        }

        public override void AI()
        {
            // Setting this in SetDefaults will disable expert mode scaling, so put it here instead
            NPC.damage = 0;

            // Despawn
            if (!CalamityPlayer.areThereAnyDamnBosses)
            {
                NPC.life = 0;
                NPC.HitEffect();
                NPC.active = false;
                NPC.netUpdate = true;
                return;
            }

            Lighting.AddLight(NPC.Center, 0f, 0f, 0.8f);

            // Float around the player
            NPC.rotation = NPC.velocity.X / 15f;

            NPC.TargetClosest();

            float velocity = 5f;
            float acceleration = 0.1f;

            if (NPC.position.Y > Main.player[NPC.target].position.Y - 300f)
            {
                if (NPC.velocity.Y > 0f)
                    NPC.velocity.Y *= 0.98f;

                NPC.velocity.Y -= acceleration;

                if (NPC.velocity.Y > velocity)
                    NPC.velocity.Y = velocity;
            }
            else if (NPC.position.Y < Main.player[NPC.target].position.Y - 400f)
            {
                if (NPC.velocity.Y < 0f)
                    NPC.velocity.Y *= 0.98f;

                NPC.velocity.Y += acceleration;

                if (NPC.velocity.Y < -velocity)
                    NPC.velocity.Y = -velocity;
            }

            if (NPC.Center.X > Main.player[NPC.target].Center.X + 300f)
            {
                if (NPC.velocity.X > 0f)
                    NPC.velocity.X *= 0.98f;

                NPC.velocity.X -= acceleration;

                if (NPC.velocity.X > 8f)
                    NPC.velocity.X = 8f;
            }
            if (NPC.Center.X < Main.player[NPC.target].Center.X - 300f)
            {
                if (NPC.velocity.X < 0f)
                    NPC.velocity.X *= 0.98f;

                NPC.velocity.X += acceleration;

                if (NPC.velocity.X < -8f)
                    NPC.velocity.X = -8f;
            }

            // Emit buff dust
            NPC.ai[0] += 1f;
            if (NPC.ai[0] >= BuffDustGateValue)
            {
                NPC.ai[0] = 0f;

                for (int dusty = 0; dusty < 10; dusty++)
                {
                    Vector2 dustVel = Main.rand.NextVector2CircularEdge(5f, 5f);
                    int sapphire = Dust.NewDust(NPC.Center, NPC.width, NPC.height, DustID.GemSapphire, 0f, 0f, 100, default, 2f);
                    Main.dust[sapphire].velocity = dustVel * Main.rand.NextFloat(1f, 2f);
                    Main.dust[sapphire].noGravity = true;
                    if (Main.rand.NextBool())
                    {
                        Main.dust[sapphire].scale = 0.5f;
                        Main.dust[sapphire].fadeIn = 1f + Main.rand.Next(10) * 0.1f;
                    }
                }

                SoundEngine.PlaySound(SoundID.Item8, NPC.Center);

                NPC.netUpdate = true;
            }
        }

        public override Color? GetAlpha(Color drawColor)
        {
            Color initialColor = new Color(0, 0, 150);
            Color newColor = initialColor;
            Color finalColor = new Color(125, 125, 255);
            float colorTelegraphGateValue = BuffDustGateValue - LightTelegraphDuration;
            if (NPC.ai[0] > colorTelegraphGateValue)
                newColor = Color.Lerp(initialColor, finalColor, (NPC.ai[0] - colorTelegraphGateValue) / LightTelegraphDuration);
            newColor.A = (byte)(255 * NPC.Opacity);

            return newColor;
        }

        public override bool CheckActive() => false;

        public override void HitEffect(NPC.HitInfo hit)
        {
            int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.GemSapphire, hit.HitDirection, -1f, 0, default, 1f);
            Main.dust[dust].noGravity = true;

            if (NPC.life <= 0)
            {
                NPC.position = NPC.Center;
                NPC.width = NPC.height = 45;
                NPC.position.X = NPC.position.X - (NPC.width / 2);
                NPC.position.Y = NPC.position.Y - (NPC.height / 2);

                for (int i = 0; i < 2; i++)
                {
                    int rubyDust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.GemSapphire, 0f, 0f, 100, default, 2f);
                    Main.dust[rubyDust].noGravity = true;
                    Main.dust[rubyDust].velocity *= 3f;
                    if (Main.rand.NextBool())
                    {
                        Main.dust[rubyDust].scale = 0.5f;
                        Main.dust[rubyDust].fadeIn = 1f + Main.rand.Next(10) * 0.1f;
                    }
                }

                for (int j = 0; j < 10; j++)
                {
                    int rubyDust2 = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.GemSapphire, 0f, 0f, 100, default, 3f);
                    Main.dust[rubyDust2].noGravity = true;
                    Main.dust[rubyDust2].velocity *= 5f;
                    rubyDust2 = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.GemSapphire, 0f, 0f, 100, default, 2f);
                    Main.dust[rubyDust2].noGravity = true;
                    Main.dust[rubyDust2].velocity *= 2f;
                }
            }
        }
    }
}
