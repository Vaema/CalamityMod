using System.Collections.Generic;
using CalamityMod.CalPlayer;
using CalamityMod.Items.Fishing.AstralCatches;
using CalamityMod.Items.Fishing.BrimstoneCragCatches;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Abyss;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    internal class CosmiliteBobber : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Fishing";
        public override string Texture => "CalamityMod/Projectiles/Typeless/DevourerofCodsBobber";      
        public override void SetDefaults()
        {
            Item.width = 9;
            Item.height = 9;
            Item.value = CalamityGlobalItem.RarityDarkBlueBuyPrice;
            Item.rare = ModContent.RarityType<CosmicPurple>();
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.fishingSkill += 10;
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.FishingBobber).
                AddIngredient<CosmiliteBar>(2).
                AddTile<CosmicAnvil>().
                Register();
        }
    }
}
