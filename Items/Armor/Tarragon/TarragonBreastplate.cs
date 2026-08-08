using CalamityMod.Items.Materials;
using CalamityMod.Rarities;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Armor.Tarragon
{
    [AutoloadEquip(EquipType.Body)]
    public class TarragonBreastplate : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Armor.PostMoonLord";

        public static float DamageBoost = 0.12f;
        public static int CritBoost = 8;
        public static int RegenBoost = 4;
        public static float DamageReductionBoost = 0.1f;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(DamageBoost.ToPercent(), CritBoost, RegenBoost.ToRegenPerSecond(), DamageReductionBoost.ToPercent());

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = CalamityGlobalItem.RarityTurquoiseBuyPrice;
            Item.defense = 36;
            Item.rare = ModContent.RarityType<Turquoise>();
        }

        public override void UpdateEquip(Player player)
        {
            player.GetDamage<GenericDamageClass>() += DamageBoost;
            player.GetCritChance<GenericDamageClass>() += CritBoost;
            player.lifeRegen += RegenBoost;
            player.endurance += DamageReductionBoost;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<UelibloomBar>(24).
                AddIngredient<DivineGeode>(18).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
