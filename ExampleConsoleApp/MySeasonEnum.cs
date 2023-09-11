using ScriptEngine;

namespace ExampleConsoleApp;

[EnumerationType("ВременаГода", "Seasons")]
public enum MySeasonEnum
{
    [EnumItem("Весна", "Spring")]
    Spring,
    [EnumItem("Лето", "Summer")]
    Summer,
    [EnumItem("Осень", "Autumn")]
    Autumn,
    [EnumItem("Зима", "Winter")]
    Winter
}