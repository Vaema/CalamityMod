using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Enemy
{
    public class HorribleHogShockwaveSpawner : ModProjectile, ILocalizedModType
    {
        public int OwnerIndex;

        public ref float Timer => ref Projectile.ai[0];

        public ref float SpawnInterval => ref Projectile.ai[1];

        public ref float MaxShockwaves => ref Projectile.ai[2];

        public ref float ShockwaveCounter => ref Projectile.localAI[0];

        public ref float Direction => ref Projectile.localAI[1];

        public ref float MaxHeightMultiplier => ref Projectile.localAI[2];

        public new string LocalizationCategory => "Projectiles.Enemy";

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetDefaults()
        {
            Projectile.width = 4;
            Projectile.height = 4;
            Projectile.penetrate = -1;
            Projectile.hide = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(OwnerIndex);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            OwnerIndex = reader.ReadInt32();
        }

        public override bool ShouldUpdatePosition() => false;

        public override bool? CanHitNPC(NPC target) => false;

        public override bool CanHitPlayer(Player target) => false;

        public override void AI()
        {
            if (ShockwaveCounter >= MaxShockwaves)
            {
                Projectile.Kill();
                return;
            }

            Direction = MathHelper.Clamp(Direction, -1, 1);
            if (Direction == 0)
            {
                Direction = Main.rand.NextBool().ToDirectionInt();
                Projectile.netUpdate = true;
            }

            if (Timer % SpawnInterval == 0f)
            {
                Vector2 shockwaveSpawnPosition = new();

                int spawnAttempts = 20;
                int shockwaveSpawnDistance = 32 * (int)ShockwaveCounter * (int)Direction;
                Point bottomInTileCoords = (Projectile.Bottom + Vector2.UnitX * shockwaveSpawnDistance).ToTileCoordinates();
                for (int i = 0; i < spawnAttempts; i++)
                {
                    int posX = bottomInTileCoords.X;
                    int posY = bottomInTileCoords.Y - i;
                    Tile tileAbove = CalamityUtils.ParanoidTileRetrieval(posX, posY - 1);
                    if (WorldGen.ActiveAndWalkableTile(posX, posY) && !tileAbove.HasTile)
                    {
                        shockwaveSpawnPosition = new Point(posX, posY).ToWorldCoordinates();
                        break;
                    }
                }

                for (int i = 0; i < spawnAttempts; i++)
                {
                    int posX = bottomInTileCoords.X;
                    int posY = bottomInTileCoords.Y + i;
                    if (WorldGen.ActiveAndWalkableTile(posX, posY))
                    {
                        shockwaveSpawnPosition = new Point(posX, posY).ToWorldCoordinates();
                        break;
                    }
                }

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    float heightMultiplier = MathHelper.Lerp(1f, MaxHeightMultiplier, ShockwaveCounter / MaxShockwaves);
                    Projectile.NewProjectile(Projectile.GetItemSource_FromThis(), shockwaveSpawnPosition, Vector2.Zero, ModContent.ProjectileType<HorribleHogShockwave>(), Projectile.damage, Projectile.knockBack, ai1: heightMultiplier, ai2: OwnerIndex);
                }

                ShockwaveCounter++;
            }

            Timer++;
        }
    }
}
