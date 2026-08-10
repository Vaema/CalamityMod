using System.Linq;
using CalamityMod.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.Abyss;

public class ThermalTorch : ModTile
{
    public Asset<Texture2D> FlameTexture;

    public override void SetStaticDefaults() => this.SetUpTorch(ModContent.ItemType<Items.Placeables.Furniture.ThermalTorch>(), true, true);

    public override bool CreateDust(int i, int j, ref int type)
    {
        Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.Stone, 0f, 0f, 1, new Color(200, 0, 0), 1f);
        return false;
    }

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        num = fail ? 1 : 3;
    }

    public override void MouseOver(int i, int j)
    {
        Player player = Main.LocalPlayer;
        player.noThrow = 2;
        player.cursorItemIconEnabled = true;
        player.cursorItemIconID = ModContent.ItemType<Items.Placeables.Furniture.ThermalTorch>();
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
        Tile tile = Main.tile[i, j];
        if (tile.TileFrameX < 66)
        {
            r = 2f;
            g = 0.5f;
            b = 0.5f;
        }
    }

    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY)
    {
        offsetY = 0;
        if (WorldGen.SolidTile(i, j - 1))
        {
            offsetY = 2;
            if (WorldGen.SolidTile(i - 1, j + 1) || WorldGen.SolidTile(i + 1, j + 1))
            {
                offsetY = 4;
            }
        }
    }

    public override void NearbyEffects(int i, int j, bool closer)
    {
        //This makes the placed torch cut through the abyss darkness.
        var pos = new Point(i, j).ToWorldCoordinates();
        if (!closer && Main.LocalPlayer.Calamity().ZoneAbyss && !Main.gamePaused)
        {
            if (EnhancedDarknessSystem.lights.Any(x => x.center == pos))
            {
                var e = EnhancedDarknessSystem.lights.First(x => x.center == pos);
                e.lifetime = 5;
            }
            else
                EnhancedDarknessSystem.lights.Add(new EnhancedDarknessSystem.LightSource(pos, scale: 2f) { lifetime = 5 });
        }
    }

    public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
    {
        FlameTexture ??= ModContent.Request<Texture2D>("CalamityMod/Tiles/Abyss/ThermalTorchFlame");
        CalamityUtils.DrawFlameEffect(FlameTexture.Value, i, j, 2);
    }

    public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
    {
        if (Main.tile[i, j].TileFrameX < 66)
            CalamityUtils.DrawFlameSparks(DustID.RedTorch, 5, i, j);
    }

    public override bool RightClick(int i, int j)
    {
        FurnitureCommon.RightClickBreak(i, j);
        return true;
    }

    public override float GetTorchLuck(Player player)
    {
        // Note: Total Torch luck never goes below zero
        return player.Calamity().ZoneAbyss ? 1f : -1f; // Abyss Torch gives positive luck when in the Abyss, otherwise some negative luck
    }
}
