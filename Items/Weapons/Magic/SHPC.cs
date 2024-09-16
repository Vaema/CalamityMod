using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Magic;
using CalamityMod.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace CalamityMod.Items.Weapons.Magic
{
    public class SHPC : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Magic";
        public static readonly SoundStyle FireSound = new("CalamityMod/Sounds/Item/AnomalysNanogunMPFBShot");

        public const int ShotsPerSoul = 50;
        public int storedSoulpower = 0;
        public int recoilProgress = 0;

        public override void SetStaticDefaults() => ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;

        public override void SetDefaults()
        {
            Item.width = 124;
            Item.height = 52;
            Item.damage = 36;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 20;
            Item.useAnimation = Item.useTime = 60;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 3.25f;
            Item.UseSound = FireSound;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<SHPB>();
            Item.shootSpeed = 20f;

            Item.value = CalamityGlobalItem.RarityPinkBuyPrice;
            Item.rare = ItemRarityID.Pink;
        }

        public override Vector2? HoldoutOffset() => new Vector2(-35, -10);

        public override bool AltFunctionUse(Player player) => true;

        public override void OnCreated(ItemCreationContext context)
        {
            if (context is RecipeItemCreationContext)
                storedSoulpower = ShotsPerSoul;
        }

        public int FindSoulForAmmo(Player player)
        {
            if (player.HasItem(ItemID.SoulofLight))
                return ItemID.SoulofLight;
            if (player.HasItem(ItemID.SoulofNight))
                return ItemID.SoulofNight;
            if (player.HasItem(ItemID.SoulofFlight))
                return ItemID.SoulofFlight;
            if (player.HasItem(ItemID.SoulofFright))
                return ItemID.SoulofFright;
            if (player.HasItem(ItemID.SoulofSight))
                return ItemID.SoulofSight;
            if (player.HasItem(ItemID.SoulofMight))
                return ItemID.SoulofMight;
            return -1;
        }

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
                Item.UseSound = CommonCalamitySounds.LaserCannonSound;
            else
                Item.UseSound = FireSound;

            return storedSoulpower > 0 || FindSoulForAmmo(player) != -1;
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse != 2)
                storedSoulpower--;

            if (storedSoulpower <= 0)
            {
                bool ammoConsumed = false;

                if (FindSoulForAmmo(player) != -1)
                {
                    player.ConsumeItem(FindSoulForAmmo(player));
                    ammoConsumed = true;
                }

                if (ammoConsumed)
                    storedSoulpower = ShotsPerSoul;
            }

            return base.UseItem(player);
        }

        public override void ModifyManaCost(Player player, ref float reduce, ref float mult)
        {
            if (player.altFunctionUse == 2)
                mult *= 0.3f;
        }

        public override float UseSpeedMultiplier(Player player)
        {
            if (player.altFunctionUse == 2)
                return 6f;

            return 1f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                for (int shootAmt = 0; shootAmt < 2; shootAmt++)
                {
                    Vector2 Speed = new Vector2(velocity.X + Main.rand.NextFloat(-1f, 1f), velocity.Y + Main.rand.NextFloat(-1f, 1f));
                    Projectile.NewProjectile(source, position + new Vector2(0, -10) + velocity * 2.6f, Speed, ModContent.ProjectileType<SHPL>(), damage, knockback * 0.5f, player.whoAmI);
                }
                return false;
            }
            else
            {
                Projectile.NewProjectile(source, position + new Vector2(0, -10) + velocity * 3f, velocity, ModContent.ProjectileType<SHPB>(), damage, knockback, player.whoAmI);
                return false;
            }
        }

        public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            float barScale = 2.5f;
            var barBG = ModContent.Request<Texture2D>("CalamityMod/UI/MiscTextures/GenericBarBack").Value;
            var barFG = ModContent.Request<Texture2D>("CalamityMod/UI/MiscTextures/GenericBarFront").Value;

            Vector2 drawPos = position + new Vector2((frame.Width - barBG.Width * 0.5f) * scale, (frame.Height + 45f) * scale);
            Rectangle frameCrop = new Rectangle(0, 0, (int)(storedSoulpower / (float)ShotsPerSoul * barFG.Width), barFG.Height);
            Color colorBG = Color.RoyalBlue;
            Color colorFG = Color.Lerp(Color.DarkGray, Main.DiscoColor, storedSoulpower / (float)ShotsPerSoul);

            spriteBatch.Draw(barBG, drawPos, null, colorBG, 0f, origin, scale * barScale, 0f, 0f);
            spriteBatch.Draw(barFG, drawPos, frameCrop, colorFG * 0.8f, 0f, origin, scale * barScale, 0f, 0f);

            CalamityUtils.DrawBorderStringEightWay(spriteBatch, FontAssets.MouseText.Value, storedSoulpower.ToString(), drawPos + new Vector2(-200, -60) * scale, Color.GreenYellow, Color.Black, scale * 2.5f);
        }

        #region Recoil Stuff
        public override void HoldItem(Player player) => player.Calamity().mouseWorldListener = true;
        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            player.ChangeDir(Math.Sign((player.Calamity().mouseWorld - player.Center).X));
            float itemRotation = player.compositeFrontArm.rotation + MathHelper.PiOver2 * player.gravDir;

            Vector2 itemPosition = player.MountedCenter + itemRotation.ToRotationVector2() * 35f;
            Vector2 itemSize = new Vector2(Item.width, Item.height);
            Vector2 itemOrigin = new Vector2(-35, 0);

            if (player.altFunctionUse != 2)
            {
                recoilProgress++;
                if (recoilProgress < Item.useAnimation / 3)
                {
                    itemPosition -= (player.Calamity().mouseWorld - player.Center).SafeNormalize(Vector2.UnitX) * (Item.useAnimation / 3 - recoilProgress) * 0.75f;
                }
                else
                {
                    if (recoilProgress >= Item.useAnimation - 1)
                        recoilProgress = 0;
                }
            }

            CalamityUtils.CleanHoldStyle(player, itemRotation, itemPosition, itemSize, itemOrigin);
            base.UseStyle(player, heldItemFrame);
        }

        public override void UseItemFrame(Player player)
        {
            player.ChangeDir(Math.Sign((player.Calamity().mouseWorld - player.Center).X));
            float rotation = (player.Center - player.Calamity().mouseWorld).ToRotation() * player.gravDir + MathHelper.PiOver2;

            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, rotation);
        }
        #endregion Recoil Stuff

        public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
        {
            // scaling legendary!!!!
            if (Main.zenithWorld)
            {
                bool plantera = NPC.downedPlantBoss;
                bool golem = NPC.downedGolemBoss;
                bool cultist = NPC.downedAncientCultist;
                bool moonLord = NPC.downedMoonlord;
                bool providence = DownedBossSystem.downedProvidence;
                bool devourerOfGods = DownedBossSystem.downedDoG;
                bool yharon = DownedBossSystem.downedYharon;
                float damageMult = 1f +
                    (plantera ? 0.1f : 0f) + //1.1
                    (golem ? 0.15f : 0f) + //1.25
                    (cultist ? 3.5f : 0f) + //4.75
                    (moonLord ? 4.5f : 0f) + //9.25
                    (providence ? 7.5f : 0f) + //16.75
                    (devourerOfGods ? 2.5f : 0f) + //19.25
                    (yharon ? 30f : 0f); //49.25
                damage *= damageMult;
            }
        }
        public override void ModifyTooltips(List<TooltipLine> list) => list.FindAndReplace("[GFB]", this.GetLocalizedValue(Main.zenithWorld ? "TooltipGFB" : "TooltipNormal"));

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<PlasmaDriveCore>().
                AddIngredient<SuspiciousScrap>(4).
                AddRecipeGroup("AnyMythrilBar", 10).
                AddIngredient(ItemID.SoulofFright, 5).
                AddIngredient(ItemID.SoulofMight, 5).
                AddIngredient(ItemID.SoulofSight, 5).
                AddTile(TileID.MythrilAnvil).
                Register();
        }

        #region Saving Ammo Amount
        public override ModItem Clone(Item item)
        {
            ModItem clone = base.Clone(item);
            if (clone is SHPC a && item.ModItem is SHPC a2)
            {
                a.storedSoulpower = a2.storedSoulpower;
            }
            return clone;
        }

        public override void SaveData(TagCompound tag)
        {
            tag["ammoStored"] = storedSoulpower;
        }

        public override void LoadData(TagCompound tag)
        {
            storedSoulpower = tag.GetInt("ammoStored");
        }

        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(storedSoulpower);
        }

        public override void NetReceive(BinaryReader reader)
        {
            storedSoulpower = reader.ReadInt32();
        }
        #endregion Saving Ammo Amount
    }
}
