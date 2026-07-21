using CalamityMod.Items.Materials;
using CalamityMod.Rarities;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Armor.Bloodflare
{
    [AutoloadEquip(EquipType.Legs)]
    public class BloodflareCuisses : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Armor.PostMoonLord";

        public static float DamageBoost = 0.1f;
        public static int CritBoost = 7;
        public static float MoveSpeedBoost = 0.17f;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(DamageBoost.ToPercent(), CritBoost, MoveSpeedBoost.ToPercent());

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = CalamityGlobalItem.RarityPureGreenBuyPrice;
            Item.defense = 28;
            Item.rare = ModContent.RarityType<PureGreen>();
        }

        public override void UpdateEquip(Player player)
        {
            player.GetDamage<GenericDamageClass>() += DamageBoost;
            player.GetCritChance<GenericDamageClass>() += CritBoost;
            player.moveSpeed += MoveSpeedBoost;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<BloodstoneCore>(14).
                AddIngredient<RuinousSoul>(3).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
