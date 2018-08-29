using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IrysIntensity
{
    class Molecule
    {
        public int DataBaseId { get; set; }
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
        public string AlignmentChannelLabelPositions { get; set; }
        public int Mapped { get; set; }
        public int ChromId { get; set; }
        public string AlignmentString { get; set; }
        public string Orientation { get; set; }
        public double[][] Pixels { get; set; }

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

        public Molecule(int dataBaseId, int molId, int runId, int scan, int column, int rowStart, int rowEnd, double xStart, double xEnd, double yStart, double yEnd) 
            : this(rowStart, rowEnd, xStart, xEnd, yStart, yEnd)
        {
            this.DataBaseId = dataBaseId;
            this.MoleculeId = molId;
            this.RunId = runId;
            this.Scan = scan;
            this.Column = column;
            this.Pixels = new double[TiffImages.totalChannels][];
        }

        public Molecule(int molId, string alignmentChPositions, int chromId, string alignmentString, string orientation, double[] channel1Pixels, double[] channel2Pixels)
        {
            this.MoleculeId = molId;
            this.AlignmentChannelLabelPositions = alignmentChPositions;
            this.ChromId = chromId;
            this.AlignmentString = alignmentString;
            this.Orientation = orientation;
            this.Pixels = new double[2][];
            this.Pixels[0] = channel1Pixels;
            this.Pixels[1] = channel2Pixels;
        }
    }
}
