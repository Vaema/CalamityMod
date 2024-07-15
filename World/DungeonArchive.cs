using System;
using System.Collections.Generic;
using System.Linq;
using CalamityMod.Schematics;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.Utilities;
using Terraria.WorldBuilding;
using static CalamityMod.Schematics.SchematicManager;

namespace CalamityMod.World
{
    public class DungeonArchive
    {
        public static void PlaceArchive()
        {
            int worldThird = Main.maxTilesX / 3;

            int dungeonArchiveColor = 0; //0 = blue, 1 = green, 2 = pink

            //start much higher above the top of hell so it doesnt get nuked by the crags generation, and so it wont generate too low
            for (int j = Main.maxTilesY - 380; j > 0; j--)
            {
                int i = 100;
                if (GenVars.dungeonSide == 1)
                {
                    i = Main.maxTilesX - 100;
                }

                bool shouldContinue = true;
                bool placedArchive = false;

                while (shouldContinue)
                {
                    if (GenVars.dungeonSide == 1)
                    {
                        i--;
                        if (i < Main.maxTilesX - worldThird)
                        {
                            shouldContinue = false;
                        }
                    }
                    else
                    {
                        i++;
                        if (i > worldThird)
                        {
                            shouldContinue = false;
                        }
                    }

                    Tile tile = Main.tile[i, j];
                    Tile tileUp1 = Main.tile[i, j - 1];
                    Tile tileUp2 = Main.tile[i, j - 2];
                    Tile tileUp3 = Main.tile[i, j - 3];
                    Tile tileUp4 = Main.tile[i, j - 4];
                    Tile tileUp5 = Main.tile[i, j - 5];

                    if (Main.tileDungeon[tile.TileType] && !tileUp1.HasTile && !tileUp2.HasTile && !tileUp3.HasTile && !tileUp4.HasTile && !tileUp5.HasTile)
                    {
                        //determine the archive brick color
                        if (tile.TileType == TileID.BlueDungeonBrick)
                            dungeonArchiveColor = 0;
                        else if (tile.TileType == TileID.GreenDungeonBrick)
                            dungeonArchiveColor = 1;
                        else if (tile.TileType == TileID.PinkDungeonBrick)
                            dungeonArchiveColor = 2;

                        placedArchive = true;
                        break;
                    }
                }

                if (placedArchive)
                {
                    bool firstItem = false;

                    if (dungeonArchiveColor == 0)
                    {
                        PlaceSchematic(BlueArchiveKey, new Point(i, j), SchematicAnchor.TopCenter,
                        ref firstItem, new Action<Chest, int, bool>(FillArchiveChests));
                    }
                    if (dungeonArchiveColor == 1)
                    {
                        PlaceSchematic(GreenArchiveKey, new Point(i, j), SchematicAnchor.TopCenter,
                        ref firstItem, new Action<Chest, int, bool>(FillArchiveChests));
                    }
                    if (dungeonArchiveColor == 2)
                    {
                        PlaceSchematic(PinkArchiveKey, new Point(i, j), SchematicAnchor.TopCenter,
                        ref firstItem, new Action<Chest, int, bool>(FillArchiveChests));
                    }

                    // Paint the archives in secret seeds
                    byte PaintType = 0;
                    if (Main.remixWorld)
                    {
                        PaintType = PaintID.DeepPurplePaint;
                        if (Main.drunkWorld && (GenVars.crimsonLeft && i < Main.maxTilesX / 2) || (!GenVars.crimsonLeft && i > Main.maxTilesX / 2))
                            PaintType = PaintID.DeepRedPaint;
                        else if (!Main.drunkWorld && WorldGen.crimson)
                            PaintType = PaintID.DeepRedPaint;
                    }
                    else if (Main.tenthAnniversaryWorld)
                        PaintType = PaintID.DeepPinkPaint;
                    else if (Main.notTheBeesWorld)
                        PaintType = PaintID.DeepOrangePaint;
                    else if (Main.drunkWorld || Main.getGoodWorld)
                    {
                        switch (dungeonArchiveColor)
                        {
                            case 0:
                                PaintType = (byte)WorldGen.genRand.Next(19, 23);
                                break;
                            case 1:
                                PaintType = (byte)WorldGen.genRand.Next(15, 19);
                                break;
                            default:
                                PaintType = WorldGen.genRand.NextBool(2) ? (byte)WorldGen.genRand.Next(23, 25) : (byte)WorldGen.genRand.Next(13, 15);
                                break;
                        }
                    }

                    if (PaintType == 0)
                        return;

                    bool BlackDungeonWall = Main.drunkWorld || Main.remixWorld || Main.getGoodWorld;
                    // All the variants are the same size so just pick one of them
                    // Note that it is anchored top center so the for loop check has to be changed accordingly
                    Vector2 schematicSize = new Vector2(TileMaps[BlueArchiveKey].GetLength(0), TileMaps[BlueArchiveKey].GetLength(1)) + Vector2.One;
                    for (int x = i - (int)schematicSize.X; x < i + schematicSize.X; x++)
                    {
                        for (int y = j; y < j + schematicSize.Y; y++)
                        {
                            Tile tile = CalamityUtils.ParanoidTileRetrieval(x, y);
                            if (Main.tileDungeon[tile.TileType] || TileID.Sets.CrackedBricks[tile.TileType])
                                tile.TileColor = PaintType;
                            if (tile.TileType == TileID.Platforms) // Dungeon platforms
                            {
                                int variant = tile.TileFrameY / 18;
                                if (variant >= 6 && variant <= 12)
                                    tile.TileColor = PaintType;
                            }
                            if (Main.wallDungeon[tile.WallType])
                                tile.WallColor = BlackDungeonWall ? PaintID.BlackPaint : PaintType;
                        }
                    }
                    break;
                }
            }
        }

        public static void FillArchiveChests(Chest chest, int Type, bool firstItem)
        {
            int potionType1 = Utils.SelectRandom(WorldGen.genRand, ItemID.HunterPotion, ItemID.IronskinPotion);
            int potionType2 = Utils.SelectRandom(WorldGen.genRand, ItemID.ShinePotion, ItemID.SwiftnessPotion);
            List<ChestItem> contents1 = new List<ChestItem>()
            {
                new ChestItem(ItemID.ShadowKey, 1),
                new ChestItem(ItemID.HealingPotion, WorldGen.genRand.Next(10, 20)),
                new ChestItem(ItemID.ManaPotion, WorldGen.genRand.Next(10, 20)),
                new ChestItem(potionType1, WorldGen.genRand.Next(4, 8)),
                new ChestItem(potionType2, WorldGen.genRand.Next(4, 8)),
                new ChestItem(ItemID.GoldCoin, WorldGen.genRand.Next(5, 10)),
            };

            List<ChestItem> contents2 = new List<ChestItem>()
            {
                new ChestItem(ItemID.SpellTome, WorldGen.genRand.Next(2, 3)),
                new ChestItem(ItemID.Book, WorldGen.genRand.Next(12, 25)),
                new ChestItem(ItemID.TallyCounter, 1),
                new ChestItem(potionType1, WorldGen.genRand.Next(4, 8)),
                new ChestItem(potionType2, WorldGen.genRand.Next(4, 8)),
                new ChestItem(ItemID.GoldCoin, WorldGen.genRand.Next(5, 10)),
            };

            //this is normally not a good idea with separate items lists, but both lists are the same size so it is fine here
            for (int i = 0; i < contents1.Count; i++)
            {
                if (!firstItem)
                {
                    chest.item[i].SetDefaults(contents1[i].Type);
                    chest.item[i].stack = contents1[i].Stack;
                }
                else
                {
                    chest.item[i].SetDefaults(contents2[i].Type);
                    chest.item[i].stack = contents2[i].Stack;
                }
            }
        }
    }
}
