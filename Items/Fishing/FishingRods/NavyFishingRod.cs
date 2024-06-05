using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables;
using CalamityMod.Projectiles.Typeless;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Fishing.FishingRods
{
    public class NavyFishingRod : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Fishing";
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 28;
            Item.useAnimation = 8;
            Item.useTime = 8;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = SoundID.Item1;
            Item.fishingPole = 20;
            Item.shootSpeed = 13f;
            Item.shoot = ModContent.ProjectileType<NavyBobber>();
            Item.value = CalamityGlobalItem.RarityGreenBuyPrice;
            Item.rare = ItemRarityID.Green;
        }

        public override void ModifyFishingLine(Projectile bobber, ref Vector2 lineOriginOffset, ref Color lineColor)
        {
            lineOriginOffset = new Vector2(56f, -37f);
            lineColor = new Color(36, 61, 111, 100);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<PearlShard>().
                AddIngredient<SeaPrism>(5).
                AddIngredient<Navystone>(8).
                AddTile(TileID.Anvils).
                Register();
        }
    }
}
