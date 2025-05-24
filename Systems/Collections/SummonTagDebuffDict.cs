using System.Collections.Generic;
using CalamityMod.Buffs.Summon.Whips;
using CalamityMod.DataStructures;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Weapons.Summon;
using CalamityMod.Items.Weapons.Summon.Whips;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Systems.Collections
{
    /// <summary>
    /// A <see cref="ModSystem"/> that contains a <see cref="IDictionary{T}"/> of all summon tag debuff's IDs and their associated SummonTag.
    /// </summary>
    public sealed class SummonTagDebuffDict : ModSystem
    {
        public static IDictionary<int, SummonTag> Dict { get; private set; }

        public override void OnModLoad()
        {
            Dict = new Dictionary<int, SummonTag>
            {
                { BuffID.BlandWhipEnemyDebuff, SummonTag.LeatherWhip },
                { BuffID.BoneWhipNPCDebuff, SummonTag.SpinalTap },
                { BuffID.CoolWhipNPCDebuff, SummonTag.CoolWhip },
                { BuffID.FlameWhipEnemyDebuff, SummonTag.Firecracker },
                { BuffID.MaceWhipNPCDebuff, SummonTag.MorningStar },
                { BuffID.RainbowWhipNPCDebuff, SummonTag.Kaleidoscope },
                { BuffID.ScytheWhipEnemyDebuff, SummonTag.DarkHarvest },
                { BuffID.SwordWhipNPCDebuff, SummonTag.Durendal },
                { BuffID.ThornWhipNPCDebuff, SummonTag.Snapthorn },
                { BuffType<ArdorBlossomSpark>(), ArdorBlossomStar.SummonTag },
                { BuffType<CnidarianWhipDebuff>(), Cnidarian.SummonTag },
                { BuffType<ProfanedCrystalWhipDebuff>(), ProfanedSoulCrystal.SummonTag },
                { BuffType<RottenMawWhipDebuff>(), RottenMaw.SummonTag },
                { BuffType<UnderBiteWhipDebuff>(), UnderBite.SummonTag },
                { BuffType<AtlasMunitionsTagDebuff>(), AtlasMunitionsBeacon.SummonTag },
                { BuffType<AberrantHorrorWhipDebuff>(), AberrantHorror.SummonTag }
            };

        }

        public override void Unload()
        {
            Dict?.Clear();
            Dict = null;
        }

        /// <summary>
        /// A shorthand method to obtain a SummonTag from its debuff.
        /// </summary>
        public static bool TryGet(int buffType, out SummonTag tag)
        {
            return Dict.TryGetValue(buffType, out tag);
        }

        public static bool Add(int buffType, SummonTag tag)
        {
            return Dict.TryAdd(buffType, tag);
        }
    }
}
