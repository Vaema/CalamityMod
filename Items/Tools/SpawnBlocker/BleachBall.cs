using System.Collections.Generic;
using System.IO;
using CalamityMod.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using static CalamityMod.CalamityUtils;

namespace CalamityMod.Items.Tools.SpawnBlocker
{
    public class BleachBall : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Tools";
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 46;
            Item.value = CalamityGlobalItem.RarityBlueBuyPrice;
            Item.rare = ItemRarityID.Blue;
        }

        public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
        {
            itemGroup = (ContentSamples.CreativeHelper.ItemGroup)CalamityResearchSorting.SpawnPrevention;
        }

        #region Toggle Feature

        public bool Enabled = true;

        public override ModItem Clone(Item item)
        {
            var clone = (BleachBall)base.Clone(item);
            clone.Enabled = Enabled;
            return clone;
        }

        public override void SaveData(TagCompound tag) => tag.Add("blockerEnabled", Enabled);

        public override void LoadData(TagCompound tag) => Enabled = tag.GetBool("blockerEnabled");

        public override void NetSend(BinaryWriter writer) => writer.Write(Enabled);

        public override void NetReceive(BinaryReader reader) => Enabled = reader.ReadBoolean();

        public override bool CanRightClick() => true;

        public override bool ConsumeItem(Player player) => false;

        public override void RightClick(Player player)
        {
            Enabled = !Enabled;
            Item.NetStateChanged();
        }

        #endregion

        public override void UpdateInventory(Player player)
        {
            player.Calamity().disableNaturalScourgeSpawns |= Enabled;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            string text;
            if (Enabled)
                text = GetTextValue("Items.Misc.SpawnBlockersOn");
            else
                text = GetTextValue("Items.Misc.SpawnBlockersOff");
            tooltips.FindAndReplace("[STATE]", text);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<BlightedGel>(5).
                AddIngredient(ItemID.CalmingPotion).
                AddTile(TileID.Anvils).
                Register();
        }
    }
}
