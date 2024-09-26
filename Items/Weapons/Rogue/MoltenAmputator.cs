using System;
using CalamityMod.Projectiles.Rogue;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Rogue
{
    public class MoltenAmputator : RogueWeapon
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;
        }
        public override void SetDefaults()
        {
            Item.width = 60;
            Item.height = 60;
            Item.damage = 766;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.autoReuse = true;
            Item.useAnimation = 35;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 35;
            Item.knockBack = 9f;
            Item.value = CalamityGlobalItem.RarityTurquoiseBuyPrice;
            Item.rare = ModContent.RarityType<Turquoise>();
            Item.shoot = ModContent.ProjectileType<MoltenAmputatorProj>();
            Item.shootSpeed = 17;
            Item.DamageType = RogueDamageClass.Instance;
        }
        public override bool AltFunctionUse(Player player) => true;
        public override float StealthDamageMultiplier => 1.07f;
        public override float UseSpeedMultiplier(Player player)
        {
            if (player.altFunctionUse == 2)
                return 2f;
            else
                return 1;

        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            /*
            if (player.Calamity().StealthStrikeAvailable())
            {
                int ss = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
                if (ss.WithinBounds(Main.maxProjectiles))
                    Main.projectile[ss].Calamity().stealthStrike = true;
                return false;
            }
            */
            if (player.altFunctionUse == 2)
            {
                for (int x = 0; x < Main.maxProjectiles; x++)
                {
                    Projectile projectile = Main.projectile[x];
                    if (projectile.active && projectile.type == type)
                    {
                        projectile.ai[2] = 5;
                    }
                }

                SoundStyle fire = new("CalamityMod/Sounds/Item/StygianCatch");
                SoundEngine.PlaySound(fire with { Volume = 0.7f, Pitch = Main.rand.NextFloat(-0.7f, -0.8f) }, position);
            }
            else
            {
                SoundStyle fire = new("CalamityMod/Sounds/Item/SwingMid");
                SoundEngine.PlaySound(fire with { Volume = 0.5f, Pitch = Main.rand.NextFloat(0.4f, 0.65f) }, position);
                Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 0, 0, 0);
            }
            return false;
        }
        public override void UseItemFrame(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                player.ChangeDir(Math.Sign((player.Calamity().mouseWorld - player.Center).X));

                float animProgress = 0.5f - player.itemTime / (float)player.itemTimeMax;
                float rotation = (player.Center - player.Calamity().mouseWorld).ToRotation() * player.gravDir + MathHelper.PiOver2;
                if (animProgress < 0.4f)
                    rotation += -0.39f * (float)Math.Pow((0.6f - animProgress) / 0.6f, 2) * player.direction;

                player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, rotation);
            }
        }
    }
}
