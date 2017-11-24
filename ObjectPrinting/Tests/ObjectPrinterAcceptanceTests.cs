using System;
using System.Globalization;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void Demo()
        {
            var person = new Person { Name = "Alex", Age = 19 };

            var printer = ObjectPrinter.For<Person>();
            //1. Исключить из сериализации свойства определенного типа
            //.Exclude<string>();
            //2. Указать альтернативный способ сериализации для определенного типа
            //.For<string>().Using(x => x.ToLower());
            //3. Для числовых типов указать культуру
            //.For<int>().Using(CultureInfo.CreateSpecificCulture("fr-FR"));
            //4. Настроить сериализацию конкретного свойства
            //.Select(x => x.Age).Using(x => (x - 2).ToString());
            //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
            //.Select(x => x.Name).CuttingBy(3);
            //6. Исключить из сериализации конкретного свойства
            //.Exclude(x => x.Age);

            string s1 = printer.PrintToString(person);
            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            //s1 = person.PrintToString();
            //8. ...с конфигурированием
            //s1 = person.PrintToString(x => x.Exclude<int>().For<string>().Using(y => y.Trim()));
        }
    }
}