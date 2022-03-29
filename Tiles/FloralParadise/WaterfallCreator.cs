using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.FloralParadise
{
    public class WaterfallCreator : ModTile
    {
        public override void SetDefaults()
        {
            Main.tileSolid[Type] = false;
            Main.tileBlockLight[Type] = true;
            Main.tileBrick[Type] = true;

            CalamityUtils.MergeWithGeneral(Type);
            CalamityUtils.MergeWithOres(Type);
            CalamityUtils.MergeWithFloralParadise(Type);

            soundType = SoundID.Tink;

            AddMapEntry(new Color(33, 56, 27));
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (Main.netMode == NetmodeID.Server || !Main.rand.NextBool() || Main.gamePaused)
                return;

            for (int k = 0; k < 16; k++)
            {
                Vector2 waterVelocity = -Vector2.UnitY.RotatedByRandom(0.29f) * Main.rand.NextFloat(1f, 2.4f);
                float waterScale = Main.rand.NextFloat(0.08f, 0.12f);
                Color waterColor = Color.Lerp(Color.Cyan, Color.SkyBlue, Main.rand.NextFloat());
                Vector2 particlePosition = new Vector2(i + 0.5f, j + 0.5f) * 16f + Vector2.UnitY * Main.rand.NextFloat(24f);
                GeneralParticleHandler.SpawnParticle(new WaterFoamParticle(particlePosition, waterVelocity, 150, waterScale, waterColor));
            }
        }
    }
}
