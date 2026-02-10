using CalamityMod.NPCs;
using CalamityMod.NPCs.Providence;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Systems
{
    public class ProvidenceMusicScene : BaseMusicSceneEffect
    {
        public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;

        public override int NPCType => ModContent.NPCType<Providence>();
        public static int ProvidenceTrack => (int)CalamityMod.Instance.GetMusicFromMusicMod("Providence");
        public static int SilenceTrack => MusicLoader.GetMusicSlot(CalamityMod.Instance, "Sounds/Music/Silence");
        public override int? MusicModMusic => ProvidenceSpawnState() < 180f && ProvUtils.StandardAI() ? SilenceTrack : ProvidenceTrack;
        public override int VanillaMusic => MusicID.LunarBoss;
        public override int OtherworldMusic => MusicID.OtherworldlyLunarBoss;
        public override void SpecialVisuals(Player player, bool isActive)
        {
            if (ProvidenceSpawnState() == 180f && ProvUtils.StandardAI())
                Main.musicFade[ProvidenceTrack] = 1f;
        }
        public static float ProvidenceSpawnState()
        {
            int provIndex = CalamityGlobalNPC.holyBoss;
            if (provIndex < 0 || provIndex >= Main.maxNPCs || !Main.npc[provIndex].active)
                return -1f;

            var prov = Main.npc[provIndex];
            return prov.Calamity().newAI[3];
        }
    }
}
