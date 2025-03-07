using System;
using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.Linq;
using System.Reflection;
using CalamityMod.NPCs;
using CalamityMod.NPCs.ExoMechs.Thanatos;
using CalamityMod.NPCs.Leviathan;
using CalamityMod.Projectiles;
using CalamityMod.Systems.Collections;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.NPCs
{
    public sealed class PierceResistNPC : GlobalNPC
    {
        private static HashSet<int> exemptProjectiles;

        public override void Load()
        {
            exemptProjectiles = new();
        }

        public override void Unload()
        {
            exemptProjectiles?.Clear();
            exemptProjectiles = null;
        }

        public override void SetStaticDefaults()
        {
            var types = AssemblyManager.GetLoadableTypes(CalamityMod.Instance.Code)
                .Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(ModProjectile)));

            foreach (var type in types)
            {
                try
                {
                    var pierceResistException = type.GetCustomAttribute<PierceResistExceptionAttribute>();
                    if (pierceResistException == null)
                        continue;

                    var projectileTypeMethod = typeof(ModContent).GetMethod(nameof(ModContent.ProjectileType));
                    var projectileTypeActualMethod = projectileTypeMethod.MakeGenericMethod(type);
                    int projectileType = (int)projectileTypeActualMethod.Invoke(null, null);

                    MarkProjectileAsExempt(projectileType);
                }
                catch (Exception e)
                {
                    CalamityMod.Instance.Logger.Error($"Exception thrown while evaluating type \"{type.Name}\": {e}");
                }
            }
        }

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (PierceResistList.Includes(npc.type) && !exemptProjectiles.Contains(projectile.type))
            PierceResistGlobal(projectile, npc, ref modifiers);
        }

        // Generalized pierce resistance that stacks with all other resistances for some specific bosses defined in a list.
        private void PierceResistGlobal(Projectile projectile, NPC npc, ref NPC.HitModifiers modifiers)
        {
            // Thanatos segments do not trigger pierce resistance if they are closed
            if (ThanatosIDList.Includes(npc.type) && npc.GetGlobalNPC<CalamityGlobalNPC>().unbreakableDR)
                return;

            // Isolates projectiles which ignore pierce resist only on Leviathan and Astrum Aureus
            if ((npc.type == NPCType<Leviathan.Leviathan>() || npc.type == NPCType<AstrumAureus.AstrumAureus>()) && PierceResistExceptionLeviAureusList.Includes(projectile.type))
                return;

            float damageReduction = projectile.Calamity().timesPierced * CalamityGlobalProjectile.PierceResistHarshness;
            if (damageReduction > CalamityGlobalProjectile.PierceResistCap)
                damageReduction = CalamityGlobalProjectile.PierceResistCap;

            modifiers.FinalDamage *= 1f - damageReduction;

            if ((projectile.penetrate > 1 || projectile.penetrate == -1) && !projectile.CountsAsClass<SummonDamageClass>() && projectile.aiStyle != ProjAIStyleID.Flail && projectile.aiStyle != ProjAIStyleID.MechanicalPiranha && projectile.aiStyle != ProjAIStyleID.Yoyo)
                projectile.Calamity().timesPierced++;
        }

        private static void MarkProjectileAsExempt<ProjectileType>() where ProjectileType : ModProjectile
        {
            MarkProjectileAsExempt(ModContent.ProjectileType<ProjectileType>());
        }

        private static void MarkProjectileAsExempt(int projectileType)
        {
            exemptProjectiles.Add(projectileType);
        }
    }
}
