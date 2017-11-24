using System;
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
            var expected = "Person\r\n\tName = Valera\r\n\tHeight = 200,5\r\n\tAge = 10\r\n";
            printer = printer.Exclude<Guid>();
            printer.PrintToString(person).Should().Be(expected);
        }
    }
}
