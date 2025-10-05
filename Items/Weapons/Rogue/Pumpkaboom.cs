using System;
using System.Collections.Generic;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.SunkenSea;
using CalamityMod.Projectiles.Rogue;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Rogue
{
    public class Pumpkaboom : RogueWeapon
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 60;
            Item.damage = 20;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.useTime = Item.useAnimation = 70;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 2f;
            Item.UseSound = null;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityGreenBuyPrice;
            Item.rare = ItemRarityID.Green;
            Item.shoot = ModContent.ProjectileType<PumpkaboomSmall>();
            Item.shootSpeed = 14f;
            Item.DamageType = RogueDamageClass.Instance;
        }

        public override float StealthDamageMultiplier => 1f;

        public override bool CanUseItem(Player player)
        {
            for (int x = 0; x < Main.maxProjectiles; x++)
            {
                Projectile projectile = Main.projectile[x];
                if (projectile.active && projectile.type == Item.shoot && projectile.localAI[1] < 5 && projectile.owner == player.whoAmI)
                {
                    return false;
                }
            }
            return true;
        }

        public override void HoldItem(Player player)
        {
            player.Calamity().mouseWorldListener = true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.Calamity().StealthStrikeAvailable())
            {
                int proj = Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<PumpkaboomBig>(), damage, knockback, player.whoAmI);
                if (proj.WithinBounds(Main.maxProjectiles))
                    Main.projectile[proj].Calamity().stealthStrike = true;
                
                return false;
            }
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.Grenade, 15).
                AddIngredient(ItemID.Pumpkin, 10).
                AddTile(TileID.Anvils).
                Register();
        }
    }
}
