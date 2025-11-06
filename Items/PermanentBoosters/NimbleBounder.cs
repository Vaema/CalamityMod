using System.Collections.Generic;
using CalamityMod.CalPlayer;
using CalamityMod.Rarities;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.PermanentBoosters
{
    public class NimbleBounder : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Misc";

        public static float MoveSpeedBoost = 0.05f;
        public static float JumpSpeedBoost = 0.25f; // This also amounts to 5% so the tooltip is shared with movement speed
        public static float AccelerationBoost = 0.1f;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MoveSpeedBoost.ToPercent(), AccelerationBoost.ToPercent());

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.consumable = true;
            Item.useAnimation = Item.useTime = 30;
            Item.UseSound = SoundID.Zombie13; // frog sfx
            Item.useStyle = ItemUseStyleID.HoldUp;
            // Same price as Frog Leg, which is used to shimmer into it
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ModContent.RarityType<HotPink>();
            Item.maxStack = Item.CommonMaxStack;
            Item.Calamity().devItem = true;
        }

        public static bool HasConsumedBefore(Player player) => player.Calamity().nimbleBounderBoost;

        public override bool CanUseItem(Player player)
        {
            if (HasConsumedBefore(player))
            {
                // Refuse Text can be added on here
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

        public override void ModifyTooltips(List<TooltipLine> list)
        {
            if (HasConsumedBefore(Main.LocalPlayer))
                list.AddConsumedTooltip();
        }
    }
}
