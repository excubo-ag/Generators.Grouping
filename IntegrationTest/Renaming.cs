using Excubo.Generators.Grouping;
using System;

namespace Renaming
{
    public partial class Container
    {
        public partial struct GGroup
        {
        }
        [Group(typeof(GGroup), "Frobulate")] public void Foo() { throw new NotImplementedException(); }
        [Group(typeof(GGroup), "Bamboozle")] public (T2, T1) Bar<T1, T2>(T1 t1, string tmp) where T1 : class { throw new NotImplementedException(); }
    }
    public class Consumption
    {
        public void Consume()
        {
            var container = new Container();
            container.Group.Frobulate();
            container.Group.Bamboozle<object, object>(null, null);
        }
    }
}