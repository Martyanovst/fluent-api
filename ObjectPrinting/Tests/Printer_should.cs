using System;
using System.Globalization;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    class Printer_should
    {
        private PrintingConfig<Person> printer;
        private Person person;
        [SetUp]
        public void SetUp()
        {
            printer = ObjectPrinter.For<Person>();
            person = new Person { Age = 10, Height = 200.5, Id = new Guid(), Name = "Valera" };
        }

        [Test]
        public void ExcludeType()
        {
            printer = printer.Exclude<Guid>();

            var expected = "Person\r\n\tName = Valera\r\n\tHeight = 200,5\r\n\tAge = 10\r\n";
            printer.PrintToString(person).Should().Be(expected);
        }

        [Test]
        public void ExcludeManyTypes()
        {
            printer = printer.Exclude<string>()
                             .Exclude<double>()
                             .Exclude<Guid>();
            var expected = "Person\r\n\tAge = 10\r\n";
            printer.PrintToString(person).Should().Be(expected);
        }

        [Test]
        public void Print_WhenExcludingObjectType()
        {
            printer.Exclude<Person>();
            var expected = "Person\r\n\tId = Guid\r\n\tName = Valera\r\n\tHeight = 200,5\r\n\tAge = 10\r\n";
            printer.PrintToString(person).Should().Be(expected);
        }

        [Test]
        public void UseSpecialSerializersForTypes()
        {
            printer = printer.ForType<string>().Using(x => x.ToUpper());

            var expected = "Person\r\n\tId = Guid\r\n\tName = VALERA\r\n\tHeight = 200,5\r\n\tAge = 10\r\n";
            printer.PrintToString(person).Should().Be(expected);
        }

        [Test]
        public void UseCustomSerializationForObjectType()
        {
            printer = printer.ForType<Person>().Using(x => x.Name + x.Age + x.Height);

            var expected = "Valera10200,5";
            printer.PrintToString(person).Should().Be(expected);
        }

        [Test]
        public void UseSpecialSerializersForProperties()
        {
            printer = printer.ForProperty(x => x.Age).Using(x => (x * 10).ToString());

            var expected = "Person\r\n\tId = Guid\r\n\tName = Valera\r\n\tHeight = 200,5\r\n\tAge = 100\r\n";
            printer.PrintToString(person).Should().Be(expected);
        }

        [Test]
        public void UseSpecialSerializersForManyProperties()
        {
            printer = printer.ForProperty(x => x.Age).Using(a => (a / 5).ToString())
                             .ForProperty(x => x.Name).Using(x => x.Replace("a", ""));

            var expected = "Person\r\n\tId = Guid\r\n\tName = Vler\r\n\tHeight = 200,5\r\n\tAge = 2\r\n";
            printer.PrintToString(person).Should().Be(expected);
        }

        [Test]
        public void UseTrimmerForStringProperties()
        {
            printer = printer.ForProperty(x => x.Name).CutTo(2);

            var expected = "Person\r\n\tId = Guid\r\n\tName = lera\r\n\tHeight = 200,5\r\n\tAge = 10\r\n";
            printer.PrintToString(person).Should().Be(expected);
        }

        [Test]
        public void ThrowArgumentException_WhenUseTrimWithNegativeNumber()
        {
            var exception = Assert.Throws<ArgumentException>(() => printer.ForProperty(x => x.Name).CutTo(-2));
            exception.Message.Should().Be("Count should be a positive number");
        }

        [Test]
        public void DoesntThrow_WhenUseTrimWithZero()
        {
            Assert.DoesNotThrow(() => printer.ForProperty(x => x.Name).CutTo(0));
            printer.PrintToString(person);
        }

        [Test]
        public void ThrowException_WhenCountIsMoreThanPropertyValueLength()
        {
            printer = printer.ForProperty(x => x.Name).CutTo(10);
            Assert.Throws<TargetInvocationException>(() => printer.PrintToString(person));
        }

        [Test]
        public void ExcludeProperties()
        {
            printer = printer.Exclude(x => x.Height);

            var expected = "Person\r\n\tId = Guid\r\n\tName = Valera\r\n\tAge = 10\r\n";
            printer.PrintToString(person).Should().Be(expected);
        }

        [Test]
        public void ExcludeManyProperties()
        {
            printer = printer.Exclude(x => x.Height).Exclude(x => x.Id);

            var expected = "Person\r\n\tName = Valera\r\n\tAge = 10\r\n";
            printer.PrintToString(person).Should().Be(expected);
        }

        [Test]
        public void PrintWithCustomSettingsOfSerialization()
        {
            printer = printer.Exclude<Guid>()
                .ForProperty(x => x.Height).Using(x => (x-0.5).ToString())
                .ForType<double>().Using(CultureInfo.InvariantCulture)
                .ForProperty(x=>x.Name).CutTo(3)
                .ForProperty(x=>x.Name).Using(x=>x.ToUpper());

            var expected = "Person\r\n\tName = ERA\r\n\tHeight = 200\r\n\tAge = 10\r\n";
            printer.PrintToString(person).Should().Be(expected);
        }

        [Test]
        public void ExtendObjectToDefaultPrinting()
        {
            var actual = person.PrintToString();
            var expected = "Person\r\n\tId = Guid\r\n\tName = Valera\r\n\tHeight = 200,5\r\n\tAge = 10\r\n";
            actual.Should().Be(expected);
        }

        [Test]
        public void ExtendObjectToCustomPrinting()
        {
            var actual = person.PrintToString(x => x.Exclude<Guid>()
                               .ForProperty(p => p.Name)
                               .Using(n => n.ToLower()));
            var expected = "Person\r\n\tName = valera\r\n\tHeight = 200,5\r\n\tAge = 10\r\n";
            actual.Should().Be(expected);
        }

        [Test]
        public void UseSpecificCultureWithNumberTypes()
        {
            printer = printer.ForType<double>().Using(CultureInfo.GetCultureInfo("eu-ES"));

            var expected = "Person\r\n\tId = Guid\r\n\tName = Valera\r\n\tHeight = 200,5\r\n\tAge = 10\r\n";
            printer.PrintToString(person).Should().Be(expected);
        }
    }
}
