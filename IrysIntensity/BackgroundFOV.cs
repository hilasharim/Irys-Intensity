using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BitMiracle.LibTiff.Classic;

namespace IrysIntensity
{
    class BackgroundFOV
    {
        public double[][] PixelValues { get; set; }
        public double AverageValue { get; set; }
        public int Channel { get; set; }
        public int ImageLength { get; set; }
        public int ImageWidth { get; set; }

        public BackgroundFOV(int channel, int imageLength, int imageWidth)
        {
            this.Channel = channel;
            this.ImageLength = imageLength;
            this.ImageWidth = imageWidth;
            this.PixelValues = new double[this.ImageLength][];
            for (int row = 0; row < this.ImageLength; row++)
            {
                this.PixelValues[row] = new double[this.ImageWidth];
            }
        }

        private void CalculateAveragePixelValues(Tiff scanTiff, IEnumerable<short> noMoleculeFrames)
        {
            short[][] framePixels;
            foreach (short frame in noMoleculeFrames)
            {
                framePixels = TiffImages.FramePixelsAsShortArray(scanTiff, frame);
                for (int row = 0; row < this.ImageLength; row++)
                {
                    for (int col = 0; col < this.ImageWidth; col++)
                    {
                        this.PixelValues[row][col] += framePixels[row][col];
                    }
                }
            }
            for (int row = 0; row < this.ImageLength; row++)
            {
                for (int col = 0; col < this.ImageWidth; col++)
                {
                    this.PixelValues[row][col] /= noMoleculeFrames.Count();
                    this.AverageValue += this.PixelValues[row][col];
                }
            }
            this.AverageValue /= (ImageLength * ImageWidth);
        }

        public void CalculateBackground(Tiff scanTiff, int projectId, int runId)
        {
            int[] colsForBackground = new int[] { 1, TiffImages.columnPerScan };
            IEnumerable<int>[] colsForBackgroundMolFrames = new IEnumerable<int>[colsForBackground.Length];
            HashSet<short> framesForBackgroundCalc = new HashSet<short>();
            for (int colPos=0; colPos < colsForBackground.Length; colPos++) 
            {
                colsForBackgroundMolFrames[colPos] = DatabaseManager.SelectColumnRows(projectId, runId, 1, colsForBackground[colPos]);
                for (int row = 1; row <= TiffImages.rowsPerColumn; row++)
                {
                    if (!colsForBackgroundMolFrames[colPos].Contains(row))
                    {
                        framesForBackgroundCalc.Add(TiffImages.GetFrameNumber(colsForBackground[colPos], row, this.Channel));
                    } 
                }
            }
            this.CalculateAveragePixelValues(scanTiff, framesForBackgroundCalc);
        }
    }
}
