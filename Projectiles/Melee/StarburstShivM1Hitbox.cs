using System;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.NPCs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace CalamityMod.Projectiles.Melee
{
    [PierceResistExceptionAttribute(true)]
    public class StarburstShivM1Hitbox : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public Player Owner => Main.player[Projectile.owner];

        public override void SetDefaults()
        {
            Projectile.width = 84;
            Projectile.height = 64;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 6;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 4;
        }

        public override void AI()
        {
            Vector2 toMouse = Utils.DirectionTo(Owner.Center, Owner.ClampedMouseWorld() + MathHelper.Pi.ToRotationVector2());

            float rotation = toMouse.ToRotation();

            Projectile.rotation = rotation;
            Projectile.velocity = toMouse * 34;
        }
    }
}
