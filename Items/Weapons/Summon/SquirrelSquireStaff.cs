using CalamityMod.Projectiles.Summon;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Summon;

public class SquirrelSquireStaff : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Weapons.Summon";

    public static float ProjectileVelocity = 20f;
    public static int TimeBeforeFalling = 30;
    public static float DistanceToMortarShoot = 240f;
    public static int ProjectileTimeAlive = 180;
    public static float ProjectileGravity = 0.5f;
    public static int ProjectileAoERadiusSize = 24;

    public override void SetStaticDefaults() => Item.staff[Type] = true;

    public override void SetDefaults()
    {
        Item.damage = 8;
        Item.DamageType = DamageClass.Summon;
        Item.shoot = ModContent.ProjectileType<SquirrelSquireMinion>();
        Item.knockBack = 0.5f;

        Item.useAnimation = Item.useTime = 30;
        Item.mana = 10;
        Item.width = 46;
        Item.height = 52;
        Item.noMelee = true;
        Item.sentry = true;
        Item.autoReuse = true;
        Item.value = CalamityGlobalItem.RarityWhiteBuyPrice;
        Item.rare = ItemRarityID.White;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.UseSound = SoundID.Item44;

        // This doesn't do anything relevant, it's just so it can be held like a staff.
        Item.shootSpeed = 1f;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        // Find the base farthest position
        Vector2 initialSpawn = player.GetFarthestSpawnPositionOnLine(position, velocity.X, velocity.Y);
        int dontLoopForever = 0;

        // Move the squirrel towards the player if the position does not have line of sight on all edges
        while ((!Collision.CanHit(initialSpawn, 0, 0, initialSpawn + Vector2.UnitX * 20f, 0, 0) ||
            !Collision.CanHit(initialSpawn, 0, 0, initialSpawn - Vector2.UnitX * 20f, 0, 0) ||
            !Collision.CanHit(initialSpawn, 0, 0, initialSpawn + Vector2.UnitY * 24f, 0, 0) ||
            !Collision.CanHit(initialSpawn, 0, 0, initialSpawn - Vector2.UnitY * 24f, 0, 0)) && dontLoopForever < 200) 
        {
            initialSpawn = Vector2.Lerp(player.Center, initialSpawn, 0.99f);
            dontLoopForever++;
        }

        Projectile.NewProjectile(source, initialSpawn, Vector2.Zero, type, damage, knockback, player.whoAmI);
        player.UpdateMaxTurrets();
        return false;
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddRecipeGroup("Wood", 10).
            AddIngredient(ItemID.Acorn).
            AddTile(TileID.WorkBenches).
            Register();
    }
}
