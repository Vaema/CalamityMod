using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Mounts;

[LegacyName("BirdSeed")]
public class FollyFeed : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Mounts";
    public override void SetDefaults()
    {
        Item.width = 34;
        Item.height = 36;
        Item.useAnimation = Item.useTime = 20;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.UseSound = SoundID.NPCHit51;
        Item.noMelee = true;
        Item.mountType = ModContent.MountType<BUMBLEDOGE>();

        Item.value = Item.sellPrice(gold: 5);
        Item.rare = ItemRarityID.Yellow;
        Item.Calamity().donorItem = true;
    }
}
