using CalamityMod.Items.Materials;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Dyes
{
    public class DragonSoulDye : BaseDye
    {
        public override ArmorShaderData ShaderDataToBind => new ArmorShaderData(Mod.Assets.Request<Effect>("Effects/Dyes/DragonSoulDyeShader"), "DyePass");
        public override void SafeSetStaticDefaults()
        {
            Item.ResearchUnlockCount = 3;
        }

        public override void SafeSetDefaults()
        {
            Item.rare = ModContent.RarityType<Violet>();
            Item.value = Item.sellPrice(0, 8, 0, 0);
        }

        public override void AddRecipes()
        {
            CreateRecipe(3).
                AddIngredient(ItemID.BottledWater, 3).
                AddIngredient<YharonSoulFragment>().
                AddTile(TileID.DyeVat).
                Register();
        }
    }
}
