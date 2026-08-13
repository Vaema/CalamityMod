using System.Collections.Generic;
using CalamityMod.Systems.Collections;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Systems;

public class BestiaryRegistrySystem : ModSystem
{
    public override void PostSetupContent()
    {
        On_BestiaryDatabaseNPCsPopulator.AddEmptyEntries_CrittersAndEnemies_Automated += ForciblyAddEmptyEntriesForCritters;
        On_NPCWasNearPlayerTracker.ScanWorldForFinds += ForciblySetWasSeenByPlayer;

        // Manually register variants post-initiailization
        foreach (var pair in CalamityNPCSets.CountVariantsAsTheSameInBestiary)
            ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[pair.Key] = ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[pair.Value];
    }

    private void ForciblyAddEmptyEntriesForCritters(On_BestiaryDatabaseNPCsPopulator.orig_AddEmptyEntries_CrittersAndEnemies_Automated orig, BestiaryDatabaseNPCsPopulator self)
    {
        orig(self);

        // Run through all entries again and remove the empty Enemy entries that are added by tMod itself.
        // Afterwards, mnually add empty Critter entries for all NPCs within the ID set.
        HashSet<int> exclusions = BestiaryDatabaseNPCsPopulator.GetExclusions();
        foreach (KeyValuePair<int, NPC> pair in ContentSamples.NpcsByNetId)
        {
            if (!exclusions.Contains(pair.Key))
            {
                if (CalamityNPCSets.ForciblyRegisterAsCritterInBestiary.Contains(pair.Value.type))
                {
                    if (BestiaryDatabaseNPCsPopulator._currentDatabase._byNpcId.TryGetValue(pair.Value.netID, out BestiaryEntry enemyEntry) && enemyEntry.UIInfoProvider is not CritterUICollectionInfoProvider)
                    {
                        BestiaryDatabaseNPCsPopulator._currentDatabase.Entries.Remove(enemyEntry);
                        NPCLoader.SetBestiary(pair.Value, BestiaryDatabaseNPCsPopulator._currentDatabase, self.Register(BestiaryEntry.Critter(pair.Key)));
                    }
                }
            }
        }
    }

    private void ForciblySetWasSeenByPlayer(On_NPCWasNearPlayerTracker.orig_ScanWorldForFinds orig, NPCWasNearPlayerTracker self)
    {
        orig(self);

        // Allow NPCs with manully added empty critter entries to be registered by player proximity.
        foreach (NPC npc in Main.ActiveNPCs)
        {
            if (!CalamityNPCSets.ForciblyRegisterAsCritterInBestiary.Contains(npc.type) || self._wasSeenNearPlayerByNetId.Contains(npc.netID))
                continue;

            for (int i = 0; i < self._playerHitboxesForBestiary.Count; i++)
            {
                if (npc.Hitbox.Intersects(self._playerHitboxesForBestiary[i]))
                {
                    self._wasSeenNearPlayerByNetId.Add(npc.netID);
                    self.RegisterWasNearby(npc);
                }
            }
        }
    }
}
