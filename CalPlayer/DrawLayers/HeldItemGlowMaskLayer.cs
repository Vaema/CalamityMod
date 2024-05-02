using System;
using System.Collections.Generic;
using CalamityMod.Items.SummonItems;
using CalamityMod.Items.Weapons.Magic;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Items.Weapons.Summon;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace CalamityMod.CalPlayer.DrawLayers
{
    public class HeldItemGlowMaskLayer : PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.HeldItem);

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Player drawPlayer = drawInfo.drawPlayer;
            List<DrawData> existingDrawData = drawInfo.DrawDataCache;

            if (drawPlayer.JustDroppedAnItem)
                return;

            Item heldItem = drawInfo.heldItem;
            int itemType = heldItem.type;

            if (itemType < ItemID.Count)
                return;

            // This is ugly and I don't give a fuck.
            // If you want it to look better, do it yourself.
            Texture2D glowMask = default;
            if (itemType == ModContent.ItemType<AbyssShocker>())
                glowMask = ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Magic/AbyssShocker_mask").Value;
            else if (itemType == ModContent.ItemType<Apotheosis>())
                glowMask = ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Magic/ApotheosisGlow").Value;
            else if (itemType == ModContent.ItemType<Auralis>())
                glowMask = ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Ranged/AuralisGlow").Value;
            else if (itemType == ModContent.ItemType<AuroraBlazer>())
                glowMask = ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Ranged/AuroraBlazerGlow").Value;
            else if (itemType == ModContent.ItemType<CleansingBlaze>())
                glowMask = ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Ranged/CleansingBlazeGlow").Value;
            else if (itemType == ModContent.ItemType<DeathhailStaff>())
                glowMask = ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Magic/DeathhailStaffGlow").Value;
            else if (itemType == ModContent.ItemType<Deathwind>())
                glowMask = ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Ranged/DeathwindGlow").Value;
            else if (itemType == ModContent.ItemType<EssenceFlayer>())
                glowMask = ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Melee/EssenceFlayerGlow").Value;
            else if (itemType == ModContent.ItemType<EtherealSubjugator>())
                glowMask = ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Summon/EtherealSubjugatorGlow").Value;
            else if (itemType == ModContent.ItemType<Excelsus>())
                glowMask = ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Melee/ExcelsusGlow").Value;
            else if (itemType == ModContent.ItemType<IridescentExcalibur>())
                glowMask = ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Melee/IridescentExcaliburGlow").Value;
            else if (itemType == ModContent.ItemType<FatesReveal>())
                glowMask = ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Magic/FatesRevealGlow").Value;
            else if (itemType == ModContent.ItemType<GrandGuardian>())
                glowMask = ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Melee/GrandGuardianGlow").Value;
            else if (itemType == ModContent.ItemType<GreatswordofJudgement>())
                glowMask = ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Melee/GreatswordofJudgementGlow").Value;
            else if (itemType == ModContent.ItemType<MajesticGuard>())
                glowMask = ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Melee/MajesticGuardGlow").Value;
            else if (itemType == ModContent.ItemType<NecroplasmicBeacon>())
                glowMask = ModContent.Request<Texture2D>("CalamityMod/Items/SummonItems/NecroplasmicBeaconGlow").Value;
            else if (itemType == ModContent.ItemType<Orderbringer>())
                glowMask = ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Melee/OrderbringerGlow").Value;
            else if (itemType == ModContent.ItemType<Photosynthesis>())
                glowMask = ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Magic/PhotosynthesisGlow").Value;
            else if (itemType == ModContent.ItemType<PlantationStaff>())
                glowMask = ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Summon/PlantationStaffGlow").Value;
            else if (itemType == ModContent.ItemType<PrismaticBreaker>())
                glowMask = ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Melee/PrismaticBreakerGlow").Value;
            else if (itemType == ModContent.ItemType<SoulPiercer>())
                glowMask = ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Magic/SoulPiercerGlow").Value;
            else if (itemType == ModContent.ItemType<SubsumingVortex>())
                glowMask = ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Magic/SubsumingVortexGlow").Value;
            else if (itemType == ModContent.ItemType<TerrorBlade>())
                glowMask = ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Melee/TerrorBladeGlow").Value;
            else if (itemType == ModContent.ItemType<TheEnforcer>())
                glowMask = ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Melee/TheEnforcerGlow").Value;
            else if (itemType == ModContent.ItemType<VernalBolter>())
                glowMask = ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Ranged/VernalBolterGlow").Value;

            if (glowMask == default)
                return;

            if (drawPlayer.heldProj >= 0 && drawInfo.shadow == 0f && !drawInfo.heldProjOverHand)
                drawInfo.projectileDrawPosition = existingDrawData.Count;

            float adjustedItemScale = drawPlayer.GetAdjustedItemScale(heldItem);
            //Main.instance.LoadItem(num);
            Vector2 position = new Vector2((int)(drawInfo.ItemLocation.X - Main.screenPosition.X), (int)(drawInfo.ItemLocation.Y - Main.screenPosition.Y));
            Rectangle itemDrawFrame = drawPlayer.GetItemDrawFrame(itemType);
            drawInfo.itemColor = Lighting.GetColor((int)((double)drawInfo.Position.X + (double)drawPlayer.width * 0.5) / 16, (int)(((double)drawInfo.Position.Y + (double)drawPlayer.height * 0.5) / 16D));

            if (drawPlayer.shroomiteStealth && heldItem.CountsAsClass<RangedDamageClass>())
            {
                float stealth = drawPlayer.stealth;
                if ((double)stealth < 0.03)
                    stealth = 0.03f;

                float stealthColorScale = (1f + stealth * 10f) / 11f;
                drawInfo.itemColor = new Color((byte)((float)(int)drawInfo.itemColor.R * stealth), (byte)((float)(int)drawInfo.itemColor.G * stealth), (byte)((float)(int)drawInfo.itemColor.B * stealthColorScale), (byte)((float)(int)drawInfo.itemColor.A * stealth));
            }

            if (drawPlayer.setVortex && heldItem.CountsAsClass<RangedDamageClass>())
            {
                float stealth = drawPlayer.stealth;
                if ((double)stealth < 0.03)
                    stealth = 0.03f;

                drawInfo.itemColor = drawInfo.itemColor.MultiplyRGBA(new Color(Vector4.Lerp(Vector4.One, new Vector4(0f, 0.12f, 0.16f, 0f), 1f - stealth)));
            }

            bool inUse = drawPlayer.itemAnimation > 0 && heldItem.useStyle != 0;
            bool visuallyHeld = heldItem.holdStyle != 0 && !drawPlayer.pulley;
            if (!drawPlayer.CanVisuallyHoldItem(heldItem))
                visuallyHeld = false;

            if (drawInfo.shadow != 0f || drawPlayer.frozen || !(inUse || visuallyHeld) || itemType <= 0 || drawPlayer.dead || heldItem.noUseGraphic || (drawPlayer.wet && heldItem.noWet) || (drawPlayer.happyFunTorchTime && drawPlayer.inventory[drawPlayer.selectedItem].createTile == TileID.Torches && drawPlayer.itemAnimation == 0))
                return;

            Color color = new Color(250, 250, 250, heldItem.alpha);
            Vector2 originOffset = Vector2.Zero;

            // Use to adjust glow mask draw offset and color.
            switch (itemType)
            {
                default:
                    break;

                // Keeping these commented here as examples.
                /*case ItemID.TheBreaker:
                case ItemID.TentacleSpike:
                case ItemID.LucyTheAxe:
                    originOffset = new Vector2(4f, -4f) * drawPlayer.Directions;
                    break;

                case ItemID.BreakerBlade:
                case ItemID.FleshGrinder:
                case ItemID.SpectrePickaxe:
                case ItemID.HamBat:
                case ItemID.BatBat:
                    originOffset = new Vector2(6f, -6f) * drawPlayer.Directions;
                    break;

                case ItemID.LightsBane:
                    {
                        float amount = Utils.Remap(drawInfo.itemColor.ToVector3().Length() / 1.731f, 0.3f, 0.5f, 1f, 0f);
                        color = Color.Lerp(Color.Transparent, new Color(255, 255, 255, 127) * 0.7f, amount);
                        break;
                    }

                case ItemID.MeteorHamaxe:
                    originOffset = new Vector2(4f, -6f) * drawPlayer.Directions;
                    break;

                case ItemID.DyeTradersScimitar:
                    originOffset = new Vector2(2f, -2f) * drawPlayer.Directions;
                    break;*/
            }

            if (itemType == ModContent.ItemType<GrandGuardian>())
            {
                color = new Color(Main.DiscoR, Main.DiscoG, Main.DiscoB, heldItem.alpha);
            }

            Vector2 origin = new Vector2((float)itemDrawFrame.Width * 0.5f - (float)itemDrawFrame.Width * 0.5f * (float)drawPlayer.direction, itemDrawFrame.Height);
            if (heldItem.useStyle == ItemUseStyleID.DrinkLiquid && drawPlayer.itemAnimation > 0)
            {
                Vector2 vector2 = new Vector2(0.5f, 0.4f);
                origin = itemDrawFrame.Size() * vector2;
            }

            if (drawPlayer.gravDir == -1f)
                origin.Y = (float)itemDrawFrame.Height - origin.Y;

            origin += originOffset;
            float itemRotation = drawPlayer.itemRotation;

            if (heldItem.useStyle == ItemUseStyleID.GolfPlay)
            {
                ref float xPos = ref position.X;
                float xOffset = xPos;
                xPos = xOffset - 0f;
                itemRotation -= MathHelper.PiOver2 * (float)drawPlayer.direction;
                origin.Y = 2f;
                origin.X += 2 * drawPlayer.direction;
            }

            ItemSlot.GetItemLight(ref drawInfo.itemColor, heldItem);
            DrawData item;

            if (heldItem.useStyle == ItemUseStyleID.Shoot)
            {
                if (Item.staff[itemType])
                {
                    float staffRotation = drawPlayer.itemRotation + MathHelper.PiOver4 * (float)drawPlayer.direction;
                    float staffXOffset = 0f;
                    float staffYOffset = 0f;
                    Vector2 staffOrigin = new Vector2(0f, itemDrawFrame.Height);

                    if (drawPlayer.gravDir == -1f)
                    {
                        if (drawPlayer.direction == -1)
                        {
                            staffRotation += MathHelper.PiOver2;
                            staffOrigin = new Vector2(itemDrawFrame.Width, 0f);
                            staffXOffset -= (float)itemDrawFrame.Width;
                        }
                        else
                        {
                            staffRotation -= MathHelper.PiOver2;
                            staffOrigin = Vector2.Zero;
                        }
                    }

                    // Extra patch context.
                    else if (drawPlayer.direction == -1)
                    {
                        staffOrigin = new Vector2(itemDrawFrame.Width, itemDrawFrame.Height);
                        staffXOffset -= (float)itemDrawFrame.Width;
                    }

                    ItemLoader.HoldoutOrigin(drawPlayer, ref staffOrigin);

                    item = new DrawData(glowMask, new Vector2((int)(drawInfo.ItemLocation.X - Main.screenPosition.X + staffOrigin.X + staffXOffset), (int)(drawInfo.ItemLocation.Y - Main.screenPosition.Y + staffYOffset)), itemDrawFrame, color, staffRotation, staffOrigin, adjustedItemScale, drawInfo.itemEffect);
                    existingDrawData.Add(item);

                    return;
                }

                int xOffset = 10;
                Vector2 offset = new Vector2(itemDrawFrame.Width / 2, itemDrawFrame.Height / 2);
                Vector2 directionalOffset = Main.DrawPlayerItemPos(drawPlayer.gravDir, itemType);
                xOffset = (int)directionalOffset.X;
                offset.Y = directionalOffset.Y;
                Vector2 drawOrigin = new Vector2(-xOffset, itemDrawFrame.Height / 2);
                if (drawPlayer.direction == -1)
                    drawOrigin = new Vector2(itemDrawFrame.Width + xOffset, itemDrawFrame.Height / 2);

                item = new DrawData(glowMask, new Vector2((int)(drawInfo.ItemLocation.X - Main.screenPosition.X + offset.X), (int)(drawInfo.ItemLocation.Y - Main.screenPosition.Y + offset.Y)), itemDrawFrame, new Color(250, 250, 250, heldItem.alpha), drawPlayer.itemRotation, drawOrigin, adjustedItemScale, drawInfo.itemEffect);
                existingDrawData.Add(item);

                return;
            }

            if (drawPlayer.gravDir == -1f)
            {
                item = new DrawData(glowMask, position, itemDrawFrame, new Color(250, 250, 250, heldItem.alpha), itemRotation, origin, adjustedItemScale, drawInfo.itemEffect);
                existingDrawData.Add(item);

                return;
            }

            item = new DrawData(glowMask, position, itemDrawFrame, color, itemRotation, origin, adjustedItemScale, drawInfo.itemEffect);
            existingDrawData.Add(item);
        }
    }
}
