using CalamityMod.Projectiles.Summon;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Summon
{
    public class VileFeeder : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Summon";

        public override void SetStaticDefaults() => Item.staff[Type] = true;

        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 36;
            Item.damage = 14;
            Item.DamageType = DamageClass.Summon;
            Item.shoot = ModContent.ProjectileType<VileFeederSummon>();
            Item.knockBack = 0.5f;

            Item.useTime = Item.useAnimation = 30;
            Item.mana = 10;
            Item.noMelee = true;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityGreenBuyPrice;
            Item.rare = ItemRarityID.Green;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.UseSound = SoundID.Item2;

            // This doesn't do anything, it's just so the item is held like a staff.
            Item.shootSpeed = 1f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectileDirect(source, Main.MouseWorld, Main.rand.NextVector2CircularEdge(5f, 5f), type, damage, knockback, player.whoAmI);
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.DemoniteBar, 5).
                AddIngredient(ItemID.ShadowScale, 9).
                AddIngredient(ItemID.Ebonwood, 20).
                AddTile(TileID.Anvils).
                Register();
        }
    }
}
