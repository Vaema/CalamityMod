using CalamityMod.DataStructures;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.DamageOverTime
{
    public class Shadowflame : ModBuff
    {
        public override LocalizedText DisplayName => Language.GetOrRegister("BuffName.ShadowFlame");
        public override LocalizedText Description => Language.GetOrRegister("BuffDescription.ShadowFlame");
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.LongerExpertDebuff[Type] = true;
            BuffDatasets.DebuffDataset[Type] = DebuffData.Shadowflame;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.Calamity().shadowflame = true;
        }

        internal static void DrawEffects(PlayerDrawSet drawInfo)
        {
            Player Player = drawInfo.drawPlayer;

            if (Main.rand.Next(5) < 4)
            {
                Dust flame = Dust.NewDustDirect(drawInfo.Position - new Vector2(2f), Player.width + 4, Player.height + 4, DustID.Shadowflame, Player.velocity.X * 0.4f, Player.velocity.Y * 0.4f, 100, default, 1.1f);
                flame.noGravity = true;
                flame.velocity *= 0.75f;
                flame.velocity.X *= 0.75f;
                flame.velocity.Y -= 3f;
                if (Main.rand.NextBool(4))
                {
                    flame.noGravity = false;
                    flame.scale *= 0.3f;
                }
            }
        }
    }
    public class ShadowflameIconItem : ModItem
    {
        private string BuffName = "Shadowflame";
        public override string Texture => $"CalamityMod/Buffs/DamageOverTime/{BuffName}";
        public override LocalizedText DisplayName => CalamityUtils.GetText($"Buffs.{BuffName}.DisplayName");
        public override LocalizedText Tooltip => CalamityUtils.GetText($"Buffs.{BuffName}.ItemTooltip");
    }
}
