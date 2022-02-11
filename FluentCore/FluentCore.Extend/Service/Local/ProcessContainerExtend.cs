using FluentCore.Service.Local;
using System;
using System.Diagnostics;

namespace FluentCore.Extend.Service.Local
{
    public static class ProcessContainerExtend
    {
        /// <summary>
        /// 挂起根进程
        /// </summary>
        public static void Suspend(this ProcessContainer processContainer)
        {
            if (processContainer.Process.Equals(null))
                throw new NullReferenceException();

            foreach (Process process in Process.GetProcesses())
                if (process.Id.Equals(processContainer.Process.Id))
                {
                    NativeWin32MethodExtend.SuspendProcess(processContainer.Process.Id);
                    processContainer.ProcessState = Model.ProcessState.Suspended;
                }

            throw new InvalidOperationException("The process has not been started or has exited. 该进程尚未启动或已退出。");
        }

        /// <summary>
        /// 恢复根进程
        /// </summary>
        public static void Resume(this ProcessContainer processContainer)
        {
            if (processContainer.Process.Equals(null))
                throw new NullReferenceException();

            foreach (Process process in Process.GetProcesses())
                if (process.Id.Equals(processContainer.Process.Id))
                {
                    NativeWin32MethodExtend.ResumeProcess(processContainer.Process.Id);
                    processContainer.ProcessState = Model.ProcessState.Running;
                }

            throw new InvalidOperationException("The process has not been started or has exited. 该进程尚未启动或已退出。");
        }
    }
}
