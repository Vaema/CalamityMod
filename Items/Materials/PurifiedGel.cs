using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Materials
{
    public class PurifiedGel : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Materials";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 25;
            ItemID.Sets.SortingPriorityMaterials[Type] = 71; // Soul of Light
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 36;
            Item.maxStack = Item.CommonMaxStack;
            Item.value = Item.sellPrice(silver: 5);
            Item.rare = ItemRarityID.LightRed;
        }
    }
}
