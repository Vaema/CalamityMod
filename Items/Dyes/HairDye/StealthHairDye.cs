using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Dyes;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Dyes.HairDye;

public class StealthHairDye : ModItem, ILocalizedModType
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
        float stealthP = player.Calamity().rogueStealth / player.Calamity().rogueStealthMax;

        if (float.IsInfinity(stealthP) || float.IsNaN(stealthP))
            stealthP = 0f;

        return Color.Lerp(player.hairColor, new Color(186, 85, 211), stealthP);
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
