using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCore.Model.Install.OptiFine
{
    /// <summary>
    /// OptiFine安装器结果模型
    /// </summary>
    public class OptiFineInstallerResultModel
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
