using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Magic;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Magic
{
    public class RecitationoftheBeast : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Magic";
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 54;
            Item.damage = 60;
            Item.mana = 17;
            Item.noMelee = true;
            Item.useAnimation = 22;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useTime = 22;
            Item.knockBack = 8.5f;
            Item.UseSound = SoundID.Item8;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityTurquoiseBuyPrice;
            Item.rare = ModContent.RarityType<Turquoise>();
            Item.shoot = ModContent.ProjectileType<BeastScythe>();
            Item.shootSpeed = 10f;
            Item.DamageType = DamageClass.Magic;
        }

        // Terraria seems to really dislike high crit values in SetDefaults
        public override void ModifyWeaponCrit(Player player, ref float crit) => crit += 20;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            for (int i = 0; i < 6; i++)
            {
                Vector2 circleVel = (MathHelper.TwoPi * i / 6f + velocity.ToRotation()).ToRotationVector2() * 2.2f;
                Projectile.NewProjectile(source, player.Center, circleVel, type, damage, knockback, Main.myPlayer);
            }
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.DemonScythe).
                AddIngredient<BloodstoneCore>(6).
                AddIngredient<EssenceofHavoc>(6).
                AddTile(TileID.Bookcases).
                Register();
        }
    }
}
