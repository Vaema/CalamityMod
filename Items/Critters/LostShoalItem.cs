using CalamityMod.NPCs.SunkenSea;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace CalamityMod.Items.Critters
{
    public class LostShoalItem : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Misc";
        public override void SetStaticDefaults()
        {
            On_Player.ItemCheck_ReleaseCritter += ReleaseColoredShoal;
            Item.ResearchUnlockCount = 5;
        }

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 24;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.autoReuse = true;
            Item.useTurn = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.maxStack = 9999;
            Item.consumable = true;
            Item.noUseGraphic = true;
            Item.value = Item.buyPrice(0, 0, 30, 0);
            Item.bait = 20;
            Item.makeNPC = (short)ModContent.NPCType<LostShoal>();
            Item.rare = ItemRarityID.Green;
        }

        // Since all Polyp Panasea variants are a single NPC type, this needs to be done in order for each item to spawn the correct color
        public static void ReleaseColoredShoal(On_Player.orig_ItemCheck_ReleaseCritter orig, Player player, Item item)
        {
            if (item.makeNPC == ModContent.NPCType<LostShoal>())
            {
                int mouseX = Main.mouseX + (int)Main.screenPosition.X;
                int mouseY = Main.mouseY + (int)Main.screenPosition.Y;
                int tileX = mouseX / 16;
                int tileY = mouseY / 16;
                if (!WorldGen.SolidTile(tileX, tileY))
                {
                    int colorType = (int)LostShoal.ShoalColor.Green;
                    if (item.type == ModContent.ItemType<LostShoalBlueItem>())
                    {
                        colorType = (int)LostShoal.ShoalColor.Blue;
                    }
                    if (item.type == ModContent.ItemType<LostShoalRedItem>())
                    {
                        colorType = (int)LostShoal.ShoalColor.Red;
                    }
                    player.ApplyItemTime(item);
                    int n = NPC.ReleaseNPC(mouseX, mouseY, item.makeNPC, item.placeStyle, player.whoAmI);
                    Main.npc[n].ai[1] = colorType;
                    Main.npc[n].catchItem = item.type;
                }
            }
            else
            {
                orig(player, item);
            }
        }
    }
}
