using Excubo.Generators.Grouping;
using System;

namespace NestingOnlyOneMethodInInner
{
    public partial class Container
    {
        public partial struct GOuter
        {
            public partial struct GInner
            {
            }
        }
        [Group(typeof(GOuter.GInner))] public void Foo() { throw new NotImplementedException(); }
    }
    public class Consumption
    {
        public void Consume()
        {
            var container = new Container();
            container.Outer.Inner.Foo();
        }
    }
}