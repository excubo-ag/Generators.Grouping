using Excubo.Generators.Grouping;
using System;

namespace Region
{
    public partial class Container
    {
        #region MyGroup
        public partial struct GGroup { }
        [Group(typeof(GGroup))] public void Foo() { }
        [Group(typeof(GGroup))] public (T2, T1) Bar<T1, T2>(T1 t1, string tmp) where T1 : class { throw new Exception(""); }
        #endregion
    }
    public class Consumption
    {
        public void Consume()
        {
            var container = new Container();
            container.Group.Foo();
            container.Group.Bar<object, object>(null, null);
        }
    }
}