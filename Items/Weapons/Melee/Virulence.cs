using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Projectiles.Melee;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Melee;

[LegacyName("VirulentKatana")]
public class Virulence : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Weapons.Melee";
    public override void SetDefaults()
    {
        Item.width = 74;
        Item.height = 90;
        Item.damage = 100;
        Item.knockBack = 5.5f;
        Item.useAnimation = Item.useTime = 15;
        Item.DamageType = DamageClass.Melee;
        Item.useTurn = true;
        Item.autoReuse = true;
        Item.shootSpeed = 9f;
        Item.shoot = ModContent.ProjectileType<VirulentWave>();

        Item.useStyle = ItemUseStyleID.Swing;
        Item.UseSound = SoundID.Item1;
        Item.value = CalamityGlobalItem.RarityYellowBuyPrice;
        Item.rare = ItemRarityID.Yellow;
    }

    public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) => damage = (int)(damage * 0.85);

    public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
    {
        target.AddBuff(ModContent.BuffType<Plague>(), 300);
    }

    public override void OnHitPvp(Player player, Player target, Player.HurtInfo hurtInfo)
    {
        target.AddBuff(ModContent.BuffType<Plague>(), 300);
    }
}
