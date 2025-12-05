using System;
using System.Collections.Generic;
using CalamityMod.Systems;
using CalamityMod.Tiles.Astral;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Metadata;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.AstralSnow
{
    public class AstralSnow : GlowMaskTile
    {
        public override string GlowMaskAsset => "CalamityMod/Tiles/AstralSnow/AstralSnowLightmask";
        public GrayscaleTexture2D NoiseTexture;
        public override void SetupStatic()
        {
            NoiseTexture = new("CalamityMod/ExtraTextures/GreyscaleGradients/BlobbyNoise");

            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileBrick[Type] = true;
            TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Snow"]);

            CalamityUtils.MergeWithGeneral(Type);
            CalamityUtils.MergeWithSnow(Type);
            CalamityUtils.MergeAstralTiles(Type);

            DustType = DustID.ShadowbeamStaff;

            HitSound = SoundID.Item48;

            AddMapEntry(new Color(189, 211, 221));
            TileID.Sets.HasSlopeFrames[Type] = true;
            TileID.Sets.Snow[Type] = true;
            TileID.Sets.Conversion.Snow[Type] = true;
            TileID.Sets.ChecksForMerge[Type] = true;
            TileID.Sets.CanBeClearedDuringOreRunner[Type] = true;

            this.RegisterBlendMergeWith(ModContent.TileType<AstralDirt>());
            this.RegisterBlendMergeWith(TileID.SnowBlock);
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }
        public override void OnUnload()
        {
            NoiseTexture?.Unload();
            NoiseTexture = null;
        }

        public override bool IsTileBiomeSightable(int i, int j, ref Color sightColor)
        {
            sightColor = Color.Cyan;
            return true;
        }
        public override Color GetGlowMaskColor(int i, int j, TileDrawInfo drawData)
        {
            int time = (int)(Main.timeForVisualEffects * 0.11);
            float brightness = 1.0f - NoiseTexture.GetRepeat((i * 60) + time, (j * 50) + time);
            brightness += (float)MathF.Sin(-j / 1f + Main.GameUpdateCount * 0.02f + -i / 30f);
            brightness -= (float)MathF.Sin(j / 8f + Main.GameUpdateCount * 0.02f - i / 11f);
            brightness -= (float)MathF.Sin(j / 1f + Main.GameUpdateCount * 0.01f - i / 2f);
            brightness -= (float)MathF.Sin(-j / 2f + Main.GameUpdateCount * 0.03f - i / 4f);
            brightness += (float)MathF.Sin(-j / 4f + Main.GameUpdateCount * 0.03f - i / 8f);
            return new Color(brightness, brightness, brightness);
        }
        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            return TileFramingSystem.BetterGemsparkFraming(i, j, resetFrame);
        }
    }
}
