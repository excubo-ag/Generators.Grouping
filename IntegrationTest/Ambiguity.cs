using Excubo.Generators.Grouping;
using System;

namespace Ambiguity
{
    public partial class Container1
    {
        public partial struct GGroup
        {
        }
        [Group(typeof(GGroup))] public void Foo() { throw new NotImplementedException(); }
    }
    public partial class Container2
    {
        public partial struct GGroup
        {
        }
        [Group(typeof(GGroup))] public void Foo() { throw new NotImplementedException(); }
    }

    public class Consumption
    {
        public void Consume()
        {
            var container1 = new Container1();
            container1.Group.Foo();
            var container2 = new Container2();
            container2.Group.Foo();
        }
    }
}