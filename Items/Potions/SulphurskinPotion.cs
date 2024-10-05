using CalamityMod.Buffs.Potions;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables;
using CalamityMod.Items.Weapons.Rogue;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions
{
    public class SulphurskinPotion : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Potions";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 20;
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 18;
            Item.useTurn = true;
            Item.maxStack = 9999;
            Item.useAnimation = 17;
            Item.useTime = 17;
            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.UseSound = SoundID.Item3;
            Item.consumable = true;
            Item.buffType = ModContent.BuffType<SulphurskinBuff>();
            Item.buffTime = CalamityUtils.SecondsToFrames(150f);

            Item.value = Item.sellPrice(silver: 2);
            Item.rare = ItemRarityID.Green;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.BottledWater).
                AddIngredient<SulphurousSand>().
                AddIngredient(ItemID.Waterleaf).
                AddTile(TileID.Bottles).
                Register();

            CreateRecipe().
                AddIngredient(ItemID.BottledWater).
                AddIngredient<BloodOrb>(5).
                AddTile(TileID.AlchemyTable).
                Register()
                .DisableDecraft();
        }
    }
}
