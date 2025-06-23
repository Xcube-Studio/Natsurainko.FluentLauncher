using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Assimp;
using HelixToolkit.WinUI;
using Natsurainko.FluentLauncher.Services.Storage;
using Nrk.FluentCore.Authentication;
using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Windows.ApplicationModel;
using static Nrk.FluentCore.Utils.PlayerTextureHelper;

namespace Natsurainko.FluentLauncher.Utils.Extensions;

internal static class PlayerTextureProfileExtensions
{
    public readonly static SamplerStateDescription SamplerStateDescription = new()
    {
        Filter = Filter.MinMagMipPoint,
        AddressU = TextureAddressMode.Wrap,
        AddressV = TextureAddressMode.Wrap,
        AddressW = TextureAddressMode.Wrap,
        ComparisonFunction = Comparison.Never,
        BorderColor = new Color4(0, 0, 0, 0),
        MinimumLod = 0,
        MaximumLod = float.MaxValue,
    };

    public static HelixToolkitScene GetRenderModel(this PlayerTextureProfile profile)
    {
        HelixToolkitScene model;

        using Importer importer = new();
        string variant = profile.ActiveSkin?.Variant ?? CalculateModel(profile.Uuid);
        string modelPath = Path.Combine(Package.Current.InstalledLocation.Path, "Assets", "Models", $"player-model-{variant}.fbx");

        model = importer.Load(modelPath);

        if (profile.ActiveCape == null)
            foreach (var node in model.Root.Traverse().Where(x => x.Name.Contains("cape", StringComparison.CurrentCultureIgnoreCase)))
                node.RenderType = RenderType.None;

        return model;
    }

    public static DiffuseMaterial CreateSkinTexture(this PlayerTextureProfile profile)
    {
        DiffuseMaterial skinTexture = new()
        {
            DiffuseMapSampler = SamplerStateDescription,
        };

        string texturePath = profile.GetSkinTexturePath(out _);

        if (File.Exists(texturePath))
            skinTexture.DiffuseMap = TextureModel.Create(texturePath);

        return skinTexture;
    }

    public static DiffuseMaterial CreateCapeTexture(this PlayerTextureProfile profile)
    {
        DiffuseMaterial capeTexture = new()
        {
            DiffuseMapSampler = SamplerStateDescription,
        };

        if (profile.TryGetCapeTexturePath(out var capePath) && File.Exists(capePath))
            capeTexture.DiffuseMap = TextureModel.Create(capePath);

        return capeTexture;
    }

    public static string GetSkinTexturePath(this PlayerTextureProfile profile, out bool isDefalutSkin)
    {
        isDefalutSkin = false;

        if (profile.ActiveSkin == null)
        {
            isDefalutSkin = true;
            string defaultSkin = (profile.ActiveSkin?.Variant ?? CalculateModel(profile.Uuid)) == "classic"
                ? "steve.png" : "alex.png";

            return Path.Combine(
                Package.Current.InstalledLocation.Path, 
                "Assets", "Textures", defaultSkin);
        }

        return Path.Combine(
            LocalStorageService.LocalFolderPath, 
            $"cache-textures\\{GetProfileServiceIdentifier(profile)}\\skins\\" +
            $"{profile.Uuid}.png");
    }

    public static bool TryGetCapeTexturePath(this PlayerTextureProfile profile, [NotNullWhen(true)] out string? path)
    {
        path = null;
        if (profile.ActiveCape == null) return false;

        path = Path.Combine(
            LocalStorageService.LocalFolderPath, 
            $"cache-textures\\{GetProfileServiceIdentifier(profile)}\\capes\\" +
            $"{profile.ActiveCape.Alias}.png");

        return true;
    }

    private static string CalculateModel(Guid uuid)
    {
        byte[] bytes = uuid.ToByteArray();
        return (bytes[6] & 0x01) == 1 ? "slim" : "classic";
    }

    public static string GetProfileServiceIdentifier(PlayerTextureProfile profile)
    {
        return profile.Type.ToString().ToLowerInvariant() +
            (string.IsNullOrEmpty(profile.YggdrasilServerUrl) ? string.Empty : $"-{new Uri(profile.YggdrasilServerUrl).Host}");
    }

    public static string GetProfileServiceIdentifier(this Account account)
    {
        return account.Type.ToString().ToLowerInvariant() +
            (account is YggdrasilAccount yggdrasilAccount ? $"-{new Uri(yggdrasilAccount.YggdrasilServerUrl).Host}" : string.Empty);
    }
}
