## Инструкция по запуску приложения

### Настройка базы данных
1. Откройте файл `appsettings.json` и обновите строку подключения:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Host=localhost;Port=5432;Database=vk_analyzer;Username=postgres;Password=ваш_пароль"
   }

2. Примените миграции для создания базы данных и таблиц:

dotnet ef database update

### Запустите приложение:

dotnet run
