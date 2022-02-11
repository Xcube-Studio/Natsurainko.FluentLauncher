namespace FluentCore.Model
{
    /// <summary>
    /// JavaAgent模型
    /// </summary>
    public class JavaAgentModel
    {
        public string AgentPath { get; set; }

        public string Parameter { get; set; }

        /// <summary>
        /// 转换为JVM参数
        /// </summary>
        /// <returns></returns>
        public string ToArgument()
        {
            return $"-javaagent:"
                + (AgentPath.Contains(" ") ? $"\"{AgentPath}\"" : AgentPath)
                + $"={Parameter}";
        }
    }
}
