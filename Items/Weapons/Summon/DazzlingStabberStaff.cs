using CalamityMod.Buffs.Summon;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Projectiles.Summon;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Summon
{
    public class DazzlingStabberStaff : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Summon";
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<MoltenAmputator>();
        }
        public override void SetDefaults()
        {
            Item.width = 56;
            Item.height = 60;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.UseSound = SoundID.DD2_DarkMageHealImpact;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 10;
            Item.damage = 43; 
            Item.knockBack = 2f;
            Item.autoReuse = true;
            Item.useAnimation = Item.useTime = 24;
            Item.buffType = ModContent.BuffType<DazzlingStabberBuff>();
            Item.shoot = ModContent.ProjectileType<DazzlingStabber>();

            Item.value = CalamityGlobalItem.RarityTurquoiseBuyPrice;
            Item.rare = ModContent.RarityType<Turquoise>();
        }
        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Item.DrawItemGlowmaskSingleFrame(spriteBatch, rotation, ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Summon/DazzlingStabberStaffGlow").Value);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);

            for (int i = 0; i < 3; i++)
            {
                var minion = Projectile.NewProjectileDirect(source, player.ClampedMouseWorld(), Vector2.Zero, type, damage, knockback, player.whoAmI, 0, 0, i + 1);
                minion.originalDamage = Item.damage;
            }
            float angleMax = MathHelper.ToRadians(360f);
            if (CalamityUtils.CountOwnedProjectiles(type, player.whoAmI) == 1)
                angleMax = 0f;
            float index = 1f;
            if (player.ownedProjectileCounts[Item.shoot] > 30)
            {
                angleMax += MathHelper.ToRadians((player.ownedProjectileCounts[Item.shoot] - 30) * 2.5f);
            }
            angleMax = angleMax > MathHelper.ToRadians(360f) ? MathHelper.ToRadians(360f) : angleMax; // More intuitive than using a min function
            foreach (Projectile p in Main.ActiveProjectiles)
            {
                if (p.type == type && p.owner == player.whoAmI)
                {
                    p.ai[1] = (index / CalamityUtils.CountOwnedProjectiles(type, player.whoAmI)) * angleMax - angleMax / 2f;
                    p.netUpdate = true;
                    index++;
                }
            }
            return false;
        }
    }
}
