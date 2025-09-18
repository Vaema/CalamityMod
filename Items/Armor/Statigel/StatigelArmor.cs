using CalamityMod.Items.Materials;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Armor.Statigel
{
    [AutoloadEquip(EquipType.Body)]
    public class StatigelArmor : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Armor.PreHardmode";

        public static int CritBoost = 5;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(CritBoost);

        // Common Set Bonus
        public static float SetBonusJumpSpeedBoost = 0.6f;
        public static float SetBonusJumpHeightPercentBoost = 0.3334f;

        public override void SetStaticDefaults()
        {
            if (Main.dedServ)
                return;

            int equipSlot = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Body);

            ArmorIDs.Body.Sets.HidesTopSkin[equipSlot] = true;
            ArmorIDs.Body.Sets.HidesArms[equipSlot] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = CalamityGlobalItem.RarityLightRedBuyPrice;
            Item.rare = ItemRarityID.LightRed;
            Item.defense = 10;
        }

        public override void UpdateEquip(Player player) => player.GetCritChance<GenericDamageClass>() += CritBoost;

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<PurifiedGel>(12).
                AddIngredient<BlightedGel>(12).
                AddTile(TileID.Solidifier).
                Register();
        }
    }
}
