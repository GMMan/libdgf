using System;
using System.Collections.Generic;
using System.Text;

namespace LibDgf.Ps2.Vif
{
    public struct VuVector
    {
        public VuFloat X;
        public VuFloat Y;
        public VuFloat Z;
        public VuFloat W;

        public override string ToString()
        {
            return $"<{X}, {Y}, {Z}, {W}>";
        }
    }
}
