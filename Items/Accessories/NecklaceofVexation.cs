using System.Collections.Generic;
using CalamityMod.CalPlayer;
using CalamityMod.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    public class NecklaceofVexation : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";
        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 34;
            Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
            Item.rare = ItemRarityID.Lime;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            CalamityPlayer modPlayer = player.Calamity();
            modPlayer.vexation = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.AvengerEmblem).
                AddIngredient<PerennialBar>(6).
                AddTile(TileID.MythrilAnvil).
                Register();
        }

        public override void ModifyTooltips(List<TooltipLine> list)
        {
            var player = Main.LocalPlayer;
            if (player != null)
                list.FindAndReplace("[DAMAGE]", (0.3f * (1 - player.statLife / (float)player.statLifeMax2)).ToPercent());
        }
    }
}
