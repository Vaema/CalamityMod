using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace CalamityMod.NPCs
{
    /// <summary>
    /// This attribute makes projectiles exempt from pierce resistance
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class PierceResistExceptionAttribute : Attribute
    {
        /// <summary>
        /// <para>Set this to true to indicate this projectile is only exempt from piercing resistance when attacking NPCs with a single large hitbox.</para>
        /// </summary>
        public bool OnlyForSingleHitbox { get; }

        public PierceResistExceptionAttribute(bool onlyForSingleHitbox = false)
        {
            OnlyForSingleHitbox = onlyForSingleHitbox;
        }
    }
}
