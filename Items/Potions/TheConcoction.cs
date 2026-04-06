using System.Collections.Generic;
using System.Linq;
using CalamityMod.Items.Materials;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions
{
    public class TheConcoction : ModItem, ILocalizedModType
    {
        private static int hoverTimer = 0;
        public new string LocalizationCategory => "Items.Potions";

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 30;
            ItemID.Sets.DrinkParticleColors[Type] = new Color[3] 
            {
                new Color(255, 190, 250),
                new Color(255, 225, 183),
                new Color(246, 34, 79)
            };
        }

        public override void SetDefaults()
        {
            Item.DefaultToHealingPotion(50, 56, 500);
            Item.value = Item.sellPrice(silver: 60);
            Item.rare = ItemRarityID.Green;
        }

        // Display different text for the first 8 frames of hovering over the item's tooltip
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (Main.HoverItem?.type != Type)
            {
                hoverTimer = 0;
                return;
            }

            hoverTimer++;

            if (hoverTimer <= 8)
            {
                TooltipLine healLine = tooltips.FirstOrDefault(x => x.Mod == "Terraria" && x.Name == "HealLife");
                if (healLine != null)
                {
                    healLine.Text = this.GetLocalization("EasterEggText").Value;
                }
            }
        }
    }
}
