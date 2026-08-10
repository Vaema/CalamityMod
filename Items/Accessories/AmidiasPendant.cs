using CalamityMod.Projectiles.Typeless;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories;

public class AmidiasPendant : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Accessories";
    public const int ShardProjectiles = 2;
    public const float ShardAngleSpread = 90;
    public int ShardCountdown = 0;

    public override void SetDefaults()
    {
        Item.width = 26;
        Item.height = 46;
        Item.value = CalamityGlobalItem.RarityGreenBuyPrice;
        Item.rare = ItemRarityID.Green;
        Item.accessory = true;
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        if (ShardCountdown <= 0)
        {
            ShardCountdown = 140;
        }
        if (ShardCountdown > 0)
        {
            ShardCountdown -= Main.rand.Next(1, 4);
            if (ShardCountdown <= 0)
            {
                if (player.whoAmI == Main.myPlayer)
                {
                    var source = player.GetSource_Accessory(Item);
                    int speed2 = 25;
                    float spawnX = Main.rand.Next(-300, 301) + player.Center.X;
                    float spawnY = -1000 + player.Center.Y;
                    Vector2 baseSpawn = new Vector2(spawnX, spawnY);
                    Vector2 baseVelocity = player.Center - baseSpawn;
                    baseVelocity.Normalize();
                    baseVelocity *= speed2;
                    int spawnOffset = ShardProjectiles * 15;
                    float spread = -ShardAngleSpread / 2f;
                    for (int i = 0; i < ShardProjectiles; i++)
                    {
                        Vector2 spawn = baseSpawn;
                        spawn.X = spawn.X + i * 30 - spawnOffset;
                        Vector2 velocity = baseVelocity.RotatedBy(MathHelper.ToRadians(spread + (ShardAngleSpread * i / (float)ShardProjectiles)));
                        velocity.X = velocity.X + 3 * Main.rand.NextFloat() - 1.5f;

                        int finalDamage = (int)player.GetBestClassDamage().ApplyTo(30);
                        Projectile.NewProjectile(source, spawn.X, spawn.Y, velocity.X / 3, velocity.Y / 2, ModContent.ProjectileType<PearlAuraShard>(), finalDamage, 5f, Main.myPlayer);
                    }
                }
            }
        }
    }
}
