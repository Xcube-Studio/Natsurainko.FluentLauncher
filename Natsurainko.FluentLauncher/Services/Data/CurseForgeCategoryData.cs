using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Natsurainko.FluentLauncher.Services.Data;

internal class CurseForgeCategoryData
{
    public CurseForgeCategoryData(string name, int id, string iconUrl)
    {
        Name = name;
        Id = id;
        IconUrl = iconUrl;
    }

    public string Name { get; }

    public int Id { get; }

    public string IconUrl { get; }
}
