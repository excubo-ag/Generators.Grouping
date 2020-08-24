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
            /// Comment on <see cref=""Inner""/>
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