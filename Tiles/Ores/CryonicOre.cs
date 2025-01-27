using CalamityMod.Systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.Ores
{
    public class CryonicOre : ModTile
    {
        public static readonly SoundStyle PreCryoHitSound = new("CalamityMod/Sounds/Custom/PlatingMine", 3);
        public override void SetStaticDefaults()
        {
            Main.tileLighted[Type] = true;
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileOreFinderPriority[Type] = 675;

            CalamityUtils.MergeWithGeneral(Type);
            CalamityUtils.MergeWithSnow(Type);

            TileID.Sets.Ore[Type] = true;

            AddMapEntry(new Color(0, 0, 150), CreateMapEntryName());
            MineResist = 2f;
            MinPick = 180;
            HitSound = SoundID.Tink;
            Main.tileSpelunker[Type] = true;
        }

        public override bool KillSound(int i, int j, bool fail)
        {
            HitSound = DownedBossSystem.downedCryogen ? SoundID.Tink : PreCryoHitSound;
            return base.KillSound(i, j, fail);
        }
        public override bool CanKillTile(int i, int j, ref bool blockDamaged) => DownedBossSystem.downedCryogen;
        public override bool CanExplode(int i, int j) => false;

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            TileFramingSystem.CustomMergeFrame(i, j, Type, TileID.SnowBlock, false, false, false);
            return false;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.02f;
            g = 0.02f;
            b = 0.06f;
        }
    }
}
