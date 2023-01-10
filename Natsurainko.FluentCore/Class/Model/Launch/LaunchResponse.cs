using Natsurainko.FluentCore.Event;
using Natsurainko.FluentCore.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Natsurainko.FluentCore.Class.Model.Launch;

public class LaunchResponse : IDisposable
{
    public LaunchState State { get; private set; }

    public IEnumerable<string> Arguemnts { get; private set; }

    public Process Process { get; private set; }

    public Stopwatch RunTime { get; set; }

    public Exception Exception { get; private set; }

    public bool EnableXmlFormat { get; set; }

    public event EventHandler<MinecraftExitedArgs> MinecraftExited;

    public event EventHandler<IProcessOutput> MinecraftProcessOutput;

    private bool disposedValue;

    private List<string> Output = new();

    private string Cache = string.Empty;

    public LaunchResponse(Process process, LaunchState state, IEnumerable<string> args)
    {
        this.Process = process;
        this.State = state;
        this.Arguemnts = args;

        if (state == LaunchState.Succeess)
        {
            this.Process.OutputDataReceived += (_, e) => AddOutput(e.Data);
            this.Process.ErrorDataReceived += (_, e) => AddOutput(e.Data);

            this.Process.Exited += (_, _) =>
            {
                this.RunTime.Stop();

                MinecraftExited?.Invoke(this, new MinecraftExitedArgs
                {
                    Crashed = this.Process.ExitCode != 0,
                    ExitCode = this.Process.ExitCode,
                    RunTime = this.RunTime,
                    Outputs = this.Output
                });
            };

            Process.Start();

            this.Process.BeginOutputReadLine();
            this.Process.BeginErrorReadLine();
        }
    }

    public LaunchResponse(Process process, LaunchState state, IEnumerable<string> args, Exception exception)
    {
        this.Process = process;
        this.State = state;
        this.Arguemnts = args;
        this.Exception = exception;
    }

    public void WaitForExit() => Process?.WaitForExit();

    public async Task WaitForExitAsync() => await Task.Run(() => Process?.WaitForExit());

    public void Stop() => Process?.Kill();

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {

            }

            this.Process?.Dispose();
            this.Arguemnts = null;
            this.Output = null;
            this.Exception = null;

            if (this.MinecraftExited != null)
                foreach (var i in this.MinecraftExited.GetInvocationList())
                    this.MinecraftExited -= (EventHandler<MinecraftExitedArgs>)i;

            if (this.MinecraftProcessOutput != null)
                foreach (var i in this.MinecraftProcessOutput.GetInvocationList())
                    this.MinecraftProcessOutput -= (EventHandler<IProcessOutput>)i;


            disposedValue = true;
        }
    }

    private void AddOutput(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        if (!this.EnableXmlFormat)
        {
            this.Cache = text;

            Output.Add(text);
            MinecraftProcessOutput?.Invoke(this, new BaseProcessOutput(this.Cache));

            this.Cache = string.Empty;
            return;
        }

        if (!(text.StartsWith("<") || text.StartsWith("]") || text.StartsWith(" ") || text.StartsWith(" ")))
        {
            MinecraftProcessOutput?.Invoke(this, new XmlProcessOutput(text, true));
            return;
        }

        if (text.Contains("</log4j:Event>"))
        {
            this.Cache += text;

            Output.Add(this.Cache);
            MinecraftProcessOutput?.Invoke(this, new XmlProcessOutput(this.Cache));
            this.Cache = string.Empty;
        }
        else this.Cache += $"{text}\r\n";
    }
}
