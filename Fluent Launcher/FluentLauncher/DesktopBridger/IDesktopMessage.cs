using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentLauncher.DesktopBridger
{
    public interface IDesktopMessage
    {
        public string Header { get; set; }

        public string Message { get; set; }
    }
}
