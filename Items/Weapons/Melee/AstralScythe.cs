using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Projectiles.Melee;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Melee;

public class AstralScythe : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Weapons.Melee";
    public override void SetDefaults()
    {
        Item.width = 56;
        Item.height = 60;
        Item.damage = 80;
        Item.DamageType = DamageClass.Melee;
        Item.useTurn = true;
        Item.useAnimation = Item.useTime = 20;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.knockBack = 6f;
        Item.UseSound = SoundID.Item71;
        Item.autoReuse = true;
        Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
        Item.rare = ItemRarityID.Lime;
        Item.shoot = ModContent.ProjectileType<AstralScytheProjectile>();
        Item.shootSpeed = 5f;
    }

    public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
    {
        target.AddBuff(ModContent.BuffType<AstralInfectionDebuff>(), 300);
    }

    public override void OnHitPvp(Player player, Player target, Player.HurtInfo hurtInfo)
    {
        target.AddBuff(ModContent.BuffType<AstralInfectionDebuff>(), 300);
    }
}
