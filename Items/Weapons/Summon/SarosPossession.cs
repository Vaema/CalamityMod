using System.Linq;
using CalamityMod.Buffs.Summon;
using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Summon;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Summon
{
    public class SarosPossession : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Summon";
        public static SoundStyle FiringSound => new SoundStyle("CalamityMod/Sounds/Item/Summon/SarosFiring") with { MaxInstances = 1 ,Volume = 0.3f, pitchVariance = 0.05f, pitch = 0.5f };
        public static SoundStyle SpawnSound => new SoundStyle("CalamityMod/Sounds/Item/Summon/SarosSpawn");
        public static SoundStyle LoopSound => SoundID.DD2_BetsyFlameBreath with { Volume = 0.2f };
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 48;
            Item.damage = 66;
            Item.mana = 10;
            Item.useAnimation = Item.useTime = 24;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.knockBack = 1.15f;
            Item.UseSound = null;
            Item.buffType = ModContent.BuffType<SarosPossessionBuff>();
            Item.shoot = ModContent.ProjectileType<SarosAura>();
            Item.DamageType = DamageClass.Summon;
            Item.channel = true;

            Item.value = CalamityGlobalItem.RarityVioletBuyPrice;
            Item.rare = ModContent.RarityType<BurnishedAuric>();
        }

        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[ModContent.ProjectileType<SarosEclipseBeam>()] <= 0;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.ownedProjectileCounts[type] > 0)
            {
                var p = Main.projectile.First(x => x.active && x.type == type && x.owner == player.whoAmI);
                p.ai[0]++;
                p.netUpdate = true;

                SoundEngine.PlaySound(SpawnSound with { MaxInstances = 10, SoundLimitBehavior = SoundLimitBehavior.IgnoreNew, pitchVariance = 0.05f, }, player.Center);
                return false;
            } else
            {
                player.channel = false;
            }
            player.AddBuff(Item.buffType, 2);

            SoundEngine.PlaySound(SpawnSound with { MaxInstances = 10, SoundLimitBehavior = SoundLimitBehavior.IgnoreNew, pitchVariance = 0.05f, }, player.Center);
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<Sirius>().
                AddIngredient<AuricBar>(5).
                AddIngredient<DarksunFragment>(15).
                AddTile<CosmicAnvil>().
                Register();
        }
    }
}
