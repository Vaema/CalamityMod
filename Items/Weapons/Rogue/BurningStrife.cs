using CalamityMod.Projectiles.Rogue;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Rogue;

public class BurningStrife : RogueWeapon
{
    public override void SetDefaults()
    {
        Item.width = 16;
        Item.height = 28;
        Item.damage = 73;
        Item.DamageType = RogueDamageClass.Instance;
        Item.useAnimation = Item.useTime = 25;
        Item.knockBack = 0.25f;
        Item.shoot = ModContent.ProjectileType<BurningStrifeProj>();
        Item.shootSpeed = 8f;

        Item.UseSound = SoundID.Item1;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.autoReuse = true;
        Item.noMelee = true;
        Item.noUseGraphic = true;

        Item.value = CalamityGlobalItem.RarityPinkBuyPrice;
        Item.rare = ItemRarityID.Pink;
    }

    public override float StealthVelocityMultiplier => 1.25f;

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        int proj = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
        if (player.Calamity().StealthStrikeAvailable() && proj.WithinBounds(Main.maxProjectiles))
            Main.projectile[proj].Calamity().stealthStrike = true;
        return false;
    }
}
