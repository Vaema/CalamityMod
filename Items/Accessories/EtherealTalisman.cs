using System.Collections.Generic;
using System.IO;
using CalamityMod.CalPlayer;
using CalamityMod.Items.Materials;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace CalamityMod.Items.Accessories
{
    public class EtherealTalisman : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 32;
            Item.value = CalamityGlobalItem.RarityDarkBlueBuyPrice;
            Item.accessory = true;
            Item.rare = ModContent.RarityType<CosmicPurple>();
        }
        #region Toggleable Mana Flower

        bool manaFlowerEnabled = true;
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (!manaFlowerEnabled)
                tooltips.RemoveAll(x => x.Name == "Tooltip3");
        }
        public override bool CanRightClick() => Main.keyState.PressingShift();
        public override void RightClick(Player player)
        {
            manaFlowerEnabled = !manaFlowerEnabled;
            Item.NetStateChanged();
        }
        public override bool ConsumeItem(Player player) => false;
        public override void SaveData(TagCompound tag)
        {
            tag.Add("manaFlower", manaFlowerEnabled);
        }

        public override void LoadData(TagCompound tag)
        {
            manaFlowerEnabled = tag.GetBool("manaFlower");
        }

        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(manaFlowerEnabled);
        }

        public override void NetReceive(BinaryReader reader)
        {
            manaFlowerEnabled = reader.ReadBoolean();
        }

        public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            CalamityUtils.DrawInventoryDot(spriteBatch, position, new Vector2(16, 16) * Main.inventoryScale, manaFlowerEnabled);
        }
        #endregion
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            CalamityPlayer modPlayer = player.Calamity();
            modPlayer.eTalisman = true;

            player.manaMagnet = true;
            if (manaFlowerEnabled)
                player.manaFlower = true;

            player.statManaMax2 += 150;
            player.GetDamage<MagicDamageClass>() += 0.15f;
            player.manaCost -= 0.1f;
            player.GetCritChance<MagicDamageClass>() += 5;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<SigilofCalamitas>().
                AddRecipeGroup("AnyManaFlower"). //Any mana flower accessory
                AddIngredient<AscendantSpiritEssence>(4).
                AddTile<CosmicAnvil>().
                Register();
        }
    }
}
