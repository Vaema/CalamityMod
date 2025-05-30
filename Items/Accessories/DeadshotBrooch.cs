using System.Collections.Generic;
using CalamityMod.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    [LegacyName("DaedalusEmblem")]
    public class DeadshotBrooch : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";
        public override void SetDefaults()
        {
            Item.width = 46;
            Item.height = 40;
            Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
            Item.rare = ItemRarityID.Lime;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.Calamity().deadshotBrooch = true;
            player.Calamity().ammoCost *= 0.8f;
            player.GetDamage<RangedDamageClass>() += 0.12f;
            player.GetCritChance<RangedDamageClass>() += 7;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips) => tooltips.IntegrateHotkey(CalamityKeybinds.AmmoCycleHotkey);

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.RangerEmblem).
                AddIngredient<CoreofCalamity>(2).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
