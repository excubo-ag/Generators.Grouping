using Excubo.Generators.Grouping;
using FluentAssertions;
using System.Linq;
using Tests_ApiGroupGenerator.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Tests_ApiGroupGenerator
{
    public class GeneratorTests : TestBase<GroupingGenerator>
    {
        public GeneratorTests(ITestOutputHelper output_helper) : base(output_helper)
        {
        }

        [Fact]
        public void Empty()
        {
            var userSource = "";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            Assert.Single(generated);
            generated.ContainsFileWithContent("GroupAttribute.cs", @"
#nullable enable
using System;
namespace Excubo.Generators.Grouping
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    sealed class GroupAttribute : Attribute
    {
        public GroupAttribute(Type group_type, string? method_name = null)
        {
            GroupType = group_type;
            MethodName = method_name;
        }
        public Type GroupType { get; set; }
        public string? MethodName { get; set; }
    }
}
#nullable restore
");
        }

        [Fact]
        public void SimpleGroup()
        {
            var userSource = @"
using Excubo.Generators.Grouping;
using System;

namespace USER
{
    public partial class Container
    {
        public partial struct _Group
        {
        }
        [Group(typeof(_Group))] public void Foo() { throw new NotImplementedException(); }
        [Group(typeof(_Group))] public (T2, T1) Bar<T1, T2>(T1 t1, string tmp) where T1 : class { throw new NotImplementedException(); }
    }
}
";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            Assert.Equal(4, generated.Length);
            generated.ContainsFileWithContent("GroupAttribute.cs", @"
#nullable enable
using System;
namespace Excubo.Generators.Grouping
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    sealed class GroupAttribute : Attribute
    {
        public GroupAttribute(Type group_type, string? method_name = null)
        {
            GroupType = group_type;
            MethodName = method_name;
        }
        public Type GroupType { get; set; }
        public string? MethodName { get; set; }
    }
}
#nullable restore
");
            generated.ContainsFileWithContent("group_USER.Container._Group.cs", @"
namespace USER
{
    public partial class Container
    {
        public partial struct _Group
        {
            private Container group_internal__parent;
            public _Group(Container parent) { this.group_internal__parent = parent; }
        }
        public _Group Group => new _Group(this);
    }
}
");
            generated.ContainsFileWithContent("group_USER.Container._Group_USER.Container.Foo().cs", @"
namespace USER
{
    public partial class Container
    {
        public partial struct _Group
        {
            public void Foo()
                => group_internal__parent.Foo();
        }
    }
}
");
            generated.ContainsFileWithContent("group_USER.Container._Group_USER.Container.Bar_T1, T2_(T1, string).cs", @"
namespace USER
{
    public partial class Container
    {
        public partial struct _Group
        {
            public (T2, T1) Bar<T1, T2>(T1 t1, string tmp) where T1 : class
                => group_internal__parent.Bar<T1, T2>(t1, tmp);
        }
    }
}
");
        }
        [Fact]
        public void Interface()
        {
            var userSource = @"
using Excubo.Generators.Grouping;
using System;

namespace USER
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
";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            Assert.Equal(7, generated.Length);
            generated.ContainsFileWithContent("GroupAttribute.cs", @"
#nullable enable
using System;
namespace Excubo.Generators.Grouping
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    sealed class GroupAttribute : Attribute
    {
        public GroupAttribute(Type group_type, string? method_name = null)
        {
            GroupType = group_type;
            MethodName = method_name;
        }
        public Type GroupType { get; set; }
        public string? MethodName { get; set; }
    }
}
#nullable restore
");
            generated.ContainsFileWithContent("group_USER.IContainer._IGroup.cs", @"
namespace USER
{
    public partial interface IContainer
    {
        public partial interface _IGroup
        {
        }
        _IGroup Group { get; }
    }
}
");
            generated.ContainsFileWithContent("group_USER.IContainer._IGroup_USER.IContainer.Foo().cs", @"
namespace USER
{
    public partial interface IContainer
    {
        public partial interface _IGroup
        {
            public void Foo()
                ;
        }
    }
}
");
            generated.ContainsFileWithContent("group_USER.IContainer._IGroup_USER.IContainer.Bar_T1, T2_(T1, string).cs", @"
namespace USER
{
    public partial interface IContainer
    {
        public partial interface _IGroup
        {
            public (T2, T1) Bar<T1, T2>(T1 t1, string tmp) where T1 : class
                ;
        }
    }
}
");
            generated.ContainsFileWithContent("group_USER.Container._Group.cs", @"
namespace USER
{
    public partial class Container
    {
        public partial struct _Group
        {
            private Container group_internal__parent;
            public _Group(Container parent) { this.group_internal__parent = parent; }
        }
        public _Group Group => new _Group(this);
        IContainer._IGroup IContainer.Group => Group;
    }
}
");
            generated.ContainsFileWithContent("group_USER.Container._Group_USER.Container.Foo().cs", @"
namespace USER
{
    public partial class Container
    {
        public partial struct _Group
        {
            public void Foo()
                => group_internal__parent.Foo();
        }
    }
}
");
            generated.ContainsFileWithContent("group_USER.Container._Group_USER.Container.Bar_T1, T2_(T1, string).cs", @"
namespace USER
{
    public partial class Container
    {
        public partial struct _Group
        {
            public (T2, T1) Bar<T1, T2>(T1 t1, string tmp) where T1 : class
                => group_internal__parent.Bar<T1, T2>(t1, tmp);
        }
    }
}
");
        }

        [Fact]
        public void Ambiguity()
        {
            var userSource = @"
using Excubo.Generators.Grouping;
using System;

namespace USER
{
    public partial class Container1
    {
        public partial struct _Group
        {
        }
        [Group(typeof(_Group))] public void Foo() { throw new NotImplementedException(); }
    }
    public partial class Container2
    {
        public partial struct _Group
        {
        }
        [Group(typeof(_Group))] public void Foo() { throw new NotImplementedException(); }
    }
}
";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            Assert.Equal(5, generated.Length);
            generated.ContainsFileWithContent("GroupAttribute.cs", @"
#nullable enable
using System;
namespace Excubo.Generators.Grouping
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    sealed class GroupAttribute : Attribute
    {
        public GroupAttribute(Type group_type, string? method_name = null)
        {
            GroupType = group_type;
            MethodName = method_name;
        }
        public Type GroupType { get; set; }
        public string? MethodName { get; set; }
    }
}
#nullable restore
");
            generated.ContainsFileWithContent("group_USER.Container1._Group.cs", @"
namespace USER
{
    public partial class Container1
    {
        public partial struct _Group
        {
            private Container1 group_internal__parent;
            public _Group(Container1 parent) { this.group_internal__parent = parent; }
        }
        public _Group Group => new _Group(this);
    }
}
");
            generated.ContainsFileWithContent("group_USER.Container1._Group_USER.Container1.Foo().cs", @"
namespace USER
{
    public partial class Container1
    {
        public partial struct _Group
        {
            public void Foo()
                => group_internal__parent.Foo();
        }
    }
}
");
            generated.ContainsFileWithContent("group_USER.Container2._Group.cs", @"
namespace USER
{
    public partial class Container2
    {
        public partial struct _Group
        {
            private Container2 group_internal__parent;
            public _Group(Container2 parent) { this.group_internal__parent = parent; }
        }
        public _Group Group => new _Group(this);
    }
}
");
            generated.ContainsFileWithContent("group_USER.Container2._Group_USER.Container2.Foo().cs", @"
namespace USER
{
    public partial class Container2
    {
        public partial struct _Group
        {
            public void Foo()
                => group_internal__parent.Foo();
        }
    }
}
");
        }

        [Fact]
        public void GenericContainer()
        {
            var userSource = @"
using Excubo.Generators.Grouping;
using System;

namespace USER
{
    public partial class Container<T>
    {
        public partial struct _Group
        {
        }
        [Group(typeof(_Group))] public void Foo() { throw new NotImplementedException(); }
        [Group(typeof(_Group))] public (T, T1) Bar<T1>(T1 t1, string tmp) where T1 : class { throw new NotImplementedException(); }
    }
}
";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            Assert.Equal(4, generated.Length);
            generated.ContainsFileWithContent("GroupAttribute.cs", @"
#nullable enable
using System;
namespace Excubo.Generators.Grouping
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    sealed class GroupAttribute : Attribute
    {
        public GroupAttribute(Type group_type, string? method_name = null)
        {
            GroupType = group_type;
            MethodName = method_name;
        }
        public Type GroupType { get; set; }
        public string? MethodName { get; set; }
    }
}
#nullable restore
");
            generated.ContainsFileWithContent("group_USER.Container_T_._Group.cs", @"
namespace USER
{
    public partial class Container<T>
    {
        public partial struct _Group
        {
            private Container<T> group_internal__parent;
            public _Group(Container<T> parent) { this.group_internal__parent = parent; }
        }
        public _Group Group => new _Group(this);
    }
}
");
            generated.ContainsFileWithContent("group_USER.Container_T_._Group_USER.Container_T_.Foo().cs", @"
namespace USER
{
    public partial class Container<T>
    {
        public partial struct _Group
        {
            public void Foo()
                => group_internal__parent.Foo();
        }
    }
}
");
            generated.ContainsFileWithContent("group_USER.Container_T_._Group_USER.Container_T_.Bar_T1_(T1, string).cs", @"
namespace USER
{
    public partial class Container<T>
    {
        public partial struct _Group
        {
            public (T, T1) Bar<T1>(T1 t1, string tmp) where T1 : class
                => group_internal__parent.Bar<T1>(t1, tmp);
        }
    }
}
");
        }

        [Fact]
        public void SimpleGroupAttribute()
        {
            var userSource = @"
using Excubo.Generators.Grouping;
using System;

namespace USER
{
    public partial class Container
    {
        public partial struct _Group
        {
        }
        [GroupAttribute(typeof(_Group))] public void Foo() { throw new NotImplementedException(); }
        [GroupAttribute(typeof(_Group))] public (T2, T1) Bar<T1, T2>(T1 t1, string tmp) where T1 : class { throw new NotImplementedException(); }
    }
}
";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            Assert.Equal(4, generated.Length);
            generated.ContainsFileWithContent("GroupAttribute.cs", @"
#nullable enable
using System;
namespace Excubo.Generators.Grouping
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    sealed class GroupAttribute : Attribute
    {
        public GroupAttribute(Type group_type, string? method_name = null)
        {
            GroupType = group_type;
            MethodName = method_name;
        }
        public Type GroupType { get; set; }
        public string? MethodName { get; set; }
    }
}
#nullable restore
");
            generated.ContainsFileWithContent("group_USER.Container._Group.cs", @"
namespace USER
{
    public partial class Container
    {
        public partial struct _Group
        {
            private Container group_internal__parent;
            public _Group(Container parent) { this.group_internal__parent = parent; }
        }
        public _Group Group => new _Group(this);
    }
}
");
            generated.ContainsFileWithContent("group_USER.Container._Group_USER.Container.Foo().cs", @"
namespace USER
{
    public partial class Container
    {
        public partial struct _Group
        {
            public void Foo()
                => group_internal__parent.Foo();
        }
    }
}
");
            generated.ContainsFileWithContent("group_USER.Container._Group_USER.Container.Bar_T1, T2_(T1, string).cs", @"
namespace USER
{
    public partial class Container
    {
        public partial struct _Group
        {
            public (T2, T1) Bar<T1, T2>(T1 t1, string tmp) where T1 : class
                => group_internal__parent.Bar<T1, T2>(t1, tmp);
        }
    }
}
");
        }

        [Fact]
        public void SimpleGroupAttributeFullyQualified()
        {
            var userSource = @"
using System;

namespace USER
{
    public partial class Container
    {
        public partial struct _Group
        {
        }
        [Excubo.Generators.Grouping.GroupAttribute(typeof(_Group))] public void Foo() { throw new NotImplementedException(); }
        [Excubo.Generators.Grouping.GroupAttribute(typeof(_Group))] public (T2, T1) Bar<T1, T2>(T1 t1, string tmp) where T1 : class { throw new NotImplementedException(); }
    }
}
";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            Assert.Equal(4, generated.Length);
            generated.ContainsFileWithContent("GroupAttribute.cs", @"
#nullable enable
using System;
namespace Excubo.Generators.Grouping
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    sealed class GroupAttribute : Attribute
    {
        public GroupAttribute(Type group_type, string? method_name = null)
        {
            GroupType = group_type;
            MethodName = method_name;
        }
        public Type GroupType { get; set; }
        public string? MethodName { get; set; }
    }
}
#nullable restore
");
            generated.ContainsFileWithContent("group_USER.Container._Group.cs", @"
namespace USER
{
    public partial class Container
    {
        public partial struct _Group
        {
            private Container group_internal__parent;
            public _Group(Container parent) { this.group_internal__parent = parent; }
        }
        public _Group Group => new _Group(this);
    }
}
");
            generated.ContainsFileWithContent("group_USER.Container._Group_USER.Container.Foo().cs", @"
namespace USER
{
    public partial class Container
    {
        public partial struct _Group
        {
            public void Foo()
                => group_internal__parent.Foo();
        }
    }
}
");
            generated.ContainsFileWithContent("group_USER.Container._Group_USER.Container.Bar_T1, T2_(T1, string).cs", @"
namespace USER
{
    public partial class Container
    {
        public partial struct _Group
        {
            public (T2, T1) Bar<T1, T2>(T1 t1, string tmp) where T1 : class
                => group_internal__parent.Bar<T1, T2>(t1, tmp);
        }
    }
}
");
        }

        [Fact]
        public void MultipleGroups()
        {
            var userSource = @"
using Excubo.Generators.Grouping;
using System;

namespace USER
{
    public partial class Container
    {
        public partial struct _Group1 {}
        public partial struct _Group2 {}
        [Group(typeof(_Group1))] [Group(typeof(_Group2))] public void Foo() { throw new NotImplementedException(); }
        [Group(typeof(_Group1))] [Group(typeof(_Group2))] public (T2, T1) Bar<T1, T2>(T1 t1, string tmp) where T1 : class { throw new NotImplementedException(); }
    }
}
";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            Assert.Equal(7, generated.Length);
            generated.ContainsFileWithContent("GroupAttribute.cs", @"
#nullable enable
using System;
namespace Excubo.Generators.Grouping
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    sealed class GroupAttribute : Attribute
    {
        public GroupAttribute(Type group_type, string? method_name = null)
        {
            GroupType = group_type;
            MethodName = method_name;
        }
        public Type GroupType { get; set; }
        public string? MethodName { get; set; }
    }
}
#nullable restore
");
            generated.ContainsFileWithContent("group_USER.Container._Group1.cs", @"
namespace USER
{
    public partial class Container
    {
        public partial struct _Group1
        {
            private Container group_internal__parent;
            public _Group1(Container parent) { this.group_internal__parent = parent; }
        }
        public _Group1 Group1 => new _Group1(this);
    }
}
");
            generated.ContainsFileWithContent("group_USER.Container._Group2.cs", @"
namespace USER
{
    public partial class Container
    {
        public partial struct _Group2
        {
            private Container group_internal__parent;
            public _Group2(Container parent) { this.group_internal__parent = parent; }
        }
        public _Group2 Group2 => new _Group2(this);
    }
}
");
            generated.ContainsFileWithContent("group_USER.Container._Group1_USER.Container.Foo().cs", @"
namespace USER
{
    public partial class Container
    {
        public partial struct _Group1
        {
            public void Foo()
                => group_internal__parent.Foo();
        }
    }
}
");
            generated.ContainsFileWithContent("group_USER.Container._Group1_USER.Container.Bar_T1, T2_(T1, string).cs", @"
namespace USER
{
    public partial class Container
    {
        public partial struct _Group1
        {
            public (T2, T1) Bar<T1, T2>(T1 t1, string tmp) where T1 : class
                => group_internal__parent.Bar<T1, T2>(t1, tmp);
        }
    }
}
");
            generated.ContainsFileWithContent("group_USER.Container._Group2_USER.Container.Foo().cs", @"
namespace USER
{
    public partial class Container
    {
        public partial struct _Group2
        {
            public void Foo()
                => group_internal__parent.Foo();
        }
    }
}
");
            generated.ContainsFileWithContent("group_USER.Container._Group2_USER.Container.Bar_T1, T2_(T1, string).cs", @"
namespace USER
{
    public partial class Container
    {
        public partial struct _Group2
        {
            public (T2, T1) Bar<T1, T2>(T1 t1, string tmp) where T1 : class
                => group_internal__parent.Bar<T1, T2>(t1, tmp);
        }
    }
}
");
        }

        [Fact]
        public void Renaming()
        {
            var userSource = @"
using Excubo.Generators.Grouping;
using System;

namespace USER
{
    public partial class Container
    {
        public partial struct _Group
        {
        }
        [Group(typeof(_Group), ""Frobulate"")] public void Foo() { throw new NotImplementedException(); }
        [Group(typeof(_Group), ""Bamboozle"")] public (T2, T1) Bar<T1, T2>(T1 t1, string tmp) where T1 : class { throw new NotImplementedException(); }
    }
}
";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            Assert.Equal(4, generated.Length);
            generated.ContainsFileWithContent("GroupAttribute.cs", @"
#nullable enable
using System;
namespace Excubo.Generators.Grouping
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    sealed class GroupAttribute : Attribute
    {
        public GroupAttribute(Type group_type, string? method_name = null)
        {
            GroupType = group_type;
            MethodName = method_name;
        }
        public Type GroupType { get; set; }
        public string? MethodName { get; set; }
    }
}
#nullable restore
");
            generated.ContainsFileWithContent("group_USER.Container._Group.cs", @"
namespace USER
{
    public partial class Container
    {
        public partial struct _Group
        {
            private Container group_internal__parent;
            public _Group(Container parent) { this.group_internal__parent = parent; }
        }
        public _Group Group => new _Group(this);
    }
}
");
            generated.ContainsFileWithContent("group_USER.Container._Group_USER.Container.Foo().cs", @"
namespace USER
{
    public partial class Container
    {
        public partial struct _Group
        {
            public void Frobulate()
                => group_internal__parent.Foo();
        }
    }
}
");
            generated.ContainsFileWithContent("group_USER.Container._Group_USER.Container.Bar_T1, T2_(T1, string).cs", @"
namespace USER
{
    public partial class Container
    {
        public partial struct _Group
        {
            public (T2, T1) Bamboozle<T1, T2>(T1 t1, string tmp) where T1 : class
                => group_internal__parent.Bar<T1, T2>(t1, tmp);
        }
    }
}
");
        }

        [Fact]
        public void Nesting()
        {
            var userSource = @"
using Excubo.Generators.Grouping;
using System;

namespace USER
{
    public partial class Container
    {
        public partial struct _Outer
        {
            public partial struct _Inner
            {
            }
        }
        [Group(typeof(_Outer._Inner))] public void Foo() { throw new NotImplementedException(); }
        [Group(typeof(_Outer))] public void Bar() { throw new NotImplementedException(); }
    }
}
";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            Assert.Equal(5, generated.Length);
            generated.ContainsFileWithContent("GroupAttribute.cs", @"
#nullable enable
using System;
namespace Excubo.Generators.Grouping
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    sealed class GroupAttribute : Attribute
    {
        public GroupAttribute(Type group_type, string? method_name = null)
        {
            GroupType = group_type;
            MethodName = method_name;
        }
        public Type GroupType { get; set; }
        public string? MethodName { get; set; }
    }
}
#nullable restore
");
            generated.ContainsFileWithContent("group_USER.Container._Outer.cs", @"
namespace USER
{
    public partial class Container
    {
        public partial struct _Outer
        {
            private Container group_internal__parent;
            public _Outer(Container parent) { this.group_internal__parent = parent; }
        }
        public _Outer Outer => new _Outer(this);
    }
}
");
            generated.ContainsFileWithContent("group_USER.Container._Outer._Inner.cs", @"
namespace USER
{
    public partial class Container
    {
        public partial struct _Outer
        {
            public partial struct _Inner
            {
                private Container group_internal__parent;
                public _Inner(Container parent) { this.group_internal__parent = parent; }
            }
            public _Inner Inner => new _Inner(this.group_internal__parent);
        }
    }
}
");
            generated.ContainsFileWithContent("group_USER.Container._Outer._Inner_USER.Container.Foo().cs", @"
namespace USER
{
    public partial class Container
    {
        public partial struct _Outer
        {
            public partial struct _Inner
            {
                public void Foo()
                    => group_internal__parent.Foo();
            }
        }
    }
}
");
            generated.ContainsFileWithContent("group_USER.Container._Outer_USER.Container.Bar().cs", @"
namespace USER
{
    public partial class Container
    {
        public partial struct _Outer
        {
            public void Bar()
                => group_internal__parent.Bar();
        }
    }
}
");
        }

        [Fact]
        public void NestingOnlyOneMethodInInner()
        {
            var userSource = @"
using Excubo.Generators.Grouping;
using System;

namespace USER
{
    public partial class Container
    {
        public partial struct _Outer
        {
            public partial struct _Inner
            {
            }
        }
        [Group(typeof(_Outer._Inner))] public void Foo() { throw new NotImplementedException(); }
    }
}
";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            Assert.Equal(4, generated.Length);
            generated.ContainsFileWithContent("GroupAttribute.cs", @"
#nullable enable
using System;
namespace Excubo.Generators.Grouping
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    sealed class GroupAttribute : Attribute
    {
        public GroupAttribute(Type group_type, string? method_name = null)
        {
            GroupType = group_type;
            MethodName = method_name;
        }
        public Type GroupType { get; set; }
        public string? MethodName { get; set; }
    }
}
#nullable restore
");
            generated.ContainsFileWithContent("group_USER.Container._Outer.cs", @"
namespace USER
{
    public partial class Container
    {
        public partial struct _Outer
        {
            private Container group_internal__parent;
            public _Outer(Container parent) { this.group_internal__parent = parent; }
        }
        public _Outer Outer => new _Outer(this);
    }
}
");
            generated.ContainsFileWithContent("group_USER.Container._Outer._Inner.cs", @"
namespace USER
{
    public partial class Container
    {
        public partial struct _Outer
        {
            public partial struct _Inner
            {
                private Container group_internal__parent;
                public _Inner(Container parent) { this.group_internal__parent = parent; }
            }
            public _Inner Inner => new _Inner(this.group_internal__parent);
        }
    }
}
");
            generated.ContainsFileWithContent("group_USER.Container._Outer._Inner_USER.Container.Foo().cs", @"
namespace USER
{
    public partial class Container
    {
        public partial struct _Outer
        {
            public partial struct _Inner
            {
                public void Foo()
                    => group_internal__parent.Foo();
            }
        }
    }
}
");
        }

        [Fact]
        public void Comments()
        {
            var userSource = @"
using Excubo.Generators.Grouping;
using System;

namespace USER
{
    public partial class Container
    {
        /// <summary>
        /// Comment on <see cref=""Outer""/>
        /// </summary>
        public partial struct _Outer
        {
            /// <summary>
            /// Comment on <see cref=""_Inner""/>
            /// </summary>
            // another comment
            /* seriously: three kinds of comments */
            public partial struct _Inner
            {
            }
        }
        [Group(typeof(_Outer._Inner))] public void Foo() { throw new NotImplementedException(); }
    }
}
";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            Assert.Equal(4, generated.Length);
            generated.ContainsFileWithContent("GroupAttribute.cs", @"
#nullable enable
using System;
namespace Excubo.Generators.Grouping
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    sealed class GroupAttribute : Attribute
    {
        public GroupAttribute(Type group_type, string? method_name = null)
        {
            GroupType = group_type;
            MethodName = method_name;
        }
        public Type GroupType { get; set; }
        public string? MethodName { get; set; }
    }
}
#nullable restore
");
            generated.ContainsFileWithContent("group_USER.Container._Outer.cs", @"
namespace USER
{
    public partial class Container
    {
        public partial struct _Outer
        {
            private Container group_internal__parent;
            public _Outer(Container parent) { this.group_internal__parent = parent; }
        }
        /// <summary>
        /// Comment on <see cref=""Outer""/>
        /// </summary>
        public _Outer Outer => new _Outer(this);
    }
}
");
            generated.ContainsFileWithContent("group_USER.Container._Outer._Inner.cs", @"
namespace USER
{
    public partial class Container
    {
        public partial struct _Outer
        {
            public partial struct _Inner
            {
                private Container group_internal__parent;
                public _Inner(Container parent) { this.group_internal__parent = parent; }
            }
            /// <summary>
            /// Comment on <see cref=""_Inner""/>
            /// </summary>
            // another comment
            /* seriously: three kinds of comments */
            public _Inner Inner => new _Inner(this.group_internal__parent);
        }
    }
}
");
            generated.ContainsFileWithContent("group_USER.Container._Outer._Inner_USER.Container.Foo().cs", @"
namespace USER
{
    public partial class Container
    {
        public partial struct _Outer
        {
            public partial struct _Inner
            {
                public void Foo()
                    => group_internal__parent.Foo();
            }
        }
    }
}
");
        }
    }
}
