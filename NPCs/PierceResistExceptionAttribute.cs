using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace CalamityMod.NPCs
{
    /// <summary>
    /// This attribute makes projectiles exempt from pierce resitance
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class PierceResistExceptionAttribute : Attribute
    {
        public PierceResistExceptionAttribute()
        {

        }
    }
}
