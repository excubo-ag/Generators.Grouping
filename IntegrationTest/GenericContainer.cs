using Excubo.Generators.Grouping;
using System;

namespace GenericContainer
{
    public partial class Container<T>
    {
        public partial struct GGroup { }
        // although the source generator does the right thing, the following would cause error CS0416.
        //[Group(typeof(GGroup))] public void Foo() { throw new NotImplementedException(); }
        //[Group(typeof(GGroup))] public (T, T1) Bar<T1>(T1 t1, string tmp) where T1 : class { throw new NotImplementedException(); }
    }

    public class Consumption
    {
        public void Consume()
        {
            var container = new Container<object>();
            //container.Group.Foo();
            //container.Group.Bar<object>(null, null);
        }
    }
}