using CalamityMod.Dusts;
using CalamityMod.Items.Placeables.Furniture.Monoliths;
using CalamityMod.Particles;
using CalamityMod.Tiles.BaseTiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace CalamityMod.Tiles.Furniture.Monoliths;

public class PlagueHumidifierTile : BaseMonolith
{
    public override int TileWidth => 2;
    public override int TileHeight => 4;
    public override int AnimationFrameCount => 4;
    public override int AnimationDelay => 8;
    public override int CursorItemType => ModContent.ItemType<PlagueHumidifier>();

    public override void SetStaticDefaults()
    {
        RegisterItemDrop(ModContent.ItemType<PlagueHumidifier>());
        Main.tileFrameImportant[Type] = true;
        TileID.Sets.HasOutlines[Type] = true;
        TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
        TileObjectData.newTile.Height = 4;
        TileObjectData.newTile.Origin = new Point16(1, 3);
        TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16, 18 };
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newTile.UsesCustomCanPlace = true;
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop, 2, 0);
        
        AnimationFrameHeight = TileObjectData.newTile.CoordinateFullHeight;
        TileObjectData.addTile(Type);

        AddMapEntry(new Color(44, 150, 54));

        DustType = (int)CalamityDusts.Plague;
    }

    public override void NearbyEffects(int i, int j, bool closer, bool monolithEnabled, Player localPlayer)
    {
        if (!monolithEnabled)
            return;

        if (localPlayer is null || !localPlayer.active)
            return;

        localPlayer.Calamity().monolithPlagueShader = 30;

        // Spawn mist particles at the bottom tiles
        if (Main.tile[i, j + 1].TileType != Type && Main.rand.NextBool(30))
        {
            var pos = new Vector2(i * 16, j * 16) + new Vector2(Main.rand.NextFloat(0, 16), Main.rand.NextFloat(16));
            var lifeTime = Main.rand.Next(40, 80);
            var size = Main.rand.NextFloat(0.8f, 1.2f);
            var speed = Vector2.UnitX * Main.rand.NextFloat(-0.8f, 0.8f);
            var particle = new PlagueHumidifierMist(pos, lifeTime, size, speed);
            GeneralParticleHandler.SpawnParticle(particle);
        }
    }
}
