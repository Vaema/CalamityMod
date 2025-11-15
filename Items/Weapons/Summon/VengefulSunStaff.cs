using System.Linq;
using CalamityMod.Buffs.Summon;
using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Summon;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Summon
{
    [LegacyName("SunGodStaff")]
    public class VengefulSunStaff : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Summon";
        public override void SetDefaults()
        {
            Item.width = 72;
            Item.height = 72;
            Item.damage = 30;
            Item.mana = 10;
            Item.useAnimation = Item.useTime = 36;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.knockBack = 1.25f;
            Item.value = CalamityGlobalItem.RarityLightPurpleBuyPrice;
            Item.rare = ItemRarityID.LightPurple;
            Item.UseSound = SoundID.Item44;
            Item.buffType = ModContent.BuffType<SolarGodSpiritBuff>();
            Item.shoot = ModContent.ProjectileType<VengefulSunSpiritMinion>();
            Item.DamageType = DamageClass.Summon;
        }

        public override bool CanUseItem(Player player)
        {
            float minionSlotsAvailable = player.maxMinions;
            foreach (var item in Main.ActiveProjectiles)
            {
                if (item.owner == player.whoAmI)
                    minionSlotsAvailable -= item.minionSlots;
            }
            return minionSlotsAvailable >= 1;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.ownedProjectileCounts[type] > 0)
            {
                var p = Main.projectile.First(x => x.active && x.type == type && x.owner == player.whoAmI);
                p.ai[0]++;
                p.netUpdate = true;
                return false;
            }
            player.AddBuff(Item.buffType, 2);
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<SunSpiritStaff>().
                AddIngredient(ItemID.MoonStone).
                AddIngredient(ItemID.SoulofFright, 3).
                AddIngredient(ItemID.SoulofMight, 3).
                AddIngredient(ItemID.SoulofSight, 3).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
