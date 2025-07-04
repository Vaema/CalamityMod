using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalamityMod.UI.WhisperingPearls
{
    public abstract class DialogueEvent
    {
        public string ID { get; set; }
        public string[] Args { get; set; }

        internal bool EventOver = true;
        public bool IsOver => EventOver;

        internal int EventCounter = 0;

        public virtual void UpdateEvent()
        {
            EventCounter++;
        }
    }
}
