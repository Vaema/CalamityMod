using CalamityMod.CalPlayer;
using CalamityMod.Items.Materials;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Armor.Demonshade
{
    [AutoloadEquip(EquipType.Body)]
    public class DemonshadeBreastplate : ModItem, IDrawArmOverShoulderpad, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Armor.PostMoonLord";
        public string FrontArmTexture => "CalamityMod/Items/Armor/Demonshade/DemonshadeBreastplate_Arms";

        public static int MaxManaBoost = 200;
        public static float AmmoReduction = 0.7f;
        public static float DamageBoost = 0.15f;
        public static int CritBoost = 15; // NOTE: Tooltip shares this number with damage % as they're equal
        public static float MeleeSpeedBoost = 0.25f;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MaxManaBoost, DamageBoost.ToPercent(), MeleeSpeedBoost.ToPercent(), (1f - AmmoReduction).ToPercent());

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.defense = 50;
            Item.value = CalamityGlobalItem.RarityHotPinkBuyPrice;
            Item.rare = ModContent.RarityType<HotPink>();
            Item.Calamity().devItem = true;
        }

        public override void UpdateEquip(Player player)
        {
            var modPlayer = player.Calamity();
            modPlayer.shadeRegen = true;
            modPlayer.ammoCost *= AmmoReduction;
            player.statManaMax2 += MaxManaBoost;
            player.GetDamage<GenericDamageClass>() += DamageBoost;
            player.GetCritChance<GenericDamageClass>() += CritBoost;
            player.GetAttackSpeed<MeleeDamageClass>() += MeleeSpeedBoost;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<ShadowspecBar>(18).
                AddTile<DraedonsForge>().
                Register();
        }
    }
}
