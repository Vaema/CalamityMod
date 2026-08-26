using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Armor.Vanity;

[AutoloadEquip(EquipType.Head)]
public class ArtemisMask : ModItem, IExtendedHat, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Armor.Vanity";
    public override void SetStaticDefaults()
    {
        if (!Main.dedServ)
        {
            int equipSlotHead = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Head);
            ArmorIDs.Head.Sets.DrawHead[equipSlotHead] = false;
        }
    }

    public override void SetDefaults()
    {
        Item.width = 28;
        Item.height = 20;
        Item.rare = ItemRarityID.Blue;
        Item.value = Item.sellPrice(silver: 75);
        Item.vanity = true;
    }

    public string ExtensionTexture => "CalamityMod/Items/Armor/Vanity/ArtemisMask_Extra";
    public Vector2 ExtensionSpriteOffset(PlayerDrawSet drawInfo) => new(drawInfo.drawPlayer.direction == 1f ? -6f : 0, 0f);
}
