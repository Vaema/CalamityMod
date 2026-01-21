using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions.Food
{
	public class Jackfruit : ModItem, ILocalizedModType
	{
		public new string LocalizationCategory => "Items.Potions";
		public override void SetStaticDefaults()
		{
			Item.ResearchUnlockCount = 5;
			Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, 3));
			ItemID.Sets.FoodParticleColors[Type] = new Color[3] {
				new Color(250, 216, 80),
				new Color(201, 207, 79),
				new Color(147, 176, 54)
			};
			ItemID.Sets.IsFood[Type] = true;
            ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.Ambrosia;
        }

		public override void SetDefaults()
		{
			Item.DefaultToFood(26, 36, BuffID.WellFed, CalamityUtils.MinutesToFrames(5));
			Item.value = Item.sellPrice(silver: 20);
			Item.rare = ItemRarityID.Blue;
		}
	}
}
