using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Rogue;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Rogue;

public class FantasyTalisman : RogueWeapon
{
    public override void SetDefaults()
    {
        Item.width = 30;
        Item.height = 30;
        Item.damage = 45;
        Item.noMelee = true;
        Item.noUseGraphic = true;
        Item.useTime = Item.useAnimation = 25;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.knockBack = 6f;
        Item.UseSound = SoundID.Item1;
        Item.autoReuse = true;
        Item.value = CalamityGlobalItem.RarityYellowBuyPrice;
        Item.rare = ItemRarityID.Yellow;
        Item.shoot = ModContent.ProjectileType<FantasyTalismanProj>();
        Item.shootSpeed = 18f;
        Item.DamageType = RogueDamageClass.Instance;
    }
    public override float StealthDamageMultiplier => 0.75f;
    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        for (int i = -1; i <= 1; i++)
        {
            Vector2 perturbedSpeed = velocity.RotatedBy(MathHelper.ToRadians(i * (player.Calamity().StealthStrikeAvailable() ? 5f : 6f)));
            int shootCard = player.Calamity().StealthStrikeAvailable() ? ModContent.ProjectileType<FantasyTalismanStealth>() : type;
            int card = Projectile.NewProjectile(source, position, perturbedSpeed, shootCard, damage, knockback, player.whoAmI);
            if (card.WithinBounds(Main.maxProjectiles) && player.Calamity().StealthStrikeAvailable())
                Main.projectile[card].Calamity().stealthStrike = true;
        }
        return false;
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<SolarVeil>(10).
            AddIngredient(ItemID.Silk, 10).
            AddIngredient(ItemID.Ectoplasm, 5).
            AddTile(TileID.MythrilAnvil).
            Register();
    }
}
