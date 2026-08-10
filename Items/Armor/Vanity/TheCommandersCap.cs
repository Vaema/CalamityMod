using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Armor.Vanity;

[AutoloadEquip(EquipType.Head)]
public class TheCommandersCap : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Armor.Vanity";
    public override void SetStaticDefaults()
    {

        if (!Main.dedServ)
            ArmorIDs.Head.Sets.DrawFullHair[Item.headSlot] = false;
    }

    public override void SetDefaults()
    {
        Item.width = 28;
        Item.height = 20;
        Item.rare = ItemRarityID.Blue;
        Item.vanity = true;
        Item.Calamity().donorItem = true;
    }

    public override bool IsArmorSet(Item head, Item body, Item legs)
    {
        return body.type == ModContent.ItemType<TheGrandGarment>() && legs.type == ModContent.ItemType<TheFormalFootwear>();
    }

    public override void UpdateVanitySet(Player player)
    {
        if (!player.CCed && !(Main.gamePaused && !Main.gameMenu) && player.velocity.X != 0f && player.velocity.Y == 0f)
        {
            for (int k = 0; k < 2; k++)
            {
                int dust = Dust.NewDust(new Vector2(player.position.X, player.position.Y + (float)((player.gravDir == 1f) ? (player.height - 2) : (-4))), player.width, 6, DustID.Cloud, 0f, 0f, 100, default, 0.1f);
                Main.dust[dust].fadeIn = 1f;
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 0.2f;
                Main.dust[dust].shader = GameShaders.Armor.GetSecondaryShader(player.ArmorSetDye(), player);
            }
        }
    }


    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient(ItemID.Silk, 5).
            AddRecipeGroup("AnyGoldBar", 1).
            AddIngredient(ItemID.BlueDye, 1).
            AddTile(TileID.Loom).
            Register();
    }
}
