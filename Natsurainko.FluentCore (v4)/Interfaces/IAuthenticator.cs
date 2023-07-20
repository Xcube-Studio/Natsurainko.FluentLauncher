using Nrk.FluentCore.Classes.Datas.Authenticate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nrk.FluentCore.Interfaces;

/// <summary>
/// 验证器接口
/// </summary>
/// <typeparam name="TAccount"></typeparam>
public interface IAuthenticator<out TAccount>
{
    TAccount Authenticate();
}
