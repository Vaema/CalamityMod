using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Dyes;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Dyes.HairDye;

public class WingTimeHairDye : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Dyes";

    public override void SetStaticDefaults()
    {
        if (!Main.dedServ)
        {
            GameShaders.Hair.BindShader(Type, new LegacyHairShaderData().UseLegacyMethod(UpdateHairDye));
        }
    }

    private static Color UpdateHairDye(Player player, Color newColor, ref bool lighting)
    {
        float wingP = player.wingTime / player.wingTimeMax;

        if (player.mount.Active) wingP = 1f;
        else if (float.IsInfinity(wingP) || float.IsNaN(wingP)) wingP = 0f;

        return Color.Lerp(player.hairColor, new Color(139, 205, 255), wingP);
    }

    public override void SetDefaults()
    {
        Item.width = 20;
        Item.height = 26;
        Item.useAnimation = Item.useTime = 17;
        Item.UseSound = SoundID.Item3;
        Item.useStyle = ItemUseStyleID.DrinkLiquid;
        Item.useTurn = true;
        Item.consumable = true;
        Item.maxStack = Item.CommonMaxStack;

        Item.value = Item.buyPrice(gold: 5); // Sold by Stylist
        Item.rare = ItemRarityID.Green;
    }
}
