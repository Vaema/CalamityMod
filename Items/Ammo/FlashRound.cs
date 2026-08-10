using CalamityMod.Projectiles.Ranged;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Ammo;

[LegacyName("FlashBullet")]
public class FlashRound : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Ammo";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 99;
    }

    public override void SetDefaults()
    {
        Item.width = 12;
        Item.height = 18;
        Item.damage = 6;
        Item.DamageType = DamageClass.Ranged;
        Item.maxStack = Item.CommonMaxStack;
        Item.consumable = true;
        Item.knockBack = 6f;
        Item.value = Item.sellPrice(copper: 1);
        Item.rare = ItemRarityID.Blue;
        Item.shoot = ModContent.ProjectileType<FlashRoundProj>();
        Item.shootSpeed = 10f;
        Item.ammo = AmmoID.Bullet;
    }

    public override void AddRecipes()
    {
        CreateRecipe(70).
            AddRecipeGroup("AnyCopperBar", 1).
            AddIngredient(ItemID.Glass, 1).
            AddTile(TileID.Anvils).
            Register();
    }
}
