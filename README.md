# Notechat desktop app

Desktop app for [notechat server](https://github.com/AlexBSoft/notechat-server) written in C# .net 6.0 with socket.io

Многопользовательское приложение, повторяющее блокнот Microsoft.

<p align="center">
    <a href="https://notechat.ru/">
        <img src="https://notechat.ru/assets/screen1.png">
    </a>
</p>

## Руководство пользователя

__Режим ввода команд (оффлайн режим)__

Нажмите __F1__ чтобы отключить синхронизацию изменений и выйти в оффлайн режим. В этом режиме вы можете спокойно набирать текст и выполнять команды

__Создание комнат (файлов)__

Комната это отдельное пространство для синхронизации текста между пользователями. Для входа в комнату используется команда Файл->создать (комбинация клавиш __ctrl+n__). Название комнаты указывается в поле ввода. Команду лучше выполнять в оффлайн режиме (клавиша F1).

__Подключение к альтернативному серверу__

Комбинация клавиш alt+c. В поле ввода должен быть указан полный URL сервера.

## Разработка

Сделано в VisualStudio 2022, требуется dotnet 6.0

### Sources

https://github.com/doghappy/socket.io-client-csharp

https://github.com/AlexBSoft/notechat-server
