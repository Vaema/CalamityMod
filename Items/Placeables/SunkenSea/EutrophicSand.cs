using CalamityMod.Items.Placeables.Walls;
using CalamityMod.Projectiles.Typeless;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.SunkenSea
{
    public class EutrophicSand : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
            ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<PolypSand>();

            // -5 flat damage
            ItemID.Sets.SandgunAmmoProjectileData[Type] = new(ModContent.ProjectileType<EutrophicSandBallGun>(),-5);
        }

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.SunkenSea.EutrophicSand>());
            Item.ammo = AmmoID.Sand;
            Item.notAmmo = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<EutrophicSandWallSafe>(4).
                AddTile(TileID.WorkBenches).
                Register();
        }
    }
}
