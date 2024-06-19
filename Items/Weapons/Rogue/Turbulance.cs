using Terraria.DataStructures;
using CalamityMod.Projectiles.Rogue;
using CalamityMod.Items.Materials;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using Steamworks;
using Terraria.Audio;
using CalamityMod.Projectiles;
using CalamityMod.Utilities;

namespace CalamityMod.Items.Weapons.Rogue
{
    public class Turbulance : RogueWeapon
    {
        public static SoundStyle LightningStrike = new SoundStyle("CalamityMod/Sounds/Item/TurbulanceLightningStrike");

        public override void SetDefaults()
        {
            Item.width = 14;
            Item.height = 14;
            Item.damage = 16;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 18;
            Item.knockBack = 5f;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityOrangeBuyPrice;
            Item.rare = ItemRarityID.Orange;
            Item.shoot = ModContent.ProjectileType<TurbulanceProjectile>();
            Item.shootSpeed = 12f;
            Item.DamageType = RogueDamageClass.Instance;
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            ExtraArmAnimations.ThrowArmAnimationSlow(player, Item);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.Calamity().mouseWorldListener = true;

            Vector2 SpawnPos = player.MountedCenter + new Vector2(0, -16);
            Vector2 vel = velocity;

            SoundEngine.PlaySound(SoundID.DD2_MonkStaffSwing.WithPitchOffset(0.8f), player.MountedCenter);

            if (player.Calamity().StealthStrikeAvailable())
            {
                SoundEngine.PlaySound(WulfrumKnife.Throw1Sound, player.MountedCenter);

                vel *= 0.15f;
            }

            int proj = Projectile.NewProjectile(source, SpawnPos, SpawnPos.DirectionTo(player.Calamity().mouseWorld) * vel.Length(), type, damage, knockback, player.whoAmI);
            Main.projectile[proj].Calamity().stealthStrike = player.Calamity().StealthStrikeAvailable();
            return false;
        }

        public override bool CanShoot(Player player)
        {
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<AerialiteBar>(7).
                AddIngredient(ItemID.SunplateBlock, 3).
                AddTile(TileID.SkyMill).
                Register();
        }
    }
}
