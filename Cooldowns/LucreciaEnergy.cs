using System;
using CalamityMod.CalPlayer;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Projectiles.Melee.Shortswords;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Cooldowns
{
    public class LucreciaEnergy : CooldownHandler
    {
        public float CompletionPercentage => MathF.Round(instance.Completion * 100);
        private bool IsEmpty => CompletionPercentage == 100;
        private float TextXOffset => -5.8f;
        private Vector2 TextPosition => new(TextXOffset, 15);
        private Color TextColor => Color.MediumPurple;
        private Color TextBorderColor = Color.AntiqueWhite;

        public static new string ID => "LucreciaEnergy";
        public override bool CanTickDown => false;
        public override LocalizedText DisplayName => CalamityUtils.GetText($"UI.Cooldowns.{ID}");
        public override bool ShouldDisplay => instance.player.HeldItem.type == ItemType<Lucrecia>();
        public override string Texture => "CalamityMod/Cooldowns/" + ID;

        public override Color CooldownStartColor => IsEmpty ? Color.DimGray : Color.Lerp(Color.Purple, Color.SlateGray, instance.Completion);
        public override Color CooldownEndColor => IsEmpty ? Color.DimGray : Color.Lerp(Color.DarkBlue, Color.SlateGray, instance.Completion);

        public override void DrawExpanded(SpriteBatch spriteBatch, Vector2 position, float opacity, float scale)
        {
            base.DrawExpanded(spriteBatch, position, opacity, scale);
            CalamityUtils.DrawBorderStringEightWay(spriteBatch, FontAssets.MouseText.Value, instance.player.Calamity().lucreciaEnergy.ToString(), position + TextPosition, TextColor, TextBorderColor);
        }

        public override void DrawCompact(SpriteBatch spriteBatch, Vector2 position, float opacity, float scale)
        {
            base.DrawCompact(spriteBatch, position, opacity, scale);
            CalamityUtils.DrawBorderStringEightWay(spriteBatch, FontAssets.MouseText.Value, instance.player.Calamity().lucreciaEnergy.ToString(), position + TextPosition, TextColor, TextBorderColor);
        }
    }
}
