using CalamityMod.Items.Materials;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories.Wings;

[AutoloadEquip(EquipType.Wings)]
[LegacyName("AureateWings")]
public class AureateBooster : BaseWings
{
    public override float BonusAscentWhileFalling => 0.75f;
    public override float BonusAscentWhileRising => 0.15f;
    public override float RisingSpeedThreshold => 1f;
    public override float MaxAscentSpeed => 2.5f;
    public override float BaseAscent => 0.125f;

    // How powerful the acceleration increase is while pressing UP
    // This also affects the flight time tick rate
    public static float BoostPower = 1.5f;

    public override void SetStaticDefaults() => ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new WingStats(120, 8f, 1.5f);

    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.width = 54;
        Item.height = 26;
        Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
        Item.rare = ItemRarityID.Lime;
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.DisableWingFlapSound();

        if (player.controlJump && player.wingTime > 0f && player.jump == 0 && player.velocity.Y != 0f && !hideVisual)
        {
            player.rocketDelay2--;
            if (player.rocketDelay2 <= 0)
            {
                SoundEngine.PlaySound(SoundID.Item13, player.Center);
                player.rocketDelay2 = 60;
            }
            int dustAmt = 2;
            if (player.controlUp)
            {
                dustAmt = 4;
            }
            for (int i = 0; i < dustAmt; i++)
            {
                int type = 6;
                float scale = 1.75f;
                int alpha = 100;
                float x = player.position.X + (float)(player.width / 2) + 16f;
                if (player.direction > 0)
                {
                    x = player.position.X + (float)(player.width / 2) - 26f;
                }
                float dustYPos = player.position.Y + (float)player.height - 18f;
                if (i == 1 || i == 3)
                {
                    x = player.position.X + (float)(player.width / 2) + 8f;
                    if (player.direction > 0)
                    {
                        x = player.position.X + (float)(player.width / 2) - 20f;
                    }
                    dustYPos += 6f;
                }
                if (i > 1)
                {
                    dustYPos += player.velocity.Y;
                }
                int boosterDust = Dust.NewDust(new Vector2(x, dustYPos), 8, 8, type, 0f, 0f, alpha, default, scale);
                Dust dust = Main.dust[boosterDust];
                dust.velocity.X *= 0.1f;
                dust.velocity.Y = Main.dust[boosterDust].velocity.Y * 1f + 2f * player.gravDir - player.velocity.Y * 0.3f;
                dust.noGravity = true;
                dust.shader = GameShaders.Armor.GetSecondaryShader(player.cWings, player);
                if (dustAmt == 4)
                {
                    dust.velocity.Y += 6f;
                }
            }
        }
    }

    public override void AdditionalFlightMovement(Player player, ref float ascentWhenFalling, ref float ascentWhenRising, ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
    {
        if (player.TryingToHoverUp)
        {
            ascentWhenFalling *= BoostPower;
            ascentWhenRising *= BoostPower;
            maxCanAscendMultiplier *= BoostPower;
            maxAscentMultiplier *= BoostPower;
            constantAscend *= BoostPower;
            player.wingTime -= BoostPower - 1f;
        }
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient(ItemID.SoulofFlight, 20).
            AddIngredient<PerennialBar>(10).
            AddTile(TileID.MythrilAnvil).
            Register();
    }
}
