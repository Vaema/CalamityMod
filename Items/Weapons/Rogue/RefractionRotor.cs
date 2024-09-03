using CalamityMod.CalPlayer;
using CalamityMod.Projectiles.Rogue;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Rogue
{
    public class RefractionRotor : RogueWeapon
    {
        public override void SetDefaults()
        {
            Item.width = Item.height = 120;
            Item.damage = 231;
            Item.knockBack = 8.5f;
            Item.useAnimation = Item.useTime = 55;
            Item.DamageType = RogueDamageClass.Instance;
            Item.autoReuse = true;
            Item.shootSpeed = 18f;
            Item.shoot = ModContent.ProjectileType<RefractionRotorProjectile>();

            Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = SoundID.Item1;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.value = CalamityGlobalItem.RarityVioletBuyPrice;
            Item.rare = ModContent.RarityType<Violet>();
        }

        public override float StealthDamageMultiplier => 0.3f;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            CalamityPlayer p = Main.player[Main.myPlayer].Calamity();
            //If stealth is full, shoot a spread of 3 shurikens
            if (p.StealthStrikeAvailable())
            {
                int spread = 30;
                for (int i = 0; i < 3; i++)
                {
                    Vector2 perturbedspeed = velocity.RotatedBy(MathHelper.ToRadians(spread));
                    int proj = Projectile.NewProjectile(source, position, perturbedspeed, type, damage, knockback, player.whoAmI, 0f, 1f);
                    if (proj.WithinBounds(Main.maxProjectiles))
                        Main.projectile[proj].Calamity().stealthStrike = true;
                    spread -= 30;
                }
                return false;
            }
            return true;
        }
    }
}
