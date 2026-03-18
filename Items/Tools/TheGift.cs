using CalamityMod.NPCs;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Tools
{
    public class TheGift : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Tools";
        public override string Texture => "Terraria/Images/Item_601"; //Placeholder

        public override void SetDefaults()
        {
            Item.width = Item.height = 20;
            Item.maxStack = Item.CommonMaxStack;
            Item.useTime = Item.useAnimation = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTurn = true;
            Item.noMelee = true;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityGreenBuyPrice;
            Item.rare = ItemRarityID.Green;
        }

        public override void Load()
        {
            On_ShopHelper.ProcessMood += TheGiftHappiness;
        }

        public override void Update(ref float gravity, ref float maxFallSpeed)
        {
            // Controls actually applying the effect to an NPC
            // None of this should work in Remix because Remix disables happiness
            if (Item.noGrabDelay <= 0 && !Main.remixWorld)
            {
                foreach (NPC n in Main.ActiveNPCs)
                {
                    // Don't run this code for the Traveling Merchant and Skeleton Merchant because they aren't affected by happiness
                    // Also don't run this if The Gift has already been used on this NPC; they all will hold grudges against you :)
                    if (!n.isLikeATownNPC || n.type == NPCID.TravellingMerchant || n.type == NPCID.SkeletonMerchant ||
                        n.GetGlobalNPC<CalamityGlobalTownNPC>().TheGiftStatus.HasValue)
                        continue;

                    if (Item.Hitbox.Intersects(n.Hitbox))
                    {
                        Item.active = false;
                        Item.type = ItemID.None;
                        Item.stack = 0;
                        if (Main.dedServ)
                            NetMessage.SendData(MessageID.SyncItem, -1, -1, null, Item.whoAmI);

                        bool positive = Main.rand.NextBool();
                        Color c = positive ? Color.Green : Color.Red;
                        n.GetGlobalNPC<CalamityGlobalTownNPC>().TheGiftStatus = positive;
                        n.GetGlobalNPC<CalamityGlobalTownNPC>().TheGiftReset = 0.0;

                        // Placeholder visual effect
                        HealingPlus s = new(n.Center, 2f, Vector2.Zero, c, c, 40);
                        GeneralParticleHandler.SpawnParticle(s);
                    }
                }
            }
        }

        private static void TheGiftHappiness(On_ShopHelper.orig_ProcessMood orig, ShopHelper self, Player player, NPC npc)
        {
            orig(self, player, npc);

            bool? gift = npc.GetGlobalNPC<CalamityGlobalTownNPC>().TheGiftStatus;
            if (gift.HasValue)
            {
                if (gift.Value)
                    self._currentPriceAdjustment = 0.5f;
                else
                    self._currentPriceAdjustment = 1.75f;
            }
        }
    }
}
