using CalamityMod.NPCs.Providence;
using CalamityMod.Projectiles.Boss;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Systems
{
    public class ProvidenceMusicScene : BaseMusicSceneEffect
    {
        public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;

        public override int NPCType => ModContent.NPCType<Providence>();
        public override int? MusicModMusic
        {
            get
            {
                for (int i = 0; i < Main.projectile.Length; i++)
                {
                    Projectile proj = Main.projectile[i];
                    if (proj.type == ModContent.ProjectileType<HolyAura>() && proj.timeLeft > 35)
                    {
                        return MusicLoader.GetMusicSlot(Mod, "Sounds/Music/Silence");
                    }
                }
                return CalamityMod.Instance.GetMusicFromMusicMod("Providence");
            }
        }
        public override int VanillaMusic => MusicID.LunarBoss;
        public override int OtherworldMusic => MusicID.OtherworldlyLunarBoss;
        public override void SpecialVisuals(Player player, bool isActive)
        {
            for (int i = 0; i < Main.projectile.Length; i++)
            {
                Projectile proj = Main.projectile[i];
                if (isActive && proj.type == ModContent.ProjectileType<HolyAura>() && proj.timeLeft <= 35)
                {
                    Main.musicFade[Main.curMusic] = 1f;
                }
            }
        }
    }
}
