using FluentLauncher.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentLauncher.Classes
{
    public static class IOHelper
    {
        public static async Task<bool> FileExist(string path)
        {
            var res = await App.DesktopBridge.SendAsync<StandardResponseModel>(new StandardRequestModel
            {
                Header = "FileExist",
                Message = path
            });

            return Convert.ToBoolean(res.Response);
        }

        public static async Task<bool> FolderExist(string path)
        {
            var res = await App.DesktopBridge.SendAsync<StandardResponseModel>(new StandardRequestModel
            {
                Header = "FolderExist",
                Message = path
            });

            return Convert.ToBoolean(res.Response);
        }
    }
}
