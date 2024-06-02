using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Typeless;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Fishing.FishingRods
{
    public class VerstaltiteFishingRod : ModItem, ILocalizedModType
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
            Item.fishingPole = 35;
            Item.shootSpeed = 15f;
            Item.shoot = ModContent.ProjectileType<VerstaltiteBobber>();
            Item.value = CalamityGlobalItem.RarityPinkBuyPrice;
            Item.rare = ItemRarityID.Pink;
        }

        public override void ModifyFishingLine(Projectile bobber, ref Vector2 lineOriginOffset, ref Color lineColor)
        {
            lineOriginOffset = new Vector2(43f, -36f);
            lineColor = new Color(95, 158, 160, 100);
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<CryonicBar>(6).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
