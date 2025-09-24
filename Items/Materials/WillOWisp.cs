using CalamityMod.Cooldowns;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Armor.Wulfrum;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using static CalamityMod.CalamityUtils;

namespace CalamityMod.Items.Materials
{
    public class WillOWisp : ModItem, ILocalizedModType
    {
        public int textureVariant = 0;

        public static Asset<Texture2D> altTexture = null;
        public new string LocalizationCategory => "Items.Materials";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 25;
            ItemID.Sets.SortingPriorityMaterials[Type] = 60; // Meteorite
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 26;
            Item.maxStack = Item.CommonMaxStack;
            Item.value = Item.sellPrice(silver: 1);
            Item.rare = ItemRarityID.Green;
        }

        public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
        {
            itemGroup = ContentSamples.CreativeHelper.ItemGroup.Material;
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            if (textureVariant == 0)
                return true;
            else
            {
                spriteBatch.Draw(altTexture.Value, Item.Center - Main.screenPosition, null, lightColor, rotation, altTexture.Size() / 2, scale, SpriteEffects.None, 0);
                return false;
            }
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            if (textureVariant == 0)
                return true;
            else
            {
                spriteBatch.Draw(altTexture.Value, position, null, drawColor, 0, altTexture.Size() / 2, scale, SpriteEffects.None, 0);
                return false;
            }
        }
        public override void OnSpawn(IEntitySource source)
        {
                textureVariant = Main.rand.NextBool().ToInt();
        }
    }
}
