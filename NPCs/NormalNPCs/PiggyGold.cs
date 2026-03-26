using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace CalamityMod.NPCs.NormalNPCs
{
    public class PiggyGold : Piggy
    {
        public override void SetStaticDefaults()
        {
            NPCID.Sets.GoldCrittersCollection.Add(Type);
            NPCID.Sets.NormalGoldCritterBestiaryPriority.Insert(NPCID.Sets.NormalGoldCritterBestiaryPriority.IndexOf(NPCID.GoldBunny) + 1, Type);
            base.SetStaticDefaults();
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            // All gold critters have the same Bestiary entry.
            var flavorText = database.FindEntryByNPCID(NPCID.GoldBunny).Info.Where(info => info is FlavorTextBestiaryInfoElement).FirstOrDefault();
            bestiaryEntry.AddTags(
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.DayTime,
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface, 
                flavorText);
        }

        public override void AI()
        {
            base.AI();
            NPC.ProduceGoldCritterDust();
        }
    }
}
