using System.Collections.Generic;
using System.Linq;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.Summon;
using CalamityMod.Projectiles.Summon;
using CalamityMod.Rarities;
using CalamityMod.Systems.Collections;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Summon;

[LegacyName("StaffoftheMechworm")]
public class VoidEaterMarionette : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Weapons.Summon";

    public bool FocusFetch = false;
    public override void SetStaticDefaults()
    {
        CalamityItemSets.ExtraDebuffTooltip_Enemy[Type] = [ModContent.BuffType<GodSlayerInferno>()];
    }
    public override void SetDefaults()
    {
        Item.width = 68;
        Item.height = 68;
        Item.damage = 110;
        Item.mana = 10;
        Item.useAnimation = Item.useTime = 24;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.noMelee = true;
        Item.knockBack = 2f;
        Item.value = CalamityGlobalItem.RarityDarkBlueBuyPrice;
        Item.rare = ModContent.RarityType<CosmicPurple>();
        Item.UseSound = SoundID.Item113;
        Item.autoReuse = true;
        Item.buffType = ModContent.BuffType<VoidEaterMarionetteBuff>();
        Item.shoot = ModContent.ProjectileType<VoidEaterMarionetteProjectile>();
        Item.DamageType = DamageClass.Summon;
        Item.shootSpeed = 8;
    }

    public override void HoldItem(Player player)
    {
        if (Main.myPlayer == player.whoAmI && player.ownedProjectileCounts[Item.shoot] > 0 && Item.JustPressedKeybind())
        {
            var p = Main.projectile.First(x => x.active && x.type == Item.shoot && x.owner == player.whoAmI);
            if (p.ModProjectile is VoidEaterMarionetteProjectile vem)
            {

                vem.FocusOnFetching = !vem.FocusOnFetching;
                p.netUpdate = true;
                if (vem.FocusOnFetching)
                    CombatText.NewText(player.Hitbox, Color.White, this.GetLocalizedValue("FetchStart"),true);
                else
                    CombatText.NewText(player.Hitbox, Color.White, this.GetLocalizedValue("FetchEnd"), true);
            }
        }
    }
    public override void ModifyTooltips(List<TooltipLine> tooltips) => tooltips.IntegrateDynamicHotkey(Item);
    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        if (player.ownedProjectileCounts[type] > 0)
        {
            var p = Main.projectile.First(x => x.active && x.type == type && x.owner == player.whoAmI);
            p.ai[0]++;
            p.netUpdate = true;
            return false;
        }
        position = Main.MouseWorld;
        var worm = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI,2);
        Projectile.NewProjectile(source, position, Vector2.Zero, ModContent.ProjectileType<DoGWeaponTeleportRift>(), 0, 0, player.whoAmI);
        return false;
    }
}
