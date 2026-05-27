using System.Collections.Generic;
using System.Linq;
using CalamityMod.Buffs.Potions;
using CalamityMod.Items.Materials;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions
{
    public class TheConcoction : ModItem, ILocalizedModType
    {
        private static int hoverTimer = 0;
        private static bool wasHovering = false;
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
            Item.value = Item.sellPrice(silver: 20);
            Item.rare = ItemRarityID.Green;
        }


        public override void OnConsumeItem(Player player)
        {
            TheConcoctionPlayer cocPlayer = player.GetModPlayer<TheConcoctionPlayer>();
            cocPlayer.swinesWrathCounter = 1200; // Creates a 10 second delay before the buff is visible (triggers at 600)
        }


        public override void UpdateInventory(Player player)
        {
            wasHovering = Main.HoverItem?.type == Type;
        }

        // Display different text for the first 10 frames of hovering over the item's tooltip.
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            bool isHovering = Main.HoverItem?.type == Type;

            if (isHovering && !wasHovering)
                hoverTimer = 0;
            if (isHovering)
                hoverTimer++;

            if (hoverTimer <= 10 && hoverTimer > 0)
            {
                TooltipLine healLine = tooltips.FirstOrDefault(x => x.Mod == "Terraria" && x.Name == "HealLife");
                healLine?.Text = this.GetLocalization("EasterEggText").Value;
            }
        }
    }

    public class TheConcoctionPlayer : ModPlayer
    {
        public int swinesWrathCounter = -1;
        
        public override void PostUpdate()
        {
            if (swinesWrathCounter > 0)
            {
                swinesWrathCounter--;

                if (swinesWrathCounter <= 600 && !Player.HasBuff<SwinesWrathBuff>()) // When there is 10 seconds left
                {
                    Player.AddBuff(ModContent.BuffType<SwinesWrathBuff>(), 600);

                    SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Item/SwinesWrathProc"), Player.Center);
                }
            }
        }
    }
}
