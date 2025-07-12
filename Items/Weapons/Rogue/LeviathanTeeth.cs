using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Projectiles.Rogue;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Rogue
{
    public class LeviathanTeeth : RogueWeapon
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<Greentide>();
        }
        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 38;
            Item.damage = 43;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.useAnimation = Item.useTime = 9;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 1f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
            Item.rare = ItemRarityID.Lime;
            Item.shoot = ModContent.ProjectileType<LeviathanTooth>();
            Item.shootSpeed = 5.5f;
            Item.DamageType = RogueDamageClass.Instance;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.Calamity().StealthStrikeAvailable())
            {
                Projectile tooth = Projectile.NewProjectileDirect(source, position, velocity * 1.5f, type, damage, knockback * 12, player.whoAmI, 0f, 4f);
                tooth.Calamity().stealthStrike = true;
                tooth.timeLeft = 140;
            }
            else
            {
                for (int i = 0; i < 2; i++)
                    Projectile.NewProjectile(source, position, velocity.RotatedByRandom(0.05f + i * 0.15f) * Main.rand.NextFloat(0.7f, 1f), type, damage, knockback, player.whoAmI, 0f, Main.rand.Next(1, 3+1));
            }
            return false;
        }
    }
}
