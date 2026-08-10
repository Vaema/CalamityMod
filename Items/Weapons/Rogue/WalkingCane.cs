using CalamityMod.Projectiles.Rogue;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Rogue;

[LegacyName("SlickCane")]
public class WalkingCane : RogueWeapon
{
    public static float MoveSpeedBoost = 0.2f;
    public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MoveSpeedBoost.ToPercent());

    public override void SetDefaults()
    {
        Item.width = 48;
        Item.height = 44;
        Item.damage = 40;
        Item.noMelee = true;
        Item.noUseGraphic = true;
        Item.channel = true;
        Item.useTime = Item.useAnimation = 16;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.knockBack = 6f;
        Item.UseSound = SoundID.DD2_GhastlyGlaivePierce;
        Item.autoReuse = true;
        Item.value = CalamityGlobalItem.RarityOrangeBuyPrice;
        Item.rare = ItemRarityID.Orange;
        Item.shoot = ModContent.ProjectileType<WalkingCaneProjectile>();
        Item.shootSpeed = 20f;
        Item.DamageType = RogueDamageClass.Instance;
    }

    public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        float ai0 = Main.rand.NextFloat() * Item.shootSpeed * 1.5f * player.direction;
        int projectileIndex = Projectile.NewProjectile(source, position + Vector2.UnitY * 100f, velocity * 2f, type, damage, knockback, player.whoAmI, ai0);
        if (projectileIndex.WithinBounds(Main.maxProjectiles))
            Main.projectile[projectileIndex].Calamity().stealthStrike = player.Calamity().StealthStrikeAvailable();
        return false;
    }
}
