using CalamityMod.Dusts;
using CalamityMod.Items.Placeables.FurnitureMonolith;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Ranged
{
    public class MonolithBow : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged";
        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.PearlwoodBow); // Monolith >= Pearlwood
            Item.width = 24;
            Item.height = 54;
            Item.damage = 19;
            Item.useTime = 5;
            Item.useAnimation = 20;
            Item.useLimitPerAnimation = 4;
            Item.reuseDelay = 25;
            Item.consumeAmmoOnFirstShotOnly = true;
            Item.shootSpeed = 9f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            for (int i = 0; i <= 2; i++)
            {
                Dust dust = Dust.NewDustPerfect(position + velocity * 3, Main.rand.NextBool() ? ModContent.DustType<AstralOrange>() : ModContent.DustType<AstralBlue>(), velocity.RotatedByRandom(MathHelper.ToRadians(10f)) * Main.rand.NextFloat(0.1f, 0.8f), 0, default, Main.rand.NextFloat(1.2f, 1.6f));
                dust.noGravity = true;
            }
            Projectile.NewProjectile(source, position + velocity * 2, velocity.RotatedByRandom(0.165f), type, damage, knockback, player.whoAmI);
            return false;
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<AstralMonolith>(10).
                AddTile(TileID.WorkBenches).
                Register();
        }
    }
}
