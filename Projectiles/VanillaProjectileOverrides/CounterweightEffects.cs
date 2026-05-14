using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Items.Weapons.Melee;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.VanillaProjectileOverrides
{
    public class CounterweightEffects : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        bool runInitialization = false;
        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            //All counterweights fall within this range.
            if (entity.type >= ProjectileID.BlackCounterweight && entity.type <= ProjectileID.YellowCounterweight)
                return true;
            return false;
        }

        public override bool PreAI(Projectile projectile)
        {
            var player = Main.player[projectile.owner];
            if (!runInitialization)
            {
                if (player.HeldItem.type == ModContent.ItemType<Pandemic>())
                    projectile.MaxUpdates++;
                runInitialization = true;
            }
            return true;
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            var player = Main.player[projectile.owner];
            if (player.HeldItem.type == ModContent.ItemType<Pandemic>())
                target.AddBuff(ModContent.BuffType<Plague>(), 300);
        }
    }
}
