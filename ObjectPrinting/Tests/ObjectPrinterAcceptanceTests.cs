using System;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        //Примеры использования
        [Test]
        public void Demo()
        {
            var person = new Person { Name = "Alex", Age = 19 };

            var printer = ObjectPrinter.For<Person>();
            //1. Исключить из сериализации свойства определенного типа
            //.Exclude<string>();
            //2. Указать альтернативный способ сериализации для определенного типа
            //.ForType<string>().Using(x => x.ToUpper());
            //3. Для числовых типов указать культуру
            //.ForType<int>().Using(CultureInfo.GetCultureInfo("fr-FR"));
            //4. Настроить сериализацию конкретного свойства
            //.ForProperty(x => x.Age).Using(x => (x - 2).ToString());
            //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
            //.ForProperty(x => x.Name).CutTo(3);
            //6. Исключить из сериализации конкретного свойства
            //.Exclude(x => x.Age);

            var s1 = printer.PrintToString(person);
            Console.WriteLine(s1);
            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            //s1 = person.PrintToString();
            //8. ...с конфигурированием
            //s1 = person.PrintToString(x => x.Exclude<int>().ForType<string>().Using(y => y.Trim()));
        }
    }
}