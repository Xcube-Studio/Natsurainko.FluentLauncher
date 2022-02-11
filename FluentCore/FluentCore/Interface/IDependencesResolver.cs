using FluentCore.Model.Launch;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FluentCore.Interface
{
    /// <summary>
    /// 资源检索器接口
    /// </summary>
    public interface IDependencesResolver
    {
        /// <summary>
        /// 要补全资源的游戏核心
        /// </summary>
        GameCore GameCore { get; set; }

        /// <summary>
        /// 获取所有的游戏依赖
        /// </summary>
        /// <returns></returns>
        IEnumerable<IDependence> GetDependences();

        /// <summary>
        /// 获取所有丢失的游戏依赖
        /// </summary>
        /// <returns></returns>
        IEnumerable<IDependence> GetLostDependences();

        /// <summary>
        /// 获取所有的游戏依赖(异步)
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<IDependence>> GetDependencesAsync();

        /// <summary>
        /// 获取所有丢失的游戏依赖(异步)
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<IDependence>> GetLostDependencesAsync();
    }
}
