using CalamityMod.DataStructures;
using CalamityMod.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.SunkenSea
{
    public class PolyperilTentacle : SunkenSeaNPC
    {
        public ref float ParentIndex => ref NPC.ai[0];

        public ref float Timer => ref NPC.ai[1];

        public List<VerletSimulatedSegment> Segments;

        // Where the tentacle will hover when not attacking
        public Vector2 anchor;

        // They attack everything
        protected override List<int> PreyIDs => new List<int>();
        
        protected override List<int> PredatorIDs => new List<int>();

        // Tentacles are not a natural enemy, they are manually spawned by Polyperils
        protected override SunkenSeaBiomeFlags BiomeDesignation => SunkenSeaBiomeFlags.None;

        public override LocalizedText DisplayName => CalamityUtils.GetText("NPCs.Polyperil.DisplayName");

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            base.SetDefaults();

            NPC.npcSlots = 0f;
            NPC.damage = 20;
            NPC.lifeMax = 200;
            NPC.defense = 0;
            NPC.knockBackResist = 0f;
            NPC.chaseable = false;
            NPC.dontTakeDamage = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;

            NPC.aiStyle = -1;
            AIType = -1;
            NPC.width = 20;
            NPC.height = 20;

            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            // Banner = NPC.type;
            // BannerItem = ModContent.ItemType<PolyperilBanner>();
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(anchor.X);
            writer.Write(anchor.Y);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            anchor = new Vector2(reader.ReadSingle(), reader.ReadSingle());
        }

        public override void AI()
        {
            NPC parent = Main.npc[(int)ParentIndex];
            if (parent == null || !parent.active || parent.life < 0 || parent.type != ModContent.NPCType<Polyperil>())
            {
                NPC.active = false;
                return;
            }

            // Create a chain
            if (Segments == null || Segments.Count < 10)
            {
                Segments = new List<VerletSimulatedSegment>(10);
                for (int i = 0; i < 10; i++)
                {
                    VerletSimulatedSegment segment = new VerletSimulatedSegment(parent.Center + Vector2.UnitY * i * 10);
                    Segments.Add(segment);
                }

                Segments[0].locked = true;
                Segments[^1].locked = true;
            }
            Entity targ = CurrentPrey != null ? CurrentPrey : CurrentPlayer;
            // While aggro'd, launch at the target then retreat
            if (targ != null)
            {
                // Remain stationary while at rest
                if (Timer < 0)
                {
                    NPC.velocity = Vector2.Zero;
                }
                // Launch
                if (Timer == 0)
                {
                    NPC.velocity = NPC.SafeDirectionTo(targ.Center) * 8;
                    SoundEngine.PlaySound(SoundID.Item152 with { Volume = 1.5f, Pitch = 0.4f }, NPC.Center);
                }
                // Retreat
                if (Timer > 20)
                {
                    NPC.velocity = NPC.SafeDirectionTo(anchor) * 5;
                    if (Timer == 21)
                    {
                        RecalculatePosition(parent.Center);
                    }
                }
                // Reset timer
                if (Timer > 30 && NPC.Distance(anchor) < 20)
                {
                    NPC.velocity = Vector2.Zero;
                    Timer = -120;
                }
            }
            // Otherwise hover around the anchor position
            else
            {
                NPC.velocity = NPC.SafeDirectionTo(anchor) * 3;
                // Randomly relocate position
                if (Timer > 300 && Main.rand.NextBool(180))
                {
                    RecalculatePosition(parent.Center);
                    Timer = 0;
                }
            }
            // If the anchor is too far from the parent (usually happens if the parent is moved) recalculate the anchor position
            if (anchor.Distance(parent.Center) > 200)
            {
                RecalculatePosition(parent.Center);
            }

            // Repel other tentacles
            float SAImovement = 0.05f;
            for (int k = 0; k < Main.maxNPCs; k++)
            {
                NPC otherTentacle = Main.npc[k];
                // Short circuits to make the loop as fast as possible
                if (!otherTentacle.active || k == NPC.whoAmI || (otherTentacle.type != ModContent.NPCType<PolyperilTentacle>()))
                    continue;

                float taxicabDist = Math.Abs(NPC.position.X - otherTentacle.position.X) + Math.Abs(NPC.position.Y - otherTentacle.position.Y);
                if (taxicabDist < NPC.width * 1.5f)
                {
                    if (NPC.position.X < otherTentacle.position.X)
                        NPC.velocity.X -= SAImovement;
                    else
                        NPC.velocity.X += SAImovement;

                    if (NPC.position.Y < otherTentacle.position.Y)
                        NPC.velocity.Y -= SAImovement;
                    else
                        NPC.velocity.Y += SAImovement;
                }
            }
            NPC.rotation = NPC.DirectionTo(parent.Center).ToRotation();

            // Update the chain
            Segments[0].oldPosition = Segments[0].position;
            Segments[0].position = parent.Center;

            Segments[^1].oldPosition = Segments[^1].position;
            Segments[^1].position = parent.active ? NPC.Center : parent.Center;

            Segments = VerletSimulatedSegment.SimpleSimulation(Segments, 5, loops: 1, gravity: 0.3f);

            NPC.netUpdate = true;
            NPC.netSpam = 0;

            Timer++;
        }

        public void RecalculatePosition(Vector2 basePos, int dist = 50)
        {
            anchor = basePos + new Vector2(Main.rand.Next(-dist, dist), Main.rand.Next(-dist, dist));
        }

        protected override bool PlayerSearchFilter(Player p)
        {
            return base.PlayerSearchFilter(p) || p == CurrentPlayer && Vector2.DistanceSquared(NPC.Center, p.Center) < 460f * 460f;
        }

        protected override bool NPCSearchFilter(NPC n)
        {
            // The tentacles go after EVERYTHING
            return (Vector2.DistanceSquared(NPC.Center, n.Center) < 660f * 660f && n.type != NPC.type && n.type != ModContent.NPCType<Polyperil>() && n.type != ModContent.NPCType<PolypPanasea>());
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // Kelpie Seadragons have high resistance to Polyperils as they eat them
            if (target.type == ModContent.NPCType<KelpieSeadragon>())
            {
                modifiers.SourceDamage *= 0.05f;
            }
        }

        public override void ModifyTypeName(ref string typeName)
        {
            NPC parent = Main.npc[(int)ParentIndex];
            if (parent == null || !parent.active || parent.life < 0 || parent.type != ModContent.NPCType<Polyperil>())
            {
                return;
            }
            if (parent.ai[1] == 3)
            {
                typeName = CalamityUtils.GetTextValue("NPCs.RadiantPolyperil");
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            NPC parent = Main.npc[(int)ParentIndex];
            if (parent == null || !parent.active || parent.life < 0 || parent.type != ModContent.NPCType<Polyperil>())
            {
                return false;
            }

            // Color is based on parent color
            Color col = parent.ai[1] switch
            {
                1 => new Color(27, 190, 255),
                2 => new Color(7, 255, 180),
                3 => new Color(122, 255, 240),
                _ => new Color(255, 140, 248)
            };

            // Desaturate as parent's health lowers
            float vibrance = MathHelper.Lerp(1, 0, Utils.GetLerpValue(1, 0.33f, parent.life / (float)parent.lifeMax, true));
            Color final = Color.Lerp(Color.White, col, vibrance);

            Texture2D tex = TextureAssets.Npc[Type].Value;
            Rectangle tip = new Rectangle(0, 0, 8, 10);
            Rectangle chain = new Rectangle(0, 12, 8, 8);
            
            if (Segments == null || Segments.Count <= 0)
            {
                spriteBatch.Draw(tex, NPC.Center - screenPos, tip, drawColor.MultiplyRGB(final), NPC.rotation, new Vector2(4, 10), NPC.scale, SpriteEffects.None, 0);
                return false;
            }

            // Draw tentacle chains
            for (int i = 0; i < Segments.Count - 1; i++)
            {
                VerletSimulatedSegment seg = Segments[i];
                float dist = i > 0 ? Vector2.Distance(seg.position, Segments[i - 1].position) : 0;
                if (dist <= 2)
                    dist = 2;
                dist += 4;
                if (i == Segments.Count - 1)
                {
                    dist = Vector2.Distance(seg.position, parent.Center) + 2;
                }
                float rot = 0f;
                if (i > 0)
                    rot = seg.position.DirectionTo(Segments[i - 1].position).ToRotation();
                SpriteEffects dir = NPC.Center.X > parent.Center.X ? SpriteEffects.FlipVertically : SpriteEffects.None;
                spriteBatch.Draw(tex, seg.position - screenPos, chain, drawColor.MultiplyRGB(final), rot + MathHelper.PiOver2, new Vector2(4, 4), new Vector2(1, dist / 8), dir, 0);
            }            

            // Draw the tip
            spriteBatch.Draw(tex, NPC.Center - screenPos, tip, drawColor.MultiplyRGB(final), NPC.rotation - MathHelper.PiOver2, new Vector2(4, 5), NPC.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}
