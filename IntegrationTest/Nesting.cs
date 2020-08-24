using Excubo.Generators.Grouping;
using System;

namespace Nesting
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
        [Group(typeof(GOuter))] public void Bar() { throw new NotImplementedException(); }
    }
    public class Consumption
    {
        public void Consume()
        {
            var container = new Container();
            container.Outer.Inner.Foo();
            container.Outer.Bar();
        }
    }
}