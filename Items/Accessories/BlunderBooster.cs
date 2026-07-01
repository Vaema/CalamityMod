using System.Collections.Generic;
using CalamityMod.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    public class BlunderBooster : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 38;
            Item.value = CalamityGlobalItem.RarityPurpleBuyPrice;
            Item.rare = ItemRarityID.Purple;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.Calamity().rogueDashItem = Item;
            player.GetDamage<ThrowingDamageClass>() += 0.12f;
            player.Calamity().rogueVelocity += 0.15f;
            player.Calamity().blunderBooster = true;
            player.Calamity().blunderBoosterVisibility = !hideVisual;
            player.Calamity().stealthGenStandstill += 0.1f;
            player.Calamity().stealthGenMoving += 0.1f;
        }

        public override void ModifyTooltips(List<TooltipLine> list) => list.IntegrateDynamicHotkey(Item);

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<PlaguedFuelPack>().
                AddIngredient<EffulgentFeather>(8).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
