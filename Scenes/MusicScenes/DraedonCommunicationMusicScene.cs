using CalamityMod.UI.DraedonSummoning;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Scenes.MusicScenes
{
    public class DraedonCommunicationMusicScene : ModSceneEffect
    {
        public override int Music => MusicLoader.GetMusicSlot("CalamityMod/Sounds/Music/DraedonTalk");
        public override SceneEffectPriority Priority => SceneEffectPriority.Environment;
        public override float GetWeight(Player player) => 1f;
        public override bool IsSceneEffectActive(Player player) => CodebreakerUI.DisplayingCommunicationText;
    }
}
