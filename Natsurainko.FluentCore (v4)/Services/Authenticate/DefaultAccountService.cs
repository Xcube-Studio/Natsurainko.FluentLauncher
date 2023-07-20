using Nrk.FluentCore.Classes.Datas.Authenticate;
using Nrk.FluentCore.Classes.Datas.Launch;
using Nrk.FluentCore.Interfaces.ServiceInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nrk.FluentCore.Services.Authenticate;

/// <summary>
/// 账户系统的默认实现（IoC适应）
/// </summary>
public class DefaultAccountService
{
    public Account ActiveAccount { get; protected set; }

    protected readonly IFluentCoreSettingsService _settingsService;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="settingsService">实际使用时请使用具体的继承类型替代之</param>
    public DefaultAccountService(IFluentCoreSettingsService settingsService)
    {
        _settingsService = settingsService;
    }
}
