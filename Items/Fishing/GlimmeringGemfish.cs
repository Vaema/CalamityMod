using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Fishing
{
    public class GlimmeringGemfish : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Fishing";
        public static List<int> LootDisplay = new List<int>
        {
            ItemID.Amethyst,
            ItemID.Topaz,
            ItemID.Sapphire,
            ItemID.Emerald,
            ItemID.Ruby,
            ItemID.Diamond,
            ItemID.Amber
        };

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 10;
            ItemID.Sets.CanBePlacedOnWeaponRacks[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 30;
            Item.maxStack = Item.CommonMaxStack;
            Item.consumable = true;
            Item.value = Item.sellPrice(silver: 10);
            Item.rare = ItemRarityID.Green;
        }

        public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
        {
            itemGroup = ContentSamples.CreativeHelper.ItemGroup.GoodieBags;
        }

        public override bool CanRightClick() => true;
        public override void ModifyItemLoot(ItemLoot itemLoot)
        {
            int gemMin = 1;
            int gemMax = 3;

            IItemDropRule[] gems = [
                new CommonDrop(ItemID.Amethyst, 2, gemMin, gemMax),
                new CommonDrop(ItemID.Topaz, 2, gemMin, gemMax),
                new CommonDrop(ItemID.Sapphire, 4, gemMin, gemMax),
                new CommonDrop(ItemID.Emerald, 4, gemMin, gemMax),
                new CommonDrop(ItemID.Ruby, 8, gemMin, gemMax),
                new CommonDrop(ItemID.Diamond, 8, gemMin, gemMax),
                new CommonDrop(ItemID.Amber, 8, gemMin, gemMax),
            ];

            itemLoot.Add(new AlwaysAtleastOneSuccessDropRule(gems));

            // Add Thorium gems if Thorium is loaded.
            Mod thorium = ExternalMods.thorium;
            if (thorium is null)
                return;

            var aquamarine = thorium.Find<ModItem>("Aquamarine");
            if (aquamarine is not null)
                itemLoot.Add(aquamarine.Type, 4, gemMin, gemMax);
            else
                CalamityMod.Log.Warn("Could not find Thorium Aquamarine gem. This item will not be added to Glimmering Gemfish.");

            var opal = thorium.Find<ModItem>("Opal");
            if (opal is not null)
                itemLoot.Add(opal.Type, 4, gemMin, gemMax);
            else
                CalamityMod.Log.Warn("Could not find Thorium Opal gem. This item will not be added to Glimmering Gemfish.");
        }

        public override void ModifyTooltips(List<TooltipLine> list)
        {
            int currentItem = (int)(Main.GlobalTimeWrappedHourly * 1.5f) % LootDisplay.Count;
            list.FindAndReplace("[ITEMS]", $"[i:{LootDisplay[currentItem]}]");
        }
    }
}
