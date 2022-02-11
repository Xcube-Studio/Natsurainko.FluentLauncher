using FluentCore.Model.Game;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace FluentCore.Model.Launch
{
    /// <summary>
    /// 游戏启动核心
    /// </summary>
    public class GameCore
    {
        public AssetIndex AsstesIndex { get; set; }

        //public Logging Logging { get; set; }

        public Dictionary<string, FileModel> Downloads { get; set; }

        public string Root { get; set; }

        public IEnumerable<Library> Libraries { get; set; }

        public IEnumerable<Native> Natives { get; set; }

        public string MainClass { get; set; }

        public string MainJar { get; set; }

        public string FrontArguments { get; set; }

        public string BehindArguments { get; set; }

        public string Id { get; set; }

        public string Type { get; set; }

        public JavaVersion JavaVersion { get; set; }
    }
}
