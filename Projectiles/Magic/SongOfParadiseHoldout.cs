using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.Items.Weapons.Magic;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using rail;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Magic
{
    public class SongOfParadiseHoldout : BaseCustomUseStyleProjectile, ILocalizedModType
    {
        public override int AssignedItemID => ModContent.ItemType<SongOfParadise>();

        public int Damage => 28;

        Vector2 LookAtPosition = Vector2.Zero;
        public static SoundStyle NoteSound = new SoundStyle("CalamityMod/Sounds/Item/SongOfParadiseNote");
        public static SoundStyle WaveSound = new SoundStyle("CalamityMod/Sounds/Item/SongOfParadiseBlast");

        public static SoundStyle Song = new SoundStyle("CalamityMod/Sounds/Item/SongOfParadise");
        public static SoundStyle EnnwaySong = new SoundStyle("CalamityMod/Sounds/Item/SongOfSpirit");
        public static SoundStyle HPUSong = new SoundStyle("CalamityMod/Sounds/Item/SongOfAscent");
        public ActiveSound ActiveSong = null;
        public override Vector2 SpriteOrigin => new(0, 44);
        public override LocalizedText DisplayName => CalamityUtils.GetItemName<SongOfParadise>();
        public override void SetDefaults()
        {
            base.SetDefaults();

            Song.MaxInstances = 2;

            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.scale = 0f;

            DrawUnconditionally = true;

            LookAtPosition = Main.player[Projectile.owner].Center + new Vector2(0, 150);
        }
        public override void OnKill(int timeLeft)
        {
            if (ActiveSong != null)
            {
                ActiveSong.Stop();
            }
        }
        public override void UseStyle()
        {
            bool retuned = false;

            DrawUnconditionally = true;

            if (ActiveSong != null)
            {
                ActiveSong.Position = Projectile.Center;
            }

            Player player = Main.player[Projectile.owner];

            if (player.HeldItem.type == ModContent.ItemType<SongOfParadise>())
            {
                retuned = (player.HeldItem.ModItem as SongOfParadise).RetunedToMelody;
            }

            Projectile.ai[0]++;

            if (Projectile.ai[0] < 30f) Projectile.scale = MathHelper.Lerp(Projectile.scale, 1f, 0.1f);

            player.Calamity().mouseWorldListener = true;

            Vector2 mW = Main.player[Projectile.owner].Calamity().mouseWorld;

            LookAtPosition = Vector2.Lerp(LookAtPosition, mW, Math.Clamp(Projectile.ai[0] / 90f, 0f, 0.2f));

            if (NumberOfAnimations % 7 == 1 && player.ItemAnimationJustStarted)
            {
                if (retuned)
                {
                    SoundStyle sng = Song;
                    if (player.name == "ENNWAY" || player.name == "ENNWAY!" || player.name == "Casey")
                    {
                        sng = EnnwaySong;
                    }
                    if (player.name == "Heart Plus Up" || player.name == "Heart Plus Up!")
                    {
                        sng = HPUSong;
                    }

                    if (ActiveSong != null)
                        ActiveSong.Stop();
                    ActiveSong = new ActiveSound(sng, Projectile.Center, VV =>
                    {
                        if (Projectile.ai[1] > 1)
                        {
                            VV.Pitch = MathHelper.Lerp(VV.Pitch, 0f, 0.1f);
                        }
                        return new ProjectileAudioTracker(Projectile).IsActiveAndInGame();
                    });
                }
            }

            if (player.ItemAnimationJustStarted && NumberOfAnimations % 7 >= 1)
            {
                Vector2 vel = EndPosition().DirectionTo(mW);
                if (NumberOfAnimations % 7 == 6)
                {
                    for (int i = 0; i < 360; i+= 45)
                        Projectile.NewProjectile(new EntitySource_ItemUse(player, player.HeldItem), player.Center, new Vector2(35, 0).RotatedBy(MathHelper.ToRadians(i)), ModContent.ProjectileType<SongOfParadiseNote>(), Damage, 3f, player.whoAmI, ai2: 2);

                    Projectile.NewProjectile(new EntitySource_ItemUse(player, player.HeldItem), player.Center, player.DirectionFrom(mW) * 15, ModContent.ProjectileType<SongOfParadiseDragon>(), Projectile.damage * 2, 5f, player.whoAmI, ai2: -1);
                    Projectile.NewProjectile(new EntitySource_ItemUse(player, player.HeldItem), player.Center, player.DirectionFrom(mW) * 15, ModContent.ProjectileType<SongOfParadiseDragon>(), Projectile.damage * 2, 5f, player.whoAmI, ai2: 1);

                    if (!retuned)
                    {
                        SoundEngine.PlaySound(WaveSound.WithPitchOffset(Main.rand.NextFloat(-0.2f, 0.2f)), Projectile.Center);
                    }

                    {
                        SoundEngine.PlaySound(SoundID.DD2_BookStaffCast.WithPitchOffset(0.4f).WithVolumeScale(0.7f), Projectile.Center);
                        SoundEngine.PlaySound(SoundID.DD2_DarkMageAttack.WithPitchOffset(-0.6f), Projectile.Center);
                    }

                    GeneralParticleHandler.SpawnParticle(new DirectionalPulseRing(EndPosition(), Vector2.Zero, Color.SkyBlue, new Vector2(0.5f, 1f), EndPosition().AngleTo(mW), 0f, 1f, 30));
                    for (int i = 0; i < 15; i++)
                    {
                        float rand = Main.rand.NextFloat(-50, 50);

                        float intensity = MathHelper.Lerp(1f, 0f, Math.Abs(rand) / 80f);

                        GeneralParticleHandler.SpawnParticle(new GlowOrbParticle(EndPosition(), vel.RotatedBy(MathHelper.ToRadians(rand)) * 6 * intensity, false, (int)(40f * intensity), Main.rand.NextFloat(1f, 1.5f) * intensity, Color.SkyBlue, true));
                    }
                }
                else
                {
                    for (int i = 0; i < 2; i++)
                        Projectile.NewProjectile(new EntitySource_ItemUse(player, player.HeldItem), EndPosition(), EndPosition().DirectionTo(mW) * Main.rand.NextFloat(6, 12), ModContent.ProjectileType<SongOfParadiseNote>(), Damage, 3f, player.whoAmI, ai2: i);

                    if (!retuned)
                    {
                        SoundEngine.PlaySound(NoteSound.WithPitchOffset(Main.rand.NextFloat(-0.2f, 0.2f)), Projectile.Center);
                    }

                    {
                        SoundEngine.PlaySound(SoundID.DD2_BookStaffCast.WithPitchOffset(1f).WithVolumeScale(0.5f), Projectile.Center);
                    }

                    for (int i = 0; i < 15; i++)
                    {
                        float rand = Main.rand.NextFloat(-30, 30);

                        float intensity = MathHelper.Lerp(1f, 0f, Math.Abs(rand) / 30f);

                        GeneralParticleHandler.SpawnParticle(new GlowOrbParticle(EndPosition(), vel.RotatedBy(MathHelper.ToRadians(rand)) * 5 * intensity, false, (int)(40f * intensity), Main.rand.NextFloat(1f, 1.5f) * intensity, Color.SkyBlue, true));
                    }
                }
            }

            ArmRotationOffset = MathHelper.ToRadians(-135f);
            ArmRotationOffsetBack = MathHelper.ToRadians(-135f);

            Offset = new(0, -6);

            Projectile.rotation = Projectile.AngleTo(LookAtPosition) + MathHelper.ToRadians(45f);

            player.direction = Math.Sign(LookAtPosition.X - player.Center.X);

            if (player.direction == -1) FlipAsSword = true;
            else FlipAsSword = false;
        }

        public override void ResetStyle()
        {
            Projectile.ai[0] = 0;

            Player player = Main.player[Projectile.owner];

            DrawUnconditionally = true;

            Projectile.ai[1]++;

            Projectile.scale = MathHelper.Lerp(Projectile.scale, 1f, -0.1f);

            LookAtPosition.Y += Projectile.ai[1] / 3;

            if (Projectile.ai[1] > 30)
            {
                Projectile.Kill();
            }

            Projectile.rotation = Projectile.AngleTo(LookAtPosition) + MathHelper.ToRadians(45f);

            player.direction = Math.Sign(LookAtPosition.X - player.Center.X);
        }

        Vector2 EndPosition()
        {
            return Main.player[Projectile.owner].Center + Offset + new Vector2(48, 0).RotatedBy(Projectile.AngleTo(LookAtPosition));
        }
    }
}
