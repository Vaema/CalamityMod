using System.Collections.Generic;
using System.Linq;
using CalamityMod.CalPlayer;
using CalamityMod.Items.Materials;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.PermanentBoosters
{
    public class CometShard : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Misc";
        public static readonly SoundStyle UseSound = new("CalamityMod/Sounds/Item/CometShardUse");
        public const int ManaBoost = 50;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(ManaBoost);

        public override void SetStaticDefaults()
        {
            // For some reason Life/Mana boosting items are in this set (along with Magic Mirror+)
            ItemID.Sets.SortingPriorityBossSpawns[Type] = 21; // Mana Crystal
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 46;
            Item.consumable = true;
            Item.useAnimation = Item.useTime = 30;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.value = Item.sellPrice(gold: 6);
            Item.rare = ItemRarityID.LightRed;
        }

        public override bool CanUseItem(Player player) => player.ConsumedManaCrystals == Player.ManaCrystalMax;

        public override bool? UseItem(Player player)
        {
            SoundEngine.PlaySound(UseSound, player.Center);
            CalamityPlayer modPlayer = player.Calamity();
            if (player.itemAnimation > 0 && player.itemTime == 0)
            {
                player.itemTime = Item.useTime;
                if (modPlayer.cShard)
                {
                    if (player.whoAmI == Main.myPlayer)
                    {
                        string key = "Mods.CalamityMod.Misc.CometShardText";
                        Color messageColor = Color.SkyBlue;
                        Main.NewText(Language.GetTextValue(key), messageColor);
                    }
                    return null;
                }

                player.UseManaMaxIncreasingItem(ManaBoost);
                modPlayer.cShard = true;
            }
            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> list)
        {
            TooltipLine line = list.FirstOrDefault(x => x.Mod == "Terraria" && x.Name == "Tooltip1");

            if (line != null && Main.LocalPlayer.Calamity().cShard)
                line.Text += "\n" + CalamityUtils.GetTextValue("Misc.GenericConsumedText");
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.MeteoriteBar, 10).
                AddIngredient(ItemID.FallenStar, 10).
                AddIngredient<StarblightSoot>(50).
                AddTile(TileID.Anvils).
                Register();
        }
    }
}
