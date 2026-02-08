using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions.Food
{
	public class Mangosteen : ModItem, ILocalizedModType
	{
		public new string LocalizationCategory => "Items.Potions";
		public override void SetStaticDefaults()
		{
			Item.ResearchUnlockCount = 5;
			Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, 3));
			ItemID.Sets.FoodParticleColors[Type] = new Color[3] {
				new Color(218, 238, 253),
				new Color(178, 199, 214),
				new Color(123, 99, 130)
			};
			ItemID.Sets.IsFood[Type] = true;
            ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.Ambrosia;
        }

		public override void SetDefaults()
		{
			Item.DefaultToFood(30, 32, BuffID.WellFed2, CalamityUtils.MinutesToFrames(5));
			Item.value = Item.sellPrice(silver: 20);
			Item.rare = ItemRarityID.Green;
		}
	}
}
