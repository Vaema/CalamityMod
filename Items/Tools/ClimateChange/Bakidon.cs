using CalamityMod.CalPlayer;
using CalamityMod.Items.Materials;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.GameContent.NetModules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Net;

namespace CalamityMod.Items.Tools.ClimateChange
{
    [LegacyName("Moonlight")]
    public class Bakidon : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Tools";

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
            if (Main.netMode != NetmodeID.Server && player == Main.LocalPlayer)
            {
                var power = CreativePowerManager.Instance.GetPower<CreativePowers.FreezeTime>();
                NetPacket packet = NetCreativePowersModule.PreparePacket(power.PowerId, 1);
                packet.Writer.Write(!power.Enabled);
                NetManager.Instance.SendToServerOrLoopback(packet);
            }
            return true;
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
}
