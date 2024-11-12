using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions
{
    public class BlasphemousDonut : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Potions";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 5;
            ItemID.Sets.FoodParticleColors[Type] = new Color[3] {
                new Color(122, 66, 59),
                new Color(206, 116, 59),
                new Color(198, 153, 113)
            };
        }
        public override void SetDefaults()
        {
            Item.DefaultToFood(40, 26, BuffID.WellFed2, CalamityUtils.MinutesToFrames(60));
            Item.value = Item.sellPrice(silver: 60);
            Item.rare = ItemRarityID.Purple;
        }
    }
}
