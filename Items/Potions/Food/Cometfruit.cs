using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions.Food;

	public class Cometfruit : ModItem, ILocalizedModType
	{
		public new string LocalizationCategory => "Items.Potions";
		public override void SetStaticDefaults()
		{
			Item.ResearchUnlockCount = 5;
			Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, 3));
			ItemID.Sets.FoodParticleColors[Type] = new Color[3] {
				new Color(153, 100, 184),
				new Color(181, 141, 166),
				new Color(147, 200, 221)
			};
			ItemID.Sets.IsFood[Type] = true;
        ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.Ambrosia;
    }

		public override void SetDefaults()
		{
			Item.DefaultToFood(32, 32, BuffID.WellFed2, CalamityUtils.MinutesToFrames(5));
			Item.value = Item.sellPrice(silver: 20);
			Item.rare = ItemRarityID.Green;
		}
	}
