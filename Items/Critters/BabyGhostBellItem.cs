using CalamityMod.NPCs.SunkenSea;
using CalamityMod.Packets;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Critters
{
    public class BabyGhostBellItem : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Misc";
        public override void SetStaticDefaults()
        {
            On_Player.ItemCheck_ReleaseCritter += ReleaseColoredGhostBell;
            Item.ResearchUnlockCount = 5;
        }

        public override void SetDefaults()
        {
            Item.DefaultToCapturedCritter(ModContent.NPCType<BabyGhostBell>());
            Item.bait = 20;
            Item.value = Item.sellPrice(silver: 10);
            Item.rare = ItemRarityID.Green;
        }

        // Since all Baby Ghost Bell variants are a single NPC type, this needs to be done in order for each item to spawn the correct color
        public static void ReleaseColoredGhostBell(On_Player.orig_ItemCheck_ReleaseCritter orig, Player player, Item item)
        {
            if (item.makeNPC == ModContent.NPCType<BabyGhostBell>())
            {
                int mouseX = Main.mouseX + (int)Main.screenPosition.X;
                int mouseY = Main.mouseY + (int)Main.screenPosition.Y;
                int tileX = mouseX / 16;
                int tileY = mouseY / 16;
                if (!WorldGen.SolidTile(tileX, tileY))
                {
                    int colorType = (int)BabyGhostBell.JellyColor.Blue;
                    if (item.type == ModContent.ItemType<BabyGhostBellGreenItem>())
                    {
                        colorType = (int)BabyGhostBell.JellyColor.Green;
                    }
                    if (item.type == ModContent.ItemType<BabyGhostBellRedItem>())
                    {
                        colorType = (int)BabyGhostBell.JellyColor.Red;
                    }
                    if (item.type == ModContent.ItemType<BabyGhostBellRadiantItem>())
                    {
                        colorType = (int)BabyGhostBell.JellyColor.Radiant;
                    }
                    if (item.type == ModContent.ItemType<BabyGhostBellGoldItem>())
                    {
                        colorType = (int)BabyGhostBell.JellyColor.Gold;
                    }
                    player.ApplyItemTime(item);

                    if (Main.netMode == NetmodeID.SinglePlayer)
                    {
                        int n = NPC.NewNPC(player.GetSource_ReleaseEntity(), mouseX, mouseY, item.makeNPC);
                        Main.npc[n].ai[1] = colorType;
                        Main.npc[n].catchItem = item.type;
                        Main.npc[n].releaseOwner = (short)player.whoAmI;
                    }
                    else
                    {
                        PlaceAltCritterPacket.Send(player, mouseX, mouseY, item, colorType);
                    }
                }
            }
            else
            {
                orig(player, item);
            }
        }
    }
}
