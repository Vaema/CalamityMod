using CalamityMod.Projectiles.Rogue;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Rogue;

public class AntlionSkewer : RogueWeapon
{
    public static float CloudDamageDebuffMult = 0.9f;

    public override void SetDefaults()
    {
        Item.width = 58;
        Item.height = 56;
        Item.damage = 19;
        Item.DamageType = RogueDamageClass.Instance;
        Item.useAnimation = Item.useTime = 28;
        Item.knockBack = 2f;
        Item.shoot = ModContent.ProjectileType<AntlionSkewerProj>();
        Item.shootSpeed = 12f;

        Item.UseSound = SoundID.Item1;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.autoReuse = true;
        Item.noMelee = true;
        Item.noUseGraphic = true;

        Item.value = CalamityGlobalItem.RarityGreenBuyPrice;
        Item.rare = ItemRarityID.Green;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        int p = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
        if (p.WithinBounds(Main.maxProjectiles) && player.Calamity().StealthStrikeAvailable())
            Main.projectile[p].Calamity().stealthStrike = true;
        return false;
    }
}
