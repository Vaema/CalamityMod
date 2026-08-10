using CalamityMod.Projectiles.Rogue;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Rogue;

public class WebBall : RogueWeapon
{
    public override void SetDefaults()
    {
        Item.width = 20;
        Item.height = 18;
        Item.damage = 11;
        Item.noMelee = true;
        Item.noUseGraphic = true;
        Item.useAnimation = 20;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTime = 20;
        Item.knockBack = 3f;
        Item.UseSound = SoundID.Item1;
        Item.value = CalamityGlobalItem.RarityWhiteBuyPrice;
        Item.rare = ItemRarityID.White;
        Item.shoot = ModContent.ProjectileType<WebBallBol>();
        Item.shootSpeed = 6.5f;
        Item.DamageType = RogueDamageClass.Instance;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        if (player.Calamity().StealthStrikeAvailable()) //setting the stealth strike
        {
            int proj = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            if (proj.WithinBounds(Main.maxProjectiles))
                Main.projectile[proj].Calamity().stealthStrike = true;
            return false;
        }
        return true;
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient(ItemID.Cobweb, 30).
            Register();
    }
}
