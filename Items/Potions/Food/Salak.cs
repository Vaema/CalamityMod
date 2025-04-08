using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions.Food
{
	public class Salak : ModItem, ILocalizedModType
	{
		public new string LocalizationCategory => "Items.Potions";
		public override void SetStaticDefaults()
		{
			Item.ResearchUnlockCount = 5;
			Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, 3));
			ItemID.Sets.FoodParticleColors[Type] = new Color[3] {
				new Color(203, 134, 81),
				new Color(162, 109, 68),
				new Color(253, 242, 233)
			};
			ItemID.Sets.IsFood[Type] = true;
            ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.Ambrosia;
        }

		public override void SetDefaults()
		{
			Item.DefaultToFood(28, 26, BuffID.WellFed, CalamityUtils.MinutesToFrames(5));
			Item.value = Item.sellPrice(silver: 20);
			Item.rare = ItemRarityID.Blue;
		}
	}
}
