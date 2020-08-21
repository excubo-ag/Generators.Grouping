using Excubo.Generators.Grouping;
using System;

namespace IntegrationTest
{
    public partial class Class1
    {
        public partial struct _Group1
        {
            public partial struct _SubGroup { }
        }
        [Group(typeof(_Group1))] public void Foo() { Console.WriteLine(nameof(Foo)); }
        [Group(typeof(_Group1._SubGroup))] public void NestedFoo() { Console.WriteLine(nameof(NestedFoo)); }
        public void Consumption()
        {
            Foo();
            Group1.Foo();
            NestedFoo();
            Group1.SubGroup.NestedFoo();
        }
    }
}
