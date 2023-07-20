using Nrk.FluentCore.Interfaces.ServiceInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nrk.FluentCore.Services.Download;

/// <summary>
/// 下载服务的默认实现（IoC适应）
/// </summary>
public class DefaultDownloadService
{
    protected readonly IFluentCoreSettingsService _settingsService;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="settingsService">实际使用时请使用具体的继承类型替代之</param>
    public DefaultDownloadService(IFluentCoreSettingsService settingsService)
    {
        _settingsService = settingsService;
    }
}
