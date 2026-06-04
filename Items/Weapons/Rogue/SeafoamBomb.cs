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
    public class SeafoamBomb : RogueWeapon
    {
        public int throwCount = 0; // Gives bubbles an order to be fused in
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 42;
            Item.damage = 35;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.useTime = Item.useAnimation = 40;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 8f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityGreenBuyPrice;
            Item.rare = ItemRarityID.Green;
            Item.shoot = ModContent.ProjectileType<SeafoamBombProj>();
            Item.shootSpeed = 7f;
            Item.DamageType = RogueDamageClass.Instance;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.Calamity().StealthStrikeAvailable()) // Stealth strikes throw a cluster bomb, the throw count is increased to insure that bubble fusing works
            {
                int stealth = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 0, 0, -5);
                Main.projectile[stealth].Calamity().stealthStrike = true;
                Main.projectile[stealth].localAI[0] = throwCount;
                throwCount += 50;

                return false;
            }
            else
            {
                int proj = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 0, 0, -5);
                Main.projectile[proj].localAI[0] = throwCount;
                throwCount++;
            }
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<SeaPrism>(10).
                AddIngredient<PearlShard>(2).
                AddIngredient<Navystone>(15).
                AddTile(TileID.Anvils).
                Register();
        }
    }
}
