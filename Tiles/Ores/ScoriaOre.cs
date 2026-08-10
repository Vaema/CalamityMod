using CalamityMod.Projectiles.Environment;
using CalamityMod.Tiles.Abyss;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.Ores;

[LegacyName("ChaoticOre")]
public class ScoriaOre : GlowMaskTile
{
    public override void SetupStatic()
    {
        Main.tileLighted[Type] = true;
        Main.tileSolid[Type] = true;
        Main.tileBlockLight[Type] = true;
        Main.tileOreFinderPriority[Type] = 850;

        CalamityUtils.MergeWithGeneral(Type);
        CalamityUtils.MergeWithAbyss(Type);

        TileID.Sets.Ore[Type] = true;

        DustType = DustID.Water_BloodMoon;
        AddMapEntry(new Color(167, 80, 22), CreateMapEntryName());
        MineResist = 3f;
        MinPick = 210;
        HitSound = SoundID.Tink;

        this.RegisterBlendMergeWith(ModContent.TileType<AbyssGravel>());
        this.RegisterBlendMergeWith(ModContent.TileType<PyreMantle>());
    }

    public override void NearbyEffects(int i, int j, bool closer)
    {
        Tile up = Main.tile[i, j - 1];
        Tile up2 = Main.tile[i, j - 2];
        if (closer && Main.rand.NextBool(30) && !up.HasTile && !up2.HasTile)
        {
            Dust dust;
            dust = Main.dust[Dust.NewDust(new Vector2(i * 16f, j * 16f), 16, 16, DustID.Flare, 0f, -10f, 47, new Color(255, 255, 255), 1.0465117f)];
            dust.noGravity = true;
            dust.fadeIn = 1.2209302f;

            dust = Main.dust[Dust.NewDust(new Vector2(i * 16f, j * 16f), 16, 16, DustID.Smoke, 0f, -1.9069767f, 195, new Color(255, 255, 255), 1f)];
            dust.noGravity = false;
            dust.fadeIn = 1.4209302f;

        }

        if (Main.gamePaused)
            return;

        if (closer && Main.rand.NextBool(400))
        {
            int tileLocationY = j + 1;
            if (Main.tile[i, tileLocationY] != null)
            {
                if (!Main.tile[i, tileLocationY].HasTile)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                        Projectile.NewProjectile(new EntitySource_WorldEvent(), (float)(i * 16 + 16), (float)(tileLocationY * 16 + 16), 0f, 0.1f, ModContent.ProjectileType<LavaChunk>(), 25, 2f, Main.myPlayer);
                }
            }
        }
    }

    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        World.Abyss.FillTileWithWater(i, j);
    }
    public override bool CanExplode(int i, int j)
    {
        return false;
    }

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        num = fail ? 1 : 3;
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
        r = 0.04f;
        g = 0.00f;
        b = 0.00f;
    }

    public override Color GetGlowMaskColor(int i, int j, TileDrawInfo drawData)
    {
        return Color.White * 0.195f;
    }
}
