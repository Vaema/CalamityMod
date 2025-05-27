using CalamityMod.Buffs.Alcohol;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Ores;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions.Alcohol
{
    public class WhiteWine : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Potions";

        public static float MagicDamageBoost = 0.08f;
        public static float DefenseLossPercent = 0.06f;
        public static int RegenLoss = 2;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MagicDamageBoost.ToPercent(), DefenseLossPercent.ToPercent(), RegenLoss.ToRegenPerSecond());

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 30;
            // Clear, yellow-green
            ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
                new Color(242, 252, 177, 180),
                new Color(250, 252, 215, 180),
                new Color(228, 245, 181, 180)
            };
        }

        public override void SetDefaults()
        {
            Item.DefaultToFood(14, 44, ModContent.BuffType<WhiteWineBuff>(), CalamityUtils.MinutesToFrames(5), true);
            Item.healMana = 300;

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
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.Ale).
                AddIngredient<HallowedOre>(3).
                AddTile(TileID.Kegs).
                Register();

            CreateRecipe().
                AddIngredient(ItemID.BottledWater).
                AddIngredient<BloodOrb>(5).
                AddIngredient<HallowedOre>().
                AddTile(TileID.AlchemyTable).
                Register()
                .DisableDecraft();
        }
    }
}
