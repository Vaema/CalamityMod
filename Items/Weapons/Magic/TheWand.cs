using CalamityMod.Projectiles.Magic;
using CalamityMod.Rarities;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Systems.Collections;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Magic;

public class TheWand : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Weapons.Magic";
    // The actual base damage of The Wand. The damage reported on the item is just the spark, which is irrelevant.
    public static int BaseDamage = 599;

    public override void SetStaticDefaults()
    {
        Item.staff[Type] = true;
        CalamityItemSets.ExtraDebuffTooltip_Enemy[Type] = [ModContent.BuffType<Dragonfire>()];
    }

    public override void SetDefaults()
    {
        Item.width = 40;
        Item.height = 36;
        Item.damage = 1;
        Item.mana = 200;
        Item.DamageType = DamageClass.Magic;
        Item.noMelee = true;
        Item.useAnimation = 19;
        Item.useTime = 19;
        Item.useTurn = true;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.knockBack = 0.5f;
        Item.UseSound = SoundID.Item102;
        Item.autoReuse = true;
        Item.value = Item.buyPrice(platinum: 15); // Sold by Shady Salesman
        Item.shoot = ModContent.ProjectileType<SparkInfernal>();
        Item.shootSpeed = 24f;
        Item.rare = ModContent.RarityType<BurnishedAuric>();
    }

    public override bool CanUseItem(Player player)
    {
        int numWandBolts = player.ownedProjectileCounts[ModContent.ProjectileType<SparkInfernal>()];
        int numTornadoStarters = player.ownedProjectileCounts[ModContent.ProjectileType<InfernadoMarkFriendly>()];
        int numTornadoPieces = player.ownedProjectileCounts[ModContent.ProjectileType<InfernadoFriendly>()];
        return numWandBolts + numTornadoStarters + numTornadoPieces < 1;
    }

    public override void MeleeEffects(Player player, Rectangle hitbox)
    {
        if (Main.rand.NextBool(3))
            Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.Torch);
    }
}
