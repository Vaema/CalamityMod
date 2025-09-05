using CalamityMod.Cooldowns;
using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.BaseProjectiles;
using CalamityMod.Projectiles.Melee;
using CalamityMod.Projectiles.Melee.Spears;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Melee
{
    public class Lucrecia : BaseSwordHoldoutItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Melee";

        public static int MaxEnergy = 100;

        public override int ProjectileType => ModContent.ProjectileType<LucreciaProj>();

        public override void SetDefaults()
        {
            Item.width = 54;
            Item.height = 54;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.damage = 57;
            Item.DamageType = DamageClass.Melee;
            Item.useAnimation = Item.useTime = 34;
            Item.shootSpeed = 10f;
            Item.knockBack = 8.25f;
            Item.value = CalamityGlobalItem.RarityYellowBuyPrice;
            Item.rare = ItemRarityID.Yellow;

            Item.autoReuse = true;
            Item.channel = true;
            // Item.noMelee = true;
            // Item.noUseGraphic = true;
            base.SetDefaults();

        }

        public override bool AltFunctionUse(Player player)
        {
            return true;
        }


        public override void HoldItem(Player player)
        {
            if (player.Calamity().cooldowns.TryGetValue(LucreciaEnergy.ID, out var cooldown))
            {
                cooldown.timeLeft = player.Calamity().lucreciaEnergy;
            }
            else
            {
                player.AddCooldown(LucreciaEnergy.ID, 0);
            }
        }

        // public override bool MeleePrefix() => true;

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<LifeAlloy>(5).
                AddIngredient(ItemID.FallenStar, 10).
                AddIngredient(ItemID.SoulofLight, 5).
                AddIngredient(ItemID.SoulofNight, 5).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
