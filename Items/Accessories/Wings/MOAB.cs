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

        public override void SaveData(TagCompound tag)
        {
            tag.Add("wingsDisabled", Item.wingSlot == -1);
        }

        public override void LoadData(TagCompound tag)
        {
            if (tag.TryGet("wingsDisabled", out bool wingsDisabled))
            {
                if (wingsDisabled) Item.wingSlot = -1;
            }
        }
        public override bool CanRightClick()
        {
            if (!Main.keyState.PressingShift())
                return false;
            return true;
        }
        public override void RightClick(Player player)
        {
            if (Item.wingSlot == wingSlot)
                Item.wingSlot = -1;
            else
                Item.wingSlot = wingSlot;
        }

        public override bool ConsumeItem(Player player) => false;

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            if (Item.wingSlot != -1 && player.controlJump && player.wingTime > 0f && player.jump == 0 && player.velocity.Y != 0f && !hideVisual)
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

        //This code is commented out so it can be used when we get a MOAB wingless sprite
        /*public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            frame = new Rectangle(0, (Item.wingSlot == -1 ? frame.Height / 2 : 0), frame.Width, frame.Height / 2);
            position -= -new Vector2(0, frame.Height/2-2);
            CalamityUtils.DrawInventoryCustomScale(
                spriteBatch,
                texture: TextureAssets.Item[Type].Value,
                position,
                frame,
                drawColor,
                itemColor,
                origin,
                scale
            );
            return false;
        }*/

        //This entire method can be deleted when we get a wingless sprite
        public override void UpdateInventory(Player player)
        {
            if (Item.wingSlot == -1)
                Item.color = Color.DarkGray;
            else
                Item.color = Color.Transparent;
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
