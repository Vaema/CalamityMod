using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;

namespace CalamityMod.Cooldowns
{
    public class WardingWave : CooldownHandler
    {
        public static new string ID => "WardingWave";
        public override bool ShouldDisplay => true;
        public override LocalizedText DisplayName => CalamityUtils.GetText("UI.Cooldowns.WardingWave");
        public override string Texture => "CalamityMod/Cooldowns/WardingWave";
        public override Color OutlineColor => new Color(158, 158, 255);
        public override Color CooldownStartColor => new Color(72, 125, 204);
        public override Color CooldownEndColor => new Color(97, 200, 255);
        public override SoundStyle? EndSound => SoundID.Item85;
    }
}
