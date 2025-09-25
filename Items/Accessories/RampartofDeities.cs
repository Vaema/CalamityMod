using System;
using System.Collections.Generic;
using System.IO;
using CalamityMod.CalPlayer;
using CalamityMod.Items.Materials;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace CalamityMod.Items.Accessories
{
    [AutoloadEquip(EquipType.Shield)]
    public class RampartofDeities : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";

        public override void SetDefaults()
        {
            Item.width = 64;
            Item.height = 62;
            Item.value = CalamityGlobalItem.RarityVioletBuyPrice;
            Item.defense = 8;
            Item.accessory = true;
            Item.rare = ModContent.RarityType<BurnishedAuric>();
        }

        #region Toggleable Panic Necklace

        bool panicNecklaceEnabled = true;
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.FindAndReplace("[TOGGLE]", panicNecklaceEnabled ? this.GetLocalizedValue("ToggleEffect") : "");
        }
        public override bool CanRightClick() => Main.keyState.PressingShift();
        public override void RightClick(Player player)
        {
            panicNecklaceEnabled = !panicNecklaceEnabled;
            Item.NetStateChanged();
        }
        public override bool ConsumeItem(Player player) => false;
        public override void SaveData(TagCompound tag)
        {
            tag.Add("panic", panicNecklaceEnabled);
        }

        public override void LoadData(TagCompound tag)
        {
            panicNecklaceEnabled = tag.GetBool("panic");
        }

        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(panicNecklaceEnabled);
        }

        public override void NetReceive(BinaryReader reader)
        {
            panicNecklaceEnabled = reader.ReadBoolean();
        }

        public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            CalamityUtils.DrawInventoryDot(spriteBatch, position, new Vector2(16, 16) * Main.inventoryScale, panicNecklaceEnabled);
        }
        #endregion
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            CalamityPlayer modPlayer = player.Calamity();

            // Directly inherits both Cross Necklace and Deific Amulet iframe boosts
            player.longInvince = true;
            modPlayer.dAmulet = true;

            //Panic Necklace effect if enabled
            player.panic = panicNecklaceEnabled;

            // Large hit iframe effect taken from Seraph Tracers
            modPlayer.rampartOfDeities = true;

            // Ice Barrier buff inherited from Frozen Turtle Shell
            if (player.statLife <= player.statLifeMax2 * 0.5)
                player.AddBuff(BuffID.IceBarrier, 5);

            // Knockback immunity inherited from Paladin's Shield
            player.noKnockback = true;

            // Paladin's Shield application
            if (player.statLife > player.statLifeMax2 * 0.25f)
            {
                player.hasPaladinShield = true;
                if (player.whoAmI != Main.myPlayer && player.miscCounter % 10 == 0)
                {
                    int myPlayer = Main.myPlayer;
                    if (Main.player[myPlayer].team == player.team && player.team != 0)
                    {
                        float teamPlayerXDist = player.position.X - Main.player[myPlayer].position.X;
                        float teamPlayerYDist = player.position.Y - Main.player[myPlayer].position.Y;
                        if ((float)Math.Sqrt(teamPlayerXDist * teamPlayerXDist + teamPlayerYDist * teamPlayerYDist) < 800f)
                            Main.player[myPlayer].AddBuff(BuffID.PaladinsShield, 20);
                    }
                }
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.FrozenShield).
                AddIngredient<DeificAmulet>().
                AddIngredient<AuricBar>(5).
                AddIngredient<AscendantSpiritEssence>(4).
                AddTile<CosmicAnvil>().
                Register();
        }
    }
}
