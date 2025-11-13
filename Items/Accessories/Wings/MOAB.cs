using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace CalamityMod.Items.Accessories.Wings
{
    [AutoloadEquip(EquipType.Wings)]
    public class MOAB : BaseWings
    {
        public override float BonusAscentWhileFalling => 0.75f;
        public override float BonusAscentWhileRising => 0.15f;
        public override float RisingSpeedThreshold => 1f;
        public override float MaxAscentSpeed => 2.5f;
        public override float BaseAscent => 0.125f;

        public static int wingSlot = 0;
        public override void SetStaticDefaults()
        {
            ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new WingStats(75, 6.5f, 1f);
            wingSlot = Item.wingSlot;
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.width = 28;
            Item.height = 32;
            Item.value = CalamityGlobalItem.RarityLightPurpleBuyPrice;
            Item.rare = ItemRarityID.LightPurple;
        }
        #region Toggleable Wings
        bool toggleEnabled
        {
            get { return Item.wingSlot != -1; }
            set
            {
                if (value)
                    Item.wingSlot = wingSlot;
                else
                    Item.wingSlot = -1;
            }
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (!toggleEnabled)
                tooltips.RemoveAll(x => x.Name == "Tooltip0");
            base.ModifyTooltips(tooltips);
        }

        public override bool CanRightClick() => Main.keyState.PressingShift();
        public override void RightClick(Player player)
        {
            toggleEnabled = !toggleEnabled;
            Item.NetStateChanged();
        }
        public override bool ConsumeItem(Player player) => false;
        public override void SaveData(TagCompound tag)
        {
            tag.Add("toggleEffect", toggleEnabled);
        }
        public override void LoadData(TagCompound tag)
        {
            toggleEnabled = tag.GetBool("toggleEffect");
        }
        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(toggleEnabled);
        }
        public override void NetReceive(BinaryReader reader)
        {
            toggleEnabled = reader.ReadBoolean();
        }
        public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            CalamityUtils.DrawInventoryDot(spriteBatch, position, new Vector2(16, 16) * Main.inventoryScale, toggleEnabled);
        }
        #endregion

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.DisableWingFlapSound();

            if (toggleEnabled && player.controlJump && player.wingTime > 0f && player.jump == 0 && player.velocity.Y != 0f && !hideVisual)
            {
                player.rocketDelay2--;
                if (player.rocketDelay2 <= 0)
                {
                    SoundEngine.PlaySound(SoundID.Item13, player.Center);
                    player.rocketDelay2 = 60;
                }
                int dustAmt = 2;
                if (player.controlUp)
                {
                    dustAmt = 4;
                }
                for (int index = 0; index < dustAmt; index++)
                {
                    int type = 6;
                    if (player.head == 41)
                    {
                        int arg_58FD_0 = player.body;
                    }
                    float scale = 1.75f;
                    int alpha = 100;
                    float xStart = player.Center.X + 16f;
                    if (player.direction > 0)
                    {
                        xStart = player.Center.X - 26f;
                    }
                    float yStart = player.position.Y + (float)player.height - 18f;
                    if (index == 1 || index == 3)
                    {
                        xStart = player.Center.X + 8f;
                        if (player.direction > 0)
                        {
                            xStart = player.Center.X - 20f;
                        }
                        yStart += 6f;
                    }
                    if (index > 1)
                    {
                        yStart += player.velocity.Y;
                    }
                    int boosterDust = Dust.NewDust(new Vector2(xStart, yStart), 8, 8, type, 0f, 0f, alpha, default, scale);
                    Dust dust = Main.dust[boosterDust];
                    dust.velocity.X *= 0.1f;
                    dust.velocity.Y = Main.dust[boosterDust].velocity.Y * 1f + 2f * player.gravDir - player.velocity.Y * 0.3f;
                    dust.noGravity = true;
                    dust.shader = GameShaders.Armor.GetSecondaryShader(player.cWings, player);
                    if (dustAmt == 4)
                    {
                        dust.velocity.Y += 6f;
                    }
                }
            }

            // Grants Cloud in a Bottle, Sandstorm in a Bottle, and Blizzard in a Bottle (like Bundle of Balloons)
            player.GetJumpState(ExtraJump.CloudInABottle).Enable();
            player.GetJumpState(ExtraJump.SandstormInABottle).Enable();
            player.GetJumpState(ExtraJump.BlizzardInABottle).Enable();
            player.jumpBoost = true;
            player.autoJump = true;
            player.jumpSpeedBoost += 1.6f;

            // Mirrors the +5% luck from Lucky Horseshoe (vanilla behavior).
            player.Calamity().calamityBonusLuck += 0.05f;
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.FrogLeg).
                AddIngredient(ItemID.HorseshoeBundle).
                AddIngredient(ItemID.Jetpack).
                AddIngredient(ItemID.SoulofFright).
                AddIngredient(ItemID.SoulofMight).
                AddIngredient(ItemID.SoulofSight).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
