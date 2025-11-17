using System;
using CalamityMod.Dusts;
using CalamityMod.Gores.Trees;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Potions.Food;
using CalamityMod.Items.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.AstralDesert
{
    public class AstralPalmTree : GlowMaskPalmTree
    {
        public override void SetStaticDefaults()
        {
            // Grows on astral sand
            GrowsOnTileId = new int[1] { ModContent.TileType<AstralSand>() };
        }

        //Copypasted from vanilla, just as ExampleMod did, due to the lack of proper explanation
        public override TreePaintingSettings TreeShaderSettings => new TreePaintingSettings
        {
            UseSpecialGroups = true,
            SpecialGroupMinimalHueValue = 11f / 72f,
            SpecialGroupMaximumHueValue = 0.25f,
            SpecialGroupMinimumSaturationValue = 0.88f,
            SpecialGroupMaximumSaturationValue = 1f
        };

        public override Asset<Texture2D> GetTexture() => ModContent.Request<Texture2D>("CalamityMod/Tiles/AstralDesert/AstralPalmTree");
        public override Asset<Texture2D> GetGlowTexture() => ModContent.Request<Texture2D>("CalamityMod/Tiles/AstralDesert/AstralPalmTreeGlow");

        public override Asset<Texture2D> GetTopTextures() => ModContent.Request<Texture2D>("CalamityMod/Tiles/AstralDesert/AstralPalmTree_Tops");
        public override Asset<Texture2D> GetTopGlowTextures() => ModContent.Request<Texture2D>("CalamityMod/Tiles/AstralDesert/AstralPalmTree_TopsGlow");


        //I don't know what this means. Why do palm trees have branches?? Since when. Will ask spriters for an oasis alt later
        public override Asset<Texture2D> GetOasisTopTextures() => ModContent.Request<Texture2D>("CalamityMod/Tiles/AstralDesert/AstralPalmTree_OasisTops");
        public override Asset<Texture2D> GetOasisTopGlowTextures() => ModContent.Request<Texture2D>("CalamityMod/Tiles/AstralDesert/AstralPalmTree_OasisTopsGlow");

        public override Color GetGlowColor(int i, int j)
        {
            float brightness = 1f;
            float declareThisHereToPreventRunningTheSameCalculationMultipleTimes = Main.GameUpdateCount * 0.012f;
            brightness *= MathF.Sin(i / 18f + declareThisHereToPreventRunningTheSameCalculationMultipleTimes);
            brightness *= MathF.Sin(j / 18f + declareThisHereToPreventRunningTheSameCalculationMultipleTimes);
            brightness *= MathF.Sin(i * 18f + declareThisHereToPreventRunningTheSameCalculationMultipleTimes);
            brightness *= MathF.Sin(j * 18f + declareThisHereToPreventRunningTheSameCalculationMultipleTimes);
            brightness = MathHelper.Clamp(brightness, 0.0f, 1.0f);
            return Color.White * MathHelper.Lerp(0.05f, 0.75f, brightness);
        }

        public override int DropWood() => ModContent.ItemType<Items.Placeables.FurnitureMonolith.AstralMonolith>();

        public override int CreateDust() => ModContent.DustType<AstralBasic>();

        public override int SaplingGrowthType(ref int style)
        {
            style = 0;
            return ModContent.TileType<AstralPalmSapling>();
        }

        public override int TreeLeaf() => ModContent.GoreType<AstralLeaf>();

        // Returning false at the end prevents vanilla behavior as the default is palm tree behavior which can include undesirable stuff like seagulls
        public override bool Shake(int x, int y, ref bool createLeaves)
        {
            // 33% chance to drop extra fruit when using Feller of Evergreens
            Vector2 worldPosition = new Vector2(x, y).ToWorldCoordinates();
            Player nearestPlayer = Main.player[Player.FindClosest(worldPosition, 16, 16)];
            if (nearestPlayer.active && nearestPlayer.HeldItem.type == ModContent.ItemType<FellerofEvergreens>() && WorldGen.genRand.NextBool(3))
            {
                int treeDropItemType = WorldGen.genRand.NextBool() ? (WorldGen.genRand.NextBool() ? ItemID.Coconut : ItemID.Banana)
                    : (WorldGen.genRand.NextBool() ? ModContent.ItemType<Barberry>() : ModContent.ItemType<Lotus>());
                Item.NewItem(WorldGen.GetItemSource_FromTreeShake(x, y), x * 16, y * 16, 16, 16, treeDropItemType);
            }

            int randAmt = Main.rand.Next(1, 3);

            if (Main.getGoodWorld && Main.rand.NextBool(15))
            {
                Projectile.NewProjectile(new EntitySource_ShakeTree(x, y), x * 16, y * 16, Main.rand.NextFloat(-100f, 100f) * 0.002f, 0f, ProjectileID.Bomb, 0, 0f, Player.FindClosest(new Vector2(x * 16, y * 16), 16, 16));
            }
            else if (Main.rand.NextBool(35) && Main.halloween)
            {
                createLeaves = true;
                Item.NewItem(WorldGen.GetItemSource_FromTreeShake(x, y), x * 16, y * 16, 16, 16, ItemID.RottenEgg, randAmt);
            }
            else if (Main.rand.NextBool(12)) // 1-3 Astral Monolith
            {
                createLeaves = true;
                Item.NewItem(WorldGen.GetItemSource_FromTreeShake(x, y), x * 16, y * 16, 16, 16, DropWood(), Main.rand.Next(1, 4));
            }
            else if (Main.rand.NextBool(20))
            {
                createLeaves = true;
                int coin = ItemID.CopperCoin;
                int amount = Main.rand.Next(50, 100);
                if (Main.rand.NextBool(30))
                {
                    coin = ItemID.GoldCoin;
                    amount = 1;
                    if (Main.rand.NextBool(5))
                        amount++;

                    if (Main.rand.NextBool(10))
                        amount++;
                }
                else if (Main.rand.NextBool(10))
                {
                    coin = ItemID.SilverCoin;
                    amount = Main.rand.Next(1, 21);
                    if (Main.rand.NextBool(3))
                        amount += Main.rand.Next(1, 21);

                    if (Main.rand.NextBool(4))
                        amount += Main.rand.Next(1, 21);
                }

                Item.NewItem(WorldGen.GetItemSource_FromTreeShake(x, y), x * 16, y * 16, 16, 16, coin, amount);
            }
            else if (Main.rand.NextBool(15))
            {
                createLeaves = true;
                int type = ModContent.ItemType<StarblightSoot>();
                if (!Main.dayTime && Main.rand.NextBool())
                    type = ItemID.FallenStar;
                Item.NewItem(WorldGen.GetItemSource_FromTreeShake(x, y), new Vector2(x, y) * 16, type, randAmt);
            }
            else if (Main.rand.NextBool(12)) // Vanilla palm fruit logic goes first
            {
                int fruitType = Main.rand.NextBool() ? ItemID.Coconut : ItemID.Banana;
                Item.NewItem(WorldGen.GetItemSource_FromTreeShake(x, y), new Vector2(x, y) * 16, fruitType);
            }
            else if (Main.rand.NextBool(12))
            {
                int fruitType = Main.rand.NextBool() ? ModContent.ItemType<Barberry>() : ModContent.ItemType<Lotus>();
                Item.NewItem(WorldGen.GetItemSource_FromTreeShake(x, y), new Vector2(x, y) * 16, fruitType);
            }
            return false;
        }
    }
}
