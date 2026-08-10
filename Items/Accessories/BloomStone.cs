using CalamityMod.CalPlayer;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories;

public class BloomStone : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Accessories";
    public override void SetDefaults()
    {
        Item.width = 38;
        Item.height = 54;
        Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
        Item.rare = ItemRarityID.Lime;
        Item.accessory = true;
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        CalamityPlayer modPlayer = player.Calamity();
        Lighting.AddLight((int)player.Center.X / 16, (int)player.Center.Y / 16, 0.25f, 0.4f, 0.2f);

        modPlayer.healingPotionMultiplier += 0.5f;
        modPlayer.bloomStone = true;
        modPlayer.bloomStoneHookVisuals = true;
    }
    public override void UpdateVanity(Player player) => player.Calamity().bloomStoneHookVisuals = true;
}
