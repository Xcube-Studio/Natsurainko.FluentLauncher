using System.Collections.Generic;

namespace Natsurainko.FluentLauncher.Classes.Data.UI;

internal class GroupInfoList : List<object>
{
    public GroupInfoList(IEnumerable<object> items) : base(items)
    {
    }
    public object Key { get; set; }

    public override string ToString()
    {
        return "Group " + Key.ToString();
    }
}
