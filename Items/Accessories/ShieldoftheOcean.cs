using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Typeless;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    public class ShieldoftheOcean : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";
        public static readonly SoundStyle TriggerSound = new("CalamityMod/Sounds/Custom/MossMine");
        public static readonly SoundStyle ParrySound = new("CalamityMod/Sounds/Custom/BubbleCracklePop");
        public const int ParryTime = 30;
        // These damage values scale in Expert and Master.
        public const int ShoveFallBaseDamage = 80;
        public const int ImmuneToShoveBaseDamage = 200;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 28;
            Item.value = CalamityGlobalItem.RarityOrangeBuyPrice;
            Item.rare = ItemRarityID.Orange;
            Item.accessory = true;
        }

        public override void ModifyTooltips(List<TooltipLine> list) => list.IntegrateHotkey(CalamityKeybinds.AccessoryParryHotKey);
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.Calamity().shieldOfTheOcean = true;
        }

        public static void HandleParryCountdown(Player player)
        {
            player.Calamity().shieldOfTheOceanParry--;

            if (player.Calamity().shieldOfTheOceanParry > 0)
            {
                player.controlJump = false;
                player.controlDown = false;
                player.controlLeft = false;
                player.controlRight = false;
                player.controlUp = false;
                player.controlUseItem = false;
                player.controlUseTile = false;
                player.controlThrow = false;
                player.gravDir = 1f;
                player.velocity = Vector2.Zero;
                player.velocity.Y = -0.1f; // Ensure Y velocity is not 0, otherwise the flight meter gets reset.
                player.RemoveAllGrapplingHooks();
            }
        }

        // GFB changes:
        // Larger radius for pushing away enemies.
        // Enemies are pushed much faster.
        public static void ActivateParry(Player player)
        {
            bool empowered = player.Calamity().shieldOfTheOceanEmpoweredParry;

            // Search for every NPC within a certain radius of the player.
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (Vector2.Distance(player.Center, npc.Center) > (Main.zenithWorld ? 720f : 240f))
                    continue;

                // Inflict Riptide on empowered parries.
                // Doze - I gave all parry accessories long debuff infliction times due to the lack of weapons that inflict debuffs for a decent time, and the scarcity of using the parry
                // Most common vanilla debuffs have a way to inflict them for 15, 20, or even 30 seconds
                if (empowered)
                    npc.AddBuff(ModContent.BuffType<RiptideDebuff>(), CalamityUtils.SecondsToFrames(15));

                // If the NPC can be moved, violently shove them away. Make them susceptible to fall damage on empowered parries.
                // Otherwise, simply deal a large amount of damage to them if empowered.
                if (npc.CanBeMoved(true))
                {
                    // The NPC has to actually have its velocity changed, because TileCollisionHarmNPC only changes its position. Not confusing at all!
                    Vector2 shoveVelocity = Utils.DirectionTo(player.Center, npc.Center) * (Main.zenithWorld ? 35f : 12.5f) - Vector2.UnitY * 6f;
                    npc.MoveNPC(Vector2.Normalize(shoveVelocity), shoveVelocity.Length(), true);

                    int scaledFallDamage = CalamityUtils.ScaleWithDifficulty(ShoveFallBaseDamage);
                    if (empowered)
                        npc.FlungNPC().ApplyCollisionDamage(npc, player, scaledFallDamage, shoveVelocity * 0.5f, 5f, true);
                    else
                        npc.FlungNPC().ApplyForcedVelocity(npc, player, shoveVelocity * 0.5f, true);

                    for (int i = -1; i <= 1; i++)
                    {
                        Vector2 particleVel = Vector2.Normalize(npc.velocity).RotatedBy(MathHelper.Pi / 6f * i) * 7.5f;
                        WaterFlavoredParticle pushHit = new(npc.Center, particleVel, false, 10, 0.7f, Color.AliceBlue);
                        GeneralParticleHandler.SpawnParticle(pushHit);
                    }
                }
                else
                {
                    if (empowered)
                    {
                        int scaledImmuneDamage = CalamityUtils.ScaleWithDifficulty(ImmuneToShoveBaseDamage) * (Main.zenithWorld ? 10 : 1);
                        Projectile.NewProjectile(player.GetSource_FromThis(), npc.Center, Vector2.Zero, ModContent.ProjectileType<DirectStrike>(), scaledImmuneDamage, 0f, player.whoAmI, npc.whoAmI);
                    }

                    float randOffset = Main.rand.NextFloat(MathHelper.TwoPi);
                    for (int i = 0; i < 6; i++)
                    {
                        Vector2 particleVel = Vector2.UnitX.RotatedBy(MathHelper.Pi / 3f * i + randOffset) * 7.5f;
                        WaterFlavoredParticle bigHit = new(npc.Center, particleVel, false, 10, 0.7f, Color.AliceBlue);
                        GeneralParticleHandler.SpawnParticle(bigHit);
                    }
                }
            }
            player.Calamity().shieldOfTheOceanEmpoweredParry = false;

            for (int b = 0; b < 20; b++)
            {
                Vector2 waterVel = Vector2.UnitX.RotatedBy(MathHelper.TwoPi / 15f * b) * Main.rand.NextFloat(12f, 24f);
                WaterFoamParticle sorryToBurstYourBubble = new(player.Center, waterVel, 15, 0.6f, Color.Blue);
                GeneralParticleHandler.SpawnParticle(sorryToBurstYourBubble);
            }
        }
    }
}
