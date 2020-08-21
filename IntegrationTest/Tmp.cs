using Excubo.Generators.Grouping;
using System;

namespace IntegrationTest.Interface
{
    public partial interface IContainer
    {
        public partial interface _IGroup { }
        [Group(typeof(_IGroup))] public void Foo();
        [Group(typeof(_IGroup))] public (T2, T1) Bar<T1, T2>(T1 t1, string tmp) where T1 : class;
    }
    public partial class Container : IContainer
    {
        public partial struct _Group : IContainer._IGroup { }
        [Group(typeof(_Group))] public void Foo() { }
        [Group(typeof(_Group))] public (T2, T1) Bar<T1, T2>(T1 t1, string tmp) where T1 : class { throw new Exception(""); }
    }
}
