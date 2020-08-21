using Excubo.Generators.Grouping;
using System;

namespace IntegrationTest
{
    public partial class Class1
    {
        public partial struct GGroup1
        {
            public partial struct GSubGroup { }
        }
        [Group(typeof(GGroup1))] public void Foo() { Console.WriteLine(nameof(Foo)); }
        [Group(typeof(GGroup1.GSubGroup))] public void NestedFoo() { Console.WriteLine(nameof(NestedFoo)); }
        public void Consumption()
        {
            Foo();
            Group1.Foo();
            NestedFoo();
            Group1.SubGroup.NestedFoo();
        }
    }
}
