using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Crags;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    public class ArchaicPowder : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";
        public override void SetDefaults()
        {
            Item.width = 56;
            Item.height = 34;
            Item.value = CalamityGlobalItem.RarityOrangeBuyPrice;
            Item.rare = ItemRarityID.Orange;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.pickSpeed -= 0.25f;
            player.Calamity().aPowder = true;
            player.Calamity().fallingBlockProtection = true;
            player.Calamity().trapProtection = true;

            // Doesn't stack with downgrades
            if (player.chiselSpeed)
                player.pickSpeed += 0.15f;
            if (player.Calamity().aFossil)
                player.pickSpeed += 0.1f;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<AncientFossil>().
                AddIngredient(ItemID.AncientChisel).
                AddIngredient<AncientBoneDust>(3).
                AddIngredient<ScorchedBone>(10).
                AddIngredient(ItemID.Bone, 15).
                AddTile(TileID.Anvils).
                Register();
        }
    }
}
