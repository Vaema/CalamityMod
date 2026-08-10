using CalamityMod.Projectiles.Typeless;
using CalamityMod.Rarities;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Items.Tools;

public class BobbitHook : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Tools";

    public static float Reach = 40f;
    public static float LaunchSpeed = 25f;
    public static float ReelbackSpeed = 28f;
    public static float PullSpeed = 24f;
    public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(Reach.ToString(), LaunchSpeed.ToString(), ReelbackSpeed.ToString(), PullSpeed.ToString());

    public override void SetDefaults()
    {
        // Instead of copying these values, we can clone and modify the ones we want to copy
        Item.CloneDefaults(ItemID.AmethystHook);
        Item.width = 30;
        Item.height = 32;
        Item.shootSpeed = LaunchSpeed; // How quickly the hook is shot.
        Item.shoot = ProjectileType<BobbitHead>();
        Item.value = CalamityGlobalItem.RarityPureGreenBuyPrice;
        Item.rare = RarityType<PureGreen>();
    }
}
