using CalamityMod.Items.Placeables.FurnitureMonolith;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Armor.TitanHeart;

[AutoloadEquip(EquipType.Body)]
public class TitanHeartMantle : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Armor.Hardmode";

    public static float RogueDamageBoost = 0.1f;
    public static float RogueKnockbackBoost = 0.5f;
    public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(RogueDamageBoost.ToPercent(), RogueKnockbackBoost.ToPercent());

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
        Item.defense = 14;
    }

    public override void UpdateEquip(Player player)
    {
        player.Calamity().titanHeartMantle = true;
        player.GetDamage<ThrowingDamageClass>() += RogueDamageBoost;
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<AstralMonolith>(20).
            AddIngredient<Materials.TitanHeart>().
            AddTile(TileID.Anvils).
            SortBeforeFirstRecipesOf(ModContent.ItemType<TitanHeartBoots>()).
            Register();
    }
}
