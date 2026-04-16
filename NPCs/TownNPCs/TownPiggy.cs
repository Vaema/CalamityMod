using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace CalamityMod.NPCs.TownNPCs
{
    [AutoloadHead]
    public class TownPiggy : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 8;
            NPCID.Sets.ExtraFramesCount[Type] = 0;
            NPCID.Sets.AttackFrameCount[Type] = 0;
            NPCID.Sets.DangerDetectRange[Type] = 250;
            NPCID.Sets.HatOffsetY[Type] = 6;
            NPCID.Sets.ShimmerTownTransform[Type] = false;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Shimmer] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.NPCFramingGroup[Type] = 8;

            NPCID.Sets.IsTownPet[Type] = true;
            NPCID.Sets.CannotSitOnFurniture[Type] = false;
            NPCID.Sets.TownNPCBestiaryPriority.Add(Type);
            NPCID.Sets.PlayerDistanceWhilePetting[Type] = 32;
            NPCID.Sets.IsPetSmallForPetting[Type] = true;

            NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new()
            {
                Velocity = 0.25f,
            };

            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
        }

        public override void SetDefaults()
        {
            NPC.townNPC = true;
            NPC.friendly = true;
            NPC.width = 20;
            NPC.height = 20;
            NPC.aiStyle = NPCAIStyleID.Passive;
            NPC.damage = 0;
            NPC.defense = 0;
            NPC.lifeMax = 50;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0.5f;
            NPC.housingCategory = 1;
            //AnimationType = NPCID.TownBunny;
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(
            [
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.TownPiggy")
            ]);
        }

        public override bool CanTownNPCSpawn(int numTownNPCs)
        {
            if (CalamityWorld.unlockedTownPig)
            {
                return true;
            }
            return false;
        }

        public override void AI()
        {
            NPC.spriteDirection = NPC.direction;
        }

        public override void FindFrame(int frameHeight)
        {
            if (NPC.velocity.X == 0)
                NPC.frame.Y = 0;
            else
            {
                if (NPC.frameCounter++ % 6 == 0)
                {
                    NPC.frame.Y += frameHeight;
                    if (NPC.frame.Y >= frameHeight * Main.npcFrameCount[Type])
                    {
                        NPC.frame.Y = frameHeight;
                    }
                }
                if (NPC.frame.Y < frameHeight)
                    NPC.frame.Y = frameHeight;
            }
        }

        public override List<string> SetNPCNameList() => new List<string>()
        {
            // Original names
            this.GetLocalizedValue("Name.Curly"),

            // Reference names
            this.GetLocalizedValue("Name.Oolong"), // Dragon Ball
            this.GetLocalizedValue("Name.Napoleon"), // Animal Farm
            this.GetLocalizedValue("Name.Waddles"), // Gravity Falls
            this.GetLocalizedValue("Name.Crenando"), // Ganondorf
            this.GetLocalizedValue("Name.Olivia"), // Olivia
            this.GetLocalizedValue("Name.Wilbur"), // Charlotte's Web
            this.GetLocalizedValue("Name.Pumbaa"), // The Lion King
            this.GetLocalizedValue("Name.Peppa"), // Peppa Pig
            this.GetLocalizedValue("Name.Conan"), // Conan the mighty pig
            this.GetLocalizedValue("Name.Reuben"), // Minecraft: Story Mode
            this.GetLocalizedValue("Name.Porky"), // Looney Tunes
            this.GetLocalizedValue("Name.Hamm"), // Toy Story
            this.GetLocalizedValue("Name.Runt"), // Chicken Little
            this.GetLocalizedValue("Name.Roko"), // Roko's Basilisk
            this.GetLocalizedValue("Name.RichardHamm"), // Pig from Clarkson's Farm named after Richard Hammond
            
            // Dedicated names
        };

        public override string GetChat()
        {
            WeightedRandom<string> chat = new();
            
            for (int i = 0; i <= 2; i++)
            {
                chat.Add(CalamityUtils.GetText("NPCs.TownPiggy.Chat." + i).Value);
            }

            return chat;
        }

        public override void SetChatButtons(ref string button, ref string button2)
        {
            button = Language.GetTextValue("UI.PetTheAnimal");
        }
    }
}
