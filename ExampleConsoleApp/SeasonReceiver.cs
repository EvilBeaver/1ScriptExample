using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ExampleConsoleApp;

[ContextClass("ПолучательСезонов", "SeasonReceiver")]
public class SeasonReceiver : AutoScriptDrivenObject<SeasonReceiver>
{
    public SeasonReceiver(LoadedModule module) : base(module)
    {
        // Тут костыль, который исправлен в 2.0
        // Если кто-то вызовет ТипЗнч(ЭтотОбъект) он получит тип "Object"
        // Чтобы этого не было, надо полуить имя типа, созданное при регистрации контекстов
        // во время AttachAssembly
        DefineType(TypeManager.GetTypeByFrameworkType(GetType()));
        
        // Выполним тело модуля
        Initialize();
    }

    public MySeasonEnum GetSeason()
    {
        // Смотрим, определил ли пользователь в своем скрипте метод ПолучитьСезон
        var idx = GetScriptMethod("ПолучитьСезон", "GetSeason");
        if (idx == -1)
        {
            // Пользователь не сделал такой метод, вернем сами что-нибудь
            return MySeasonEnum.Summer;
        }

        var value = CallScriptMethod(idx, Array.Empty<IValue>());
        
        // Поскольку язык динамический, пользователь мог вернуть из метода вообще что угодно
        // перечсления CLR представлены в BSL типом CLREnumValueWrapper
        if (value is CLREnumValueWrapper<MySeasonEnum> enumWrapper)
        {
            // все хорошо, пользователь соблюдал контракт и вернул правильный тип
            return enumWrapper.UnderlyingValue;
        }
        else
        {
            // Ругнемся на пользователя, который не соблюдал контракт
            throw new RuntimeException("Script must return enum value Seasons");
        }
    }
}