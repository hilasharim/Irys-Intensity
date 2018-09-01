using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IrysIntensity
{
    class Scan
    {
        public int ScanNumber { get; set; }
        public List<Molecule>[] ColumnMolecules { get; set; }

        public Scan(int scanNumber)
        {
            this.ScanNumber = scanNumber;
            this.ColumnMolecules = new List<Molecule>[TiffImages.columnPerScan];
            for (int currCol = 0; currCol < TiffImages.columnPerScan; currCol++)
            {
                this.ColumnMolecules[currCol] = new List<Molecule>();
            }
        }

        public void AddMolecule(int columnNumber, Molecule molecule)
        {
            this.ColumnMolecules[columnNumber-1].Add(molecule);
        }
    }
}
