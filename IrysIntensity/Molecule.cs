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
        public float XStart { get; set; }
        public float XEnd { get; set; }
        public float YStart { get; set; }
        public float YEnd { get; set; }

        public Molecule(int moleculeId, float length, int runId, int scan, int originalId)
        {
            this.MoleculeId = moleculeId;
            this.Length = length;
            this.RunId = runId;
            this.Scan = scan;
            this.OriginalId = originalId;
        }
    }
}
