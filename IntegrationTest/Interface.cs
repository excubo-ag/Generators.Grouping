using Excubo.Generators.Grouping;
using System;

namespace Interface
{
    public partial interface IContainer
    {
        public partial interface IGGroup { }
        [Group(typeof(IGGroup))] public void Foo();
        [Group(typeof(IGGroup))] public (T2, T1) Bar<T1, T2>(T1 t1, string tmp) where T1 : class;
    }
    public partial class Container : IContainer
    {
        public partial struct GGroup : IContainer.IGGroup { }
        [Group(typeof(GGroup))] public void Foo() { }
        [Group(typeof(GGroup))] public (T2, T1) Bar<T1, T2>(T1 t1, string tmp) where T1 : class { throw new Exception(""); }
    }
    public class Consumption
    {
        public void Consume()
        {
            var container = new Container();
            container.Group.Foo();
            container.Group.Bar<object, object>(null, null);
            var icontainer = container as IContainer;
            icontainer.Group.Foo();
            icontainer.Group.Bar<object, object>(null, null);
        }
    }
}