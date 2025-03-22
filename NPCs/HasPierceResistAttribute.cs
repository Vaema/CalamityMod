using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace CalamityMod.NPCs
{
    /// <summary>
    /// This attribute gives an NPC universal pierce resistance
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class HasPierceResistAttribute : Attribute
    {
        /// <summary>
        /// <para>If present, the NPC has a single large hitbox relevant for certeain exemptions</para>
        /// </summary>
        public bool SingleHitbox { get; }

        public HasPierceResistAttribute(bool singleHitbox = false)
        {
            SingleHitbox = singleHitbox;
        }
    }
}
