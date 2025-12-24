using System;
using System.Windows.Markup;
using CalamityMod.CalPlayer;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Items.Weapons.Ranged;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Cooldowns
{
    public class Starburst : CooldownHandler
    {
        public float CompletionPercentage => MathF.Round(instance.Completion * 100);
        private bool IsEmpty => CompletionPercentage == 100;
        private float TextXOffset => -5.4f;
        private Vector2 TextPosition => new(TextXOffset, 15);
        private Color TextColor => Color.Lerp(Color.LightSkyBlue,Color.White,(MathF.Sin(Main.GlobalTimeWrappedHourly)+1)*0.5f);
        private Color TextBorderColor = Color.DarkSlateBlue;

        public static new string ID => "Starburst";
        public override bool CanTickDown => false;
        public override LocalizedText DisplayName => CalamityUtils.GetText($"UI.Cooldowns.{ID}");
        public override bool ShouldDisplay => instance.player.Calamity().StratusStarburstResetTimer > 0;
        public override string Texture => "CalamityMod/Cooldowns/" + ID;

        public override Color CooldownStartColor => IsEmpty ? Color.DimGray : Color.Lerp(Color.SkyBlue, Color.DeepSkyBlue, instance.Completion);
        public override Color CooldownEndColor => IsEmpty ? Color.DimGray : Color.Lerp(Color.SkyBlue, Color.DeepSkyBlue, instance.Completion);
        public override void DrawExpanded(SpriteBatch spriteBatch, Vector2 position, float opacity, float scale)
        {
            base.DrawExpanded(spriteBatch, position, opacity, scale);
            var value = CalamityPlayer.MaxStratusStarburst - instance.timeLeft;
            float Xoffset = value > 9 ? value > 99 ? -12.5f : -10f : -5;
            CalamityUtils.DrawBorderStringEightWay(spriteBatch, FontAssets.MouseText.Value, (value).ToString(), position + new Vector2(Xoffset, 4) * scale, TextColor, TextBorderColor, scale);
        }

        public override void DrawCompact(SpriteBatch spriteBatch, Vector2 position, float opacity, float scale)
        {
            base.DrawCompact(spriteBatch, position, opacity, scale);

            CalamityUtils.DrawBorderStringEightWay(spriteBatch, FontAssets.MouseText.Value, MathHelper.Min(instance.player.Calamity().StratusStarburst, CalamityPlayer.MaxStratusStarburst).ToString(), position + TextPosition, TextColor, TextBorderColor);
        }
    }
}
