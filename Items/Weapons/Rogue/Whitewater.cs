using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Projectiles.Rogue;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Rogue;

[LegacyName("BrackishFlask")]
public class Whitewater : RogueWeapon
{
    public bool splitDirection = false;
    public override void SetDefaults()
    {
        Item.width = 36;
        Item.height = 40;
        Item.damage = 80;
        Item.noMelee = true;
        Item.noUseGraphic = true;
        Item.useTime = Item.useAnimation = 34;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.knockBack = 6.5f;
        Item.UseSound = SoundID.Item106 with { Volume = 0.7f };
        Item.autoReuse = true;
        Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
        Item.rare = ItemRarityID.Lime;
        Item.shoot = ModContent.ProjectileType<WhitewaterProj>();
        Item.shootSpeed = 6f;
        Item.DamageType = RogueDamageClass.Instance;
    }

    public override float StealthDamageMultiplier => 0.9f;

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        if (player.Calamity().StealthStrikeAvailable()) //setting the stealth strike
        {
            int stealth = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            if (stealth.WithinBounds(Main.maxProjectiles))
            {
                Main.projectile[stealth].Calamity().stealthStrike = true;
            }
        }
        else
        {
            int proj = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            Main.projectile[proj].ai[1] = splitDirection ? 1 : -1;
            splitDirection = !splitDirection;
        }
        return false;
    }
}
