using System;
using System.Collections.Generic;

namespace Univox
{
    public static class AxisOrderingX
    {
        public static readonly IReadOnlyList<AxisOrdering> Values = (AxisOrdering[]) Enum.GetValues(typeof(AxisOrdering));
    }
}