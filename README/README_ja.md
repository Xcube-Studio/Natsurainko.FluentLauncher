<div align="center">

![Fluent Launcherのヒーロー画像](../docs/images/Hero_Image.png)

![スター](https://img.shields.io/github/stars/Xcube-Studio/Natsurainko.FluentLauncher)
![アクティビティ](https://img.shields.io/github/commit-activity/y/Xcube-Studio/Natsurainko.FluentLauncher)
![リポジトリサイズ](https://img.shields.io/github/repo-size/Xcube-Studio/Natsurainko.FluentLauncher)
[![ダウンロード](https://img.shields.io/github/downloads/Xcube-Studio/Natsurainko.FluentLauncher/total?style=social&logo=github)](https://github.com/Xcube-Studio/Natsurainko.FluentLauncher/releases/latest)
![貢献者](https://img.shields.io/github/contributors/Xcube-Studio/Natsurainko.FluentLauncher)
![ライセンス](https://img.shields.io/badge/license-MIT-yellow)

#### Windows 11向けに特別に設計されたMinecraftランチャー、クリーンで流動的な視覚体験を提供
#### 🏪 [Microsoft Storeからインストール](https://apps.microsoft.com/detail/Natsurianko.FluentLauncher/9p4nqqxq942p) | ⬇️ [プレビューチャンネルからインストール](https://github.com/Xcube-Studio/FluentLauncher.Preview.Installer) | 🔧 [開発ドキュメント](https://github.com/Xcube-Studio/Natsurainko.FluentLauncher/wiki/%23-%E5%BC%80%E5%8F%91) | 🚧 [ロードマップ](https://github.com/Xcube-Studio/Natsurainko.FluentLauncher/wiki/%E5%BC%80%E5%8F%91%EF%BC%9A%E8%B7%AF%E7%BA%BF%E5%9B%BE) | 🌐 [他の言語のREADME](README_index.md)

</div>

## ✨ 機能リスト

### 基本機能
+ [x] Minecraftインスタンスの管理とインストール
+ [x] 独立したMinecraftインスタンス設定
+ [x] Minecraftインスタンスのモッドとセーブデータの管理
+ [x] すべてのバージョンのMinecraftの起動サポート
+ [x] マルチスレッドによるゲーム依存リソースの並列補完
+ [x] インストールされたJavaランタイムの自動検出
+ [x] Windowsタスクバーからの素早いゲーム起動
+ [x] ランチャーの外観のカスタマイズ（様々な背景やテーマカラーを含む）
+ [x] 公式Minecraftニュースの取得

### 認証方法
+ [x] Microsoft認証
+ [x] Yggdrasil認証（外部認証）
+ [x] オフライン認証

### ローダーサポート
+ [x] Neoforge \ Forgeローダーのインストールサポート
+ [x] Fabricローダーのインストールサポート
+ [x] OptiFineローダーのインストールサポート
+ [x] Quiltローダーのインストールサポート
> ⚠️ LiteLoderローダーはサポートされていません

### サードパーティリソース
+ [x] CurseForgeからのリソースダウンロードサポート
+ [x] Modrinthからのリソースダウンロードサポート
+ [x] [Bmcl Api](https://bmclapidoc.bangbang93.com/)サードパーティミラーからのダウンロードサポート
+ [x] [MCIM](https://github.com/mcmod-info-mirror/mcim-api)からのモッド説明翻訳の取得サポート

### プレビューチャンネル機能
+ [x] ランチャーアプリケーションの自動更新サポート
+ [x] 一部のバージョンではプラグインの読み込みをサポート [^1]

## ✈️ インストール

> [!IMPORTANT] 
> _**プログラムを起動する前に、お使いのデバイスが以下の推奨要件を満たしていることを確認してください:**_  
> 
> 1. Windows バージョン10.0.19041.0 [^2]以上  
> 2. [.NET 9ランタイム](https://dotnet.microsoft.com/zh-cn/download/dotnet/9.0)をインストール

### Microsoft Storeからインストール
<a href="https://apps.microsoft.com/detail/Natsurianko.FluentLauncher/9p4nqqxq942p"><img src="https://get.microsoft.com/images/en-us%20dark.svg" height="48"/> </a>

### プレビューチャンネルからインストール
`FluentLauncher.Preview.Installer`リポジトリに移動し、Releaseセクションから[FluentLauncher.UniversalInstallerセットアップウィザードをダウンロード](https://github.com/Xcube-Studio/FluentLauncher.Preview.Installer)してください

> msixbundleパッケージを手動でインストールしたり、非推奨のFluentLauncher.PreviewChannel.PackageInstallerを使用して更新パッケージを手動でインストールすることは推奨されなくなりましたが、[こちら](https://github.com/Xcube-Studio/Natsurainko.FluentLauncher/wiki/%E5%85%B3%E4%BA%8E%EF%BC%9A%E6%89%8B%E5%8A%A8%E5%AE%89%E8%A3%85%E9%A2%84%E8%A7%88%E7%89%88%E5%90%AF%E5%8A%A8%E5%99%A8%E5%8C%85)で手順を確認できます

## 💬 ヘルプの取得

**ヘルプを求める**ために、これらのコミュニティに参加できます：

[![GitHub Issues](https://img.shields.io/github/issues-search/Xcube-Studio/Natsurainko.FluentLauncher?query=is%3Aopen&logo=github&label=Issues&color=%233fb950)](https://github.com/Xcube-Studio/Natsurainko.FluentLauncher/issues)
[![GitHub Discussions](https://img.shields.io/github/discussions/Xcube-Studio/Natsurainko.FluentLauncher?&logo=Github&label=Discussions)](https://github.com/Xcube-Studio/Natsurainko.FluentLauncher/discussions)
[![QQグループに参加](https://img.shields.io/badge/QQ_%E7%BE%A4-Xcube_Studio-%230066cc?logo=TencentQQ)](https://qm.qq.com/q/wAo0DKH4xa)

遭遇している問題が**バグ**であると確信している場合、または**新機能**を提案したい場合は、[Issueを提出](https://github.com/Xcube-Studio/Natsurainko.FluentLauncher/issues/new/choose)してください。

## 🔧 開発と貢献

<div align="center">

![Repobeats分析画像](https://repobeats.axiom.co/api/embed/0dcf1b6a60fa8c1c6cefe6042c482f59d2d60538.svg)

</div>

| ブランチ | 開発状況 | 情報 |
| --- | --- | --- |
| [`main`](https://github.com/Xcube-Studio/Natsurainko.FluentLauncher) | 現在長期的なメンテナンスと更新中。 | [![CI](https://github.com/Xcube-Studio/Natsurainko.FluentLauncher/actions/workflows/ci.yml/badge.svg)](https://github.com/Xcube-Studio/Natsurainko.FluentLauncher/actions/workflows/ci.yml) |
| [`legacy/old-uwp-edition`](https://github.com/Xcube-Studio/Natsurainko.FluentLauncher/tree/legacy/old-uwp-edition) | このバージョンはもはやメンテナンスされておらず、アーカイブ目的でのみ保持されています。| ![](https://img.shields.io/badge/Legacy-Stopped-red) |

### 主要貢献者

**[@ natsurainko](https://github.com/natsurainko)** — 起動コア、ランチャー機能実装；ランチャーUIデザイン  
**[@ gaviny82](https://github.com/gaviny82)** — 起動コア、ランチャーアーキテクチャ設計  
**[@ xingxing2008](https://github.com/xingxing2008)** — ランチャーリリース、バックエンドサービスメンテナンス  

その他の貢献者とテスター  

*[貢献者](https://github.com/Xcube-Studio/Natsurainko.FluentLauncher/contributors)セクションでこのプロジェクトに参加したすべての開発者を見ることもできます。*

**このプロジェクトに貢献したい場合は、[開発ドキュメント](https://github.com/Xcube-Studio/Natsurainko.FluentLauncher/wiki/%23-%E5%BC%80%E5%8F%91)を参照してください**

### ライセンス

このプロジェクトはMITライセンスの下でライセンスされています。詳細は[LICENSE](../LICENSE)を参照してください  

### 謝辞

_**まず、すべての貢献者の共同努力に感謝します**_  

- Minecraftダウンロードミラーサービスを提供してくれた[bangbang93](https://github.com/bangbang93)に感謝します。彼らをサポートしたい場合は、[Bmcl Apiをスポンサー](https://afdian.com/@bangbang93)することができます  
- ModrinthとCurseforgeからのモッド翻訳情報を提供してくれた[mcim](https://github.com/mcmod-info-mirror/mcim-api)に感謝します  
- クラウドサービスを提供してくれた[Cloudflare CDN](https://www.cloudflare.com)に感謝します


[^1]: すべてのプレビューバージョンがプラグインローダーをサポートしているわけではありません。プレビューバージョンがローダーをサポートしているかどうかを判断するには、そのリリースに`"enableLoadExtensions": true`プロパティがあるかどうかを確認してください
[^2]: [Windows 10バージョン情報](https://learn.microsoft.com/zh-cn/windows/release-health/release-information)を参照してください