using System.Linq;
using CalamityMod.CalPlayer;
using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Melee;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace CalamityMod.Items.Accessories.Wings
{
    [AutoloadEquip(EquipType.Wings)]
    [LegacyName("CelestialTracers")]
    public class TracersSeraph : BaseWings
    {
        public override float BonusAscentWhileFalling => 0.95f;
        public override float BonusAscentWhileRising => 0.16f;
        public override float RisingSpeedThreshold => 1.2f;
        public override float MaxAscentSpeed => 2.9f;
        public override float BaseAscent => 0.145f;

        public static int wingSlot = 0;

        public override void SetStaticDefaults()
        {
            ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new WingStats(250, 11f, 2.8f);
            wingSlot = Item.wingSlot;
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
            if (Main.LocalPlayer.armor.Contains(Item)) foreach (var item in Main.LocalPlayer.armor)
            {
                if (item.wingSlot > 0 && item.wingSlot != wingSlot)
                    return false;
            }
            return true;
        }
        public override void RightClick(Player player)
        {
            if (Item.wingSlot == wingSlot){
                Item.wingSlot = -1;
                Item.color = Color.Gray;
            }
            else
            {
                Item.wingSlot = wingSlot;
                Item.color = Color.White;
            }
        }

        public override bool ConsumeItem(Player player) => false;
        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.width = 36;
            Item.height = 32;
            Item.value = CalamityGlobalItem.RarityVioletBuyPrice;
            Item.rare = ModContent.RarityType<BurnishedAuric>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            if (player.controlJump && player.wingTime > 0f && player.jump == 0 && player.velocity.Y != 0f && !hideVisual)
            {
                int dustXOffset = 4;
                if (player.direction == 1)
                {
                    dustXOffset = -40;
                }
                int flightDust = Dust.NewDust(new Vector2(player.position.X + (float)(player.width / 2) + (float)dustXOffset, player.position.Y + (float)(player.height / 2) - 15f), 30, 30, DustID.GemDiamond, 0f, 0f, 100, default, 2.4f);
                Main.dust[flightDust].noGravity = true;
                Main.dust[flightDust].velocity *= 0.3f;
                if (Main.rand.NextBool(10))
                {
                    Main.dust[flightDust].fadeIn = 2f;
                }
                Main.dust[flightDust].shader = GameShaders.Armor.GetSecondaryShader(player.cWings, player);
            }
            CalamityPlayer modPlayer = player.Calamity();
            player.accRunSpeed = 9f;
            player.moveSpeed += 0.18f;
            player.iceSkate = true;
            player.waterWalk = true;
            player.fireWalk = true;
            player.lavaImmune = true;
            player.buffImmune[BuffID.OnFire] = true;
            player.noFallDmg = true;
            if (Item.wingSlot == -1) //Only applies if tracers are plucked
            {
                player.rocketBoots = player.vanityRocketBoots = 2;
                modPlayer.angelTreads = true;
            }
            modPlayer.tracersDust = !hideVisual;
            modPlayer.tracersSeraph = true; // Grants immunity to Auric Rejection and other walk-on-block effects
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<TracersElysian>().
                AddIngredient<WingsofRebirth>().
                AddIngredient<AuricBar>(5).
                AddTile<CosmicAnvil>().
                Register();
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            //This code is commented out as it's for when Tracers get wingless sprites. Uncomment it then.
            //frame = new Rectangle(0, (Item.wingSlot == -1 ? frame.Height / 2 : 0), frame.Width, frame.Height / 2); //Draws the tracers with/without wings depending on if they're set to function as wings.
            //position -= -new Vector2((Item.wingSlot == -1 ? 4 : 0), frame.Height / 2 - (Item.wingSlot == -1 ? 6 : 4));
            CalamityUtils.DrawInventoryCustomScale(
                spriteBatch,
                texture: TextureAssets.Item[Type].Value,
                position,
                frame,
                drawColor,
                itemColor,
                origin,
                scale,
                wantedScale: 0.8f,
                drawOffset: new(1f, 0f)
            );
            return false;
        }
        // This code is to color the sprite until we get wingless sprites.
        public override void UpdateInventory(Player player)
        {
            if (Item.wingSlot == -1)
                Item.color = Color.DarkGray;
            else
                Item.color = Color.Transparent;
        }
    }
}
