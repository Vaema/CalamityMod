using CalamityMod.Buffs.Alcohol;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions.Alcohol
{
    public class WhiteWine : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Potions";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 5;
            // Clear, yellow-green
            ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
                new Color(242, 252, 177, 180),
                new Color(250, 252, 215, 180),
                new Color(228, 245, 181, 180)
            };
        }

        public override void SetDefaults()
        {
            Item.DefaultToFood(14, 44, ModContent.BuffType<WhiteWineBuff>(), CalamityUtils.SecondsToFrames(300f), true);
            Item.healMana = 300;
            // Cirrus overcharges: 10% sell value instead of 20%
            Item.value = Item.sellPrice(silver: 40);
            Item.rare = ItemRarityID.LightPurple;
        }

        public override void OnConsumeItem(Player player)
        {
            if (PlayerInput.Triggers.JustPressed.QuickBuff)
            {
                player.statMana += Item.healMana;
                if (player.statMana > player.statManaMax2)
                {
                    player.statMana = player.statManaMax2;
                }
                player.AddBuff(BuffID.ManaSickness, Player.manaSickTime, true);
                if (Main.myPlayer == player.whoAmI)
                {
                    player.ManaEffect(Item.healMana);
                }
            }
            player.AddBuff(Item.buffType, Item.buffTime);
        }
    }
}
