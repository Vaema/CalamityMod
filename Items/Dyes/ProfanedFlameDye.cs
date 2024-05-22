using CalamityMod.Items.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Dyes
{
    public class ProfanedFlameDye : BaseDye
    {
        public override ArmorShaderData ShaderDataToBind => new ArmorShaderData(Mod.Assets.Request<Effect>("Effects/Dyes/ProfanedFlameDyeShader"), "DyePass").
            UseColor(new Color(255, 191, 73)).UseSecondaryColor(new Color(255, 255, 186)).UseImage("Images/Misc/Perlin");
        public override void SafeSetStaticDefaults()
        {
            Item.ResearchUnlockCount = 3;
        }

        public override void SafeSetDefaults()
        {
            Item.rare = ItemRarityID.Purple;
            Item.value = Item.sellPrice(0, 2, 50, 0);
        }

        public override void AddRecipes()
        {
            CreateRecipe(3).
                AddIngredient(ItemID.BottledWater, 3).
                AddIngredient<UnholyEssence>(5).
                AddTile(TileID.DyeVat).
                Register();
        }
    }
}
