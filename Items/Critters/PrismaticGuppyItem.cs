using System.IO;
using CalamityMod.NPCs.SunkenSea;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace CalamityMod.Items.Critters
{
    public abstract class PrismaticGuppyItem : ModItem, ILocalizedModType
    {
        public int shapeVariant = 0;
        public abstract Texture2D CubicTexture { get; }
        public abstract Texture2D AngelicTexture { get; }

        public new string LocalizationCategory => "Items.Misc";

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 5;
        }

        public override void SetDefaults()
        {
            Item.DefaultToCapturedCritter(ModContent.NPCType<PrismaticGuppy>());
            Item.value = Item.sellPrice(silver: 20); // they sell for more
            Item.rare = ItemRarityID.Green;
        }

        public override void OnSpawn(IEntitySource source)
        {
            if (source is EntitySource_Caught catchEntity)
            {
                if (catchEntity.Entity is NPC n)
                {
                    if (n.type == ModContent.NPCType<PrismaticGuppy>())
                    {
                        shapeVariant = (int)n.ai[2];
                    }
                }
            }
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D tex = TextureAssets.Item[Item.type].Value;
            switch (shapeVariant)
            {
                case 1:
                    tex = CubicTexture;
                    break;
                case 2:
                    tex = AngelicTexture;
                    break;
            }
            spriteBatch.Draw(tex, Item.Center - Main.screenPosition, null, lightColor, rotation, tex.Size() / 2, scale, SpriteEffects.None, 0f);
            return false;
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D tex = TextureAssets.Item[Item.type].Value;
            switch (shapeVariant)
            {
                case 1:
                    tex = CubicTexture;
                    break;
                case 2:
                    tex = AngelicTexture;
                    break;
            }
            spriteBatch.Draw(tex, position, null, drawColor, 0, tex.Size() / 2, scale, SpriteEffects.None, 0f);
            return false;
        }

        public override void SaveData(TagCompound tag)
        {
            tag.Add("shapeVariant", shapeVariant);
        }

        public override void LoadData(TagCompound tag)
        {
            shapeVariant = tag.GetInt("shapeVariant");
        }

        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(shapeVariant);
        }

        public override void NetReceive(BinaryReader reader)
        {
            shapeVariant = reader.ReadInt32();
        }
    }

    // RIP "Throwing these in an aquarium would be insanity" flavor text
    [LegacyName("PrismaticGuppy")]
    public class PrismaticGuppyBlueItem : PrismaticGuppyItem
    {
        public override Texture2D AngelicTexture => angelicTex.Value;
        public override Texture2D CubicTexture => cubicTex.Value;

        public static Asset<Texture2D> angelicTex;
        public static Asset<Texture2D> cubicTex;
        public override void Load()
        {
            cubicTex = ModContent.Request<Texture2D>("CalamityMod/Items/Critters/PrismaticGuppyBlue2Item");
            angelicTex = ModContent.Request<Texture2D>("CalamityMod/Items/Critters/PrismaticGuppyBlue3Item");
        }
    }

    public class PrismaticGuppyPinkItem : PrismaticGuppyItem
    {
        public override Texture2D AngelicTexture => angelicTex.Value;
        public override Texture2D CubicTexture => cubicTex.Value;

        public static Asset<Texture2D> angelicTex;
        public static Asset<Texture2D> cubicTex;
        public override void Load()
        {
            cubicTex = ModContent.Request<Texture2D>("CalamityMod/Items/Critters/PrismaticGuppyPink2Item");
            angelicTex = ModContent.Request<Texture2D>("CalamityMod/Items/Critters/PrismaticGuppyPink3Item");
        }
    }

    public class PrismaticGuppyGreenItem : PrismaticGuppyItem
    {
        public override Texture2D AngelicTexture => angelicTex.Value;
        public override Texture2D CubicTexture => cubicTex.Value;

        public static Asset<Texture2D> angelicTex;
        public static Asset<Texture2D> cubicTex;
        public override void Load()
        {
            cubicTex = ModContent.Request<Texture2D>("CalamityMod/Items/Critters/PrismaticGuppyGreen2Item");
            angelicTex = ModContent.Request<Texture2D>("CalamityMod/Items/Critters/PrismaticGuppyGreen3Item");
        }
    }

    public class PrismaticGuppyGoldItem : PrismaticGuppyItem
    {
        public override Texture2D AngelicTexture => angelicTex.Value;
        public override Texture2D CubicTexture => cubicTex.Value;

        public static Asset<Texture2D> angelicTex;
        public static Asset<Texture2D> cubicTex;
        public override void Load()
        {
            cubicTex = ModContent.Request<Texture2D>("CalamityMod/Items/Critters/PrismaticGuppyGold2Item");
            angelicTex = ModContent.Request<Texture2D>("CalamityMod/Items/Critters/PrismaticGuppyGold3Item");
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.value = Item.sellPrice(gold: 15); // base guppies are said to sell for a lot already
        }
    }

    public class PrismaticGuppyRadiantItem : PrismaticGuppyItem
    {
        public override Texture2D AngelicTexture => angelicTex.Value;
        public override Texture2D CubicTexture => cubicTex.Value;

        public static Asset<Texture2D> angelicTex;
        public static Asset<Texture2D> cubicTex;
        public override void Load()
        {
            cubicTex = ModContent.Request<Texture2D>("CalamityMod/Items/Critters/PrismaticGuppyRadiant2Item");
            angelicTex = ModContent.Request<Texture2D>("CalamityMod/Items/Critters/PrismaticGuppyRadiant3Item");
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.value = Item.sellPrice(gold: 15); // base guppies are said to sell for a lot already
        }
    }
}
