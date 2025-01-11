# <img src="../docs/images/AppIcon.png" alt="Логотип" width="24" height="24"> Fluent Launcher
![](https://img.shields.io/badge/license-MIT-green)
![](https://img.shields.io/github/repo-size/Xcube-Studio/Natsurainko.FluentLauncher)
![](https://img.shields.io/github/stars/Xcube-Studio/Natsurainko.FluentLauncher)
![](https://img.shields.io/github/contributors/Xcube-Studio/Natsurainko.FluentLauncher)
![](https://img.shields.io/github/commit-activity/y/Xcube-Studio/Natsurainko.FluentLauncher)

Лаунчер Minecraft Java, заснований на технологіях .NET 8 та WinUI3.
Лаунчер Minecraft для Windows 11
Забезпечує чистий та плавний візуальний досвід

## Скріншоти 🪟
<img src="../docs/images/home.png">

## Плани на майбутнє 📝

| Функції | Статус |
| ---------------------------------------- | ------------------ |
| Підтримка NativeAOT | В процесі [ ] |
| Поширення в каналах Stable, Preview, Dev | В процесі [ ] |
| Оновлення до CommunityToolkit 8.1 | Заблоковано MarkdownTextBlock [ ] |
| Підтримка OAuth для Little Skin | [ ] |
| Імпорт пакету ігрової інтеграції | [ ] |

## Список функцій ✨

+ Основні
  + [x] Керування ігровими ядрами в .minecraft, встановлення ігрових ядер
  + [x] Налаштування для конкретних ігрових ядер, налаштування ізоляції версій
  + [x] Керування модулями для конкретних ігрових ядер
  + [x] Створення, запуск та керування процесами Minecraft
  + [x] Багатопотокове, високошвидкісне завершення ігрових ресурсів
  + [x] Пошук встановленої Java Runtime
  + [x] Швидкий запуск ігор з панелі завдань
  + [x] Підтримка сторонніх дзеркал завантаження [Bmclapi, Mcbbs](https://bmclapidoc.bangbang93.com/)
+ Підтримка кількох схем автентифікації
  + [x] Microsoft-автентифікація
  + [x] Yggdrasil-автентифікація (зовнішня)
  + [x] Офлайн-автентифікація
  + [ ] Unified Pass автентифікація (`потребує обговорення?`)
+ Підтримка встановлювачів різних завантажувачів
  + [x] Forge Installer (NeoForge тимчасово)
  + [x] Fabric Installer
  + [x] OptiFine Installer
  + [x] Quilt Installer
  + [ ] LiteLoder (`Застарів і не підтримується`)
+ Підтримка завантаження сторонніх ресурсів
  + [x] Завантаження ресурсів з CurseForge
  + [x] Завантаження ресурсів з Modrinth

## Поширення додатку ✈️

#### *Для запуску додатку необхідно встановити [.NET 8 runtime](https://dotnet.microsoft.com/zh-cn/download/dotnet/8.0) (незалежно від того, з якого каналу ви його встановлюєте)*

### *Стабільний канал*

+ Отримайте наш додаток з Microsoft Store  
<a href="https://apps.microsoft.com/detail/Natsurianko.FluentLauncher/9p4nqqxq942p">
  <img src="https://get.microsoft.com/images/en-us%20dark.svg" width="200"/>
</a>

+ Ручне встановлення пакету msixbundle з Releases
	+ [Як встановити пакет Msixbundle?](https://github-com.translate.goog/Xcube-Studio/Natsurainko.FluentLauncher/wiki/%E5%A6%82%E4%BD%95%E5%AE%89%E8%A3%85-Msixbundle-%E5%8C%85?_x_tr_sl=auto&_x_tr_tl=en&_x_tr_hl=en&_x_tr_pto=wapp)

### *Канал попередніх версій*

+ Завантажте [FluentLauncher.PreviewChannel.PackageInstaller](https://github.com/Xcube-Studio/FluentLauncher.PreviewChannel.PackageInstaller/releases/tag/v0.0.2) та пакет оновлення Preview з Releases (наприклад, файл `updatePackage-x64.zip`) **(Будь ласка, завантажуйте файл, що відповідає архітектурі вашої системи)**
+ Помістіть обидва файли в одну директорію
+ Запустіть FluentLauncher.PreviewChannel.PackageInstaller

*Канал попередніх версій підтримує автооновлення додатку, ви можете перевірити наявність оновлень на сторінці `Налаштування-Про програму`*

### *Канал розробки*

Клонуйте цей репозиторій та скомпілюйте програму вручну з вихідного коду
Подробиці про компіляцію див. у розділі [Як скомпілювати вихідний код](#Developments)

## Розробка 🔧

### Як скомпілювати вихідний код

Попередні вимоги для компіляції:
> + Встановіть .NET Desktop Development для Visual Studio 2022
> + Встановіть інструмент розробки [.NET SDK](https://dotnet.microsoft.com/en-us/download/visual-studio-sdks)
> + Встановіть середовище розробки [Windows App SDK](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/set-up-your-development-environment?tabs=cs-vs-community%2Ccpp-vs-community%2Cvs-2022-17-1-a%2Cvs-2022-17-1-b) та [розширення Visual Studio](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/single-project-msix?tabs=csharp)

Після підготовки вищезазначеного середовища:

1. Клонуйте репозиторій GitHub та його підмодулі.
2. Переконайтеся, що код повний, та відкрийте його локально в Visual Studio.
3. Додайте джерело пакетів CommunityToolkit-Labs у менеджер пакетів Nuget  
https://pkgs.dev.azure.com/dotnet/CommunityToolkit/_packaging/CommunityToolkit-Labs/nuget/v3/index.json (не потрібно після оновлення до CommunityToolkit 8.0)
4. Натисніть F5 для компіляції та запуску

### Розробка локалізації

Скрипти локалізації були змінені та поки не перелічені.

#### Внесок у ресурси локалізації
Подробиці див. у репозиторії **[Xcube-Studio/FluentLauncher.Localisation](https://github.com/Xcube-Studio/FluentLauncher.Localization)**.

### Як зробити внесок у проект

1. Створіть гілку репозиторію, натиснувши `Fork` у правому верхньому куті, потім `Create fork` внизу.
2. Створіть гілку для вашого контенту: `git checkout -b feature/[ваша-функція]`.
3. Зафіксуйте зміни: `git commit -m '[опис змін]'`
4. Відправте зміни у віддалену гілку: `git push origin feature/[ваша-функція]`
5. Створіть pull request

## Основні учасники 🧑‍💻

* **natsurainko** - *стартове ядро лаунчера*
* **gavinY** - *стартова архітектура backend*
* **xingxing520** - *Сервіси випуску лаунчера в Microsoft Store*
Інші учасники та бета-тестери

![Alt](https://repobeats.axiom.co/api/embed/0dcf1b6a60fa8c1c6cefe6042c482f59d2d60538.svg "Зображення аналітики Repobeats")

*Ви також можете побачити всіх розробників, що беруть участь у проекті, у списку учасників.*

## Зв'яжіться з нами ☕️

Група розробників Xcube Studio (qq): 1138713376  
Email Natsurainko: a-275@qq.com  

Якщо у вас є питання щодо коду проекту, рекомендуємо залишати issues, учасники зайняті і не мають часу відповідати на особисті повідомлення.

## Цитування та подяки 🎉🎉🎉🎉✨

#### Цитування
+ Цей шаблон readme засновано на [readme-template](https://github.com/iuricode/readme-template)  

#### Подяки
+ Перш за все, дякуємо всім учасникам за спільні зусилля!  
+ Дякуємо bangbang93 та mcbbs за надання дзеркал, якщо ви підтримуєте їхні сервіси, ви можете [Підтримати Bmclapi](https://afdian.net/@bangbang93)  
+ Дякуємо [Cloudflare CDN](https://www.cloudflare.com) за хмарний сервіс.

## Авторські права

Цей проект ліцензовано під ліцензією MIT, подробиці див. у [LICENSE](LICENSE).  
Copyright (c) 2022-2024 Xcube Studio
