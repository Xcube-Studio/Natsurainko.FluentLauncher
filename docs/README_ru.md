# <img src="../docs/images/AppIcon.png" alt="Логотип" width="24" height="24"> Fluent Launcher
![](https://img.shields.io/badge/license-MIT-green)
![](https://img.shields.io/github/repo-size/Xcube-Studio/Natsurainko.FluentLauncher)
![](https://img.shields.io/github/stars/Xcube-Studio/Natsurainko.FluentLauncher)
![](https://img.shields.io/github/contributors/Xcube-Studio/Natsurainko.FluentLauncher)
![](https://img.shields.io/github/commit-activity/y/Xcube-Studio/Natsurainko.FluentLauncher)

Лаунчер Minecraft Java, основанный на технологиях .NET 8 и WinUI3.
Лаунчер Minecraft для Windows 11
Обеспечивает чистый и плавный визуальный опыт

## Скриншоты 🪟
<img src="../docs/images/home.png">

## Планы на будущее 📝

| Функции | Статус |
| ---------------------------------------- | ------------------ |
| Поддержка NativeAOT | В процессе [ ] |
| Распространение в каналах Stable, Preview, Dev | В процессе [ ] |
| Обновление до CommunityToolkit 8.1 | Заблокировано MarkdownTextBlock [ ] |
| Поддержка OAuth для Little Skin | [ ] |
| Импорт пакета игровой интеграции | [ ] |

## Список функций ✨

+ Основные
  + [x] Управление игровыми ядрами в .minecraft, установка игровых ядер
  + [x] Настройки для конкретных игровых ядер, настройки изоляции версий
  + [x] Управление модулями для конкретных игровых ядер
  + [x] Создание, запуск и управление процессами Minecraft
  + [x] Многопоточное, высокоскоростное завершение игровых ресурсов
  + [x] Поиск установленной Java Runtime
  + [x] Быстрый запуск игр с панели задач
  + [x] Поддержка сторонних зеркал загрузки [Bmclapi, Mcbbs](https://bmclapidoc.bangbang93.com/)
+ Поддержка нескольких схем аутентификации
  + [x] Microsoft-аутентификация
  + [x] Yggdrasil-аутентификация (внешняя)
  + [x] Оффлайн-аутентификация
  + [ ] Unified Pass аутентификация (`требует обсуждения?`)
+ Поддержка установщиков различных загрузчиков
  + [x] Forge Installer (NeoForge временно)
  + [x] Fabric Installer
  + [x] OptiFine Installer
  + [x] Quilt Installer
  + [ ] LiteLoder (`Устарел и не поддерживается`)
+ Поддержка загрузки сторонних ресурсов
  + [x] Загрузка ресурсов с CurseForge
  + [x] Загрузка ресурсов с Modrinth

## Распространение приложения ✈️

#### *Для запуска приложения необходимо установить [.NET 8 runtime](https://dotnet.microsoft.com/zh-cn/download/dotnet/8.0) (независимо от того, из какого канала вы его устанавливаете)*

### *Стабильный канал*

+ Получите наше приложение из Microsoft Store  
<a href="https://apps.microsoft.com/detail/Natsurianko.FluentLauncher/9p4nqqxq942p">
  <img src="https://get.microsoft.com/images/en-us%20dark.svg" width="200"/>
</a>

+ Ручная установка пакета msixbundle из Releases
	+ [Как установить пакет Msixbundle?](https://github-com.translate.goog/Xcube-Studio/Natsurainko.FluentLauncher/wiki/%E5%A6%82%E4%BD%95%E5%AE%89%E8%A3%85-Msixbundle-%E5%8C%85?_x_tr_sl=auto&_x_tr_tl=en&_x_tr_hl=en&_x_tr_pto=wapp)

### *Канал предварительных версий*

+ Скачайте [FluentLauncher.PreviewChannel.PackageInstaller](https://github.com/Xcube-Studio/FluentLauncher.PreviewChannel.PackageInstaller/releases/tag/v0.0.2) и пакет обновления Preview из Releases (например, файл `updatePackage-x64.zip`) **(Пожалуйста, скачивайте файл, соответствующий архитектуре вашей системы)**
+ Поместите оба файла в одну директорию
+ Запустите FluentLauncher.PreviewChannel.PackageInstaller

*Канал предварительных версий поддерживает автообновление приложения, вы можете проверить наличие обновлений на странице `Настройки-О программе`*

### *Канал разработки*

Клонируйте этот репозиторий и скомпилируйте программу вручную из исходного кода
Подробности о компиляции см. в разделе [Как скомпилировать исходный код](#Developments)

## Разработка 🔧

### Как скомпилировать исходный код

Предварительные требования для компиляции:
> + Установите .NET Desktop Development для Visual Studio 2022
> + Установите инструмент разработки [.NET SDK](https://dotnet.microsoft.com/en-us/download/visual-studio-sdks)
> + Установите среду разработки [Windows App SDK](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/set-up-your-development-environment?tabs=cs-vs-community%2Ccpp-vs-community%2Cvs-2022-17-1-a%2Cvs-2022-17-1-b) и [расширение Visual Studio](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/single-project-msix?tabs=csharp)

После подготовки вышеуказанной среды:

1. Клонируйте репозиторий GitHub и его подмодули.
2. Убедитесь, что код полный, и откройте его локально в Visual Studio.
3. Добавьте источник пакетов CommunityToolkit-Labs в менеджер пакетов Nuget  
https://pkgs.dev.azure.com/dotnet/CommunityToolkit/_packaging/CommunityToolkit-Labs/nuget/v3/index.json (не требуется после обновления до CommunityToolkit 8.0)
4. Нажмите F5 для компиляции и запуска

### Разработка локализации

Скрипты локализации были изменены и пока не перечислены.

#### Вклад в ресурсы локализации
Подробности см. в репозитории **[Xcube-Studio/FluentLauncher.Localisation](https://github.com/Xcube-Studio/FluentLauncher.Localization)**.

### Как внести вклад в проект

1. Создайте ветку репозитория, нажав `Fork` в правом верхнем углу, затем `Create fork` внизу.
2. Создайте ветку для вашего контента: `git checkout -b feature/[ваша-функция]`.
3. Зафиксируйте изменения: `git commit -m '[описание изменений]'`
4. Отправьте изменения в удаленную ветку: `git push origin feature/[ваша-функция]`
5. Создайте pull request

## Основные участники 🧑‍💻

* **natsurainko** - *стартовое ядро лаунчера*
* **gavinY** - *стартовая архитектура backend*
* **xingxing520** - *Сервисы выпуска лаунчера в Microsoft Store*
Другие участники и бета-тестеры

![Alt](https://repobeats.axiom.co/api/embed/0dcf1b6a60fa8c1c6cefe6042c482f59d2d60538.svg "Изображение аналитики Repobeats")

*Вы также можете увидеть всех разработчиков, участвующих в проекте, в списке участников.*

## Свяжитесь с нами ☕️

Группа разработчиков Xcube Studio (qq): 1138713376  
Email Natsurainko: a-275@qq.com  

Если у вас есть вопросы по коду проекта, рекомендуем оставлять issues, участники заняты и не имеют времени отвечать на личные сообщения.

## Цитирование и благодарности 🎉🎉🎉🎉✨

#### Цитирование
+ Этот шаблон readme основан на [readme-template](https://github.com/iuricode/readme-template)  

#### Благодарности
+ Прежде всего, спасибо всем участникам за совместные усилия!  
+ Спасибо bangbang93 и mcbbs за предоставление зеркал, если вы поддерживаете их сервисы, вы можете [Поддержать Bmclapi](https://afdian.net/@bangbang93)  
+ Спасибо [Cloudflare CDN](https://www.cloudflare.com) за облачный сервис.

## Авторские права

Этот проект лицензирован под лицензией MIT, подробности см. в [LICENSE](LICENSE).  
Copyright (c) 2022-2024 Xcube Studio
