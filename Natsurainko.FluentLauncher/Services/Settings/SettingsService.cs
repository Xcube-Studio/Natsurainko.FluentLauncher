using AppSettingsManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Services.Settings;

class SettingsService : SettingsContainer
{

    public SettingsService(ISettingsStorage storage) : base(storage)
    {
    }
}
