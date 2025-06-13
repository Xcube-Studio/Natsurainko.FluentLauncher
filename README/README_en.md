<div align="center">

![Hero image for Fluent Launcher](../docs/images/Hero_Image.png)

![Stars](https://img.shields.io/github/stars/Xcube-Studio/Natsurainko.FluentLauncher)
![Activity](https://img.shields.io/github/commit-activity/y/Xcube-Studio/Natsurainko.FluentLauncher)
![Repo-Size](https://img.shields.io/github/repo-size/Xcube-Studio/Natsurainko.FluentLauncher)
[![Downloads](https://img.shields.io/github/downloads/Xcube-Studio/Natsurainko.FluentLauncher/total?style=social&logo=github)](https://github.com/Xcube-Studio/Natsurainko.FluentLauncher/releases/latest)
![Contributors](https://img.shields.io/github/contributors/Xcube-Studio/Natsurainko.FluentLauncher)
![License](https://img.shields.io/badge/license-MIT-yellow)

#### A Minecraft launcher designed specifically for Windows 11, providing a clean and fluid visual experience
#### 🏪 [Microsoft Store Installation](https://apps.microsoft.com/detail/Natsurianko.FluentLauncher/9p4nqqxq942p) | ⬇️ [Preview Channel Installation](https://github.com/Xcube-Studio/FluentLauncher.Preview.Installer) | 🔧 [Development Documentation](https://github.com/Xcube-Studio/Natsurainko.FluentLauncher/wiki/%23-%E5%BC%80%E5%8F%91) | 🚧 [Roadmap](https://github.com/Xcube-Studio/Natsurainko.FluentLauncher/wiki/%E5%BC%80%E5%8F%91%EF%BC%9A%E8%B7%AF%E7%BA%BF%E5%9B%BE) | 🌐 [README IN OTHER LANGUAGES](README_index.md)

</div>

## ✨ Feature List

### Basic Features
+ [x] Manage and install Minecraft instances
+ [x] Independent Minecraft instance settings
+ [x] Manage Minecraft instance mods and saves
+ [x] Support for launching all versions of Minecraft
+ [x] Multi-threaded parallel completion of game dependency resources
+ [x] Automatically find installed Java runtime
+ [x] Quick game launch via Windows taskbar
+ [x] Customize launcher appearance (including various backgrounds and theme colors)
+ [x] Get official Minecraft news

### Authentication Methods
+ [x] Microsoft authentication
+ [x] Yggdrasil authentication (external authentication)
+ [x] Offline authentication

### Loader Support
+ [x] Support for installing Neoforge \ Forge loader
+ [x] Support for installing Fabric loader
+ [x] Support for installing OptiFine loader
+ [x] Support for installing Quilt loader
> ⚠️ LiteLoder loader is not supported

### Third-party Resources
+ [x] Support for downloading resources from CurseForge
+ [x] Support for downloading resources from Modrinth
+ [x] Support for [Bmcl Api](https://bmclapidoc.bangbang93.com/) third-party mirror download
+ [x] Support for getting mod description translations from [MCIM](https://github.com/mcmod-info-mirror/mcim-api)

### Preview Channel Features
+ [x] Support for launcher application auto-update
+ [x] Some versions support loading plugins [^1]

## ✈️ Installation

> [!IMPORTANT] 
> _**Before launching the program, please ensure your device meets the following recommended requirements:**_  
> 
> 1. Windows version 10.0.19041.0 [^2] or above  
> 2. Install [.NET 9 Runtime](https://dotnet.microsoft.com/zh-cn/download/dotnet/9.0)

### Install from Microsoft Store
<a href="https://apps.microsoft.com/detail/Natsurianko.FluentLauncher/9p4nqqxq942p"><img src="https://get.microsoft.com/images/en-us%20dark.svg" height="48"/> </a>

### Install from Preview Channel
Go to the `FluentLauncher.Preview.Installer` repository and [download](https://github.com/Xcube-Studio/FluentLauncher.Preview.Installer) the FluentLauncher.UniversalInstaller setup wizard from the Release section

> We no longer recommend manually installing msixbundle packages or using the deprecated FluentLauncher.PreviewChannel.PackageInstaller to manually install update packages, but you can still find instructions [here](https://github.com/Xcube-Studio/Natsurainko.FluentLauncher/wiki/%E5%85%B3%E4%BA%8E%EF%BC%9A%E6%89%8B%E5%8A%A8%E5%AE%89%E8%A3%85%E9%A2%84%E8%A7%88%E7%89%88%E5%90%AF%E5%8A%A8%E5%99%A8%E5%8C%85)

## 💬 Getting Help

You can join these communities to **seek help**:

[![GitHub Issues](https://img.shields.io/github/issues-search/Xcube-Studio/Natsurainko.FluentLauncher?query=is%3Aopen&logo=github&label=Issues&color=%233fb950)](https://github.com/Xcube-Studio/Natsurainko.FluentLauncher/issues)
[![GitHub Discussions](https://img.shields.io/github/discussions/Xcube-Studio/Natsurainko.FluentLauncher?&logo=Github&label=Discussions)](https://github.com/Xcube-Studio/Natsurainko.FluentLauncher/discussions)
[![Join QQ Group](https://img.shields.io/badge/QQ_%E7%BE%A4-Xcube_Studio-%230066cc?logo=TencentQQ)](https://qm.qq.com/q/wAo0DKH4xa)

If you are certain that the issue you are experiencing is a **Bug**, or you want to propose a **new feature**, please [submit an Issue](https://github.com/Xcube-Studio/Natsurainko.FluentLauncher/issues/new/choose).

## 🔧 Development and Contribution

<div align="center">

![Repobeats analytics image](https://repobeats.axiom.co/api/embed/0dcf1b6a60fa8c1c6cefe6042c482f59d2d60538.svg)

</div>

| Branch | Development Status | Information |
| --- | --- | --- |
| [`main`](https://github.com/Xcube-Studio/Natsurainko.FluentLauncher) | Currently under long-term maintenance and updates. | [![CI](https://github.com/Xcube-Studio/Natsurainko.FluentLauncher/actions/workflows/ci.yml/badge.svg)](https://github.com/Xcube-Studio/Natsurainko.FluentLauncher/actions/workflows/ci.yml) |
| [`legacy/old-uwp-edition`](https://github.com/Xcube-Studio/Natsurainko.FluentLauncher/tree/legacy/old-uwp-edition) | This version is no longer maintained, kept for archival purposes only.| ![](https://img.shields.io/badge/Legacy-Stopped-red) |

### Main Contributors

**[@ natsurainko](https://github.com/natsurainko)** — Launch core, launcher feature implementation; launcher UI design  
**[@ gaviny82](https://github.com/gaviny82)** — Launch core, launcher architecture design  
**[@ xingxing2008](https://github.com/xingxing2008)** — Launcher release, backend service maintenance  

And other contributors and testers  

*You can also see all developers who participated in this project in the [Contributors](https://github.com/Xcube-Studio/Natsurainko.FluentLauncher/contributors) section.*

**If you want to contribute to this project, please refer to the [Development Documentation](https://github.com/Xcube-Studio/Natsurainko.FluentLauncher/wiki/%23-%E5%BC%80%E5%8F%91)**

### License

This project is licensed under the MIT License, see [LICENSE](../LICENSE) for details  

### Acknowledgements

_**First of all, thanks to all contributors for their joint efforts**_  

- Thanks to [bangbang93](https://github.com/bangbang93) for providing the Minecraft download mirror service, if you want to support them, you can [sponsor Bmcl Api](https://afdian.com/@bangbang93)  
- Thanks to [mcim](https://github.com/mcmod-info-mirror/mcim-api) for providing mod translation information from Modrinth and Curseforge  
- Thanks to [Cloudflare CDN](https://www.cloudflare.com) for providing cloud services


[^1]: Not all preview versions support the plugin loader. To determine if a preview version supports the loader, check if there is a `"enableLoadExtensions": true` property in its release
[^2]: Please refer to [Windows 10 Version Information](https://learn.microsoft.com/zh-cn/windows/release-health/release-information)