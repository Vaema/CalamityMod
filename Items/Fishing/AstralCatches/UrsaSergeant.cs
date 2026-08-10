using CalamityMod.CalPlayer;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Fishing.AstralCatches;

public class UrsaSergeant : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Fishing";

    public static int CooldownReducedPerKill = 180;
    public static int MaxCooldown = 300;
    public static int BaseSwipeDamage = 325; // About 65 dps

    public override void SetDefaults()
    {
        Item.width = 36;
        Item.height = 26;
        Item.value = CalamityGlobalItem.RarityPinkBuyPrice;
        Item.rare = ItemRarityID.Pink;
        Item.accessory = true;
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        CalamityPlayer modPlayer = player.Calamity();
        modPlayer.ursaSergeant = true;
        if (!hideVisual)
            modPlayer.ursaSergeantVisual = true;
        else
            modPlayer.ursaSergeantVisual = false;
    }
}
