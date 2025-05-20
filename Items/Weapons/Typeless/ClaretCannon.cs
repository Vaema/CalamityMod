using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Typeless;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;
using System;

namespace CalamityMod.Items.Weapons.Typeless
{
    public class ClaretCannon : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Typeless";

        public int ClaretCooldown = 0;

        public static int ClaretCooldownMax = 600;

        public float baseUseDir = 0;
        public override void SetDefaults()
        {
            Item.width = 48;
            Item.height = 30;
            Item.damage = 500;
            Item.DamageType = AverageDamageClass.Instance;
            Item.useTime = 10;
            Item.useAnimation = 10;
            Item.useLimitPerAnimation = 1;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 5.5f;
            Item.value = CalamityGlobalItem.RarityTurquoiseBuyPrice;
            Item.rare = ModContent.RarityType<Turquoise>();
            Item.UseSound = SoundID.Item40;
            Item.autoReuse = true;
            Item.shootSpeed = 12f;
            Item.shoot = ModContent.ProjectileType<ClaretCannonProj>();
        }

        public override Vector2? HoldoutOffset() => new Vector2(-5, 0);

        public override void UpdateInventory(Player player)
        {
            ClaretCooldown--;
            if (ClaretCooldown < 0)
                ClaretCooldown = 0;   
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<BloodstoneCore>(4).
                AddTile(TileID.MythrilAnvil).
                Register();
        }

        public override bool CanUseItem(Player player)
        {
            if (ClaretCooldown > 0)
                return false;
            return true;
        }

        public override void UseAnimation(Player player)
        {
        }

        public override void UseItemFrame(Player player)
        {
            float comp = 1 - player.itemTime / (float)player.itemTimeMax;
            float lerpValue1 = MathHelper.Lerp(0, 90, MathF.Pow(comp, 0.3f));
            float lerpValue2 = MathHelper.Lerp(90, 0, MathF.Pow(comp, 2));
            player.itemRotation = baseUseDir - MathHelper.ToRadians(Math.Min(lerpValue1, lerpValue2)) * player.direction;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            baseUseDir = player.itemRotation; 
            ClaretCooldown = ClaretCooldownMax;
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }

        public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            float fill = ClaretCooldown / (float)ClaretCooldownMax;
            if (fill <= 0)
                return;

            float barScale = 1.5f;

            var barBG = ModContent.Request<Texture2D>("CalamityMod/UI/MiscTextures/GenericBarBack").Value;
            var barFG = ModContent.Request<Texture2D>("CalamityMod/UI/MiscTextures/GenericBarFront").Value;

            Vector2 barOrigin = barBG.Size() * 0.5f;
            float yOffset = 5f;
            Vector2 drawPos = position + Vector2.UnitY * scale * (frame.Height - yOffset);
            Rectangle frameCrop = new Rectangle(0, 0, (int)((fill) * barFG.Width), barFG.Height);
            Color colorBG = Color.Crimson;
            Color colorFG = Color.Lerp(Color.OrangeRed, Color.DarkOrange, fill);

            spriteBatch.Draw(barBG, drawPos, null, colorBG, 0f, barOrigin, scale * barScale, 0f, 0f);
            spriteBatch.Draw(barFG, drawPos, frameCrop, colorFG, 0f, barOrigin, scale * barScale, 0f, 0f);
        }

    }
}
