using CalamityMod.Projectiles.Rogue;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Rogue
{
    public class SpentFuelContainer : RogueWeapon
    {

        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 24;
            Item.damage = 80;
            Item.useAnimation = 50;
            Item.useTime = 50;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 4.5f;
            Item.value = CalamityGlobalItem.RarityPinkBuyPrice;
            Item.rare = ItemRarityID.Pink;
            Item.UseSound = SoundID.Item106;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<SpentFuelContainerProjectile>();
            Item.shootSpeed = 15f;
            Item.DamageType = RogueDamageClass.Instance;
        }
        public override float StealthDamageMultiplier => 0.5f;
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {

            bool stealthAvailable = player.Calamity().StealthStrikeAvailable();
            int p = Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<SpentFuelContainerProjectile>(), damage, knockback, player.whoAmI, stealthAvailable ? 1f : 0f);
            if (stealthAvailable && p.WithinBounds(Main.maxProjectiles))
                Main.projectile[p].Calamity().stealthStrike = true;
            return false;
        }
    }
}
