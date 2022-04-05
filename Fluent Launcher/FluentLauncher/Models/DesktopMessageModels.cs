using FluentLauncher.DesktopBridger;

namespace FluentLauncher.Models
{
    public class StandardRequestModel : IDesktopMessage
    {
        public string Header { get; set; }

        public string Message { get; set; }
    }

    public class StandardResponseModel : IDesktopMessage
    {
        public string Header { get; set; }

        public string Message { get; set; }

        public string Response { get; set; }
    }

    public class GetCoreListRequest : StandardRequestModel
    {
        public GetCoreListRequest() => this.Header = "GetMinecraftCoreList";

        public string Folder { get; set; }
    }

    public class DeleteMinecraftCoreRequest : StandardRequestModel
    {
        public DeleteMinecraftCoreRequest() => this.Header = "DeleteMinecraftCore";

        public string Folder { get; set; }

        public string Name { get; set; }
    }

    #region

    public class MicrosoftAuthenticationRequest : StandardRequestModel
    {
        public MicrosoftAuthenticationRequest(string code)
        {
            this.Header = "GetMicrosoftAuthenticatorResult";
            this.Code = code;
        }
        public string Code { get; set; }
    }

    public class MicrosoftAuthenticationResponse : StandardResponseModel
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public int ExpiresIn { get; set; }

        public string Time { get; set; }
    }

    public class RefreshMicrosoftAuthenticationRequest : StandardRequestModel
    {
        public RefreshMicrosoftAuthenticationRequest(string refreshToken)
        {
            Header = "GetRefreshMicrosoftAuthenticatorResult";
            RefreshToken = refreshToken;
        }

        public string RefreshToken { get; set; }
    }

    public class RefreshMicrosoftAuthenticationResponse : StandardResponseModel
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public int ExpiresIn { get; set; }

        public string Time { get; set; }
    }

    public class OfflineAuthenticationRequest : StandardRequestModel
    {
        public OfflineAuthenticationRequest(string name)
        {
            this.Header = "GetOfflineAuthenticatorResult";
            this.Name = name;
        }
        public string Name { get; set; }
    }

    public class OfflineAuthenticationResponse : StandardResponseModel
    {
        public string Id { get; set; }

        public string AccessToken { get; set; }

        public string Name { get; set; }
    }

    public class YggdrasilAuthenticationRequest : StandardRequestModel
    {
        public YggdrasilAuthenticationRequest(string email, string password, string serverUrl = null)
        {
            this.Header = "GetYggdrasilAuthenticatorResult";
            this.Email = email;
            this.Password = password;
            this.YggdrasilServerUrl = serverUrl;
        }

        public string Email { get; set; }

        public string Password { get; set; }

        public string YggdrasilServerUrl { get; set; }
    }

    public class YggdrasilAuthenticationResponse : StandardResponseModel
    {
        public string Id { get; set; }

        public string AccessToken { get; set; }

        public string ClientToken { get; set; }

        public string Name { get; set; }
    }

    #endregion

    public class SetDownloadOptitionsRequest : StandardRequestModel
    {
        public SetDownloadOptitionsRequest()
        {
            this.Header = "SetDownloadOptitions";

            DownloadSource = ShareResource.DownloadSource;
            MaxThreads = ShareResource.MaxThreads;
        }

        public string DownloadSource { get; set; }

        public int MaxThreads { get; set; }
    }

    public class LaunchMinecraftRequest : StandardRequestModel
    {
        public LaunchMinecraftRequest()
        {
            this.Header = "LaunchMinecraft";

            Uuid = ShareResource.SelectedAccount.Uuid;
            AccessToken = ShareResource.SelectedAccount.AccessToken;
            UserName = ShareResource.SelectedAccount.UserName;

            JavaPath = ShareResource.SelectedJava.Path;
            GameFolder = ShareResource.SelectedFolder.Path;
            Id = ShareResource.SelectedCore.Id;
            IsIndependent = ShareResource.WorkingFolder == "Independent [.minecraft/versions/{version}/]";
            MinimumMemory = ShareResource.MinMemory;
            MaximumMemory = ShareResource.MaxMemory;
        }

        public string MoreFrontArgs { get; set; } = string.Empty;

        public string MoreBehindArgs { get; set; } = string.Empty;

        public bool IsIndependent { get; set; }

        public string Uuid { get; set; }

        public string AccessToken { get; set; }

        public string UserName { get; set; }

        public string JavaPath { get; set; }

        public string GameFolder { get; set; }

        public string Id { get; set; }

        public int MaximumMemory { get; set; } = 1024;

        public int? MinimumMemory { get; set; } = 512;
    }

    public class InstallMinecraftRequest : StandardRequestModel
    {
        public InstallMinecraftRequest() => this.Header = "InstallMinecraft";

        public string JavaPath { get; set; }

        public string McVersion { get; set; }

        public string Folder { get; set; }

        public string ModLoader { get; set; }
    }

    public class InstallMinecraftProgressResponse : StandardResponseModel
    {
        public double Progress { get; set; }
    }

    public class GetRequiredJavaVersionRequest : StandardRequestModel
    {
        public GetRequiredJavaVersionRequest() => this.Header = "GetRequiredJavaVersion";

        public string Path { get; set; } = ShareResource.SelectedFolder.Path;

        public string McVersion { get; set; } = ShareResource.SelectedCore.Id;
    }

    public class RenameMinecraftCoreRequest : StandardRequestModel
    {
        public RenameMinecraftCoreRequest(string mcVersionId, string newId)
        {
            this.Header = "RenameMinecraftCore";
            McVersionId = mcVersionId;
            NewId = newId;
        }

        public string McVersionId { get; set; }

        public string Folder { get; set; } = ShareResource.SelectedFolder.Path;

        public string NewId { get; set; }
    }
}
