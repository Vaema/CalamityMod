using CalamityMod.CalPlayer;
using CalamityMod.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories;

[AutoloadEquip(EquipType.Shoes)]
public class AngelTreads : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Accessories";

    public static float RunningSpeed = 7.5f; // 6.75 from Lightning-Terraspark
    public static float MoveSpeedBoost = 0.12f;
    public static float FlightTimeBoost = 0.1f;
    public static int LavaImmunityTime = 420; // Identical to Terraspark Boots and upgrades prior
    public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MoveSpeedBoost.ToPercent(), FlightTimeBoost.ToPercent(), LavaImmunityTime.FramesToSeconds());

    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 36;
        Item.value = CalamityGlobalItem.RarityLightPurpleBuyPrice;
        Item.rare = ItemRarityID.LightPurple;
        Item.accessory = true;
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        CalamityPlayer modPlayer = player.Calamity();
        modPlayer.angelTreads = true;
        player.accRunSpeed = RunningSpeed;
        player.rocketBoots = player.vanityRocketBoots = 3;
        player.moveSpeed += MoveSpeedBoost;
        player.iceSkate = true;
        player.waterWalk = true;
        player.fireWalk = true;
        player.lavaMax += LavaImmunityTime;
        player.lavaRose = true;
    }

    public override void UpdateVanity(Player player)
    {
        player.vanityRocketBoots = 3;
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient(ItemID.TerrasparkBoots).
            AddIngredient<HarpyRing>().
            AddIngredient<EssenceofSunlight>(5).
            AddIngredient(ItemID.SoulofFright).
            AddIngredient(ItemID.SoulofMight).
            AddIngredient(ItemID.SoulofSight).
            AddTile(TileID.MythrilAnvil).
            Register();
    }
}
