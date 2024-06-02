using CalamityMod.Projectiles.Rogue;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Rogue
{
    public class Glaive : RogueWeapon
    {
        //These 2 are used on the Projectile AI and thus remain here
        public static float Speed = 10f;
        public static float StealthSpeedMult = 1.8f;

        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 32;
            Item.damage = 45;
            Item.DamageType = RogueDamageClass.Instance;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 3;
            Item.value = CalamityGlobalItem.RarityOrangeBuyPrice;
            Item.rare = ItemRarityID.Orange;
            Item.UseSound = SoundID.Item1;

            Item.shootSpeed = 10;
            Item.shoot = ModContent.ProjectileType<GlaiveProj>();
        }

        // Terraria seems to really dislike high crit values in SetDefaults
        public override void ModifyWeaponCrit(Player player, ref float crit) => crit += 4;

        public override float StealthVelocityMultiplier => StealthSpeedMult;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            float ai1 = 0f;
            if (player.Calamity().StealthStrikeAvailable())
            {
                ai1 = 1f;
            }

            int p = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 0f, ai1);
            if (player.Calamity().StealthStrikeAvailable() && p.WithinBounds(Main.maxProjectiles))
                Main.projectile[p].Calamity().stealthStrike = true;
            return false;
        }

        public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] < 3;
    }
}
