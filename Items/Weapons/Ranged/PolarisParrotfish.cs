using CalamityMod.CalPlayer;
using CalamityMod.Projectiles.Ranged;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Sounds;
using System;
using Terraria.Audio;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.Projectiles.Boss;
using static Humanizer.In;
using System.Collections.Generic;

namespace CalamityMod.Items.Weapons.Ranged
{
    public class PolarisParrotfish : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged";

        public int SetUseTime = 6;
        public int SetUseAnimation = 6;
        public int ShotNumber = 0;
        public bool Happy = false;

        public static readonly SoundStyle Shot = new("CalamityMod/Sounds/Item/PolarisShot") { Volume = 0.6f };
        public static readonly SoundStyle Squeak = new("CalamityMod/Sounds/Custom/CuteSqueak") { Volume = 0.75f };
        public override void SetStaticDefaults()
        {
            Item.staff[Item.type] = true; //so it doesn't look weird af when holding it
        }

        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 34;
            Item.damage = 35;
            Item.DamageType = DamageClass.Ranged;

            Item.useTime = SetUseTime;
            Item.useAnimation = SetUseAnimation;

            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 0.5f;
            Item.value = CalamityGlobalItem.Rarity4BuyPrice;
            Item.rare = ItemRarityID.LightRed;
            Item.UseSound = null;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<PolarStar>();
            Item.shootSpeed = 10f;
        }
        public override void ModifyTooltips(List<TooltipLine> list) => list.FindAndReplace("[GFB]", this.GetLocalizedValue(Main.zenithWorld ? "TooltipGFB" : "TooltipNormal"));
        public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] < 2; // Max of 2 shots on screen at once, get closer to fire faster
        public override bool AltFunctionUse(Player player) => Main.zenithWorld ? true : false;
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (Main.zenithWorld && player.altFunctionUse == 2)
            {
                //GFB stuff
                if (Happy && Main.rand.NextBool(50)) // If you pet her too much, you will regret it
                {
                    Happy = false;
                    player.itemTime = 200;
                    player.itemAnimation = 200;
                    player.Calamity().GeneralScreenShakePower = 26f;
                    player.AddBuff(BuffID.Obstructed, 600);
                    Main.NewText("Too much love...", 255, 0, 0);
                    SoundEngine.PlaySound(Squeak with { Pitch = -1f }, player.Center);
                    SoundStyle roar = new("CalamityMod/Sounds/Custom/CeaselessVoidDeathBuild");
                    SoundEngine.PlaySound(roar with { Pitch = 0.5f }, player.Center);
                    int theFuckening = ModContent.ProjectileType<AstralFlame>();
                    int projDamage = 500;
                    int totalProjectiles = 50;
                    float radians = MathHelper.TwoPi / totalProjectiles;
                    Vector2 spinningPoint = Vector2.Normalize(new Vector2(-1f, -1f));
                    for (int k = 0; k < totalProjectiles; k++)
                    {
                        Vector2 projVelocity = spinningPoint.RotatedBy(radians * k);
                        Projectile.NewProjectile(player.GetSource_FromThis(), player.Center + projVelocity * 2000, -projVelocity * 4, theFuckening, projDamage, 0f, Main.myPlayer);
                    }
                }
                else // Pet Polaris
                {
                    SoundEngine.PlaySound(Squeak, player.Center);
                    CombatText.NewText(player.Hitbox, Color.Violet, "^-^");
                    Happy = true;
                    switch (Main.rand.Next(1, 5 + 1))
                    {
                        case 5:
                            Main.NewText("You give Polaris belly rubs", 72, 209, 204);
                            break;
                        case 4:
                            Main.NewText("You give Polaris a small treat", 72, 209, 204);
                            break;
                        case 3:
                            Main.NewText("You tell Polaris she's a good girl", 72, 209, 204);
                            break;
                        case 2:
                            Main.NewText("You let Polaris cuddle your arm", 72, 209, 204);
                            break;
                        default:
                            Main.NewText("You pet Polaris", 72, 209, 204);
                            break;
                    }

                    for (int i = 0; i <= 6; i++)
                    {
                        Vector2 hVelocity = new Vector2(0, -4).RotateRandom(0.45);
                        hVelocity.X *= 0.66f;
                        hVelocity *= Main.rand.NextFloat(1f, 2f);

                        int heart = Gore.NewGore(player.GetSource_FromThis(), player.Center + velocity * 4, hVelocity, 331, Main.rand.NextFloat(0.2f, 1.3f));
                        Main.gore[heart].sticky = false;
                        Main.gore[heart].velocity *= 0.4f;
                        Main.gore[heart].velocity.Y -= 0.85f;
                    }
                }
            }
            else
            {
                if (Happy) // If she's happy, fire much faster
                {
                    Item.useTime = (int)(SetUseTime * 0.5f);
                    Item.useAnimation = (int)(SetUseAnimation * 0.5f);
                }
                else // Otherwise reset usetime
                {
                    Item.useTime = SetUseTime;
                    Item.useAnimation = SetUseAnimation;
                }

                if (Main.zenithWorld) // 1% chance to get tired when firing a projectile
                {
                    if (Happy && Main.rand.NextBool(100))
                    {
                        CombatText.NewText(player.Hitbox, Color.Violet, ">~<");
                        Happy = false;
                        SoundEngine.PlaySound(Squeak with { Pitch = -0.6f }, player.Center);
                    }
                    else
                        SoundEngine.PlaySound(Shot, player.Center);
                }
                else
                    SoundEngine.PlaySound(Shot, player.Center);

                Projectile.NewProjectile(source, position + velocity * 5f, velocity.RotatedByRandom(0.05f), ModContent.ProjectileType<PolarStar>(), damage, knockback, player.whoAmI, 0f, ShotNumber);

                if (ShotNumber >= 2) // Cycle the shot color
                    ShotNumber = 0;
                else
                    ShotNumber++;
            }
            return false;
        }
    }
}
