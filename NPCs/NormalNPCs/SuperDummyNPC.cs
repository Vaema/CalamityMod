using System.IO;
using CalamityMod.NPCs.VanillaNPCAIOverrides.RegularEnemies;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.NormalNPCs;

public class SuperDummyNPC : ModNPC
{
    public int deathCounter = 0;
    public RevengeanceAndDeathAI.MimicAI ZenithSeedMimicAI;

    public override void SetStaticDefaults()
    {
        this.HideFromBestiary();
        Main.npcFrameCount[Type] = 11;
        NPCID.Sets.CantTakeLunchMoney[Type] = true;
    }

    public override void SetDefaults()
    {
        NPC.width = 18;
        NPC.height = 48;
        NPC.damage = 0;
        NPC.defense = 0;
        NPC.lifeMax = 9999999;
        NPC.HitSound = null;
        NPC.DeathSound = SoundID.NPCDeath2;
        NPC.knockBackResist = 0f;
        NPC.netAlways = true;
        NPC.aiStyle = NPCAIStyleID.FaceClosestPlayer;

        ZenithSeedMimicAI = new RevengeanceAndDeathAI.MimicAI();
        ZenithSeedMimicAI.NPC = NPC;
    }

    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write(deathCounter);
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        deathCounter = reader.ReadInt32();
    }

    public override bool PreAI()
    {
        if (Main.zenithWorld)
        {
            deathCounter++;
            // If you don't attack the Dummy for a minute in gfb, it becomes sentient
            if (deathCounter >= 6000)
            {
                NPC.damage = NPC.lifeMax;
                ZenithSeedMimicAI.AI(Mod);
                return false;
            }
        }
        return true;
    }

    public override void UpdateLifeRegen(ref int damage)
    {
        if (NPC.lifeRegen >= 0)
            NPC.lifeRegen += 2000000;
    }

    public override bool CanHitPlayer(Player target, ref int cooldownSlot) => Main.zenithWorld;

    public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position) => false;

    public override void HitEffect(NPC.HitInfo hit)
    {
        // Dummy AI, no way
        NPC.localAI[0] = (int)hit.Damage;
        if (NPC.localAI[0] < 20f)
        {
            NPC.localAI[0] = 20f;
        }
        if (NPC.localAI[0] > 120f)
        {
            NPC.localAI[0] = 120f;
        }
        NPC.localAI[1] = hit.HitDirection;
        // Reset hit timer if it isn't enraged
        if (deathCounter > 0 && deathCounter < 6000)
        {
            deathCounter = 0;
        }
        SoundStyle toPlay = Main.rand.Next(3) switch
        {
            0 => SoundID.NPCHit15,
            1 => SoundID.NPCHit16,
            2 => SoundID.NPCHit17,
            _ => SoundID.NPCHit15
        };
        if (NPC.soundDelay <= 0)
        {
            SoundEngine.PlaySound(toPlay, NPC.Center);
        }
    }

    public override void FindFrame(int frameHeight)
    {
        int hitDirection = (int)NPC.localAI[1];
        if (NPC.direction == 1)
        {
            hitDirection *= -1;
        }
        if (NPC.localAI[0] > 24f)
        {
            NPC.localAI[0] = 24f;
        }
        if (NPC.localAI[0] > 0f)
        {
            NPC.localAI[0] -= 1f;
        }
        if (NPC.localAI[0] < 0f)
        {
            NPC.localAI[0] = 0f;
        }
        int animationSpeed = ((hitDirection == -1) ? 4 : 6);
        int currentFrame = (int)NPC.localAI[0] / animationSpeed;
        if (NPC.localAI[0] % (float)animationSpeed != 0f)
        {
            currentFrame++;
        }
        if (currentFrame != 0 && hitDirection == 1)
        {
            currentFrame += 5;
        }
        NPC.frame.Y = currentFrame * frameHeight;
    }

    public override bool CheckDead()
    {
        if (NPC.lifeRegen < 0)
        {
            NPC.life = NPC.lifeMax;
            return false;
        }
        return true;
    }
}
