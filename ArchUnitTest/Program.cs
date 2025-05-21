using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;

using static ArchUnitNET.Fluent.ArchRuleDefinition;
using ArchUnitNET.Fluent.Slices;
using ArchUnitNET.Fluent.PlantUml;
using ArchUnitNET.Domain.Extensions;

namespace ExampleTest
{
    public class ExampleArchUnitTest
    {
        private static readonly Architecture Architecture = new ArchLoader().LoadAssemblies(
            System.Reflection.Assembly.Load("AufgabenService.API")
        ).Build();

        private readonly IObjectProvider<IType> AufgabenLayer =
            Types().That().ResideInAssembly("AufgabenService.API").As("Aufgaben Layer");

        public void ExampleClassesShouldNotCallForbiddenMethods()
        {
            Classes().That().Are(AufgabenLayer).Should().ResideInNamespace("AufgabenService.API")
                .Check(Architecture);
        }

        static void Main(string[] args)
        {
            var test = new ExampleArchUnitTest();

            Console.WriteLine("Loaded Classes: " + ExampleArchUnitTest.Architecture.Classes.Count());

            Architecture.Classes.ForEach(c =>
            {
                Console.WriteLine(c.FullName);
            });

            Console.WriteLine(PlantUmlDefinition.ComponentDiagram().WithDependenciesFromTypes(Architecture.Types).AsString());

            test.ExampleClassesShouldNotCallForbiddenMethods();
        }
    }
}