using System.Collections.Generic;

namespace FluentCore.Model.Install.Forge
{
    /// <summary>
    /// Forge安装器结果模型
    /// </summary>
    public class ForgeInstallerResultModel
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// 安装过程中进程输出
        /// </summary>
        public IEnumerable<string> ProcessOutput { get; set; }

        /// <summary>
        /// 安装过程中进程错误输出
        /// </summary>
        public IEnumerable<string> ProcessErrorOutput { get; set; }

        /// <summary>
        /// 安装结果
        /// </summary>
        public string Message { get; set; }
    }
}
