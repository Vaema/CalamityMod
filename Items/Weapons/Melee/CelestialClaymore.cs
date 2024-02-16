using Terraria.DataStructures;
using CalamityMod.Projectiles.Melee;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Melee
{
    public class CelestialClaymore : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Melee";
        public override void SetDefaults()
        {
            Item.width = 80;
            Item.height = 82;
            Item.damage = 59;
            Item.DamageType = DamageClass.Melee;
            Item.useAnimation = 24;
            Item.useTime = 24;
            Item.useTurn = true;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 5.25f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.Rarity4BuyPrice;
            Item.rare = ItemRarityID.LightRed;
            Item.shoot = ModContent.ProjectileType<CosmicSpiritBomb1>();
            Item.shootSpeed = 0.1f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 realPlayerPos = player.RotatedRelativePoint(player.MountedCenter, true);
            float mouseXDist = Main.mouseX + Main.screenPosition.X - realPlayerPos.X;
            float mouseYDist = Main.mouseY + Main.screenPosition.Y - realPlayerPos.Y;
            if (player.gravDir == -1f)
            {
                mouseYDist = Main.screenPosition.Y + Main.screenHeight + Main.mouseY + realPlayerPos.Y;
            }
            if ((float.IsNaN(mouseXDist) && float.IsNaN(mouseYDist)) || (mouseXDist == 0f && mouseYDist == 0f))
            {
                mouseXDist = player.direction;
                mouseYDist = 0f;
            }

            Vector2 realPositionOrig = realPlayerPos;
            mouseXDist *= 0.8f;
            mouseYDist *= 0.8f;

            for (int i = 0; i < 2; i++)
            {
                realPlayerPos = realPositionOrig;
                realPlayerPos.X += Main.rand.Next(-100, 101);
                realPlayerPos.Y += Main.rand.Next(-100, 101);
                realPlayerPos += new Vector2(mouseXDist, mouseYDist);

                switch (Main.rand.Next(3))
                {
                    case 0:
                        type = ModContent.ProjectileType<CosmicSpiritBomb1>();
                        break;
                    case 1:
                        type = ModContent.ProjectileType<CosmicSpiritBomb2>();
                        break;
                    case 2:
                        type = ModContent.ProjectileType<CosmicSpiritBomb3>();
                        break;
                    default:
                        break;
                }
                Projectile.NewProjectile(source, realPlayerPos.X, realPlayerPos.Y, 0f, 0f, type, (int)(damage * 0.8), knockback, player.whoAmI, 0f, (float)Main.rand.Next(3));
            }
            return false;
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            if (Main.rand.NextBool(4))
            {
                int dustType = Main.rand.Next(2);
                if (dustType == 0)
                {
                    dustType = 15;
                }
                else if (dustType == 1)
                {
                    dustType = 73;
                }
                else
                {
                    dustType = 244;
                }
                int swingDust = Dust.NewDust(new Vector2((float)hitbox.X, (float)hitbox.Y), hitbox.Width, hitbox.Height, dustType, (float)(player.direction * 2), 0f, 150, default, 1.3f);
                Main.dust[swingDust].velocity *= 0.2f;
            }
        }
    }
}
