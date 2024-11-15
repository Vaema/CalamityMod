using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Systems.Collections
{
    public sealed class BossValueDict : ModSystem
    {
        public static IDictionary<int, int> Dict { get; private set; }

        public override void OnModLoad()
        {
            // NOTE: This does not account for Calamity's base value increases
            Dict = new SortedDictionary<int, int>
            {
                { NPCID.KingSlime, Item.buyPrice(0, 2) },
                { NPCID.EyeofCthulhu, Item.buyPrice(0, 2) },
                // Evil bosses drop 5 gold in vanilla; unmodified
                { NPCID.QueenBee, Item.buyPrice(0, 8) },
                { NPCID.Deerclops, Item.buyPrice(0, 8) },
                { NPCID.SkeletronHead, Item.buyPrice(0, 12) },
                { NPCID.WallofFlesh, Item.buyPrice(0, 12) },
                { NPCID.QueenSlimeBoss, Item.buyPrice(0, 16) },
                { NPCID.Spazmatism, Item.buyPrice(0, 16) },
                { NPCID.Retinazer, Item.buyPrice(0, 16) },
                { NPCID.TheDestroyer, Item.buyPrice(0, 16) },
                { NPCID.SkeletronPrime, Item.buyPrice(0, 16) },
                { NPCID.Plantera, Item.buyPrice(0, 20) },
                { NPCID.Golem, Item.buyPrice(0, 25) },
                { NPCID.HallowBoss, Item.buyPrice(0, 30) },
                { NPCID.DukeFishron, Item.buyPrice(0, 30) },
                { NPCID.CultistBoss, Item.buyPrice(0, 50) }
                // Moon Lord drops 1 plat in vanilla; unmodified
            };
        }

        public override void Unload()
        {
            Dict?.Clear();
            Dict = null;
        }

        public static bool TryGet(int npcType, out int value)
        {
            return Dict.TryGetValue(npcType, out value);
        }
    }
}
