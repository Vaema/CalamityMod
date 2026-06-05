using System;
using CalamityMod.Items.SummonItems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    public class VictideBobber : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public override string Texture => "CalamityMod/Projectiles/Summon/CnidarianJellyfishOnTheString";

        public static int ReelChancePerFrame = 100;

        public Player Owner => Main.player[Projectile.owner];
        public ref float ParentProjectile => ref Projectile.ai[2];
        public Projectile Parent => Main.projectile[(int)ParentProjectile];

        public int stuckTimer;

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 20;
            Projectile.aiStyle = ProjAIStyleID.Bobber;
            Projectile.bobber = true;
        }

        public override bool PreAI()
        {
            // Snap if the snail is shelled, or just nowhere to be found
            if (!Parent.active || Owner.HeldItem.fishingPole <= 0 || Parent == null || Parent.frame < 6)
            {
                Projectile.Kill();
                return false;
            }
            return true;
        }

        public override void AI()
        {
            // Automatic reeling
            if (Projectile.ai[0] == 0f && Projectile.ai[1] < 0f && Main.rand.NextBool(ReelChancePerFrame))
            {
                Projectile.ai[0] = 1f;
                Projectile.localAI[0] = 1f;

                // Consume bait as needed
                int baitType = Owner.GetFishingConditions().BaitItemType;
                int baitSlot = GetBaitSlot(Owner, baitType);
                if (baitSlot != -1)
                {
                    Item currentBait = Owner.inventory[baitSlot];
                    bool consume = false;

                    if (baitType == ItemID.TruffleWorm)
                        consume = true;
                    else if (baitType == ItemID.GoldWorm)
                        consume = Main.rand.NextBool(20);
                    else
                    {
                        float chanceMult = MathF.Max(1f, 1f + currentBait.bait / 6f);
                        if (Owner.accTackleBox)
                            chanceMult += 1f;
                        if (Main.rand.NextFloat() * chanceMult < 1f)
                            consume = true;

                        // Junk (or Quest fish) default to no bait consumption
                        if (Projectile.localAI[1] > 0f)
                        {
                            Item dummyCatch = new Item();
                            dummyCatch.SetDefaults((int)Projectile.localAI[1]);
                            if (dummyCatch.rare < ItemRarityID.White)
                                consume = false;
                        }
                    }
                    if (CombinedHooks.CanConsumeBait(Owner, currentBait).GetValueOrDefault(consume))
                    {
                        if (currentBait.type == ItemID.LadyBug || currentBait.type == ItemID.GoldLadyBug)
                            NPC.LadyBugKilled(Owner.Center, currentBait.type == ItemID.GoldLadyBug);

                        currentBait.stack--;
                        if (currentBait.stack <= 0)
                            currentBait.SetDefaults();
                    }
                }

                // Summon Duke Fishron as this code otherwise only runs on player rod usage
                if (baitType == ItemID.TruffleWorm)
                {
                    Projectile.ai[0] = 2f;
                    CalamityUtils.SpawnBossUsingItem(Owner, NPCID.DukeFishron);
                }
                // Line snapping behaviour
                else if (!Owner.accFishingLine && Main.rand.NextBool(7))
                    Projectile.ai[0] = 2f;
                // Return back items
                else if (Projectile.localAI[1] > 0f)
                {
                    Projectile.ai[1] = Projectile.localAI[1];
                    Projectile.localAI[1] = 0f;
                }
                // Spawn enemies
                else if (Projectile.localAI[1] < 0f)
                {
                    int spawnType = (int)(-Projectile.localAI[1]);
                    Point spawnPos = new Point((int)Projectile.position.X, (int)Projectile.position.Y);
                    if (spawnType == NPCID.BloodNautilus)
                        spawnPos.Y += 64;

                    if (Main.netMode == NetmodeID.MultiplayerClient)
                        NetMessage.SendData(MessageID.FishOutNPC, -1, -1, null, spawnPos.X / 16, spawnPos.Y / 16, spawnType);
                    else
                    {
                        Projectile.ai[0] = 2f;
                        NPC.NewNPC(new EntitySource_FishedOut(Owner), spawnPos.X, spawnPos.Y, spawnType);

                        if (spawnType == NPCID.TownSlimeRed)
                            NPC.unlockedSlimeRedSpawn = true;
                        WorldGen.CheckAchievement_RealEstateAndTownSlimes();
                    }
                }
            }

            // Anti-stuck auto reelback
            if (Projectile.ai[0] == 0f && Projectile.localAI[1] == 0f && Projectile.velocity.Length() <= 0.2f)
            {
                stuckTimer++;
                if (stuckTimer > 60)
                    Projectile.ai[0] = 1f;
            }
        }

        public static int GetBaitSlot(Player owner, int baitType)
        {
            // Ignore Bloodworm as the Old Duke summon method already manually consumes it
            if (baitType == ModContent.ItemType<BloodwormItem>())
                return -1;

            // Check ammo slots first
            for (int i = 54; i < Main.InventorySlotsTotal; i++)
            {
                if (owner.inventory[i].type == baitType)
                    return i;
            }
            for (int j = 0; j < 54; j++)
            {
                if (owner.inventory[j].type == baitType)
                    return j;
            }
            return -1;
        }

        // Anchor the bobber to the parent projectile, since the default is bound to the player
        // Yes. This method is obsolete. No, there is no other way and I won't make another IL edit to remedy this fact. - Iris
        public override void ModifyFishingLine(ref Vector2 lineOriginOffset, ref Color lineColor)
        {
            lineColor = Color.Cyan;
            if (Parent.active)
            {
                Vector2 originalPos = Owner.MountedCenter + Vector2.UnitY * (Owner.gfxOffY - (Owner.gravDir == -1 ? 12f : 0f));
                lineOriginOffset = Parent.Center - originalPos + Vector2.UnitY * 13f;

                // How this is a thing is beyond me
                lineOriginOffset.X -= 2f;
                if (Owner.direction < 0)
                    lineOriginOffset.X += (Owner.MountedCenter.X - Parent.Center.X) * 2f;
            }
        }

        public override bool PreDrawExtras()
        {
            Lighting.AddLight(Projectile.Center, 0f, 0.2f, 0.2f);
            return true;
        }
    }
}
