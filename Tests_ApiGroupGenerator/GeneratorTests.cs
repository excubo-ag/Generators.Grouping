﻿using Excubo.Generators.Grouping;
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
            RunGenerator(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            Assert.Single(generated);
            generated.ContainsFileWithContent("GroupAttribute.cs", @"
// <auto-generated />
#nullable enable
using System;
namespace Excubo.Generators.Grouping
{
    /// <summary>
    /// Annotate a method with this attribute to add this method to the specified group.
    /// <br/>
    /// To use, you first need to define a struct or interface as follows:
    /// <br/>
    ///<example>
    ///<code>
    ///public class Foo // this is the class containing the method you're annotating right now!<br/>
    ///{<br/>
    ///   public partial struct GGroup // define this struct<br/>
    ///   {<br/>
    ///   }<br/>
    ///   [Group(typeof(GGroup))] // reference the above struct<br/>
    ///   public void Bar() {} // this is the method you're annotating right now!<br/>
    ///}<br/>
    ///</code>
    ///</example>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    sealed class GroupAttribute : Attribute
    {
        /// <param name=""group_type"">The interface or struct that this method should be grouped into</param>
        /// <param name=""method_name"">Optional method alias. When not null, this string must be a valid C# method identifier and it will be used instead of the methods name within the group</param>
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
";
            RunGenerator(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            Assert.Equal(4, generated.Length);
            Assert.True(generated.Any(g => g.Filename.EndsWith("GroupAttribute.cs")));
            generated.ContainsFileWithContent("group_SimpleGroup.Container.GGroup.cs", @"
// <auto-generated />
namespace SimpleGroup
{
    public partial class Container
    {
        public partial struct GGroup
        {
            private Container group_internal__parent;
            public GGroup(Container parent) { this.group_internal__parent = parent; }
        }
        public GGroup Group => new GGroup(this);
    }
}
");
            generated.ContainsFileWithContent("group_SimpleGroup.Container.GGroup_SimpleGroup.Container.Foo().cs", @"
// <auto-generated />
namespace SimpleGroup
{
    public partial class Container
    {
        public partial struct GGroup
        {
            public void Foo()
                => group_internal__parent.Foo();
        }
    }
}
");
            generated.ContainsFileWithContent("group_SimpleGroup.Container.GGroup_SimpleGroup.Container.Bar_T1, T2_(T1, string).cs", @"
// <auto-generated />
namespace SimpleGroup
{
    public partial class Container
    {
        public partial struct GGroup
        {
            public (T2, T1) Bar<T1, T2>(T1 t1, string tmp) where T1 : class
                => group_internal__parent.Bar<T1, T2>(t1, tmp);
        }
    }
}
");
        }

        [Fact]
        public void Params()
        {
            var userSource = @"
using Excubo.Generators.Grouping;
using System;

namespace Params
{
    public partial class Container
    {
        public partial struct GGroup
        {
        }
        [Group(typeof(GGroup))] public void Params(params (int v1, int v2)[] objects) { throw new NotImplementedException(); }
    }
    public class Consumption
    {
        public void Consume()
        {
            var container = new Container();
            container.Group.Params((0, 1), (1, 2));
        }
    }
}
";
            RunGenerator(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            Assert.Equal(3, generated.Length);
            Assert.True(generated.Any(g => g.Filename.EndsWith("GroupAttribute.cs")));
            generated.ContainsFileWithContent("group_Params.Container.GGroup.cs", @"
// <auto-generated />
namespace Params
{
    public partial class Container
    {
        public partial struct GGroup
        {
            private Container group_internal__parent;
            public GGroup(Container parent) { this.group_internal__parent = parent; }
        }
        public GGroup Group => new GGroup(this);
    }
}
");
            generated.ContainsFileWithContent("group_Params.Container.GGroup_Params.Container.Params(params (int v1, int v2)[]).cs", @"
// <auto-generated />
namespace Params
{
    public partial class Container
    {
        public partial struct GGroup
        {
            public void Params(params (int v1, int v2)[] objects)
                => group_internal__parent.Params(objects);
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
";
            RunGenerator(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            Assert.Equal(7, generated.Length);
            Assert.True(generated.Any(g => g.Filename.EndsWith("GroupAttribute.cs")));
            generated.ContainsFileWithContent("group_Interface.IContainer.IGGroup.cs", @"
// <auto-generated />
namespace Interface
{
    public partial interface IContainer
    {
        public partial interface IGGroup
        {
        }
        IGGroup Group { get; }
    }
}
");
            generated.ContainsFileWithContent("group_Interface.IContainer.IGGroup_Interface.IContainer.Foo().cs", @"
// <auto-generated />
namespace Interface
{
    public partial interface IContainer
    {
        public partial interface IGGroup
        {
            public void Foo()
                ;
        }
    }
}
");
            generated.ContainsFileWithContent("group_Interface.IContainer.IGGroup_Interface.IContainer.Bar_T1, T2_(T1, string).cs", @"
// <auto-generated />
namespace Interface
{
    public partial interface IContainer
    {
        public partial interface IGGroup
        {
            public (T2, T1) Bar<T1, T2>(T1 t1, string tmp) where T1 : class
                ;
        }
    }
}
");
            generated.ContainsFileWithContent("group_Interface.Container.GGroup.cs", @"
// <auto-generated />
namespace Interface
{
    public partial class Container
    {
        public partial struct GGroup
        {
            private Container group_internal__parent;
            public GGroup(Container parent) { this.group_internal__parent = parent; }
        }
        public GGroup Group => new GGroup(this);
        IContainer.IGGroup IContainer.Group => Group;
    }
}
");
            generated.ContainsFileWithContent("group_Interface.Container.GGroup_Interface.Container.Foo().cs", @"
// <auto-generated />
namespace Interface
{
    public partial class Container
    {
        public partial struct GGroup
        {
            public void Foo()
                => group_internal__parent.Foo();
        }
    }
}
");
            generated.ContainsFileWithContent("group_Interface.Container.GGroup_Interface.Container.Bar_T1, T2_(T1, string).cs", @"
// <auto-generated />
namespace Interface
{
    public partial class Container
    {
        public partial struct GGroup
        {
            public (T2, T1) Bar<T1, T2>(T1 t1, string tmp) where T1 : class
                => group_internal__parent.Bar<T1, T2>(t1, tmp);
        }
    }
}
");
        }

        [Fact]
        public void Region()
        {
            var userSource = @"
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
";
            RunGenerator(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            Assert.Equal(4, generated.Length);
            Assert.True(generated.Any(g => g.Filename.EndsWith("GroupAttribute.cs")));
            generated.ContainsFileWithContent("group_Region.Container.GGroup.cs", @"
// <auto-generated />
namespace Region
{
    public partial class Container
    {
        public partial struct GGroup
        {
            private Container group_internal__parent;
            public GGroup(Container parent) { this.group_internal__parent = parent; }
        }
        public GGroup Group => new GGroup(this);
    }
}
");
            generated.ContainsFileWithContent("group_Region.Container.GGroup_Region.Container.Foo().cs", @"
// <auto-generated />
namespace Region
{
    public partial class Container
    {
        public partial struct GGroup
        {
            public void Foo()
                => group_internal__parent.Foo();
        }
    }
}
");
            generated.ContainsFileWithContent("group_Region.Container.GGroup_Region.Container.Bar_T1, T2_(T1, string).cs", @"
// <auto-generated />
namespace Region
{
    public partial class Container
    {
        public partial struct GGroup
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

namespace Ambiguity
{
    public partial class Container1
    {
        public partial struct GGroup
        {
        }
        [Group(typeof(GGroup))] public void Foo() { throw new NotImplementedException(); }
    }
    public partial class Container2
    {
        public partial struct GGroup
        {
        }
        [Group(typeof(GGroup))] public void Foo() { throw new NotImplementedException(); }
    }

    public class Consumption
    {
        public void Consume()
        {
            var container1 = new Container1();
            container1.Group.Foo();
            var container2 = new Container2();
            container2.Group.Foo();
        }
    }
}
";
            RunGenerator(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            Assert.Equal(5, generated.Length);
            Assert.True(generated.Any(g => g.Filename.EndsWith("GroupAttribute.cs")));
            generated.ContainsFileWithContent("group_Ambiguity.Container1.GGroup.cs", @"
// <auto-generated />
namespace Ambiguity
{
    public partial class Container1
    {
        public partial struct GGroup
        {
            private Container1 group_internal__parent;
            public GGroup(Container1 parent) { this.group_internal__parent = parent; }
        }
        public GGroup Group => new GGroup(this);
    }
}
");
            generated.ContainsFileWithContent("group_Ambiguity.Container1.GGroup_Ambiguity.Container1.Foo().cs", @"
// <auto-generated />
namespace Ambiguity
{
    public partial class Container1
    {
        public partial struct GGroup
        {
            public void Foo()
                => group_internal__parent.Foo();
        }
    }
}
");
            generated.ContainsFileWithContent("group_Ambiguity.Container2.GGroup.cs", @"
// <auto-generated />
namespace Ambiguity
{
    public partial class Container2
    {
        public partial struct GGroup
        {
            private Container2 group_internal__parent;
            public GGroup(Container2 parent) { this.group_internal__parent = parent; }
        }
        public GGroup Group => new GGroup(this);
    }
}
");
            generated.ContainsFileWithContent("group_Ambiguity.Container2.GGroup_Ambiguity.Container2.Foo().cs", @"
// <auto-generated />
namespace Ambiguity
{
    public partial class Container2
    {
        public partial struct GGroup
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

namespace GenericContainer
{
    public partial class Container<T>
    {
        public partial struct GGroup
        {
        }
        [Group(typeof(GGroup))] public void Foo() { throw new NotImplementedException(); }
        [Group(typeof(GGroup))] public (T, T1) Bar<T1>(T1 t1, string tmp) where T1 : class { throw new NotImplementedException(); }
    }

    public class Consumption
    {
        public void Consume()
        {
            var container = new Container<object>();
            container.Group.Foo();
            container.Group.Bar<object>(null, null);
        }
    }
}
";
            RunGenerator(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            Assert.Equal(4, generated.Length);
            Assert.True(generated.Any(g => g.Filename.EndsWith("GroupAttribute.cs")));
            generated.ContainsFileWithContent("group_GenericContainer.Container_T_.GGroup.cs", @"
// <auto-generated />
namespace GenericContainer
{
    public partial class Container<T>
    {
        public partial struct GGroup
        {
            private Container<T> group_internal__parent;
            public GGroup(Container<T> parent) { this.group_internal__parent = parent; }
        }
        public GGroup Group => new GGroup(this);
    }
}
");
            generated.ContainsFileWithContent("group_GenericContainer.Container_T_.GGroup_GenericContainer.Container_T_.Foo().cs", @"
// <auto-generated />
namespace GenericContainer
{
    public partial class Container<T>
    {
        public partial struct GGroup
        {
            public void Foo()
                => group_internal__parent.Foo();
        }
    }
}
");
            generated.ContainsFileWithContent("group_GenericContainer.Container_T_.GGroup_GenericContainer.Container_T_.Bar_T1_(T1, string).cs", @"
// <auto-generated />
namespace GenericContainer
{
    public partial class Container<T>
    {
        public partial struct GGroup
        {
            public (T, T1) Bar<T1>(T1 t1, string tmp) where T1 : class
                => group_internal__parent.Bar<T1>(t1, tmp);
        }
    }
}
");
        }

        [Fact]
        public void FullAttributeName()
        {
            var userSource = @"
using Excubo.Generators.Grouping;
using System;

namespace FullAttributeName
{
    public partial class Container
    {
        public partial struct GGroup
        {
        }
        [GroupAttribute(typeof(GGroup))] public void Foo() { throw new NotImplementedException(); }
        [GroupAttribute(typeof(GGroup))] public (T2, T1) Bar<T1, T2>(T1 t1, string tmp) where T1 : class { throw new NotImplementedException(); }
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
";
            RunGenerator(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            Assert.Equal(4, generated.Length);
            Assert.True(generated.Any(g => g.Filename.EndsWith("GroupAttribute.cs")));
            generated.ContainsFileWithContent("group_FullAttributeName.Container.GGroup.cs", @"
// <auto-generated />
namespace FullAttributeName
{
    public partial class Container
    {
        public partial struct GGroup
        {
            private Container group_internal__parent;
            public GGroup(Container parent) { this.group_internal__parent = parent; }
        }
        public GGroup Group => new GGroup(this);
    }
}
");
            generated.ContainsFileWithContent("group_FullAttributeName.Container.GGroup_FullAttributeName.Container.Foo().cs", @"
// <auto-generated />
namespace FullAttributeName
{
    public partial class Container
    {
        public partial struct GGroup
        {
            public void Foo()
                => group_internal__parent.Foo();
        }
    }
}
");
            generated.ContainsFileWithContent("group_FullAttributeName.Container.GGroup_FullAttributeName.Container.Bar_T1, T2_(T1, string).cs", @"
// <auto-generated />
namespace FullAttributeName
{
    public partial class Container
    {
        public partial struct GGroup
        {
            public (T2, T1) Bar<T1, T2>(T1 t1, string tmp) where T1 : class
                => group_internal__parent.Bar<T1, T2>(t1, tmp);
        }
    }
}
");
        }

        [Fact]
        public void FullyQualifiedAttributeName()
        {
            var userSource = @"
using System;

namespace FullyQualifiedAttributeName
{
    public partial class Container
    {
        public partial struct GGroup
        {
        }
        [Excubo.Generators.Grouping.GroupAttribute(typeof(GGroup))] public void Foo() { throw new NotImplementedException(); }
        [Excubo.Generators.Grouping.GroupAttribute(typeof(GGroup))] public (T2, T1) Bar<T1, T2>(T1 t1, string tmp) where T1 : class { throw new NotImplementedException(); }
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
";
            RunGenerator(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            Assert.Equal(4, generated.Length);
            Assert.True(generated.Any(g => g.Filename.EndsWith("GroupAttribute.cs")));
            generated.ContainsFileWithContent("group_FullyQualifiedAttributeName.Container.GGroup.cs", @"
// <auto-generated />
namespace FullyQualifiedAttributeName
{
    public partial class Container
    {
        public partial struct GGroup
        {
            private Container group_internal__parent;
            public GGroup(Container parent) { this.group_internal__parent = parent; }
        }
        public GGroup Group => new GGroup(this);
    }
}
");
            generated.ContainsFileWithContent("group_FullyQualifiedAttributeName.Container.GGroup_FullyQualifiedAttributeName.Container.Foo().cs", @"
// <auto-generated />
namespace FullyQualifiedAttributeName
{
    public partial class Container
    {
        public partial struct GGroup
        {
            public void Foo()
                => group_internal__parent.Foo();
        }
    }
}
");
            generated.ContainsFileWithContent("group_FullyQualifiedAttributeName.Container.GGroup_FullyQualifiedAttributeName.Container.Bar_T1, T2_(T1, string).cs", @"
// <auto-generated />
namespace FullyQualifiedAttributeName
{
    public partial class Container
    {
        public partial struct GGroup
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
";
            RunGenerator(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            Assert.Equal(7, generated.Length);
            Assert.True(generated.Any(g => g.Filename.EndsWith("GroupAttribute.cs")));
            generated.ContainsFileWithContent("group_MultipleGroups.Container.GGroup1.cs", @"
// <auto-generated />
namespace MultipleGroups
{
    public partial class Container
    {
        public partial struct GGroup1
        {
            private Container group_internal__parent;
            public GGroup1(Container parent) { this.group_internal__parent = parent; }
        }
        public GGroup1 Group1 => new GGroup1(this);
    }
}
");
            generated.ContainsFileWithContent("group_MultipleGroups.Container.GGroup2.cs", @"
// <auto-generated />
namespace MultipleGroups
{
    public partial class Container
    {
        public partial struct GGroup2
        {
            private Container group_internal__parent;
            public GGroup2(Container parent) { this.group_internal__parent = parent; }
        }
        public GGroup2 Group2 => new GGroup2(this);
    }
}
");
            generated.ContainsFileWithContent("group_MultipleGroups.Container.GGroup1_MultipleGroups.Container.Foo().cs", @"
// <auto-generated />
namespace MultipleGroups
{
    public partial class Container
    {
        public partial struct GGroup1
        {
            public void Foo()
                => group_internal__parent.Foo();
        }
    }
}
");
            generated.ContainsFileWithContent("group_MultipleGroups.Container.GGroup1_MultipleGroups.Container.Bar_T1, T2_(T1, string).cs", @"
// <auto-generated />
namespace MultipleGroups
{
    public partial class Container
    {
        public partial struct GGroup1
        {
            public (T2, T1) Bar<T1, T2>(T1 t1, string tmp) where T1 : class
                => group_internal__parent.Bar<T1, T2>(t1, tmp);
        }
    }
}
");
            generated.ContainsFileWithContent("group_MultipleGroups.Container.GGroup2_MultipleGroups.Container.Foo().cs", @"
// <auto-generated />
namespace MultipleGroups
{
    public partial class Container
    {
        public partial struct GGroup2
        {
            public void Foo()
                => group_internal__parent.Foo();
        }
    }
}
");
            generated.ContainsFileWithContent("group_MultipleGroups.Container.GGroup2_MultipleGroups.Container.Bar_T1, T2_(T1, string).cs", @"
// <auto-generated />
namespace MultipleGroups
{
    public partial class Container
    {
        public partial struct GGroup2
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

namespace Renaming
{
    public partial class Container
    {
        public partial struct GGroup
        {
        }
        [Group(typeof(GGroup), ""Frobulate"")] public void Foo() { throw new NotImplementedException(); }
        [Group(typeof(GGroup), ""Bamboozle"")] public (T2, T1) Bar<T1, T2>(T1 t1, string tmp) where T1 : class { throw new NotImplementedException(); }
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
";
            RunGenerator(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            Assert.Equal(4, generated.Length);
            Assert.True(generated.Any(g => g.Filename.EndsWith("GroupAttribute.cs")));
            generated.ContainsFileWithContent("group_Renaming.Container.GGroup.cs", @"
// <auto-generated />
namespace Renaming
{
    public partial class Container
    {
        public partial struct GGroup
        {
            private Container group_internal__parent;
            public GGroup(Container parent) { this.group_internal__parent = parent; }
        }
        public GGroup Group => new GGroup(this);
    }
}
");
            generated.ContainsFileWithContent("group_Renaming.Container.GGroup_Renaming.Container.Foo().cs", @"
// <auto-generated />
namespace Renaming
{
    public partial class Container
    {
        public partial struct GGroup
        {
            public void Frobulate()
                => group_internal__parent.Foo();
        }
    }
}
");
            generated.ContainsFileWithContent("group_Renaming.Container.GGroup_Renaming.Container.Bar_T1, T2_(T1, string).cs", @"
// <auto-generated />
namespace Renaming
{
    public partial class Container
    {
        public partial struct GGroup
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
";
            RunGenerator(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            Assert.Equal(5, generated.Length);
            Assert.True(generated.Any(g => g.Filename.EndsWith("GroupAttribute.cs")));
            generated.ContainsFileWithContent("group_Nesting.Container.GOuter.cs", @"
// <auto-generated />
namespace Nesting
{
    public partial class Container
    {
        public partial struct GOuter
        {
            private Container group_internal__parent;
            public GOuter(Container parent) { this.group_internal__parent = parent; }
        }
        public GOuter Outer => new GOuter(this);
    }
}
");
            generated.ContainsFileWithContent("group_Nesting.Container.GOuter.GInner.cs", @"
// <auto-generated />
namespace Nesting
{
    public partial class Container
    {
        public partial struct GOuter
        {
            public partial struct GInner
            {
                private Container group_internal__parent;
                public GInner(Container parent) { this.group_internal__parent = parent; }
            }
            public GInner Inner => new GInner(this.group_internal__parent);
        }
    }
}
");
            generated.ContainsFileWithContent("group_Nesting.Container.GOuter.GInner_Nesting.Container.Foo().cs", @"
// <auto-generated />
namespace Nesting
{
    public partial class Container
    {
        public partial struct GOuter
        {
            public partial struct GInner
            {
                public void Foo()
                    => group_internal__parent.Foo();
            }
        }
    }
}
");
            generated.ContainsFileWithContent("group_Nesting.Container.GOuter_Nesting.Container.Bar().cs", @"
// <auto-generated />
namespace Nesting
{
    public partial class Container
    {
        public partial struct GOuter
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

namespace NestingOnlyOneMethodInInner
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
    }
    public class Consumption
    {
        public void Consume()
        {
            var container = new Container();
            container.Outer.Inner.Foo();
        }
    }
}
";
            RunGenerator(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            Assert.Equal(4, generated.Length);
            Assert.True(generated.Any(g => g.Filename.EndsWith("GroupAttribute.cs")));
            generated.ContainsFileWithContent("group_NestingOnlyOneMethodInInner.Container.GOuter.cs", @"
// <auto-generated />
namespace NestingOnlyOneMethodInInner
{
    public partial class Container
    {
        public partial struct GOuter
        {
            private Container group_internal__parent;
            public GOuter(Container parent) { this.group_internal__parent = parent; }
        }
        public GOuter Outer => new GOuter(this);
    }
}
");
            generated.ContainsFileWithContent("group_NestingOnlyOneMethodInInner.Container.GOuter.GInner.cs", @"
// <auto-generated />
namespace NestingOnlyOneMethodInInner
{
    public partial class Container
    {
        public partial struct GOuter
        {
            public partial struct GInner
            {
                private Container group_internal__parent;
                public GInner(Container parent) { this.group_internal__parent = parent; }
            }
            public GInner Inner => new GInner(this.group_internal__parent);
        }
    }
}
");
            generated.ContainsFileWithContent("group_NestingOnlyOneMethodInInner.Container.GOuter.GInner_NestingOnlyOneMethodInInner.Container.Foo().cs", @"
// <auto-generated />
namespace NestingOnlyOneMethodInInner
{
    public partial class Container
    {
        public partial struct GOuter
        {
            public partial struct GInner
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

namespace Comments
{
    public partial class Container
    {
        /// <summary>
        /// Comment on <see cref=""Outer""/>
        /// </summary>
        public partial struct GOuter
        {
            /// <summary>
            /// Comment on <see cref=""GInner""/>
            /// </summary>
            // another comment
            /* seriously: three kinds of comments */
            public partial struct GInner
            {
            }
        }
        /// <summary>
        /// Method summary
        /// </summary>
        [Group(typeof(GOuter.GInner))] public void Foo() { throw new NotImplementedException(); }
    }
    public class Consumption
    {
        public void Consume()
        {
            var container = new Container();
            container.Outer.Inner.Foo();
        }
    }
}
";
            RunGenerator(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            Assert.Equal(4, generated.Length);
            Assert.True(generated.Any(g => g.Filename.EndsWith("GroupAttribute.cs")));
            generated.ContainsFileWithContent("group_Comments.Container.GOuter.cs", @"
// <auto-generated />
namespace Comments
{
    public partial class Container
    {
        public partial struct GOuter
        {
            private Container group_internal__parent;
            public GOuter(Container parent) { this.group_internal__parent = parent; }
        }
        /// <summary>
        /// Comment on <see cref=""Outer""/>
        /// </summary>
        public GOuter Outer => new GOuter(this);
    }
}
");
            generated.ContainsFileWithContent("group_Comments.Container.GOuter.GInner.cs", @"
// <auto-generated />
namespace Comments
{
    public partial class Container
    {
        public partial struct GOuter
        {
            public partial struct GInner
            {
                private Container group_internal__parent;
                public GInner(Container parent) { this.group_internal__parent = parent; }
            }
            /// <summary>
            /// Comment on <see cref=""GInner""/>
            /// </summary>
            // another comment
            /* seriously: three kinds of comments */
            public GInner Inner => new GInner(this.group_internal__parent);
        }
    }
}
");
            generated.ContainsFileWithContent("group_Comments.Container.GOuter.GInner_Comments.Container.Foo().cs", @"
// <auto-generated />
namespace Comments
{
    public partial class Container
    {
        public partial struct GOuter
        {
            public partial struct GInner
            {
                /// <summary>
                /// Method summary
                /// </summary>
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