// See https://aka.ms/new-console-template for more information

using ExampleConsoleApp;
using ScriptEngine.HostedScript;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

var engine = new HostedScriptEngine(); // Создаем движок

// подключаем текущую сборку. Это значит - сканируем сборку на предмет типов, помеченных, как типы OneScript
// ищем перечисления, глобальные контексты, классы и т.п.
engine.AttachAssembly(typeof(Program).Assembly);

// Пусть существует два скрипта, у каждого метод ПолучитьСезон()
// Они могут быть где угодно, в файловой системе или ресурсах, или лежать в БД, 
// так или иначе, все это представлено типом ICodeSource

// Создадим код
var code = @"Перем Сезоны;
Перем Счетчик;
Функция ПолучитьСезон() Экспорт 
    Счетчик = Счетчик + 1;
    Если Счетчик = 4 Тогда
        Счетчик = 0;
    КонецЕсли; 
    Возврат Сезоны[Счетчик];
КонецФункции

Счетчик = 0;
Сезоны = Новый Массив;
Сезоны.Добавить(ВременаГода.Лето);
Сезоны.Добавить(ВременаГода.Осень);
Сезоны.Добавить(ВременаГода.Зима);
Сезоны.Добавить(ВременаГода.Весна);
";

// есть 2 встроенных реализации StringBasedSource и FileBasedSource и фабрика-хелпер Loader в движке
var source = engine.Loader.FromString(code);

// Скомпилируем скрипт
var image = engine.GetCompilerService().Compile(source);
// Образ (image) это просто байткод, он еще не может исполняться
// Чтобы образ стал исполнимым его надо загрузить (прочитать из образа все константы и связать с системой типов)
var loaded = engine.EngineInstance.LoadModuleImage(image);

// теперь инициализируем движок и можно будет им пользоваться
engine.Initialize();

// Есть 2 сценария применения, простой и сложный.
// Простой - мы ничего не хотим, просто хотим вызвать какой-то скрипт и этот скрипт будет иметь тип Сценарий
// В этом случае, нам нужен класс UserScriptContextInstance
// Сложный случай: нас интересует обычный код C# и лишь иногда в приложении что-то заскриптовано. Поэтому, мы хотим видеть наш код
// как код на C# и не забивать себе голову механикой вызова скриптов
// т.е. мы хотим из какого-то класса системы вызывать методы на BSL
// тогда нам надо создать наследника ScriptDrivenObject и использовать его, как обычный класс C#

// Пойдем по сложному пути и создадим класс SeasonReceiver, который будет возвращать сезон, определяемый в скрипте

// Создадим наш Receiver и передадим ему бизнес-логику, написанную на bsl
var receiver = new SeasonReceiver(loaded);

// Дальше пишем наше приложение, как обычное приложение дотнет
Console.WriteLine("Press key to select a season. Press ESC to exit");
ConsoleKey key;
do
{
    key = Console.ReadKey().Key;
    var season = receiver.GetSeason();
    Console.WriteLine($"Season '{season}' was chosen");

} while (key != ConsoleKey.Escape); // esc

// Простой способ заключается в использовании класса UserScriptContextInstance

var userContext = engine.EngineInstance.NewObject(loaded);
var methIdx = userContext.FindMethod("ПолучитьСезон");
userContext.CallAsFunction(methIdx, Array.Empty<IValue>(), out var result);

Console.WriteLine((result as CLREnumValueWrapper<MySeasonEnum>).ValuePresentation);