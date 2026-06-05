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
            Item.value = Item.buyPrice(gold: 1);
            Item.rare = ItemRarityID.Green;
        }


        public override void OnConsumeItem(Player player)
        {
            TheConcoctionPlayer concoctionPlayer = player.GetModPlayer<TheConcoctionPlayer>();
            concoctionPlayer.swinesWrathCounter = 1200; // Creates a 10 second delay before the buff is visible (triggers at 600)
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            TheConcoctionPlayer concoctionPlayer = Main.LocalPlayer.GetModPlayer<TheConcoctionPlayer>();

            if (concoctionPlayer.hoverTimer <= 10 && concoctionPlayer.hoverTimer > 0)
            {
                TooltipLine healLine = tooltips.FirstOrDefault(x => x.Mod == "Terraria" && x.Name == "HealLife");
                if (healLine != null)
                {
                    healLine.Text = this.GetLocalization("EasterEggText").Value;
                }
            }
        }
    }

    public class TheConcoctionPlayer : ModPlayer
    {
        public int swinesWrathCounter = -1;

        public int hoverTimer = 0;
        public bool wasHovering = false;

        public override void PostUpdate()
        {
            if (swinesWrathCounter > 0)
            {
                swinesWrathCounter--;

                if (swinesWrathCounter <= 600 && !Player.HasBuff<SwinesWrathBuff>())
                {
                    Player.AddBuff(ModContent.BuffType<SwinesWrathBuff>(), 600);
                    SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Item/SwinesWrathProc"), Player.Center);
                }
            }

            if (Main.myPlayer == Player.whoAmI)
            {
                // Check if hover over the right item
                bool isHovering = Main.HoverItem?.type == ModContent.ItemType<TheConcoction>();

                if (isHovering && !wasHovering)
                    hoverTimer = 0;

                if (isHovering)
                    hoverTimer++;
                else
                    hoverTimer = 0;

                // Save the state for the next frame
                wasHovering = isHovering;
            }
        }
    }
}
