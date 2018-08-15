using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IrysIntensity
{
    class Molecule
    {
        public int MoleculeId { get; set; }
        public float Length { get; set; }
        public int RunId { get; set; }
        public int Scan { get; set; }
        public int OriginalId { get; set; }
        public int Column {get; set;}
        public int RowStart { get; set; }
        public int RowEnd { get; set; }
        public double XStart { get; set; }
        public double XEnd { get; set; }
        public double YStart { get; set; }
        public double YEnd { get; set; }

        public Molecule(int moleculeId, float length, int runId, int scan, int originalId)
        {
            this.MoleculeId = moleculeId;
            this.Length = length;
            this.RunId = runId;
            this.Scan = scan;
            this.OriginalId = originalId;
        }

        public Molecule(int rowStart, int rowEnd, double xStart, double xEnd, double yStart, double yEnd)
        {
            this.RowStart = rowStart;
            this.RowEnd = rowEnd;
            this.XStart = xStart;
            this.XEnd = xEnd;
            this.YStart = yStart;
            this.YEnd = yEnd;
        }
    }
}
