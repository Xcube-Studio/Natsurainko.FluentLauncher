using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentLauncher.Models
{
    public class MinecraftCoreInfo
    {
        public string Icon
        {
            get
            {
                if (Tag.Contains("old_beta") || Tag.Contains("old_alpha"))
                    return "Dirt_Podzol";
                if (Tag.Contains("snapshot"))
                    return "Crafting_Table";
                if (Tag.Contains("modded"))
                    return "Furnace";


                return "Grass";
            }
        }

        public string Id { get; set; }

        public string Tag { get; set; }

        public override bool Equals(object obj)
        {
            var item = (MinecraftCoreInfo)obj;
            if (this.Tag == item.Tag && this.Id == item.Id)
                return true;

            return false;
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode() ^ this.Tag.GetHashCode();
        }
    }
}
