using Excubo.Generators.Grouping;
using FluentAssertions;
using System.Linq;
using Tests_APIGroupGenerator.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Tests_APIGroupGenerator
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
            Assert.Contains(generated, g => g.Filename.EndsWith("GroupAttribute.cs"));
            generated.First(g => g.Filename.EndsWith("GroupAttribute.cs")).Content.Should().BeIgnoringLineEndings(@"
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
            Assert.Contains(generated, g => g.Filename.EndsWith("GroupAttribute.cs"));
            Assert.Contains(generated, g => g.Filename.EndsWith("group__Group.cs"));
            Assert.Contains(generated, g => g.Filename.EndsWith("group__Group_Foo.cs"));
            Assert.Contains(generated, g => g.Filename.EndsWith("group__Group_Bar.cs"));
            generated.First(g => g.Filename.EndsWith("GroupAttribute.cs")).Content.Should().BeIgnoringLineEndings(@"
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
            generated.First(g => g.Filename.EndsWith("group__Group.cs")).Content.Should().BeIgnoringLineEndings(@"
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
            generated.First(g => g.Filename.EndsWith("group__Group_Foo.cs")).Content.Should().BeIgnoringLineEndings(@"
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
            generated.First(g => g.Filename.EndsWith("group__Group_Bar.cs")).Content.Should().BeIgnoringLineEndings(@"
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
            Assert.Contains(generated, g => g.Filename.EndsWith("GroupAttribute.cs"));
            Assert.Contains(generated, g => g.Filename.EndsWith("group__Group.cs"));
            Assert.Contains(generated, g => g.Filename.EndsWith("group__Group_Foo.cs"));
            Assert.Contains(generated, g => g.Filename.EndsWith("group__Group_Bar.cs"));
            generated.First(g => g.Filename.EndsWith("GroupAttribute.cs")).Content.Should().BeIgnoringLineEndings(@"
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
            generated.First(g => g.Filename.EndsWith("group__Group.cs")).Content.Should().BeIgnoringLineEndings(@"
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
            generated.First(g => g.Filename.EndsWith("group__Group_Foo.cs")).Content.Should().BeIgnoringLineEndings(@"
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
            generated.First(g => g.Filename.EndsWith("group__Group_Bar.cs")).Content.Should().BeIgnoringLineEndings(@"
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
            Assert.Contains(generated, g => g.Filename.EndsWith("GroupAttribute.cs"));
            Assert.Contains(generated, g => g.Filename.EndsWith("group__Group.cs"));
            Assert.Contains(generated, g => g.Filename.EndsWith("group__Group_Foo.cs"));
            Assert.Contains(generated, g => g.Filename.EndsWith("group__Group_Bar.cs"));
            generated.First(g => g.Filename.EndsWith("GroupAttribute.cs")).Content.Should().BeIgnoringLineEndings(@"
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
            generated.First(g => g.Filename.EndsWith("group__Group.cs")).Content.Should().BeIgnoringLineEndings(@"
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
            generated.First(g => g.Filename.EndsWith("group__Group_Foo.cs")).Content.Should().BeIgnoringLineEndings(@"
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
            generated.First(g => g.Filename.EndsWith("group__Group_Bar.cs")).Content.Should().BeIgnoringLineEndings(@"
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
            Assert.Contains(generated, g => g.Filename.EndsWith("GroupAttribute.cs"));
            Assert.Contains(generated, g => g.Filename.EndsWith("group__Group.cs"));
            Assert.Contains(generated, g => g.Filename.EndsWith("group__Group_Foo.cs"));
            Assert.Contains(generated, g => g.Filename.EndsWith("group__Group_Bar.cs"));
            generated.First(g => g.Filename.EndsWith("GroupAttribute.cs")).Content.Should().BeIgnoringLineEndings(@"
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
            generated.First(g => g.Filename.EndsWith("group__Group.cs")).Content.Should().BeIgnoringLineEndings(@"
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
            generated.First(g => g.Filename.EndsWith("group__Group_Foo.cs")).Content.Should().BeIgnoringLineEndings(@"
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
            generated.First(g => g.Filename.EndsWith("group__Group_Bar.cs")).Content.Should().BeIgnoringLineEndings(@"
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
            Assert.Contains(generated, g => g.Filename.EndsWith("GroupAttribute.cs"));
            Assert.Contains(generated, g => g.Filename.EndsWith("group__Group1.cs"));
            Assert.Contains(generated, g => g.Filename.EndsWith("group__Group2.cs"));
            Assert.Contains(generated, g => g.Filename.EndsWith("group__Group1_Foo.cs"));
            Assert.Contains(generated, g => g.Filename.EndsWith("group__Group1_Bar.cs"));
            Assert.Contains(generated, g => g.Filename.EndsWith("group__Group2_Foo.cs"));
            Assert.Contains(generated, g => g.Filename.EndsWith("group__Group2_Bar.cs"));
            generated.First(g => g.Filename.EndsWith("GroupAttribute.cs")).Content.Should().BeIgnoringLineEndings(@"
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
            generated.First(g => g.Filename.EndsWith("group__Group1.cs")).Content.Should().BeIgnoringLineEndings(@"
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
            generated.First(g => g.Filename.EndsWith("group__Group2.cs")).Content.Should().BeIgnoringLineEndings(@"
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
            generated.First(g => g.Filename.EndsWith("group__Group1_Foo.cs")).Content.Should().BeIgnoringLineEndings(@"
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
            generated.First(g => g.Filename.EndsWith("group__Group1_Bar.cs")).Content.Should().BeIgnoringLineEndings(@"
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
            generated.First(g => g.Filename.EndsWith("group__Group2_Foo.cs")).Content.Should().BeIgnoringLineEndings(@"
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
            generated.First(g => g.Filename.EndsWith("group__Group2_Bar.cs")).Content.Should().BeIgnoringLineEndings(@"
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
            Assert.Contains(generated, g => g.Filename.EndsWith("GroupAttribute.cs"));
            Assert.Contains(generated, g => g.Filename.EndsWith("group__Group.cs"));
            Assert.Contains(generated, g => g.Filename.EndsWith("group__Group_Foo.cs"));
            Assert.Contains(generated, g => g.Filename.EndsWith("group__Group_Bar.cs"));
            generated.First(g => g.Filename.EndsWith("GroupAttribute.cs")).Content.Should().BeIgnoringLineEndings(@"
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
            generated.First(g => g.Filename.EndsWith("group__Group.cs")).Content.Should().BeIgnoringLineEndings(@"
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
            generated.First(g => g.Filename.EndsWith("group__Group_Foo.cs")).Content.Should().BeIgnoringLineEndings(@"
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
            generated.First(g => g.Filename.EndsWith("group__Group_Bar.cs")).Content.Should().BeIgnoringLineEndings(@"
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
            Assert.Contains(generated, g => g.Filename.EndsWith("GroupAttribute.cs"));
            Assert.Contains(generated, g => g.Filename.EndsWith("group__Outer.cs"));
            Assert.Contains(generated, g => g.Filename.EndsWith("group__Inner.cs"));
            Assert.Contains(generated, g => g.Filename.EndsWith("group__Inner_Foo.cs"));
            Assert.Contains(generated, g => g.Filename.EndsWith("group__Outer_Bar.cs"));
            generated.First(g => g.Filename.EndsWith("GroupAttribute.cs")).Content.Should().BeIgnoringLineEndings(@"
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
            generated.First(g => g.Filename.EndsWith("group__Outer.cs")).Content.Should().BeIgnoringLineEndings(@"
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
            generated.First(g => g.Filename.EndsWith("group__Inner.cs")).Content.Should().BeIgnoringLineEndings(@"
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
            generated.First(g => g.Filename.EndsWith("group__Inner_Foo.cs")).Content.Should().BeIgnoringLineEndings(@"
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
            generated.First(g => g.Filename.EndsWith("group__Outer_Bar.cs")).Content.Should().BeIgnoringLineEndings(@"
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
            Assert.Contains(generated, g => g.Filename.EndsWith("GroupAttribute.cs"));
            Assert.Contains(generated, g => g.Filename.EndsWith("group__Outer.cs"));
            Assert.Contains(generated, g => g.Filename.EndsWith("group__Inner.cs"));
            Assert.Contains(generated, g => g.Filename.EndsWith("group__Inner_Foo.cs"));
            generated.First(g => g.Filename.EndsWith("GroupAttribute.cs")).Content.Should().BeIgnoringLineEndings(@"
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
            generated.First(g => g.Filename.EndsWith("group__Outer.cs")).Content.Should().BeIgnoringLineEndings(@"
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
            generated.First(g => g.Filename.EndsWith("group__Inner.cs")).Content.Should().BeIgnoringLineEndings(@"
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
            generated.First(g => g.Filename.EndsWith("group__Inner_Foo.cs")).Content.Should().BeIgnoringLineEndings(@"
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
