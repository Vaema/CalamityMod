using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Systems
{
    public class MonolithDistortionScene : ModSceneEffect
    {
        public override SceneEffectPriority Priority => SceneEffectPriority.Environment;

        public override bool IsSceneEffectActive(Player player) => Main.LocalPlayer.Calamity().monolithDevourerBShader > 0 || Main.LocalPlayer.Calamity().monolithDevourerPShader > 0;

        public override void SpecialVisuals(Player player, bool isActive)
        {
            player.ManageSpecialBiomeVisuals("CalamityMod:DevourerofGodsHead", isActive);
        }
    }
}
