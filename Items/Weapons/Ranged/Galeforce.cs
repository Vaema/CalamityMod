using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Ranged;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Projectiles.Magic;
using System.Linq;

namespace CalamityMod.Items.Weapons.Ranged
{
    public class Galeforce : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged";
        public override void SetDefaults()
        {
            Item.damage = 22;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 38;
            Item.height = 68;
            Item.useTime = 48;
            Item.useAnimation = 48;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.channel = true;
            Item.knockBack = 3f;
            Item.value = CalamityGlobalItem.RarityOrangeBuyPrice;
            Item.rare = ItemRarityID.Orange;
            Item.UseSound = SoundID.Item17;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<GaleforceHoldout>();
            Item.shootSpeed = 20f;
            Item.Calamity().canFirePointBlankShots = true;
        }
        public override void HoldItem(Player player)
        {
            player.Calamity().mouseWorldListener = true;
        }

        public override bool CanUseItem(Player player)
        {
            return !Main.projectile.Any(n => n.active && n.owner == player.whoAmI && n.type == ModContent.ProjectileType<GaleforceHoldout>());
        }

        public override void UseItemFrame(Player player)
        {
            //Calculate the dirction in which the players arms should be pointing at.
            Vector2 playerToCursor = (player.Calamity().mouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
            float armPointingDirection = (playerToCursor.ToRotation());

            player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, armPointingDirection - MathHelper.PiOver2);
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armPointingDirection - MathHelper.PiOver2);
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            if (player.Calamity().mouseWorld.X > player.Center.X)
            {
                player.ChangeDir(1);
            }
            else
            {
                player.ChangeDir(-1);
            }

            CalamityUtils.CleanHoldStyle(player, player.compositeFrontArm.rotation + MathHelper.PiOver2, player.GetFrontHandPosition(player.compositeFrontArm.stretch, player.compositeFrontArm.rotation).Floor(), new Vector2(38, 68), new Vector2(-9, 0));
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<AerialiteBar>(8).
                AddIngredient(ItemID.SunplateBlock, 3).
                AddTile(TileID.SkyMill).
                Register();
        }
    }
}
