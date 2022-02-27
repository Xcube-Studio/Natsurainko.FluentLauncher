using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCore.Event
{
    public class InstallerProgressChangedEventArgs
    {
        public string StepName { get; set; }

        public double Progress { get; set; }
    }
}
