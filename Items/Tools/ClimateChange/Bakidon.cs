using CalamityMod.Items.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.GameContent.NetModules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Net;

namespace CalamityMod.Items.Tools.ClimateChange;

[LegacyName("Moonlight")]
public class Bakidon : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Tools";

    public static int FreezeTime => CalamityUtils.MinutesToFrames(10);
    public static float RechargeMultiplier => 1;

    public override void SetDefaults()
    {
        Item.width = 20;
        Item.height = 20;
        Item.rare = ItemRarityID.LightRed;
        Item.useAnimation = 9;
        Item.useTime = 9;
        Item.autoReuse = false; // Explicitly not autofire, since it can be used quickly now
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.UseSound = SoundID.Item60;
        Item.consumable = false;
    }

    public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
    {
        itemGroup = (ContentSamples.CreativeHelper.ItemGroup)CalamityResearchSorting.ToolsOther;
    }

    public override bool? UseItem(Player player)
    {
        player.Calamity().WeakTimeFreezeInUse = !player.Calamity().WeakTimeFreezeInUse;
        if (Main.netMode != NetmodeID.Server && player == Main.LocalPlayer)
        {
            var power = CreativePowerManager.Instance.GetPower<CreativePowers.FreezeTime>();
            NetPacket packet = NetCreativePowersModule.PreparePacket(power.PowerId, 1);
            packet.Writer.Write(player.Calamity().WeakTimeFreezeInUse);
            NetManager.Instance.SendToServerOrLoopback(packet);
        }
        return true;
    }

    public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        var cplayer = Main.LocalPlayer.Calamity();
        float fill =  1 - cplayer.WeakTimeFreezeUseTimer / (float)(FreezeTime / RechargeMultiplier);
        if (fill >= 1)
            return;

        float barScale = 1.1f;

        var barBG = ModContent.Request<Texture2D>("CalamityMod/UI/MiscTextures/GenericBarBack").Value;
        var barFG = ModContent.Request<Texture2D>("CalamityMod/UI/MiscTextures/GenericBarFront").Value;

        Vector2 barOrigin = barBG.Size() * 0.5f;
        float yOffset = 7.5f;
        Vector2 drawPos = position + Vector2.UnitY * scale * (frame.Height - yOffset);
        Rectangle frameCrop = new Rectangle(0, 0, (int)((fill) * barFG.Width), barFG.Height);
        Color colorBG = Color.DarkViolet * 0.5f;
        Color colorFG = Color.Lerp(Color.Orange, Color.Green, fill);

        spriteBatch.Draw(barBG, drawPos, null, colorBG, 0f, barOrigin, scale * barScale, 0f, 0f);
        spriteBatch.Draw(barFG, drawPos, frameCrop, colorFG, 0f, barOrigin, scale * barScale, 0f, 0f);
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient(ItemID.FallenStar, 10).
            AddIngredient(ItemID.SoulofLight, 7).
            AddIngredient(ItemID.SoulofNight, 7).
            AddIngredient<EssenceofSunlight>(5).
            AddTile(TileID.DemonAltar).
            Register();
    }
}
