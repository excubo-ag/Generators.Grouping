using Excubo.Generators.Grouping;
using System;

namespace SimpleGroup
{
    public partial class Container
    {
        public partial struct GGroup
        {
        }
        [Group(typeof(GGroup))] public void Foo() { throw new NotImplementedException(); }
        [Group(typeof(GGroup))] public (T2, T1) Bar<T1, T2>(T1 t1, string tmp) where T1 : class { throw new NotImplementedException(); }
    }
    public class Consumption
    {
        public void Consume()
        {
            var container = new Container();
            container.Group.Foo();
            container.Group.Bar<object, object>(new object(), null);
        }
    }
}