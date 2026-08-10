using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Shaders;
using Terraria.Localization;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Cooldowns;

public class KillMode : CooldownHandler
{
    public static new string ID => "KillMode";
    public override bool ShouldDisplay => true;
    public static int cooldownMax = 300;
    public static int buffMax = 180;
    public override LocalizedText DisplayName => CalamityUtils.GetText("UI.Cooldowns.KillMode" + (instance.timeLeft > cooldownMax ? "Active" : "Cooldown"));
    public override string Texture => "CalamityMod/Cooldowns/KillMode";
    public override string OutlineTexture => "CalamityMod/Cooldowns/KillModeOutline";
    public override string OverlayTexture => "CalamityMod/Cooldowns/KillModeOverlay";
    public override Color OutlineColor => instance.timeLeft > cooldownMax ? Color.MediumOrchid : Color.BlueViolet;
    public override Color CooldownStartColor => instance.timeLeft > cooldownMax ? Color.Lerp(Color.Indigo, Color.DarkMagenta, (instance.timeLeft - cooldownMax) / buffMax) : Color.MediumOrchid;
    public override Color CooldownEndColor => instance.timeLeft > cooldownMax ? Color.Lerp(Color.BlueViolet, Color.DarkOrchid, (instance.timeLeft - cooldownMax) / buffMax) : Color.DarkOrchid;
    public override SoundStyle? EndSound => new("CalamityMod/Sounds/Item/DemonSwordKillModeOffCooldown");

    //Charge down at first, and then charge back up
    private float AdjustedCompletion => instance.timeLeft > cooldownMax ? Utils.GetLerpValue(cooldownMax, cooldownMax + buffMax, instance.timeLeft) : Utils.GetLerpValue(cooldownMax, 0, instance.timeLeft);
    public override void Tick()
    {

    }
    public override void ApplyBarShaders(float opacity)
    {
        //Use the adjusted completion
        GameShaders.Misc["CalamityMod:CircularBarShader"].UseOpacity(opacity);
        GameShaders.Misc["CalamityMod:CircularBarShader"].UseSaturation(AdjustedCompletion);
        GameShaders.Misc["CalamityMod:CircularBarShader"].UseColor(CooldownStartColor);
        GameShaders.Misc["CalamityMod:CircularBarShader"].UseSecondaryColor(CooldownEndColor);
        GameShaders.Misc["CalamityMod:CircularBarShader"].Apply();
    }

    public override void DrawCompact(SpriteBatch spriteBatch, Vector2 position, float opacity, float scale)
    {
        Texture2D sprite = Request<Texture2D>(Texture).Value;
        Texture2D outline = Request<Texture2D>(OutlineTexture).Value;
        Texture2D overlay = Request<Texture2D>(OverlayTexture).Value;

        //Draw the outline
        spriteBatch.Draw(outline, position, null, OutlineColor * opacity, 0, outline.Size() * 0.5f, scale, SpriteEffects.None, 0f);

        //Draw the icon
        spriteBatch.Draw(sprite, position, null, Color.White * opacity, 0, sprite.Size() * 0.5f, scale, SpriteEffects.None, 0f);

        //Draw the small overlay
        int lostHeight = (int)Math.Ceiling(overlay.Height * AdjustedCompletion);
        Rectangle crop = new Rectangle(0, lostHeight, overlay.Width, overlay.Height - lostHeight);
        spriteBatch.Draw(overlay, position + Vector2.UnitY * lostHeight * scale, crop, OutlineColor * opacity * 0.9f, 0, sprite.Size() * 0.5f, scale, SpriteEffects.None, 0f);
    }
}
