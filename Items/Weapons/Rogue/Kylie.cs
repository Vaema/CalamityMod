using CalamityMod.Projectiles.Rogue;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Rogue
{
    public class Kylie : RogueWeapon
    {
        public static float Speed = 14f;
        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 50;
            Item.damage = 15;
            Item.knockBack = 12;
            Item.DamageType = RogueDamageClass.Instance;
            Item.value = CalamityGlobalItem.RarityBlueBuyPrice;
            Item.rare = ItemRarityID.Blue;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = SoundID.Item1;
            Item.shootSpeed = Speed;
            Item.shoot = ModContent.ProjectileType<KylieBoomerang>();
            Item.noMelee = true;
            Item.noUseGraphic = true;
        }

        // Terraria seems to really dislike high crit values in SetDefaults
        public override void ModifyWeaponCrit(Player player, ref float crit) => crit += 16;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.Calamity().StealthStrikeAvailable())
            {
                int proj = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
                if (proj.WithinBounds(Main.maxProjectiles))
                {
                    Main.projectile[proj].Calamity().stealthStrike = true;
                    Main.projectile[proj].penetrate = 7;
                }
                return false;
            }
            return true;
        }
    }
}
