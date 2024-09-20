using CalamityMod.Buffs.Potions;
using CalamityMod.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions
{
    public class BoundingPotion : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Potions";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 20;
        }

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 38;
            Item.useTurn = true;
            Item.maxStack = 9999;
            Item.useAnimation = 17;
            Item.useTime = 17;
            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.UseSound = SoundID.Item3;
            Item.consumable = true;
            Item.buffType = ModContent.BuffType<BoundingBuff>();
            Item.buffTime = CalamityUtils.SecondsToFrames(300f);

            Item.value = Item.sellPrice(silver: 2);
            Item.rare = ItemRarityID.Blue;
        }

        public override void AddRecipes()
        {
            CreateRecipe(3).
                AddIngredient(ItemID.BottledWater, 3).
                AddIngredient(ItemID.Frog).
                AddIngredient(ItemID.Moonglow).
                AddTile(TileID.Bottles).
                Register();

            CreateRecipe().
                AddIngredient(ItemID.BottledWater).
                AddIngredient<BloodOrb>(10).
                AddTile(TileID.AlchemyTable).
                Register()
                .DisableDecraft();
        }
    }
}
