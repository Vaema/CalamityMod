using CalamityMod.CalPlayer;
using CalamityMod.Rarities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.PermanentBoosters
{
    public class NimbleBounder : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Misc";
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.useAnimation = Item.useTime = 30;
            // Same price as Frog Leg, which is used to shimmer into it
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ModContent.RarityType<HotPink>();
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.UseSound = SoundID.Zombie13; // frog sfx
            Item.consumable = true;
            Item.Calamity().devItem = true;
        }

        public override bool CanUseItem(Player player)
        {
            CalamityPlayer modPlayer = player.Calamity();
            if (modPlayer.nimbleBounderBoost)
            {
                return false;
            }
            return true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.itemAnimation > 0 && player.itemTime == 0)
            {
                player.itemTime = Item.useTime;
                CalamityPlayer modPlayer = player.Calamity();
                modPlayer.nimbleBounderBoost = true;
            }
            return true;
        }

        // Gives a purple light when dropped as an item
        public override void PostUpdate()
        {
            Lighting.AddLight((int)((Item.position.X + Item.width / 2) / 16f), (int)((Item.position.Y + Item.height / 2) / 16f), 0.51f, 0.14f, 0.57f);
        }
    }
}
