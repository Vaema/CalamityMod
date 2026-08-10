using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Armor.Vanity;

[AutoloadEquip(EquipType.Head)]
[LegacyName("BrimstoneWaifuMask")]
public class BrimstoneElementalMask : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Armor.Vanity";
    public override void SetStaticDefaults()
    {

        if (!Main.dedServ)
            ArmorIDs.Head.Sets.DrawHead[Item.headSlot] = false;
    }

    public override void SetDefaults()
    {
        Item.width = 30;
        Item.height = 28;
        Item.rare = ItemRarityID.Blue;
        Item.value = Item.sellPrice(silver: 75);
        Item.vanity = true;
    }
}
