using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Projectiles.Summon;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Summon
{
    public class DormantBrimseeker : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Summon";
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<Brimlance>();
        }
        public override void SetDefaults()
        {
            Item.width = Item.height = 32;
            Item.damage = 42;
            Item.mana = 10;
            Item.useAnimation = Item.useTime = 36;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.knockBack = 3f;
            Item.UseSound = SoundID.Item20;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<DormantBrimseekerSummoner>();
            Item.shootSpeed = 10f;
            Item.DamageType = DamageClass.Summon;

            Item.value = CalamityGlobalItem.RarityPinkBuyPrice;
            Item.rare = ItemRarityID.Pink;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            float totalSlots = 0f;
            foreach (Projectile p in Main.ActiveProjectiles)
            {
                if (p.minion && p.owner == player.whoAmI)
                {
                    totalSlots += p.minionSlots;
                }
            }
            if (totalSlots < player.maxMinions)
            {
                int p = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
                if (Main.projectile.IndexInRange(p))
                    Main.projectile[p].originalDamage = Item.damage;
            }
            else if (player.ownedProjectileCounts[ModContent.ProjectileType<DormantBrimseekerAura>()] <= 0f)
            {
                int p = Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<DormantBrimseekerAura>(), damage * 2, knockback, player.whoAmI);
                if (Main.projectile.IndexInRange(p))
                    Main.projectile[p].originalDamage = Item.damage * 2;
            }
            return false;
        }
    }
}
