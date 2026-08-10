using CalamityMod.Projectiles.Melee;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Tools;

//Its not like its a renamed version of the spear, but i put this here more as a way to "refund" the item, so it doesnt end up rotting as an unloaded item.
[LegacyName("MarniteSpear")]
public class MarniteDeconstructor : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Tools";

    public static int ArmorPenetration = 10;
    public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(ArmorPenetration);

    public override void SetDefaults()
    {
        Item.width = 36;
        Item.height = 18;
        Item.damage = 6;
        Item.DamageType = TrueMeleeNoSpeedDamageClass.Instance;
        Item.ArmorPenetration = ArmorPenetration;
        Item.hammer = 59;
        Item.tileBoost = 7;
        Item.useAnimation = 25;
        Item.useTime = 4;
        Item.knockBack = 0.5f;
        Item.shoot = ModContent.ProjectileType<MarniteDeconstructorProj>();
        Item.shootSpeed = 40f;

        Item.UseSound = SoundID.Item23;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.autoReuse = true;
        Item.channel = true;
        Item.noMelee = true;
        Item.noUseGraphic = true;

        Item.value = Item.sellPrice(silver: 30);
        Item.rare = ItemRarityID.Blue;
    }

    public override void HoldItem(Player player) => player.Calamity().mouseWorldListener = true;

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient(ItemID.Sapphire).
            AddRecipeGroup("AnyGoldBar", 3).
            AddIngredient(ItemID.Granite, 5).
            AddIngredient(ItemID.Marble, 5).
            AddTile(TileID.Anvils).
            Register();
    }
}
