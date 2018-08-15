using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IrysIntensity
{
    class FOV
    {
        public double Angle { get; set; }
        public int XShift { get; set; }
        public int YShift { get; set; }
        public int CumsumXShift { get; set; }
        public int CumsumYShift { get; set; }

        public FOV(double angle, int xShift, int yShift, int cumsumXShift, int cumsumYShift)
        {
            this.Angle = angle;
            this.XShift = xShift;
            this.YShift = yShift;
            this.CumsumXShift = cumsumXShift;
            this.CumsumYShift = cumsumYShift;
        }
    }
}
