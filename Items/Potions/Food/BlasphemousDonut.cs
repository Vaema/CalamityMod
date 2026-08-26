using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions.Food;

public class BlasphemousDonut : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Potions";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 5;
        Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, 3));
        ItemID.Sets.FoodParticleColors[Type] = new Color[3] {
            new(122, 66, 59),
            new(206, 116, 59),
            new(198, 153, 113)
        };
        ItemID.Sets.IsFood[Type] = true;
    }
    public override void SetDefaults()
    {
        Item.DefaultToFood(40, 26, BuffID.WellFed2, CalamityUtils.MinutesToFrames(60));
        Item.value = Item.sellPrice(silver: 60);
        Item.rare = ItemRarityID.Purple;
    }
}
