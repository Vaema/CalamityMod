using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Ores;
using CalamityMod.Rarities;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Armor.Prismatic;

[AutoloadEquip(EquipType.Body)]
public class PrismaticRegalia : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Armor.PostMoonLord";

    public static float MagicDamageBoost = 0.15f;
    public static int MagicCritBoost = 15; // NOTE: Tooltip shares this number with damage % as they're equal
    public static float NonMagicDamageDecrease = 0.2f;
    public static int RocketChanceDenominator = 20;
    public static float RocketDamageRatio = 0.25f;
    public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MagicDamageBoost.ToPercent(), NonMagicDamageDecrease.ToPercent(), RocketChanceDenominator.GetChanceFromDenominator());

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
        Item.defense = 33;

        Item.value = CalamityGlobalItem.RarityTurquoiseBuyPrice;
        Item.rare = ModContent.RarityType<Turquoise>();
        Item.Calamity().donorItem = true;
    }

    public override void UpdateEquip(Player player)
    {
        player.Calamity().prismaticRegalia = true;
        player.GetDamage<MagicDamageClass>() += MagicDamageBoost;
        player.GetCritChance<MagicDamageClass>() += MagicCritBoost;
        player.GetDamage<GenericDamageClass>() -= NonMagicDamageDecrease;
        player.GetDamage<MagicDamageClass>() += NonMagicDamageDecrease;
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<ArmoredShell>(3).
            AddIngredient<ExodiumCluster>(5).
            AddIngredient<DivineGeode>(8).
            AddIngredient(ItemID.Nanites, 300).
            AddTile(TileID.MythrilAnvil).
            SortBeforeFirstRecipesOf(ModContent.ItemType<PrismaticGreaves>()).
            Register();
    }
}
