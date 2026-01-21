using System;
using Microsoft.Xna.Framework;
using Terraria;

namespace CalamityMod.Utilities
{
    public class ExtraArmAnimations
    {
        public static void ThrowArmAnimationSlow(Player player, Item item)
        {
            player.Calamity().mouseWorldListener = true;

            // 15NOV2024: Ozzatron: clamped mouse position unnecessary, only used for direction
            Vector2 mW = player.Calamity().mouseWorld;

            float value1 = player.direction * -90;
            float value2 = player.direction * 180;
            float value3 = player.direction * -240;

            float g1 = (float)item.useAnimation;
            float g2 = (float)player.itemAnimation;

            float gg = g2 / g1;

            player.direction = Math.Sign(mW.X - player.Center.X);
            if (player.direction == 0) player.direction = 1;

            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, player.AngleTo(mW) + (player.direction == 1 ? MathHelper.ToRadians(180) : 0) + MathHelper.ToRadians(MathHelper.Lerp(value2, value1, CalamityUtils.SineInOutEasing(gg, 1))));
            player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, player.AngleTo(mW) + (player.direction == 1 ? MathHelper.ToRadians(180) : 0) + MathHelper.ToRadians(MathHelper.Lerp(value1, value3, CalamityUtils.SineInOutEasing(gg, 1))));
        }
        public static void ThrowArmAnimationFast(Player player, Item item)
        {
            player.Calamity().mouseWorldListener = true;

            // 15NOV2024: Ozzatron: clamped mouse position unnecessary, only used for direction
            Vector2 mW = player.Calamity().mouseWorld;

            float value1 = player.direction * -90;
            float value2 = player.direction * 180;
            float value3 = player.direction * -240;

            float g1 = (float)item.useAnimation;
            float g2 = (float)player.itemAnimation;

            float gg = g2 / g1;

            player.direction = Math.Sign(mW.X - player.Center.X);
            if (player.direction == 0) player.direction = 1;

            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, player.AngleTo(mW) + (player.direction == 1 ? MathHelper.ToRadians(180) : 0) + MathHelper.ToRadians(MathHelper.Lerp(value2, value1, CalamityUtils.CircInEasing(gg, 1))));
            player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, player.AngleTo(mW) + (player.direction == 1 ? MathHelper.ToRadians(180) : 0) + MathHelper.ToRadians(MathHelper.Lerp(value1, value3, CalamityUtils.CircInEasing(gg, 1))));
        }
    }
}
