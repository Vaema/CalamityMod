using CalamityMod.Projectiles.Rogue;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Rogue
{
    public class CelestialReaper : RogueWeapon
    {

        public override void SetDefaults()
        {
            Item.width = 66;
            Item.height = 76;
            Item.damage = 140;
            Item.useAnimation = 31;
            Item.useTime = 31;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 6f;
            Item.rare = ItemRarityID.Purple;
            Item.UseSound = SoundID.Item71;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityPurpleBuyPrice;
            Item.shoot = ModContent.ProjectileType<CelestialReaperProjectile>();
            Item.shootSpeed = 20f;
            Item.DamageType = RogueDamageClass.Instance;
        }

        public override float StealthDamageMultiplier => 0.9f;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            bool usingStealth = player.Calamity().StealthStrikeAvailable();
            float strikeValue = usingStealth.ToInt(); //0 if false, 1 if true

            int p = Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<CelestialReaperProjectile>(), damage, knockback, player.whoAmI, strikeValue);
            if (player.Calamity().StealthStrikeAvailable() && p.WithinBounds(Main.maxProjectiles))
                Main.projectile[p].Calamity().stealthStrike = true;
            return false;
        }
    }
}
