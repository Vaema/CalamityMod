using System;
using CalamityMod.Systems;
using CalamityMod.Tiles.Astral;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Metadata;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.AstralSnow
{
    public class AstralIce : GlowMaskTile
    {
        public override string GlowMaskAsset => $"{Texture}Lightmask";
        public override void SetupStatic()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = false;
            Main.tileBrick[Type] = true;
            TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Ice"]);
            Main.tileLighted[Type] = true;
            CalamityUtils.MergeWithGeneral(Type);
            CalamityUtils.MergeWithSnow(Type);
            CalamityUtils.MergeAstralTiles(Type);

            DustType = DustID.ShadowbeamStaff;

            HitSound = SoundID.Item50;

            AddMapEntry(new Color(153, 143, 168));

            TileID.Sets.HasSlopeFrames[Type] = true;
            TileID.Sets.Ices[Type] = true;
            TileID.Sets.IcesSlush[Type] = true;
            TileID.Sets.IcesSnow[Type] = true;
            TileID.Sets.ChecksForMerge[Type] = true;
            TileID.Sets.Conversion.Ice[Type] = true;
            TileID.Sets.CanBeClearedDuringOreRunner[Type] = true;

            this.RegisterBlendMergeWith(ModContent.TileType<AstralSnow>());
            this.RegisterBlendMergeWith(ModContent.TileType<AstralDirt>());
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override void FloorVisuals(Player player)
        {
            player.slippy = true;
            base.FloorVisuals(player);
        }

        public override bool IsTileBiomeSightable(int i, int j, ref Color sightColor)
        {
            sightColor = Color.Cyan;
            return true;
        }
        public override Color GetGlowMaskColor(int i, int j, TileDrawInfo drawData)
        {
            int time = (int)(Main.timeForVisualEffects * 0.11);
            float brightness = 0.2f;
            brightness += (float)MathF.Sin(-j / 1f + Main.GameUpdateCount * 0.002f + -i / 30f);
            brightness -= (float)MathF.Sin(j / 8f + Main.GameUpdateCount * 0.002f - i / 11f);
            brightness -= (float)MathF.Sin(j / 1f + Main.GameUpdateCount * 0.001f - i / 2f);
            brightness -= (float)MathF.Sin(-j / 2f + Main.GameUpdateCount * 0.003f - i / 4f);
            brightness += (float)MathF.Sin(-j / 4f + Main.GameUpdateCount * 0.003f - i / 8f);
            return new Color(brightness, brightness, brightness);
        }
        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            return TileFramingSystem.BetterGemsparkFraming(i, j, resetFrame);
        }
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            float brightness = 0.2f;
            brightness *= (float)MathF.Sin(-j / 40f + Main.GameUpdateCount * 0.005f + i / 8f);
            r = 131f / 1500f;
            g = 111f / 1500f;
            b = 171f / 1500f;
            brightness += (float)MathF.Sin(-j / 1f + Main.GameUpdateCount * 0.002f + -i / 30f);
            brightness -= (float)MathF.Sin(j / 8f + Main.GameUpdateCount * 0.002f - i / 11f);
            brightness -= (float)MathF.Sin(j / 1f + Main.GameUpdateCount * 0.001f - i / 2f);
            brightness -= (float)MathF.Sin(-j / 2f + Main.GameUpdateCount * 0.003f - i / 4f);
            brightness += (float)MathF.Sin(-j / 4f + Main.GameUpdateCount * 0.003f - i / 8f);

            brightness -= 0.05f;
            r *= brightness;
            g *= brightness;
            b *= brightness;
        }
    }
}
