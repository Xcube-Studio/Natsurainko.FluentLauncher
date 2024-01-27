# <img src="docs/images/AppIcon.png" alt="Logo" width="24" height="24"> Fluent Launcher
![](https://img.shields.io/badge/license-MIT-green)
![](https://img.shields.io/github/repo-size/Xcube-Studio/Natsurainko.FluentLauncher)
![](https://img.shields.io/github/stars/Xcube-Studio/Natsurainko.FluentLauncher)
![](https://img.shields.io/github/contributors/Xcube-Studio/Natsurainko.FluentLauncher)
![](https://img.shields.io/github/commit-activity/y/Xcube-Studio/Natsurainko.FluentLauncher)

基于 .NET 8 以及 WinUI3 技术开发的 Minecraft Java 版启动器  
专为 Windows 11 设计的 Minecraft 启动器  
提供简洁、流畅的视觉体验

>
> 下一次更新预告：  
> 更新高频期预计在二月份，现在还在等着放寒假  
> 具体的更新事项我会在 Project 的路线图中把所有问题列出来  
> —— 2024.1.27
>

## 窗口截图 🪟
<img src="docs/images/image0.png">

## 未来路线计划 📝

| 功能                                     | 状态               |
| ---------------------------------------- | ------------------ |
| 升级到 CommunityToolkit 8.0 `[紧急]`     | [ ]                |
| 启动过程异步化                           | [ ]                |
| 游戏整合包导入                           | [ ]                |
| 皮肤管理与 3D 预览 (需要讨论?)           | [ ]                |
| Native AOT 支持 (需要讨论?)              | [ ]                |

## 功能列表 ✨

+ 基本功能
  + [x] 管理 .minecraft 中的游戏核心、安装游戏核心
  + [x] 特定游戏核心设置、版本隔离设置
  + [x] 管理特定游戏核心的模组
  + [x] 创建、启动、管理 Minecraft 进程 
  + [x] 多线程高速补全游戏资源
  + [x] 查找已安装的 Java 运行时
  + [x] 任务栏快速启动游戏
  + [x] 支持第三方下载镜像源 [Bmclapi、Mcbbs](https://bmclapidoc.bangbang93.com/)
+ 多种验证方案的支持
  + [x] 微软验证
  + [x] Yggdrasil 验证 (外置验证)
  + [x] 离线验证
  + [ ] 统一通行证验证 (`需要讨论?`)
+ 多种加载器安装器的支持
  + [x] Forge 安装器 (NeoForge 暂用)
  + [x] Fabric 安装器
  + [x] OptiFine 安装器
  + [x] Quilt 安装器
  + [ ] LiteLoder (`已过时而未支持`)
+ 第三方资源下载的支持
  + [x] 下载 CurseForge 上的资源
  + [x] 下载 Modrinth 上的资源

## 安装本应用 ✈️

#### *一定需要安装 [.NET 8 运行时](https://dotnet.microsoft.com/zh-cn/download/dotnet/8.0) (不论是从哪种渠道安装)*


+ 从 Microsoft Store 获取我们的应用 <a><img src="https://get.microsoft.com/images/en-us%20dark.svg" alt="Logo" width="70" height="20"></a>  
+ 从 Action 下载最近一次的 Build 的 msixbundle 安装包进行手动安装 
  + [如何安装 Msixbundle 包 ?](https://github.com/Xcube-Studio/Natsurainko.FluentLauncher/wiki/%E5%A6%82%E4%BD%95%E5%AE%89%E8%A3%85-Msixbundle-%E5%8C%85)
+ 克隆本仓库，从源码手动编译程序

## 开发事宜 🔧

### 如何编译源代码

编译前提：
> + 安装 .NET 桌面开发的 Visual Studio 2022
> + 安装 .NET 8 SDK 的开发工具
> + 安装 WindowsAppSDK 1.4 的开发环境和 VS 扩展

准备好上述环境后

1. 克隆该仓库及其子模块 Natsurainko.FluentCore、FluentLauncher.LocalizationPoroject
2. 确保代码完整后，在本地用 Visual Studio 打开
3. 在 Nuget 包管理器中添加 CommunityToolkit-Labs 的 Nuget 包源  
https://pkgs.dev.azure.com/dotnet/CommunityToolkit/_packaging/CommunityToolkit-Labs/nuget/v3/index.json (升级到 CommunityToolkit 8.0 后将不需要)  
4. F5 编译运行

### 本地化调试开发

`本地化脚本目前有所改动，暂时先不列出`

#### 贡献本地化资源
详细见仓库 **[Xcube-Studio/FluentLauncher.LocalizationPoroject](https://github.com/Xcube-Studio/FluentLauncher.LocalizationPoroject)**

### 如何贡献该项目

1. 点击右上角 `Fork` ，再点击下方 `Create fork` 创建一份本仓库的分支
2. 创建你的内容分支：`git checkout -b feature/[your-feature]`
3. 提交你的更改：`git commit -m '[描述你的更改]'`
4. 将更改推送至远程分支：`git push origin feature/[your-feature]`
5. 创建拉取请求

## 主要贡献者 🧑‍💻

* **natsurainko** - *启动核心 启动器*
* **gavinY** - *启动器 后端架构*
* **xingxing520** - *启动器发布 微软商店服务*

等其他贡献者与参与测试人员

*您也可以在贡献者名单中参看所有参与该项目的开发者。*

## 与我们联系 ☕️

Xcube Studio 开发群(qq): 1138713376  
Natsurainko 的邮箱: a-275@qq.com  

如果有任何项目代码的问题还是建议留 issues，各位贡献者都比较忙碌，不太有时间回复私信之类的

## 引用及鸣谢 🎉

#### 引用
+ 本篇 readme 模板引用自 [readme-template](https://github.com/iuricode/readme-template)  

#### 鸣谢
+ 首先感谢各位贡献者的共同努力  
+ 感谢 bangbang93 以及 mcbbs 提供镜像站服务 如果支持他们的服务话 可以[赞助Bmclapi](https://afdian.net/@bangbang93)  
+ 感谢 [Cloudflare CDN](https://www.cloudflare.com) 提供的云服务

## 版权

该项目签署了MIT 授权许可，详情请参阅 [LICENSE](LICENSE)  
Copyright (c) 2022-2024 Xcube Studio
