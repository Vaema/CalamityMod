using System.Collections.Generic;
using CalamityMod.Items.TreasureBags.MiscGrabBags;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace CalamityMod.Tiles.Abyss
{
    public class AbyssalPots : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileLavaDeath[Type] = true;
            Main.tileWaterDeath[Type] = false;
            Main.tileOreFinderPriority[Type] = (short)100;
            Main.tileSpelunker[Type] = true;
            Main.tileCut[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.newTile.DrawYOffset = 2;
            TileObjectData.addTile(Type);
            AddMapEntry(new Color(47, 79, 79), Language.GetText("MapObject.Pot")); // dark slate gray
            DustType = 29;
            HitSound = SoundID.Shatter;
        }

        public override IEnumerable<Item> GetItemDrops(int i, int j)
        {
            Tile tileAtPosition = CalamityUtils.ParanoidTileRetrieval(i, j);
            if (tileAtPosition.TileFrameX % 36 == 0 && tileAtPosition.TileFrameY % 36 == 0)
            {
                if (!Main.dedServ)
                {
                    int goreAmt = Main.rand.Next(2, 4 + 1);
                    for (int k = 0; k < goreAmt; k++)
                    {
                        Gore.NewGore(new EntitySource_TileBreak(i, j), new Vector2(i, j) * 16, Main.rand.NextVector2CircularEdge(3f, 3f), Mod.Find<ModGore>($"AbyssPot{WorldGen.genRand.Next(1, 6 + 1)}").Type);
                    }
                }

                // Coin Portal @ 0.25% (varies with luck)
                if (Player.GetClosestRollLuck(i, j, 400) == 0f)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                        Projectile.NewProjectile(new EntitySource_TileBreak(i, j), i * 16 + 16, j * 16 + 16, 0f, -12f, ProjectileID.CoinPortal, 0, 0f, Main.myPlayer);
                }
                // FTW/GFB drop bombs @ 16.63%
                else if (Main.getGoodWorld && Main.rand.NextBool(6))
                    Projectile.NewProjectile(new EntitySource_TileBreak(i, j), i * 16 + 16, j * 16 + 8, (float)Main.rand.Next(-100, 101) * 0.002f, 0f, ProjectileID.Bomb, 0, 0f, Player.FindClosest(new Vector2(i * 16, j * 16), 16, 16));
                // Remix/GFB drop fallen stars @ 19.95% (@ 16.68% GFB)
                else if (Main.remixWorld && Main.rand.NextBool(5))
                    yield return new Item(ItemID.FallenStar);
                else
                    yield return new Item(ModContent.ItemType<AbyssalTreasure>());
            }
        }

        public override bool CreateDust(int i, int j, ref int type)
        {
            if (Main.rand.NextBool())
                type = 29;
            else
                type = 186;

            return true;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }
    }
}
