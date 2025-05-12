using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Ores;
using CalamityMod.Projectiles.Ranged;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Ranged
{
    public class Starfleet : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged";
        public override void SetDefaults()
        {
            Item.width = 76;
            Item.height = 36;
            Item.damage = 115; 
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 55;
            Item.useAnimation = 55;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 15f;
            Item.value = CalamityGlobalItem.RarityPureGreenBuyPrice;
            Item.rare = ModContent.RarityType<PureGreen>();
            Item.UseSound = SoundID.Item92;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<PlasmaBlast>();
            Item.shootSpeed = 12f;
            Item.useAmmo = AmmoID.FallenStar;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-8, -11);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            for (int index = 0; index < 5; ++index)
            {
                float SpeedX = velocity.X + Main.rand.Next(-40, 41) * 0.05f;
                float SpeedY = velocity.Y + Main.rand.Next(-40, 41) * 0.05f;
                Projectile.NewProjectile(source, position.X, position.Y, SpeedX, SpeedY, type, damage, knockback, player.whoAmI);
            }
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.SuperStarCannon).
                AddIngredient<Lumenyl>(6).
                AddIngredient<RuinousSoul>(4).
                AddIngredient<ExodiumCluster>(12).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
