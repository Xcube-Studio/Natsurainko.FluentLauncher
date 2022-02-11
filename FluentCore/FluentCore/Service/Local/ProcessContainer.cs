using FluentCore.Event.Process;
using FluentCore.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FluentCore.Service.Local
{
    /// <summary>
    /// 提供一个容器，该容器可用于管理一个进程
    /// </summary>
    public class ProcessContainer : IDisposable
    {
        /// <summary>
        /// 初始化进程容器，但不启动根进程
        /// 该进程容器中会强制设置进程不使用shell，并且重定向输入、输出流
        /// </summary>
        /// <param name="processStartInfo">指定启动进程时使用的一组值</param>
        public ProcessContainer(ProcessStartInfo processStartInfo)
        {
            processStartInfo.UseShellExecute = false;
            processStartInfo.RedirectStandardOutput = processStartInfo.RedirectStandardError = processStartInfo.RedirectStandardInput = true;

            this.Process = new Process
            {
                EnableRaisingEvents = true,
                StartInfo = processStartInfo
            };

            Process.OutputDataReceived += Process_OutputDataReceived;
            Process.ErrorDataReceived += Process_ErrorDataReceived;
            Process.Exited += Process_Exited;

            this.ProcessState = ProcessState.Initialized;
        }

        /// <summary>
        /// 等效于 Process.OutputDataReceived
        /// <para>但该输出中包含了Error流的输出</para>
        /// </summary>
        public event EventHandler<DataReceivedEventArgs> OutputDataReceived;

        /// <summary>
        /// 等效于 Process.ErrorDataReceived
        /// </summary>
        public event EventHandler<DataReceivedEventArgs> ErrorDataReceived;

        /// <summary>
        /// 等效于 Process.Exited
        /// </summary>
        public event EventHandler<ProcessExitedEventArgs> Exited;

        /// <summary>
        /// 当根进程非正常退出(崩溃)时发生
        /// </summary>
        public event EventHandler<ProcessCrashedEventArgs> Crashed;

        /// <summary>
        /// 当根进程启动时发生
        /// </summary>
        public event EventHandler Started;

        /// <summary>
        /// 当根进程进入未响应状态时发生
        /// </summary>
        public event EventHandler Unresponded;

        /// <summary>
        /// 当ProcessState改变时发生
        /// </summary>
        public event EventHandler<ProcessStateChangedEventArgs> ProcessStateChanged;

        /// <summary>
        /// 进程容器的根进程
        /// </summary>
        public Process Process { get; private set; }

        /// <summary>
        /// 进程容器中根进程的运行状态
        /// <para>修改此项可能会导致错误</para>
        /// </summary>
        public ProcessState ProcessState { get { return state; } set { state = value; ProcessStateChanged?.Invoke(null, new ProcessStateChangedEventArgs { ProcessState = value }); } }

        /// <summary>
        /// 测量运行时间
        /// </summary>
        public Stopwatch Stopwatch { get; private set; }

        /// <summary>
        /// 记录根进程是否已经启动过
        /// <para>该值变为true后将不会再改变</para>
        /// </summary>
        public bool HasStarted { get; set; } = false;

        /// <summary>
        /// 错误日志集合
        /// </summary>
        public List<string> ErrorData { get; private set; }

        /// <summary>
        /// 日志集合
        /// </summary>
        public List<string> OutputData { get; private set; }

        protected Task observeRespondTask;

        protected CancellationTokenSource tokenSource;

        protected ProcessState state = ProcessState.Undefined;

        /// <summary>
        /// 启动容器根进程
        /// </summary>
        public void Start()
        {
            if (Process.Start())
                ProcessState = ProcessState.Running;
            else throw new NotSupportedException("启动根进程失败");

            this.Stopwatch = Stopwatch.StartNew();

            this.Process.BeginOutputReadLine();
            this.Process.BeginErrorReadLine();

            if (!this.HasStarted)
                this.HasStarted = true;

            ErrorData = new List<string>();
            OutputData = new List<string>();

            this.Started?.Invoke(this, new EventArgs());
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            this.ProcessState = ProcessState.Exited;
            this.Stopwatch.Stop();

            this.Exited?.Invoke(sender, new ProcessExitedEventArgs
            {
                RunTime = this.Stopwatch.Elapsed,
                ExitCode = this.Process.ExitCode,
                IsNormal = this.Process.ExitCode == 0
            });

            if (this.Process.ExitCode != 0)
                this.Crashed?.Invoke(sender, new ProcessCrashedEventArgs
                {
                    CrashData = ErrorData
                });
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data))
                return;

            this.ErrorDataReceived?.Invoke(sender, e);
            this.OutputDataReceived?.Invoke(sender, e);

            this.ErrorData.Add(e.Data);
            this.OutputData.Add(e.Data);
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data))
                return;

            this.OutputDataReceived?.Invoke(sender, e);

            this.OutputData.Add(e.Data);
        }

        /// <summary>
        /// 关闭现有根进程(若根进程已启动但未退出)，并启动
        /// <para>
        /// 若该进程还未进行过启动，则直接调用Start()
        /// </para>
        /// </summary>
        public void ReStart()
        {
            if (HasStarted)
            {
                foreach (Process process in Process.GetProcesses())
                    if (process.Id.Equals(this.Process.Id))
                        Process.Kill();

                this.Start();
            }
            else this.Start();
        }

        /// <summary>
        /// 等效于 Process.CloseMainWindow()
        /// <para>
        /// 当该进程没有关联的主窗口时会引发错误
        /// </para>
        /// </summary>
        public void CloseMainWindow()
        {
            this.Process.Refresh();

            if (this.Process.MainWindowHandle.Equals(IntPtr.Zero))
                throw new NotSupportedException("该进程没有关联的主窗口");
            this.Process.CloseMainWindow();
        }

        /// <summary>
        /// <code>
        /// => Process.Kill()
        /// </code>
        /// </summary>
        public void Kill() => this.Process.Kill();

        /// <summary>
        /// 启动进程阻塞(未响应)监视器
        /// <para>
        /// *若不开启此项，则ProcessState属性永远不会出现Responding值
        /// </para>
        /// </summary>
        public void StartObserveRespond()
        {
            tokenSource = new CancellationTokenSource();
            observeRespondTask = Task.Run(async delegate
            {
                while (true)
                {
                    await Task.Delay(1000);
                    if (tokenSource.Token.IsCancellationRequested)
                        tokenSource.Token.ThrowIfCancellationRequested();
                    else
                    {
                        switch (this.ProcessState)
                        {
                            case ProcessState.Running:
                                this.Unresponded?.Invoke(this, new EventArgs());

                                if (!this.Process.Responding)
                                    this.ProcessState = ProcessState.Unresponding;
                                break;
                            case ProcessState.Unresponding:
                                if (this.Process.Responding)
                                    this.ProcessState = ProcessState.Running;
                                break;
                        }
                    }
                }
            }, tokenSource.Token);
        }

        /// <summary>
        /// 关闭进程阻塞监视器
        /// </summary>
        public void StopObserveRespond()
        {
            if (tokenSource.Equals(null))
                throw new NullReferenceException();

            tokenSource.Cancel();
            observeRespondTask.Dispose();
            tokenSource.Dispose();
            tokenSource = null;
            observeRespondTask = null;
        }

        /// <summary>
        /// 异步设置根进程主窗口的标题文字
        /// </summary>
        /// <param name="title">要设置的标题文字</param>
        /// <returns></returns>
        public async void SetMainWindowTitleAsync(string title)
        {
            while (string.IsNullOrEmpty(this.Process.MainWindowTitle))
            {
                await Task.Delay(50);

                this.Process.Refresh();
                _ = NativeWin32Method.SetWindowText(this.Process.MainWindowHandle, title);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Process != null)
                {
                    foreach (Process process in Process.GetProcesses())
                        if (process.Id.Equals(Process.Id))
                            Process.Kill();

                    /*
                    OutputDataReceived.GetInvocationList().ToList().ForEach(x =>
                    {
                        OutputDataReceived -= x as EventHandler<DataReceivedEventArgs>;
                    });*/

                    Process.Close();
                    Process.Dispose();
                    Process = null;
                }

                if (tokenSource != null)
                    StopObserveRespond();
            }
        }
    }
}
