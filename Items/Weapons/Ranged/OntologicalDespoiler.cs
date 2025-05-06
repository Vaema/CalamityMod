using System.Collections.Generic;
using System.Linq;
using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Ranged;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Ranged
{
    [LegacyName("MolecularManipulator")]
    public class OntologicalDespoiler : ModItem, ILocalizedModType
    {
        public static readonly SoundStyle ChargeLV1 = new("CalamityMod/Sounds/Item/ArcNovaDiffuserChargeLV1");
        public static readonly SoundStyle ChargeLV2 = new("CalamityMod/Sounds/Item/ArcNovaDiffuserChargeLV2");
        public static readonly SoundStyle ChargeStart = new("CalamityMod/Sounds/Item/ArcNovaDiffuserChargeStart");
        public static readonly SoundStyle ChargeLoop = new("CalamityMod/Sounds/Item/ArcNovaDiffuserChargeLoop");
        internal static readonly int ChargeLoopSoundFrames = 151;
        public static readonly SoundStyle SmallShot = new("CalamityMod/Sounds/Item/ArcNovaDiffuserSmallShot");
        public static readonly SoundStyle BigShot = new("CalamityMod/Sounds/Item/ArcNovaDiffuserBigShot");
        public static readonly SoundStyle BigShot2 = new("CalamityMod/Sounds/Item/OntologicalDespoilerLargeShot");
        public static readonly SoundStyle SmallImpact = new("CalamityMod/Sounds/Item/ArcNovaDiffuserChargeLV1");
        public static readonly SoundStyle LargeImpact = new("CalamityMod/Sounds/Item/ArcNovaDiffuserChargeLV2");

        public bool shotType = true; // true = positive shot, false = negative shot
        public new string LocalizationCategory => "Items.Weapons.Ranged";

        public override void SetStaticDefaults()
        {
            ItemID.Sets.IsRangedSpecialistWeapon[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 88;
            Item.height = 34;
            Item.damage = 445;
            Item.DamageType = DamageClass.Ranged;
            Item.useAnimation = Item.useTime = 8;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 6f;
            Item.value = CalamityGlobalItem.RarityTurquoiseBuyPrice;
            Item.rare = ModContent.RarityType<Turquoise>();
            Item.UseSound = null;
            Item.autoReuse = false;
            Item.shoot = ModContent.ProjectileType<OntologicalDespoilerHoldout>();
            Item.shootSpeed = 12f;
        }
        public override void ModifyWeaponCrit(Player player, ref float crit) => crit += 25;
        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;

        public override void HoldItem(Player player)
        {
            if (Main.myPlayer == player.whoAmI)
                player.Calamity().rightClickListener = true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Only one out at a time
            if (Main.projectile.Any(n => n.active && n.type == Item.shoot && n.owner == player.whoAmI))
                return false;

            // 14NOV2024: Ozzatron: clamped mouse position unnecessary, only used for direction

            if (player.Calamity().mouseRight && player.whoAmI == Main.myPlayer && !Main.mapFullscreen && !Main.blockMouse)
            {
                Projectile holdout2 = Projectile.NewProjectileDirect(source, player.Center, Vector2.Zero, Item.shoot, player.ActiveItem().damage, 0f, player.whoAmI, 0, 0, 10 + (shotType ? 5 : 0));
                holdout2.velocity = (player.Calamity().mouseWorld - player.MountedCenter).SafeNormalize(Vector2.Zero);
                SoundStyle fire = new("CalamityMod/Sounds/Item/DudFire");
                SoundEngine.PlaySound(fire with { Volume = 0.7f, Pitch = -0.5f + (shotType ? 0.5f : 0) }, player.Center);
                shotType = !shotType;
            }
            else
            {
                Projectile holdout = Projectile.NewProjectileDirect(source, player.MountedCenter, Vector2.Zero, type, damage, knockback, player.whoAmI, 0, 0, shotType ? 0 : 5);
                holdout.velocity = player.Calamity().mouseWorld - player.RotatedRelativePoint(player.MountedCenter);
            }
            return false;
        }
        public override void ModifyTooltips(List<TooltipLine> list)
        {
            Player Owner = Main.LocalPlayer;
            if (Owner is null)
                return;
            float rate = (Main.GlobalTimeWrappedHourly * 3);
            List<Color> eColors = new List<Color>()
                {
                    Owner.shirtColor,
                    Color.Lerp(Owner.shirtColor, Color.Black, 0.3f),
                    Color.Lerp(Owner.shirtColor, Color.White, 0.2f),
                    Color.Lerp(Owner.shirtColor, Color.White, 0.4f)
            };
            int colorIndex = (int)(rate / 2 % eColors.Count);
            Color currentColor = eColors[colorIndex];
            Color nextColor = eColors[(colorIndex + 1) % eColors.Count];
            Color eTooltipColor = Color.Lerp(currentColor, nextColor, rate % 2f > 1f ? 1f : rate % 1f);
            if (Owner.shirtColor == Color.White)
                eTooltipColor = new Color(Main.DiscoR, Main.DiscoG, Main.DiscoB);

            TooltipLine line = list.FirstOrDefault(x => x.Mod == "Terraria" && x.Name == "Tooltip7");
            if (line != null)
                line.OverrideColor = Color.Lerp(eTooltipColor, Color.White, 0.2f);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<ArcNovaDiffuser>().
                AddIngredient<NullificationPistol>().
                AddIngredient<DarkPlasma>(2).
                AddIngredient<CoreofCalamity>().
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
