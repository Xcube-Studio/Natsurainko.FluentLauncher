# <img src="../docs/images/AppIcon.png" alt="Logo" width="24" height="24"> Fluent Launcher
![](https://img.shields.io/badge/license-MIT-green)
![](https://img.shields.io/github/repo-size/Xcube-Studio/Natsurainko.FluentLauncher)
![](https://img.shields.io/github/stars/Xcube-Studio/Natsurainko.FluentLauncher)
![](https://img.shields.io/github/contributors/Xcube-Studio/Natsurainko.FluentLauncher)
![](https://img.shields.io/github/commit-activity/y/Xcube-Studio/Natsurainko.FluentLauncher)

Minecraft Java launcher based on .NET 8 and WinUI3 technologies.
Minecraft launcher for Windows 11
Provides a clean and smooth visual experience

## Screenshots 🪟
<img src="../docs/images/home.png">

## Future Plans 📝

| Functions | Status |
| ---------------------------------------- | ------------------ |
| NativeAOT support | In progress [ ] |
| Distributed in Stable, Preview, Dev channels | In progress [ ] |
| Upgrade to CommunityToolkit 8.1 | Blocked by MarkdownTextBlock [ ] |
| OAuth support for Little Skin | [ ] |
| Game integration package import | [ ] |

## List of Functions ✨

+ Basics
  + [x] Managing Game Cores in .minecraft, Installing Game Cores
  + [x] Game-specific core settings, version isolation settings
  + [x] Managing modules for specific game cores
  + [x] Create, start, and manage Minecraft processes
  + [x] Multi-threaded, high-speed game resource completion
  + [x] Finding the Installed Java Runtime
  + [x] Quickly Launch Games from Taskbar
  + [x] Support for 3rd party download mirror sources [Bmclapi, Mcbbs](https://bmclapidoc.bangbang93.com/)
+ Support for multiple authentication schemes
  + [x] Microsoft Verification
  + [x] Yggdrasil validation (external validation)
  + [x] Offline verification
  + [ ] Unified Pass Validation (`need to discuss?`)
+ Multiple loader installer support
  + [x] Forge Installer (NeoForge Temporary)
  + [x] Fabric Installer
  + [x] OptiFine Installer
  + [x] Quilt Installer
  + [ ] LiteLoder (`Outdated but not supported`)
+ Support for third-party resource downloads
  + [x] Download resources from CurseForge
  + [x] Download resources from Modrinth

## Install this application ✈️

#### *[.NET 8 runtime](https://dotnet.microsoft.com/zh-cn/download/dotnet/8.0) must be installed to run the application (regardless of which channel you install it from)*

+ Get our app from the Microsoft Store
<a href="https://apps.microsoft.com/detail/Natsurianko.FluentLauncher/9p4nqqxq942p">
	<img src="https://get.microsoft.com/images/en-us%20dark.svg" width="200"/>
</a>

+ Download the msixbundle for the latest Build from Action and install it manually.
  	+ [How to install Msixbundle package?](https://github-com.translate.goog/Xcube-Studio/Natsurainko.FluentLauncher/wiki/%E5%A6%82%E4%BD%95%E5%AE%89%E8%A3%85-Msixbundle-%E5%8C%85?_x_tr_sl=auto&_x_tr_tl=en&_x_tr_hl=en&_x_tr_pto=wapp)
+ Clone this repository and compile the program manually from the source code

## Application Distribution ✈️

#### *[.NET 8 runtime](https://dotnet.microsoft.com/zh-cn/download/dotnet/8.0) must be installed to run the application (regardless of which channel you install it from)*

### *Stable Channel*

+ Get our app from the Microsoft Store  
<a href="https://apps.microsoft.com/detail/Natsurianko.FluentLauncher/9p4nqqxq942p">
  <img src="https://get.microsoft.com/images/en-us%20dark.svg" width="200"/>
</a>
+ Download the msixbundle for the latest Build from Action and install it manually.
  	+ [How to install Msixbundle package?](https://github-com.translate.goog/Xcube-Studio/Natsurainko.FluentLauncher/wiki/%E5%A6%82%E4%BD%95%E5%AE%89%E8%A3%85-Msixbundle-%E5%8C%85?_x_tr_sl=auto&_x_tr_tl=en&_x_tr_hl=en&_x_tr_pto=wapp)

### *Preview Channel*

+ Download [FluentLauncher.PreviewChannel.PackageInstaller](https://github.com/Xcube-Studio/FluentLauncher.PreviewChannel.PackageInstaller/releases/tag/v0.0.2) and the Preview update package in Releases (such as `updatePackage-x64.zip` file) **(Please download the file that suits your system architecture)**
+ Put both in the same directory
+ Run FluentLauncher.PreviewChannel.PackageInstaller

*Preview channel supports app self-update, you can check for updates in `Settings-About` page*

### *Dev Channel*

Clone this repository and compile the program manually from source code
For details on compilation, see [How to compile the source code](#Developments)

## Developments 🔧

### How to compile the source code

Compile Prerequisites:
> + Install the .NET Desktop Development for Visual Studio 2022
> + Install the [.NET SDK](https://dotnet.microsoft.com/en-us/download/visual-studio-sdks) development tool
> + Install the [Windows App SDK](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/set-up-your-development-environment?tabs=cs-vs-community%2Ccpp-vs-community%2Cvs-2022-17-1-a%2Cvs-2022-17-1-b) development environment and [Visual Studio extension](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/single-project-msix?tabs=csharp)

After preparing the above environment

1. Clone the GitHub repository and its submodules.
2. Make sure the code is complete and open it locally in Visual Studio.
3. Add the CommunityToolkit-Labs Nuget package source to the Nuget package manager  
https://pkgs.dev.azure.com/dotnet/CommunityToolkit/_packaging/CommunityToolkit-Labs/nuget/v3/index.json (not required after upgrading to CommunityToolkit 8.0). (not required after upgrading to CommunityToolkit 8.0)  
4. Press F5 to compile and run

### Localised debugging development

The localisation scripts have been changed and are not listed yet.

#### Contributions localisation resources
See the repository for details **[Xcube-Studio/FluentLauncher.Localisation](https://github.com/Xcube-Studio/FluentLauncher.Localization)**.

### How to contribute to the project

1. Create a branch of the repository by clicking `Fork` in the top right corner and then `Create fork` at the bottom. 
2. Create a branch of your content: `git checkout -b feature/[your-feature]`.
3. Commit your changes: `git commit -m ‘[describe your changes]’`
4. Push your changes to the remote branch: `git push origin feature/[your-feature]`
5. Create a pull request

## Main Contributors 🧑‍💻

* **natsurainko** - *startup kernel launcher*
* **gavinY** - *starter Backend Architecture*
* **xingxing520** - *Launcher release Microsoft Store Services*
Other contributors and beta testers

![Alt](https://repobeats.axiom.co/api/embed/0dcf1b6a60fa8c1c6cefe6042c482f59d2d60538.svg "Repobeats analytics image")

*You can also see all the developers involved in the project in the contributors list. *#

## Contact us at ☕️

Xcube Studio developer group (qq): 1138713376  
Natsurainko's email: a-275@qq.com  

If you have any questions about the project code, we recommend leaving issues, contributors are busy and don't have time to answer private messages.

## Citations and acknowledgements 🎉🎉🎉🎉✨

#### Citations
+ This readme template is referenced from [readme-template](https://github.com/iuricode/readme-template)  

#### Acknowledgements
+ First of all, thanks to all contributors for their joint efforts!  
+ Thanks to bangbang93 and mcbbs for providing mirrors, if you support their services you can [Sponsor Bmclapi](https://afdian.net/@bangbang93)  
+ Thanks to [Cloudflare CDN](https://www.cloudflare.com) for the cloud service.

## Copyright

This project is licensed under the MIT Licence, see [LICENSE](LICENSE) for details.  
Copyright (c) 2022-2024 Xcube Studio
