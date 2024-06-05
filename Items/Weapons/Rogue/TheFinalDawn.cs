using CalamityMod.Projectiles.Rogue;
using CalamityMod.Rarities;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Items.Weapons.Rogue
{
    [LegacyName("FinalDawn")]
    public class TheFinalDawn : RogueWeapon
    {
        public static readonly SoundStyle UseSound = new("CalamityMod/Sounds/Item/FinalDawnSlash");
        public override void SetDefaults()
        {
            Item.width = 78;
            Item.height = 66;
            Item.damage = 2500;
            Item.DamageType = RogueDamageClass.Instance;
            Item.noMelee = true;
            Item.useTime = Item.useAnimation = 15;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 4;

            Item.value = CalamityGlobalItem.RarityVioletBuyPrice;
            Item.rare = ModContent.RarityType<Violet>();
            Item.Calamity().donorItem = true;

            Item.autoReuse = false;
            Item.shoot = ProjectileType<FinalDawnProjectile>();
            Item.shootSpeed = 1f;
            Item.useTurn = false;
            Item.channel = true;
            Item.noUseGraphic = true;
        }

        public override float StealthDamageMultiplier => 0.6f;
        public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] +
            player.ownedProjectileCounts[ProjectileType<FinalDawnFireSlash>()] +
            player.ownedProjectileCounts[ProjectileType<FinalDawnHorizontalSlash>()] +
            player.ownedProjectileCounts[ProjectileType<FinalDawnThrow>()] +
            player.ownedProjectileCounts[ProjectileType<FinalDawnThrow2>()] <= 0;
    }
}
