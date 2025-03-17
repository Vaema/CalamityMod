using System;
using System.Collections.Generic;
using CalamityMod.CustomRecipes;
using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.DraedonsArsenal;
using CalamityMod.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.DraedonsArsenal
{
    public class GaussRifle : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.DraedonsArsenal";
        public static readonly SoundStyle FireSound = new("CalamityMod/Sounds/Item/LargeMechGaussRifle");

        public override void SetDefaults()
        {
            CalamityGlobalItem modItem = Item.Calamity();

            Item.width = 70;
            Item.height = 30;
            Item.DamageType = DamageClass.Ranged;
            Item.damage = 100;
            Item.knockBack = 0.2f;
            Item.useTime = Item.useAnimation = 10;
            Item.autoReuse = true;

            Item.useStyle = ItemUseStyleID.Shoot;
            Item.UseSound = new SoundStyle("CalamityMod/Sounds/Item/GaussFire") with { Volume = 0.7f, Pitch = Main.rand.NextFloat(0.5f, 0.65f) };
            Item.noMelee = true;

            Item.value = CalamityGlobalItem.RarityYellowBuyPrice;
            Item.rare = ItemRarityID.Yellow;

            Item.shoot = ModContent.ProjectileType<GaussRifleBlast>();
            Item.shootSpeed = 7f;

            modItem.UsesCharge = true;
            modItem.MaxCharge = 135f;
            modItem.ChargePerUse = 0.0225f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position + velocity * 6, velocity.RotatedByRandom(0.03f), ModContent.ProjectileType<GaussRifleBlast>(), damage, knockback, player.whoAmI);
            return false;
        }
        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Item.DrawItemGlowmaskSingleFrame(spriteBatch, rotation, ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/DraedonsArsenal/GaussRifleGlow").Value);
        }
        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            player.ChangeDir(Math.Sign((player.Calamity().mouseWorld - player.Center).X));
            float itemRotation = player.compositeFrontArm.rotation + MathHelper.PiOver2 * player.gravDir;

            Vector2 itemPosition = player.MountedCenter + itemRotation.ToRotationVector2() * 7f;
            Vector2 itemSize = new Vector2(70, 30);
            Vector2 itemOrigin = new Vector2(-18, 6);

            CalamityUtils.CleanHoldStyle(player, itemRotation, itemPosition, itemSize, itemOrigin);

            base.UseStyle(player, heldItemFrame);
        }
        public override void UseItemFrame(Player player)
        {
            player.ChangeDir(Math.Sign((player.Calamity().mouseWorld - player.Center).X));

            float animProgress = 0.5f - player.itemTime / (float)player.itemTimeMax;
            float rotation = (player.Center - player.Calamity().mouseWorld).ToRotation() * player.gravDir + MathHelper.PiOver2;
            if (animProgress < 0.4f)
                rotation += -0.025f * (float)Math.Pow((0.6f - animProgress) / 0.6f, 2) * player.direction;

            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, rotation);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips) => CalamityGlobalItem.InsertKnowledgeTooltip(tooltips, 3);

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<MysteriousCircuitry>(18).
                AddIngredient<DubiousPlating>(12).
                AddIngredient<LifeAlloy>(5).
                AddIngredient<InfectedArmorPlating>(10).
                AddCondition(ArsenalTierGatedRecipe.ConstructRecipeCondition(3, out Func<bool> condition), condition).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
