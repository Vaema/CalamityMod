using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Typeless;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Fishing.FishingRods
{
    public class WulfrumRod : ModItem, ILocalizedModType
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
            Item.rare = ItemRarityID.Blue;
            Item.fishingPole = 10;
            Item.shootSpeed = 10f;
            Item.shoot = ModContent.ProjectileType<WulfrumBobber>();
            Item.value = CalamityGlobalItem.RarityBlueBuyPrice;
        }

        public override void ModifyFishingLine(Projectile bobber, ref Vector2 lineOriginOffset, ref Color lineColor)
        {
            lineOriginOffset = new Vector2(35f, -27f);
            lineColor = new Color(200, 200, 200, 100);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<WulfrumMetalScrap>(6).
                AddTile(TileID.Anvils).
                Register();
        }
    }
}
