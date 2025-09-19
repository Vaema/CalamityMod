using System.Collections.Generic;
using System.Linq;
using CalamityMod.Buffs.Summon;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.NPCs.DevourerofGods;
using CalamityMod.Projectiles.Summon;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Summon
{
    [LegacyName("StaffoftheMechworm")]
    public class VoidEaterMarionette : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Summon";

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.FindAndReplace("ff00ff", Utils.Hex3(DevourerofGodsHead.SpecialMoveColor));
            tooltips.FindAndReplace("ff00ff", Utils.Hex3(DevourerofGodsHead.SpecialMoveColor));
        }
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<DimensionTearingDisk>();
        }
        public override void SetDefaults()
        {
            Item.width = 68;
            Item.height = 68;
            Item.damage = 110;
            Item.mana = 10;
            Item.useAnimation = Item.useTime = 10; // 9 because of useStyle 1
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.knockBack = 2f;
            Item.value = CalamityGlobalItem.RarityDarkBlueBuyPrice;
            Item.rare = ModContent.RarityType<CosmicPurple>();
            Item.UseSound = SoundID.Item113;
            Item.autoReuse = true;
            Item.buffType = ModContent.BuffType<VoidEaterMarionetteBuff>();
            Item.shoot = ModContent.ProjectileType<VoidEaterMarionetteHead>();
            Item.DamageType = DamageClass.Summon;
        }
        public override bool CanUseItem(Player player)
        {
            float neededSlots = 1;
            float foundSlotsCount = 0;
            foreach (Projectile p in Main.ActiveProjectiles)
            {
                if (p.minion && p.owner == player.whoAmI)
                {
                    foundSlotsCount += p.minionSlots;
                    if (foundSlotsCount + neededSlots > player.maxMinions)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static void SummonBaseMechworm(IEntitySource source, int damage, int baseDamage, float knockback, Player owner, out int tailIndex)
        {
            tailIndex = -1;
            if (Main.myPlayer != owner.whoAmI)
                return;

            Vector2 mouse = owner.ClampedMouseWorld();

            int curr = Projectile.NewProjectile(source, mouse, Vector2.Zero, ModContent.ProjectileType<VoidEaterMarionetteHead>(), damage, knockback, owner.whoAmI, 0f, 0f);
            if (Main.projectile.IndexInRange(curr))
                Main.projectile[curr].originalDamage = baseDamage;
            curr = Projectile.NewProjectile(source, mouse, Vector2.Zero, ModContent.ProjectileType<VoidEaterMarionetteBody>(), damage, knockback, owner.whoAmI, Main.projectile[curr].identity, 0f);
            if (Main.projectile.IndexInRange(curr))
                Main.projectile[curr].originalDamage = baseDamage;
            int prev = curr;
            curr = Projectile.NewProjectile(source, mouse, Vector2.Zero, ModContent.ProjectileType<VoidEaterMarionetteBody>(), damage, knockback, owner.whoAmI, Main.projectile[curr].identity, 0f);
            if (Main.projectile.IndexInRange(curr))
                Main.projectile[curr].originalDamage = baseDamage;
            Main.projectile[prev].localAI[1] = curr;
            prev = curr;
            curr = Projectile.NewProjectile(source, mouse, Vector2.Zero, ModContent.ProjectileType<VoidEaterMarionetteTail>(), damage, knockback, owner.whoAmI, Main.projectile[curr].identity, 0f);
            if (Main.projectile.IndexInRange(curr))
                Main.projectile[curr].originalDamage = baseDamage;
            Main.projectile[prev].localAI[1] = curr;

            tailIndex = curr;
        }

        public static void AddSegmentToMechworm(IEntitySource source, int tailIndex, int damage, int baseDamage, float knockback, Player owner)
        {
            if (Main.myPlayer != owner.whoAmI)
                return;

            Vector2 spawnPosition = Main.projectile[tailIndex].Center;
            Projectile tailAheadSegment = Main.projectile.Take(Main.maxProjectiles).FirstOrDefault(proj => VoidEaterMarionetteBody.SameIdentity(proj, owner.whoAmI, (int)Main.projectile[tailIndex].ai[0]));
            int body = Projectile.NewProjectile(source, spawnPosition, Vector2.Zero, ModContent.ProjectileType<VoidEaterMarionetteBody>(), damage, knockback, owner.whoAmI, tailAheadSegment.identity, 0f);
            int body2 = body;
            body = Projectile.NewProjectile(source, spawnPosition, Vector2.Zero, ModContent.ProjectileType<VoidEaterMarionetteBody>(), damage, knockback, owner.whoAmI, Main.projectile[body].identity, 0f);

            var m = Main.projectileIdentity;
            Main.projectile[tailIndex].ai[0] = Main.projectile[body].identity;
            Main.projectile[tailIndex].netUpdate = true;
            if (Main.projectile.IndexInRange(body))
                Main.projectile[body].originalDamage = baseDamage;
            if (Main.projectile.IndexInRange(body2))
                Main.projectile[body2].originalDamage = baseDamage;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            int head = -1;
            int tail = -1;
            foreach (Projectile p in Main.ActiveProjectiles)
            {
                if (p.owner == Main.myPlayer)
                {
                    if (head == -1 && p.type == ModContent.ProjectileType<VoidEaterMarionetteHead>())
                    {
                        head = p.whoAmI;
                    }
                    if (tail == -1 && p.type == ModContent.ProjectileType<VoidEaterMarionetteTail>())
                    {
                        tail = p.whoAmI;
                    }
                    if (head != -1 && tail != -1)
                    {
                        break;
                    }
                }
            }
            if (head == -1 && tail == -1)
                SummonBaseMechworm(source, damage, Item.damage, knockback, player, out _);
            else if (head != -1 && tail != -1)
                AddSegmentToMechworm(source, tail, damage, Item.damage, knockback, player);
            return false;
        }
    }
}
