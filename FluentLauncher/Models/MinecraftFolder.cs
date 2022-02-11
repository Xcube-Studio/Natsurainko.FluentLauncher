using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentLauncher.Models
{
    public class MinecraftFolder
    {
        public string Title { get; set; } = "New Minecraft Folder";

        public string Path { get; set; }

        public override bool Equals(object obj)
        {
            var item = (MinecraftFolder)obj;
            if (this.Title == item.Title && this.Path == item.Path)
                return true;

            return false;
        }

        public override int GetHashCode()
        {
            return this.Title.GetHashCode() ^ this.Path.GetHashCode();
        }
    }
}
