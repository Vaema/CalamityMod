using CalamityMod.Buffs.Potions;
using CalamityMod.Items.Materials;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions
{
    public class AstralInjection : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Potions";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 30;
            ItemID.Sets.DrinkParticleColors[Type] = new Color[2] {
                new Color(255, 164, 94),
                new Color(109, 242, 196)
            };
        }

        public override void SetDefaults()
        {
            Item.DefaultToFood(14, 34, ModContent.BuffType<AstralInjectionBuff>(), CalamityUtils.SecondsToFrames(5f), true);
            Item.value = Item.sellPrice(silver: 2);
            Item.rare = ItemRarityID.Lime;
        }

        public override void OnConsumeItem(Player player)
        {
            player.AddBuff(BuffID.ManaSickness, Player.manaSickTime / 2, true);
            player.statLife -= 5;
            if (Main.myPlayer == player.whoAmI)
            {
                player.HealEffect(-5, true);
            }
            if (player.statLife <= 0)
            {
                player.KillMe(PlayerDeathReason.ByCustomReason(CalamityUtils.GetText("Status.Death.AstralInjection" + Main.rand.Next(1, 2 + 1)).Format(player.name)), 1000.0, 0, false);
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe(15).
                AddIngredient(ItemID.BottledWater, 15).
                AddIngredient<StarblightSoot>(4).
                AddIngredient<AureusCell>().
                AddTile(TileID.AlchemyTable).
                AddConsumeItemCallback(Recipe.ConsumptionRules.Alchemy).
                Register();

            CreateRecipe(15).
                AddIngredient(ItemID.BottledWater, 15).
                AddIngredient<BloodOrb>(5).
                AddIngredient<AureusCell>().
                AddTile(TileID.AlchemyTable).
                Register()
                .DisableDecraft();
        }
    }
}
