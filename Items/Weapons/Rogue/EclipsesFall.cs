using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Rogue;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Rogue
{
    public class EclipsesFall : RogueWeapon
    {
        public override void SetDefaults()
        {
            Item.width = 82;
            Item.height = 88;
            Item.damage = 900;
            Item.knockBack = 3.5f;
            Item.useAnimation = Item.useTime = 24;
            Item.autoReuse = true;
            Item.DamageType = RogueDamageClass.Instance;
            Item.shootSpeed = 32f;
            Item.shoot = ModContent.ProjectileType<EclipsesFallSpear>();

            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.UseSound = SoundID.Item1;
            Item.value = CalamityGlobalItem.RarityDarkBlueBuyPrice;
            Item.rare = ModContent.RarityType<CosmicPurple>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int proj = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            if (proj.WithinBounds(Main.maxProjectiles))
                Main.projectile[proj].Calamity().stealthStrike = player.Calamity().StealthStrikeAvailable();
            return false;
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI) => Item.DrawItemGlowmaskSingleFrame(spriteBatch, rotation, ModContent.Request<Texture2D>(Texture + "Glow").Value);

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<Vega>().
                AddIngredient<AuricBar>(5).
                AddIngredient<DarksunFragment>(15).
                AddTile<CosmicAnvil>().
                Register();
        }
    }
}
