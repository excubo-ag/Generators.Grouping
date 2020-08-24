using Excubo.Generators.Grouping;
using System;

namespace MultipleGroups
{
    public partial class Container
    {
        public partial struct GGroup1 { }
        public partial struct GGroup2 { }
        [Group(typeof(GGroup1))] [Group(typeof(GGroup2))] public void Foo() { throw new NotImplementedException(); }
        [Group(typeof(GGroup1))] [Group(typeof(GGroup2))] public (T2, T1) Bar<T1, T2>(T1 t1, string tmp) where T1 : class { throw new NotImplementedException(); }
    }
    public class Consumption
    {
        public void Consume()
        {
            var container = new Container();
            container.Group1.Foo();
            container.Group1.Bar<object, object>(new object(), null);
            container.Group2.Foo();
            container.Group2.Bar<object, object>(new object(), null);
        }
    }
}