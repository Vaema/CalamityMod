using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Ores;
using CalamityMod.Rarities;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Armor.Prismatic;

[AutoloadEquip(EquipType.Legs)]
public class PrismaticGreaves : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Armor.PostMoonLord";

    public static float MagicDamageBoost = 0.1f;
    public static int MagicCritBoost = 12;
    public static float NonMagicDamageDecrease = 0.2f;
    public static float FlightTimeBoost = 0.1f;
    public static float JumpSpeedBoost = 0.1f;
    public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MagicDamageBoost.ToPercent(), MagicCritBoost, NonMagicDamageDecrease.ToPercent(), FlightTimeBoost.ToPercent(), JumpSpeedBoost.ToJumpSpeedPercent());

    public override void SetDefaults()
    {
        Item.width = 18;
        Item.height = 18;
        Item.defense = 21;

        Item.value = CalamityGlobalItem.RarityTurquoiseBuyPrice;
        Item.rare = ModContent.RarityType<Turquoise>();
        Item.Calamity().donorItem = true;
    }

    public override void UpdateEquip(Player player)
    {
        player.Calamity().prismaticGreaves = true;
        player.GetDamage<MagicDamageClass>() += MagicDamageBoost;
        player.GetCritChance<MagicDamageClass>() += MagicCritBoost;
        player.jumpSpeedBoost += JumpSpeedBoost;
        player.GetDamage<GenericDamageClass>() -= NonMagicDamageDecrease;
        player.GetDamage<MagicDamageClass>() += NonMagicDamageDecrease;
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<ArmoredShell>(3).
            AddIngredient<ExodiumCluster>(5).
            AddIngredient<DivineGeode>(6).
            AddIngredient(ItemID.Nanites, 300).
            AddTile(TileID.MythrilAnvil).
            Register();
    }
}
