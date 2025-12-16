using CalamityMod.Buffs.Summon;
using CalamityMod.Items.Materials;
using CalamityMod.Packets;
using CalamityMod.Projectiles.Summon;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Summon
{
    public class IgneousExaltation : ModItem, ILocalizedModType
    {
        public static int ChargeDuration = 30;
        public static int ChargeCooldown = 120;
        public new string LocalizationCategory => "Items.Weapons.Summon";
        private static Texture2D BladeOutline = null;
        public static Texture2D GetBladeOutlineTex()
        {
            if (BladeOutline == null)
            {
                var texture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Summon/IgneousBlade").Value;
                BladeOutline = new Texture2D(Main.graphics.GraphicsDevice, texture.Width, texture.Height);

                var BaseArray = new Color[BladeOutline.Width * BladeOutline.Height];
                var ColorArray = new Color[BladeOutline.Width * BladeOutline.Height];
                texture.GetData(BaseArray);
                for (var i = 0; i < BaseArray.Length; i++)
                {
                    ColorArray[i] = new Color(255, 255, 255) * (((float)BaseArray[i].A) / 255f);
                }
                BladeOutline.SetData(ColorArray);
            }
            return BladeOutline;
        }
        public override void SetStaticDefaults()
        {
            Item.staff[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 52;
            Item.height = 50;
            Item.damage = 34;
            Item.mana = 10;
            Item.useAnimation = Item.useTime = 36;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 4.5f;
            Item.value = CalamityGlobalItem.RarityPinkBuyPrice;
            Item.rare = ItemRarityID.Pink;
            Item.UseSound = SoundID.Item71;
            Item.autoReuse = true;
            Item.buffType = ModContent.BuffType<IgneousExaltationBuff>();
            Item.shoot = ModContent.ProjectileType<IgneousBlade>();
            Item.shootSpeed = 10f;
            Item.DamageType = DamageClass.Summon;
        }

        public override bool CanRightClick()
        {
            if (!Main.keyState.PressingShift())
                return false;
            return true;
        }
        public override void RightClick(Player player)
        {
            Main.LocalPlayer.Calamity().InvertExaltationLineRotationDirections = !Main.LocalPlayer.Calamity().InvertExaltationLineRotationDirections;
            if (Main.netMode != NetmodeID.SinglePlayer)
                ExaltationDirectionSyncPacket.Send(Main.LocalPlayer.Calamity());
        }

        public override bool ConsumeItem(Player player) => false;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            float totalSlots = 0f;
            foreach (Projectile p in Main.ActiveProjectiles)
            {
                if (p.minion && p.owner == player.whoAmI)
                {
                    totalSlots += p.minionSlots;
                }
            }
            if (totalSlots >= player.maxMinions)
            {
                foreach (Projectile pro in Main.ActiveProjectiles)
                {
                    if (pro.type == type && pro.owner == player.whoAmI && pro.ai[1] >= 0 && pro.ai[0] == 0)
                    {
                        pro.ModProjectile<IgneousBlade>().CurrentState = IgneousBlade.AIState.TransitionToLaunch;
                        pro.netUpdate = true;
                    }
                }
            }
            else
            {
                player.AddBuff(Item.buffType, 2);
                var minion = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI, 0f, 1f);
                minion.originalDamage = Item.damage;

                int bladeIndex = 0;
                foreach (Projectile pro in Main.ActiveProjectiles)
                {
                    if (pro.type == type && pro.owner == player.whoAmI)
                    {
                        pro.ModProjectile<IgneousBlade>().BladeIndex = bladeIndex++;
                        pro.ModProjectile<IgneousBlade>().AITimer = -ChargeCooldown;
                        pro.ModProjectile<IgneousBlade>().DistanceTimer = -IgneousExaltation.ChargeCooldown;
                        pro.ModProjectile<IgneousBlade>().CurrentState = IgneousBlade.AIState.CircleOwner;
                        pro.netUpdate = true;
                    }
                }
            }
            return false;
        }
        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            var tex = TextureAssets.Item[Type].Value;
            CalamityUtils.DrawInventoryCustomScale(
                spriteBatch,
                tex,
                position,
                frame,
                drawColor,
                itemColor,
                origin,
                scale,
                wantedScale: 0.75f,
                spriteEffects: Main.LocalPlayer.Calamity().InvertExaltationLineRotationDirections ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                rotation: Main.LocalPlayer.Calamity().InvertExaltationLineRotationDirections ? MathHelper.PiOver2 : 0
            );
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<UnholyCore>(10).
                AddIngredient<EssenceofHavoc>(5).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
