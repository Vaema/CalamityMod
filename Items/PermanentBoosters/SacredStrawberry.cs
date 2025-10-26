using System.Collections.Generic;
using System.Linq;
using CalamityMod.CalPlayer;
using CalamityMod.Items.Materials;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.PermanentBoosters
{
    [LegacyName("Dragonfruit")]
    public class SacredStrawberry : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Misc";

        public const int LifeBoost = 25;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(LifeBoost);

        public static readonly SoundStyle UseSound = new("CalamityMod/Sounds/Item/DragonfruitConsume");
        public override void SetStaticDefaults()
        {
            // For some reason Life/Mana boosting items are in this set (along with Magic Mirror+)
            ItemID.Sets.SortingPriorityBossSpawns[Type] = 20; // Life Fruit
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.consumable = true;
            Item.useAnimation = Item.useTime = 30;
            Item.UseSound = UseSound;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.value = Item.sellPrice(gold: 48);
            Item.rare = ModContent.RarityType<BurnishedAuric>();
        }

        public override bool CanUseItem(Player player) => player.ConsumedLifeFruit == Player.LifeFruitMax;

        public override bool? UseItem(Player player)
        {
            CalamityPlayer modPlayer = player.Calamity();
            if (player.itemAnimation > 0 && player.itemTime == 0)
            {
                player.itemTime = Item.useTime;
                if (modPlayer.sStrawberry)
                {
                    if (player.whoAmI == Main.myPlayer)
                    {
                        string key = "Mods.CalamityMod.Misc.SacredStrawberryText";
                        Color messageColor = Color.SpringGreen;
                        Main.NewText(Language.GetTextValue(key), messageColor);
                    }
                    return null;
                }

                player.UseHealthMaxIncreasingItem(LifeBoost);
                modPlayer.sStrawberry = true;
            }
            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> list)
        {
            TooltipLine line = list.FirstOrDefault(x => x.Mod == "Terraria" && x.Name == "Tooltip1");

            if (line != null && Main.LocalPlayer.Calamity().sStrawberry)
                line.Text += "\n" + CalamityUtils.GetTextValue("Misc.GenericConsumedText");
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.LifeFruit).
                AddIngredient<YharonSoulFragment>(5).
                AddIngredient<AscendantSpiritEssence>().
                AddTile<CosmicAnvil>().
                Register();
        }
    }
}
