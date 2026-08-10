using System.Linq;
using CalamityMod.Cooldowns;
using CalamityMod.Items.Weapons.Magic;
using CalamityMod.Projectiles.Ranged;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Ranged;

public class Hellborn : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Weapons.Ranged";
    public override void SetStaticDefaults()
    {
        ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
        ItemID.Sets.IsRangedSpecialistWeapon[Type] = true;
    }
    public override void SetDefaults()
    {
        Item.width = 62;
        Item.height = 34;
        Item.damage = 300;
        Item.DamageType = DamageClass.Ranged;
        Item.useAnimation = Item.useTime = 30;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.knockBack = 2.5f;

        Item.value = CalamityGlobalItem.RarityPinkBuyPrice;
        Item.rare = ItemRarityID.Pink;

        Item.noMelee = true;
        Item.UseSound = null;
        Item.noUseGraphic = true;
        Item.channel = true;
        Item.autoReuse = true;

        Item.shoot = ModContent.ProjectileType<HellbornHoldout>();
        Item.shootSpeed = 12f;
    }
    public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;
    public override bool AltFunctionUse(Player player) => true;
    public override void HoldItem(Player player)
    {
        if (Main.myPlayer == player.whoAmI)
            player.Calamity().rightClickListener = true;


        if (player.Calamity().cooldowns.TryGetValue(HellbornShots.ID, out var cooldown))
        {
            cooldown.timeLeft = 12 - player.Calamity().hellbornShots;
        }
        else
        {
            player.AddCooldown(HellbornShots.ID, 12);
        }
    }
    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        // Only one out at a time
        if (Main.projectile.Any(n => n.active && n.type == Item.shoot && n.owner == player.whoAmI))
            return false;
        float gunType = 0;
        if (player.Calamity().mouseRight && player.whoAmI == Main.myPlayer && !Main.mapFullscreen && !Main.blockMouse)
            gunType = 5;
        Projectile holdout = Projectile.NewProjectileDirect(source, player.MountedCenter, Vector2.Zero, type, damage, knockback, player.whoAmI, 0, 0, gunType);

        // 14NOV2024: Ozzatron: clamped mouse position unnecessary, only used for direction
        // We set the rotation to the direction to the mouse so the first frame doesn't appear bugged out.
        holdout.velocity = (player.Calamity().mouseWorld - player.MountedCenter).SafeNormalize(Vector2.Zero);
        return false;
    }
}
