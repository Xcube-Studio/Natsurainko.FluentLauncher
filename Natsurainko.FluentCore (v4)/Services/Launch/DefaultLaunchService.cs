using Nrk.FluentCore.Classes.Datas.Launch;
using Nrk.FluentCore.DefaultComponents.Launch;
using Nrk.FluentCore.DefaultComponents.Parse;
using Nrk.FluentCore.Interfaces.ServiceInterfaces;
using Nrk.FluentCore.Services.Authenticate;
using Nrk.FluentCore.Utils;
using System.IO;
using System.Linq;

namespace Nrk.FluentCore.Services.Launch;

/// <summary>
/// 启动服务的默认实现（IoC适应）
/// </summary>
public class DefaultLaunchService
{
    protected readonly IFluentCoreSettingsService _settingsService;
    protected readonly DefaultGameService _gameService;
    protected readonly DefaultAccountService _accountService;

    public DefaultLaunchService(
        IFluentCoreSettingsService settingsService,
        DefaultGameService gameService,
        DefaultAccountService accountService)
    {
        _settingsService = settingsService;
        _gameService = gameService;
        _accountService = accountService;
    }

    public virtual DefaultLaunchProcess CreateLaunchProcess(GameInfo gameInfo)
    {
        var libraryParser = new DefaultLibraryParser(gameInfo);
        libraryParser.EnumerateLibraries(out var enabledLibraries, out var enabledNativesLibraries);

        return new DefaultLaunchProcessBuilder(gameInfo)
            .SetInspectAction(() =>
            {
                if (_settingsService.ActiveJava == null) return false; // 检查是否有选中的 Java
                if (_accountService.ActiveAccount == null) return false; // 检查是否有选中的账户 

                return true;
            })
            // TODO: 验证账户、刷新账户令牌
            //.SetAuthenticateFunc(() => 
            //{
            //
            //})
            .SetCompleteResourcesAction(() =>
            {
                // 解压 Natives
                UnzipUtils.BatchUnzip(
                    Path.Combine(gameInfo.MinecraftFolderPath, "versions", gameInfo.AbsoluteId, "natives"),
                    enabledNativesLibraries.Select(x => x.AbsolutePath));

                // TODO: 补全 Libraries 和 Assets
            })
            .SetBuildArgumentsFunc(() =>
            {
                var builder = new DefaultArgumentsBuilder(gameInfo)
                    .SetLibraries(enabledLibraries)
                    .SetAccountSettings(_accountService.ActiveAccount, _settingsService.EnableDemoUser)
                    .SetJavaSettings(_settingsService.ActiveJava, _settingsService.JavaMemory, _settingsService.JavaMemory); //TODO: 自动选择 Java、自动分配启动内存
                                                                                                                             //.AddExtraParameters(); TODO: 加入额外的虚拟机参数  如：-XX:+UseG1GC 以及额外游戏参数 如：--fullscreen
                                                                                                                             //.SetGameDirectory(); TODO: 版本隔离设置游戏目录

                return builder.Build();
            })
            .Build();
    }
}
