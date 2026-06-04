using CalamityMod.Projectiles.Rogue;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Rogue
{
    public class Exorcism : RogueWeapon
    {
        public override void SetDefaults()
        {
            Item.width = 109;
            Item.height = 128;
            Item.damage = 222;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            // Due to the amazing sound engine, having multiple crosses embedded in enemies cuts off the sound
            // Therefore this thing has to be very slow (Not that I've ever complained about that before)
            Item.useAnimation = Item.useTime = 96;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 12f;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityLightPurpleBuyPrice;
            Item.rare = ItemRarityID.LightPurple;
            Item.shoot = ModContent.ProjectileType<ExorcismProj>();
            Item.shootSpeed = 6f;
            Item.DamageType = RogueDamageClass.Instance;
        }
        public override float StealthDamageMultiplier => 0.65f;
        public override void HoldItem(Player player)
        {
            player.Calamity().mouseWorldListener = true;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.Calamity().StealthStrikeAvailable())
            {
                int p = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 1f, damage);
                if (p.WithinBounds(Main.maxProjectiles))
                    Main.projectile[p].Calamity().stealthStrike = true;
            }
            else
            {
                Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 0.25f, damage);
            }
            return false;
        }
         
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.HallowedBar, 20).
                AddIngredient(ItemID.SoulofFright, 3).
                AddIngredient(ItemID.SoulofMight, 3).
                AddIngredient(ItemID.SoulofSight, 3).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
