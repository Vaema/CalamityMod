using CalamityMod.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Armor.Brimflame;

[AutoloadEquip(EquipType.Body)]
public class BrimflameRobes : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Armor.Hardmode";

    public static float MagicDamageBoost = 0.07f;
    public static int MagicCritBoost = 7; // NOTE: Tooltip shares this number with damage % as they're equal
    public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MagicDamageBoost.ToPercent());

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
        Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
        Item.rare = ItemRarityID.Lime;
        Item.defense = 16;
    }

    public override void UpdateEquip(Player player)
    {
        player.GetDamage<MagicDamageClass>() += MagicDamageBoost;
        player.GetCritChance<MagicDamageClass>() += MagicCritBoost;
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<UnholyCore>(12).
            AddIngredient<AshesofCalamity>(8).
            AddTile(TileID.MythrilAnvil).
            SortBeforeFirstRecipesOf(ModContent.ItemType<BrimflameBoots>()).
            Register();
    }
}
