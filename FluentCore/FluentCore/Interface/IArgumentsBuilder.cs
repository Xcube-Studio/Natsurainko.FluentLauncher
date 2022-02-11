using FluentCore.Model.Launch;

namespace FluentCore.Interface
{
    /// <summary>
    /// 参数构造器接口
    /// </summary>
    public interface IArgumentsBuilder
    {
        /// <summary>
        /// 游戏核心
        /// </summary>
        GameCore GameCore { get; set; }

        /// <summary>
        /// 构建整体参数
        /// </summary>
        /// <param name="withJavaPath">是否在参数中带有java可执行文件路径</param>
        /// <returns></returns>
        string BulidArguments(bool withJavaPath = false);

        /// <summary>
        /// 获取前置参数
        /// <para>
        /// MainClass之前的参数
        /// </para>
        /// </summary>
        /// <returns></returns>
        string GetFrontArguments();

        /// <summary>
        /// 获取后置参数
        /// <para>
        /// MainClass之后的参数
        /// </para>
        /// </summary>
        /// <returns></returns>
        string GetBehindArguments();

        /// <summary>
        /// 获取ClassPath参数
        /// </summary>
        /// <returns></returns>
        string GetClasspath();
    }
}
