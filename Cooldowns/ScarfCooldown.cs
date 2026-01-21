using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Localization;

namespace CalamityMod.Cooldowns
{
    public class ScarfCooldown : CooldownHandler
    {
        public static new string ID => "ScarfCooldown";

        public override bool ShouldDisplay => true;
        public override LocalizedText DisplayName => CalamityUtils.GetText($"UI.Cooldowns.{ID}");
        public override string Texture => "CalamityMod/Cooldowns/" + skinTexture;
        public override Color OutlineColor => outlineColor;
        public override Color CooldownStartColor => cooldownColorStart;
        public override Color CooldownEndColor => cooldownColorEnd;

        // It's the same cooldown with different skins each time, basically.
        public string skinTexture;
        public Color outlineColor;
        public Color cooldownColorStart;
        public Color cooldownColorEnd;

        public ScarfCooldown() : this("") { }
        public ScarfCooldown(string skin)
        {
            switch (skin)
            {
                case "evasionscarf":
                    skinTexture = "EvasionScarf";
                    outlineColor = Color.Lerp(new Color(255, 194, 150), new Color(255, 160, 150), (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2f) * 0.5f + 0.5f);
                    cooldownColorStart = new Color(132, 23, 32);
                    cooldownColorEnd = new Color(164, 52, 45);
                    break;
                default:
                    skinTexture = "CounterScarf";
                    outlineColor = Color.Lerp(new Color(255, 115, 178), new Color(255, 76, 76), (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2f) * 0.5f + 0.5f);
                    cooldownColorStart = new Color(194, 75, 97);
                    cooldownColorEnd = new Color(255, 76, 76);
                    break;
            }
        }
    }
}
