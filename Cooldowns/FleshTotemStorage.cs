using CalamityMod.CalPlayer;
using CalamityMod.Items.Accessories;
using Microsoft.Xna.Framework;
using Terraria.Localization;

namespace CalamityMod.Cooldowns
{
    public class FleshTotemStorage : CooldownHandler
    {
        public static new string ID => "FleshTotemStorage";

        private CalamityPlayer CalPlayer => instance.player.Calamity();
        public override bool ShouldDisplay => CalPlayer.fleshTotem;
        public override LocalizedText DisplayName => CalamityUtils.GetText($"UI.Cooldowns.{ID}");
        public override string Texture => "CalamityMod/Cooldowns/FleshTotem";
        public override Color OutlineColor => new Color(157, 248, 234);
        public override Color CooldownStartColor => new Color(111, 169, 241);
        public override Color CooldownEndColor => new Color(111, 169, 241);
    }
}
