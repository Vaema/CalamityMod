using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Ores;
using CalamityMod.Projectiles.Ranged;
using CalamityMod.Projectiles.Typeless;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Ranged
{
    [LegacyName("StarfleetMK2")]
    public class Starmada : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged";
        public override void SetDefaults()
        {
            Item.width = 122;
            Item.height = 50;
            Item.damage = 150;
            Item.DamageType = DamageClass.Ranged;
            Item.useAnimation = Item.useTime = 15;
            Item.knockBack = 15f;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.UseSound = SoundID.Item92;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<PlasmaBlast>();
            Item.shootSpeed = 16f;
            Item.useAmmo = AmmoID.FallenStar;
            Item.rare = ModContent.RarityType<CosmicPurple>();
            Item.value = CalamityGlobalItem.RarityDarkBlueBuyPrice;
        }

        public override bool CanConsumeAmmo(Item ammo, Player player) => Main.rand.NextBool(3);

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            for (int i = 0; i < 5; i++)
            {
                int starType = Utils.SelectRandom(Main.rand,
                [
                    ModContent.ProjectileType<PlasmaBlast>(),
                    ModContent.ProjectileType<AstralStar>(),
                    ProjectileID.StarCannonStar,
                    ProjectileID.Starfury
                ]);
                int star = Projectile.NewProjectile(source, position + Main.rand.NextVector2Circular(21f, 21f), velocity * Main.rand.NextFloat(0.8f, 1.2f), starType, damage, knockback, player.whoAmI);
                if (star.WithinBounds(Main.maxProjectiles))
                {
                    Main.projectile[star].penetrate = 1;
                    Main.projectile[star].timeLeft = 300;
                    Main.projectile[star].DamageType = DamageClass.Ranged;
                    Main.projectile[star].netUpdate = true;
                }
            }
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<Starfleet>().
                AddIngredient<StarSputter>().
                AddIngredient<CosmiliteBar>(8).
                AddIngredient<DarksunFragment>(8).
                AddIngredient<ExodiumCluster>(15).
                AddTile<CosmicAnvil>().
                Register();
        }
    }
}
