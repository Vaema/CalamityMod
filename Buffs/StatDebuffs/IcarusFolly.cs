using CalamityMod.Dusts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.StatDebuffs
{
    public class IcarusFolly : ModBuff
    {
        public static int MaxFlightTimeCap = 400;
        public static float FlightTimeLossPercent = 0.34f;
        public override LocalizedText Description => base.Description.WithFormatArgs(FlightTimeLossPercent.ToPercent());

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.Calamity().icarusFolly = true;
        }

        internal static void DrawEffects(PlayerDrawSet drawInfo)
        {
            Player Player = drawInfo.drawPlayer;

            if (Main.rand.NextBool(12))
            {
                Dust dust = Dust.NewDustDirect(drawInfo.Position - new Vector2(2f), Player.width + 4, Player.height + 4, (int)CalamityDusts.ProfanedFire, Player.velocity.X * 0.4f, Player.velocity.Y * 0.4f, 100, default, 3f);
                dust.noGravity = true;
                dust.velocity *= 1.8f;
                dust.velocity.Y -= 0.5f;
                drawInfo.DustCache.Add(dust.dustIndex);
            }
        }
    }
}
