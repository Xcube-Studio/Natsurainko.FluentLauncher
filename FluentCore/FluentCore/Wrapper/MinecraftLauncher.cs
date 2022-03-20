using FluentCore.Exceptions.Launcher;
using FluentCore.Interface;
using FluentCore.Model.Launch;
using FluentCore.Service.Component.Launch;
using FluentCore.Service.Local;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace FluentCore.Wrapper
{
    /// <summary>
    /// Minecraft启动器 封装类
    /// </summary>
    public class MinecraftLauncher : ILauncher
    {
        /// <summary>
        /// 用游戏目录和启动配置信息来初始化启动器类
        /// </summary>
        /// <param name="coreLocator">.minecraft目录路径</param>
        /// <param name="config">启动配置信息</param>
        public MinecraftLauncher(ICoreLocator coreLocator, LaunchConfig config)
        {
            this.CoreLocator = coreLocator;
            this.LaunchConfig = config;
        }

        /// <summary>
        /// 启动器调用的进程容器
        /// </summary>
        public ProcessContainer ProcessContainer { get; private set; }

        /// <summary>
        /// 启动参数生成器
        /// </summary>
        public IArgumentsBuilder ArgumentsBuilder { get; private set; }

        /// <summary>
        /// 游戏核心定位器
        /// </summary>
        public ICoreLocator CoreLocator { get; set; }

        /// <summary>
        /// 启动器启动时的配置信息
        /// </summary>
        public LaunchConfig LaunchConfig { get; set; }

        /// <summary>
        /// 根据游戏核心id来启动游戏
        /// </summary>
        /// <param name="id"></param>
        public virtual void Launch(string id)
        {
            if (this.ProcessContainer?.ProcessState == Model.ProcessState.Running)
                throw new GameHasRanException() { ProcessContainer = this.ProcessContainer };
            else this.ProcessContainer?.Dispose();

            var core = this.CoreLocator.GetGameCoreFromId(id);
            if (core == null)
                throw new GameCoreNotFoundException() { Id = id };

            this.ArgumentsBuilder = new ArgumentsBuilder(core, this.LaunchConfig);

            if (string.IsNullOrEmpty(this.LaunchConfig.NativesFolder))
                this.LaunchConfig.NativesFolder = $"{PathHelper.GetVersionFolder(core.Root, id)}{PathHelper.X}natives";

            var nativesDecompressor = new NativesDecompressor(core.Root, id);
            nativesDecompressor.Decompress(core.Natives, this.LaunchConfig.NativesFolder);

            this.ProcessContainer = new ProcessContainer(
                new ProcessStartInfo
                {
                    WorkingDirectory = Directory.Exists(LaunchConfig.WorkingFolder) ? LaunchConfig.WorkingFolder : core.Root,
                    Arguments = ArgumentsBuilder.BulidArguments(),
                    FileName = this.LaunchConfig.JavaPath,
                });

            ProcessContainer.Start();
        }

        /// <summary>
        /// 等待游戏结束并取回启动结果
        /// </summary>
        /// <returns></returns>
        public virtual async Task<LaunchResult> WaitForResult()
        {
            if (this.ProcessContainer?.ProcessState == Model.ProcessState.Exited)
                return new LaunchResult
                {
                    Args = this.ProcessContainer.Process.StartInfo.Arguments,
                    Errors = this.ProcessContainer.ErrorData,
                    Logs = this.ProcessContainer.OutputData,
                    IsCrashed = this.ProcessContainer.Process.ExitCode != 0
                };

            if (this.ProcessContainer?.ProcessState == Model.ProcessState.Initialized)
                throw new Exception("游戏未启动");

            await ProcessContainer.Process.WaitForExitAsync();
            return new LaunchResult
            {
                Args = this.ProcessContainer.Process.StartInfo.Arguments,
                Errors = this.ProcessContainer.ErrorData,
                Logs = this.ProcessContainer.OutputData,
                IsCrashed = this.ProcessContainer.Process.ExitCode != 0
            };
        }

        /// <summary>
        /// 终止游戏 => 立即杀死进程
        /// </summary>
        public void Stop() => this.ProcessContainer.Dispose();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (ProcessContainer != null)
                {
                    this.ProcessContainer.Dispose();
                    this.ProcessContainer = null;
                }

                this.LaunchConfig = null;
                this.CoreLocator = null;
            }
        }

        /// <summary>
        /// 根据游戏核心来启动游戏(异步)
        /// <para>
        /// 静态方法
        /// </para>
        /// </summary>
        /// <param name="core">游戏核心</param>
        /// <param name="config">启动配置信息</param>
        /// <returns></returns>
        public static async Task<LaunchResult> LaunchAsync(GameCore core, LaunchConfig config)
        {
            var args = new ArgumentsBuilder(core, config).BulidArguments();
            if (string.IsNullOrEmpty(config.NativesFolder))
                config.NativesFolder = $"{PathHelper.GetVersionFolder(core.Root, core.Id)}{PathHelper.X}natives";

            var process = new ProcessContainer(new ProcessStartInfo
            {
                WorkingDirectory = Directory.Exists(config.WorkingFolder) ? config.WorkingFolder : core.Root,
                Arguments = args,
                FileName = config.JavaPath,
            });

            process.Start();
            await process.Process.WaitForExitAsync();

            return new LaunchResult
            {
                Args = args,
                Errors = process.ErrorData,
                Logs = process.OutputData,
                IsCrashed = process.Process.ExitCode != 0
            };
        }

        /// <summary>
        /// 根据游戏核心来启动游戏
        /// <para>
        /// 静态方法
        /// </para>
        /// </summary>
        /// <param name="core">游戏核心</param>
        /// <param name="config">启动配置信息</param>
        /// <returns></returns>
        public static LaunchResult Launch(GameCore core, LaunchConfig config) => LaunchAsync(core, config).GetAwaiter().GetResult();
    }
}
