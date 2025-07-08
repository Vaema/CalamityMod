using System;
using CalamityMod.Items.Weapons.Magic;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Projectiles.Ranged;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Ranged
{
    public class DragonsBreath : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged";

        public static readonly SoundStyle FireballSound = new("CalamityMod/Sounds/Custom/Yharon/YharonFireball", 3) { PitchVariance = 0.3f, Volume = 0.75f };
        public static readonly SoundStyle WeldingStart = new("CalamityMod/Sounds/Item/DragonsBreathStrongStart") { Volume = 1.75f };
        public static readonly SoundStyle WeldingBurn = new("CalamityMod/Sounds/Item/WeldingBurn") { Volume = 0.65f };
        public static readonly SoundStyle WeldingShoot = new("CalamityMod/Sounds/Item/WeldingShoot") { Volume = 0.45f };

        public int Counter = 0;
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<PhoenixFlameBarrage>();
        }
        public override void SetDefaults()
        {
            Item.width = 124;
            Item.height = 72;
            Item.damage = 328;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.autoReuse = true;
            Item.channel = true;
            Item.useStyle = ItemUseStyleID.Shoot;

            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.knockBack = 4.5f;
            Item.UseSound = null;
            Item.shoot = ModContent.ProjectileType<DragonsBreathHoldout>();
            Item.shootSpeed = 3.5f;
            Item.useAmmo = AmmoID.Gel;

            Item.rare = ModContent.RarityType<BurnishedAuric>();
            Item.value = CalamityGlobalItem.RarityVioletBuyPrice;
        }
        public override bool CanConsumeAmmo(Item ammo, Player player) => player.ownedProjectileCounts[Item.shoot] > 0;
        public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;
        public override void HoldItem(Player player) => player.Calamity().mouseWorldListener = true;
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile holdout = Projectile.NewProjectileDirect(source, position, Vector2.Zero, ModContent.ProjectileType<DragonsBreathHoldout>(), damage, knockback, player.whoAmI);

            // 14NOV2024: Ozzatron: clamped mouse position unnecessary, only used for direction
            // We set the rotation to the direction to the mouse so the first frame doesn't appear bugged out.
            holdout.velocity = (player.Calamity().mouseWorld - player.MountedCenter).SafeNormalize(Vector2.Zero);

            return false;
        }
    }
}
