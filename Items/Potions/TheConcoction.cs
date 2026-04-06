using CalamityMod.Items.Materials;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using Terraria;
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
            ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
                new Color(255, 190, 250),
                new Color(255, 225, 183),
                new Color(246, 34, 79)
            };
        }

        public override void SetDefaults()
        {
            Item.DefaultToHealingPotion(50, 56, 500);
            Item.value = Item.sellPrice(silver: 60);
            Item.rare = ItemRarityID.Green;
        }
    }
}
