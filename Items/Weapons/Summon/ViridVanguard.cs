using CalamityMod.Buffs.Summon;
using CalamityMod.Items.Materials;
using CalamityMod.Packets;
using CalamityMod.Projectiles.Summon;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Weapons.Summon
{
    public class ViridVanguard : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Summon";

        #region AI-Related Balancing Properties

        public static float IdleCirclingSpeed => 0.0375f;
        public static float AxeCirclingSpeedMultiplier => 3f;
        public static float ActiveAttackCirclingSpeedMultiplier => 10f;

        //The next three are the amount of times the sword does each attack before moving to the next one
        public static int HorizontalSlashAmount => 7;
        public static int VerticalPierceAmount => 5;
        public static int StabAmount => 6;
        /// <summary>
        /// How long the player must wait after an active ends to start the next one
        /// </summary>
        public static int ActiveAttackCooldown => 300;
        public static int ActiveAttackStartup => 90;
        public static int ActiveAttackEndlag => 30;
        /// <summary>
        /// How many slashes each vanguard will do during the active
        /// </summary>
        public static int ActiveAttackSlashCount => 4;
        #endregion
        #region Damage Balancing Properties


        //Sage Poison's damage is kept in the Sage Poison file
        public static float ActiveAttackSlashDmgMult => 4f;

        //Note: the following two only apply when not using the active attack
        public static float AxeDmgMult => 1.25f;
        public static float SwordDmgMult => 1f;
        #endregion

        private static Texture2D BladeOutline = null;
        public static Texture2D GetBladeOutlineTex()
        {
            if (BladeOutline == null)
            {
                var texture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Summon/ViridVanguardBlade").Value;
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
            Item.width = 26;
            Item.height = 36;
            Item.damage = 55;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 10;
            Item.useAnimation = Item.useTime = 24;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.noMelee = true;
            Item.knockBack = 5f;
            Item.value = CalamityGlobalItem.RarityTurquoiseBuyPrice;
            Item.rare = ItemRarityID.Red;
            Item.UseSound = SoundID.Item71;
            Item.autoReuse = true;
            Item.buffType = ModContent.BuffType<ViridVanguardBuff>();
            Item.shoot = ModContent.ProjectileType<ViridVanguardBlade>();
            Item.rare = ModContent.RarityType<Turquoise>();
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
                wantedScale: 0.9f,
                spriteEffects: Main.LocalPlayer.Calamity().InvertExaltationLineRotationDirections ? SpriteEffects.FlipHorizontally : SpriteEffects.None
            );
            return false;
        }
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
                    if (pro.type == type && pro.owner == player.whoAmI && pro.ModProjectile<ViridVanguardBlade>().ActiveTimer == 0)
                    {
                        pro.ModProjectile<ViridVanguardBlade>().BeginSuperEpicPhotonRipperZenithKnockoffAttack();
                    }
                }
            }
            else
            {
                player.AddBuff(Item.buffType, 2);

                var minion = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI, 0f, 1f);
                minion.ModProjectile<ViridVanguardBlade>().BladeIndex = player.ownedProjectileCounts[type];

                int bladeIndex = 0;
                foreach (Projectile pro in Main.ActiveProjectiles)
                {
                    if (pro.type == type && pro.owner == player.whoAmI)
                    {
                        pro.ModProjectile<ViridVanguardBlade>().BladeIndex = bladeIndex++;
                        pro.ModProjectile<ViridVanguardBlade>().ActiveTimer = ViridVanguard.ActiveAttackCooldown;
                        pro.ModProjectile<ViridVanguardBlade>().AITimer = 0f;
                        pro.netUpdate = true;
                    }
                }
            }
            return false;
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<IgneousExaltation>().
                AddIngredient<ViralSprout>().
                AddIngredient<UelibloomBar>(15).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }

}
