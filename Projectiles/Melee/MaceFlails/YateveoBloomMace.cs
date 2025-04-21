using CalamityMod.Items.Weapons.Melee;
using CalamityMod.NPCs;
using CalamityMod.Projectiles.BaseProjectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee.MaceFlails
{
    [PierceResistException]
    public class YateveoBloomMace : BaseMaceFlailProjectile
    {
        public override int AssociatedItemID => ModContent.ItemType<YateveoBloom>();

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 30;
            base.SetDefaults();
        }

        public override bool ExtraBehavior()
        {
            if (Main.rand.NextBool(3))
            {
                int dustType = Main.rand.Next(5);
                switch (dustType)
                {
                    case 0:
                        dustType = 2;
                        break;
                    case 1:
                        dustType = 44;
                        break;
                    default:
                        dustType = 136;
                        break;
                }
                Dust spore = Dust.NewDustDirect(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, dustType, 0f, 0f);
                spore.noGravity = true;
                spore.scale = 1.5f;
            }
            return true;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(BuffID.Poisoned, 180);
    }
}
