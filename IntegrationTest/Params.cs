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