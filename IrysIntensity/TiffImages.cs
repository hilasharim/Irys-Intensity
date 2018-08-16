using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.IO;
using System.Diagnostics;
using BitMiracle.LibTiff.Classic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace IrysIntensity
{
    class TiffImages
    {
        private const int totalChannels = 3;
        private const int rowsPerColumn = 12;
        public const int columnPerScan = 95;
        private const int xOpticalAxis = 170;
        private const int moleculePixelsPadding = 4;
        private static readonly double[] relativeMagnifications = new double[] { 1, 0.99, 1.006 };

        static short[] columnFrames = new short[rowsPerColumn];
        static int imageLength;
        static int imageWidth;
        static int scanlineSize;
        static short bitsPerPixel; //16 for 16-bit tiff
        static short spp; //tiff samples per pixel: 1 for greyscale, 3 for RGB

        //Parse a scan's FOV file to return a dictionary where the keys are a tuple (col, row) and the values are FOV instances. returns null if FOV file not found
        private static Dictionary<Tuple<int, int>, FOV> ParseFOVFile(string fovFilePath)
        {
            if (!File.Exists(fovFilePath))
            {
                return null;
            }

            using (var fileStream = File.OpenRead(fovFilePath))
            {
                using (StreamReader streamReader = new StreamReader(fileStream))
                {
                    Dictionary<Tuple<int, int>, FOV> fovAnglesAndShifts = new Dictionary<Tuple<int, int>, FOV>();
                    int cumsumXShift = 0;
                    int cumsumYShift = 0;
                    int prevCol = 0;
                    string line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        if (Char.IsNumber(line[0]))
                        {
                            string[] fovInfo = line.Split('\t');
                            if (int.Parse(fovInfo[2]) != prevCol)
                            {
                                cumsumXShift = 0;
                                cumsumYShift = 0;
                                prevCol = int.Parse(fovInfo[2]);
                            }
                            else
                            {
                                cumsumXShift += int.Parse(fovInfo[4]);
                                cumsumYShift += int.Parse(fovInfo[5]);
                            }
                            FOV fov = new FOV(double.Parse(fovInfo[3]), int.Parse(fovInfo[4]), int.Parse(fovInfo[5]), cumsumXShift, cumsumYShift);
                            fovAnglesAndShifts[new Tuple<int, int>(int.Parse(fovInfo[2]), int.Parse(fovInfo[1]))] = fov;
                        }
                    }
                    return fovAnglesAndShifts;
                }
            }
        }

        /*Function receives an open tiff file and a frame number, and returns the specified frame's pixel data as a short[][] array*/
        private static short[][] FramePixelsAsShortArray(Tiff scanTiff, short frameNumber)
        {
            short[][] pixelData = new short[imageLength][];
            byte[] buf = new byte[scanlineSize];

            scanTiff.SetDirectory(frameNumber);
            for (int row = 0; row < imageLength; row++)
            {
                scanTiff.ReadScanline(buf, row);
                short[] rowPixels = new short[buf.Length / sizeof(Int16)];
                Buffer.BlockCopy(buf, 0, rowPixels, 0, buf.Length);
                pixelData[row] = rowPixels;
            }

            return pixelData;
        }

        /*Function to find the value of pixel (x,y) using bilinear interpolation (4 closest points)*/
        public static double PixelBilinearInterpolation(short[][] pixelValues, double x, double y)
        {
            int xBase = (int)x;
            int yBase = (int)y;
            double xFraction = x - xBase;
            double yFraction = y - yBase;
            short lowerLeft = pixelValues[yBase][xBase];
            short lowerRight = pixelValues[yBase][xBase + 1];
            short upperRight = pixelValues[yBase + 1][xBase + 1];
            short upperLeft = pixelValues[yBase + 1][xBase];
            double upperAverage = upperLeft + xFraction * (upperRight - upperLeft);
            double lowerAverage = lowerLeft + xFraction * (lowerRight - lowerLeft);
            return lowerAverage + yFraction * (upperAverage - lowerAverage);
        }

        private static Tuple<double, double> GetRotatedXYPosition(double radians, double rotationCenterX, double rotationCenterY, double x, double y)
        {
            double angleCos = Math.Cos(radians);
            double angleSin = Math.Sin(radians);
            double rotatedX = (x - rotationCenterX) * angleCos - (y - rotationCenterY) * angleSin + rotationCenterX;
            double rotatedY = (x - rotationCenterX) * angleSin + (y - rotationCenterY) * angleCos + rotationCenterY;
            if ((rotatedX >= -0.01) && (rotatedX < imageWidth) && (rotatedY >= -0.01) && (rotatedY < imageLength))
            {
                if (rotatedX < 0) rotatedX = 0;
                if (rotatedX > imageWidth - 1) rotatedX = imageWidth - 1.001;
                if (rotatedY < 0) rotatedY = 0;
                if (rotatedY > imageLength - 1) rotatedY = imageLength - 1.001;
            }
            return new Tuple<double, double>(rotatedX, rotatedY); 
        }

        /*Function to rotate an image represented as a short[][] array around a given point, using bilinear interpolation without resizing*/
        private static short[][] RotateBilinear(short[][] pixelValues, double radians, double rotationCenterX, double rotationCenterY)
        {
            short[][] rotatedPixels = new short[imageLength][];
            double rotationAngle = -radians;
            Tuple<double, double> rotatedPixelPos;
            for (int row = 0; row < imageLength; row++)
            {
                rotatedPixels[row] = new short[imageWidth];
                for (int col = 0; col < imageWidth; col++)
                {
                    rotatedPixelPos = GetRotatedXYPosition(rotationAngle, rotationCenterX, rotationCenterY, col, row);
                    if ((rotatedPixelPos.Item1 >= 0) && (rotatedPixelPos.Item1 <= imageWidth - 1.001) && (rotatedPixelPos.Item2 >= 0) && (rotatedPixelPos.Item2 < imageLength - 1.001))
                    {
                        rotatedPixels[row][col] = (short)(PixelBilinearInterpolation(pixelValues, rotatedPixelPos.Item1, rotatedPixelPos.Item2) + 0.5);
                    }
                    else
                    {
                        rotatedPixels[row][col] = 0;
                    }
                }
            }
            return rotatedPixels;
        }

        private static short[][] Rotate180AroundCenter(short[][] pixelValues)
        {
            short[][] rotatedPixels = new short[imageLength][];
            for (int row = 0; row < imageLength; row++)
            {
                rotatedPixels[row] = new short[imageWidth];
                for (int col = 0; col < imageWidth; col++)
                {
                    rotatedPixels[row][col] = pixelValues[imageLength - 1 - row][imageWidth - 1 - col];
                }
            }
            return rotatedPixels;
        }

        private static void TranslateX(short[][] pixelValues, int offset)
        {
            if (offset == 0)
                return;
            short[] background = new short[Math.Abs(offset)];
            if (offset > 0)
            {
                for (int row = 0; row < imageLength; row++)
                {
                    Array.Copy(pixelValues[row], 0, pixelValues[row], offset, pixelValues[row].Length - offset);
                    Array.Copy(background, pixelValues[row], offset);
                }   
            }
            else
            {
                for (int row = 0; row < imageLength; row++)
                {
                    Array.Copy(pixelValues[row], -offset, pixelValues[row], 0, pixelValues[row].Length + offset);
                    Array.Copy(background, 0, pixelValues[row], pixelValues[row].Length + offset, -offset);
                }
            }
        }

        /*destinationPixelValues is large enough to conatin the final merged image, and its first rows are of images already read and merged. Merging by averaging the values
         assumes yOverlap is negative value*/
        private static void MergeOnYOverlap(short[][] destinationPixelValues, short[][] imageToMergePixelValues, int yOverlap, int rowNumber, int cumSumYOverlap)
        {
            int destinationStartRow = imageLength * (rowNumber - 1) + cumSumYOverlap;
            double averageIncrement;
            double weight;
            for (int row = 0; row < -yOverlap; row++)
            {
                averageIncrement = 1.0 / (-yOverlap);
                weight = row * averageIncrement;
                for (int col = 0; col < imageWidth; col++)
                {
                    destinationPixelValues[destinationStartRow + row][col] = (short)(weight * imageToMergePixelValues[row][col] + (1 - weight) * destinationPixelValues[destinationStartRow + row][col]);
                }
            }

            destinationStartRow += (-yOverlap); 
            for (int row = 0; row < imageLength + yOverlap; row++)
            {
                destinationPixelValues[destinationStartRow + row] = imageToMergePixelValues[-yOverlap+row];
            }
        }

        //Calculate the 12 frames that need to be opened for a specific column number in a specific channel, in top to bottom order.
        private static void GetFrameNumbers(int column, int channel, short[] columnChannelFrameNumbers)
        {   
            short firstFrame;
            if (column % 2 != 0) //odd column - first frame on top
            {
                firstFrame = (short)((column - 1) * rowsPerColumn * totalChannels + channel);
                for (int currPos = 0; currPos < rowsPerColumn; currPos++)
                {
                    columnChannelFrameNumbers[currPos] = (short)(firstFrame + currPos * totalChannels);
                }
            }
            else //even column - last frame on top
            {
                firstFrame = (short)(column * rowsPerColumn * totalChannels - totalChannels + channel);
                for (int currPos = 0; currPos < rowsPerColumn; currPos++)
                {
                    columnChannelFrameNumbers[currPos] = (short)(firstFrame - currPos * totalChannels);
                }
            }
        }


        private static IEnumerable<double[]> getMoleculesPixels(int columnNumber, short[][] columnPixelData, IEnumerable<Molecule> moleculePositions, Dictionary<Tuple<int, int>, FOV> FOVShifts,
                                                                double rotationCenterX, double rotationCenterY, int channel)
        {
            List<double[]> moleculesPixels = new List<double[]>();
            foreach (Molecule molecule in moleculePositions)
            {
                Tuple<int, int> colRowStart = new Tuple<int, int>(columnNumber, molecule.RowStart);
                Tuple<int, int> colRowEnd = new Tuple<int, int>(columnNumber, molecule.RowEnd);
                int molStartFOVCumsumXShift = FOVShifts[colRowStart].CumsumXShift;
                int molStartFOVCumsumYShift = FOVShifts[colRowStart].CumsumYShift;
                double molStartFOVAngle = FOVShifts[colRowStart].Angle;
                int molEndFOVCumsumXShift = FOVShifts[colRowEnd].CumsumXShift;
                int molEndFOVCumsumYShift = FOVShifts[colRowEnd].CumsumYShift;
                double molEndFOVAngle = FOVShifts[colRowEnd].Angle;
                Tuple<double, double> molXYStart = GetRotatedXYPosition(molStartFOVAngle, rotationCenterX, rotationCenterY, molecule.XStart + molStartFOVCumsumXShift, molecule.YStart);
                Tuple<double, double> molXYEnd = GetRotatedXYPosition(molEndFOVAngle, rotationCenterX, rotationCenterY, molecule.XEnd + molEndFOVCumsumXShift, molecule.YEnd);
                int molStartReadY = (int)Math.Floor(molXYStart.Item2) + imageLength * (molecule.RowStart - 1) + molStartFOVCumsumYShift;
                int molEndReadY = (int)Math.Ceiling(molXYEnd.Item2) + imageLength * (molecule.RowEnd - 1) + molEndFOVCumsumYShift;
                int molStartReadX = (int)Math.Floor((molXYStart.Item1 - xOpticalAxis) / relativeMagnifications[channel] + xOpticalAxis) - (int)Math.Floor(moleculePixelsPadding * relativeMagnifications[channel]);
                int molEndReadX = (int)Math.Ceiling((molXYEnd.Item1 - xOpticalAxis) / relativeMagnifications[channel] + xOpticalAxis) + (int)Math.Floor(moleculePixelsPadding * relativeMagnifications[channel]);
                int molLength = molEndReadY - molStartReadY + 1;
                int molWidth = molEndReadX - molStartReadX + 1;

                double[] moleculePixelData = new double[molLength];
                for (int row = molStartReadY; row <= molEndReadY; row++)
                {
                    double rowSum = 0;
                    for (int col = molStartReadX; col <= molEndReadX; col++)
                    {
                        rowSum += columnPixelData[row][col];
                    }
                    moleculePixelData[row - molStartReadY] = (rowSum / molWidth);
                }
                moleculesPixels.Add(moleculePixelData);
            }
            return moleculesPixels;
        }

        private static void ProcessColumnImages(Tiff scanTiff, int columnNumber, Dictionary<Tuple<int, int>, FOV> FOVShifts)
        {
            IEnumerable<Molecule> moleculePositions = DatabaseManager.SelectColumnMolecules(1, 1, 1, columnNumber);
            int totalYShift = FOVShifts[new Tuple<int, int>(columnNumber, rowsPerColumn)].CumsumYShift;
            short[][] framePixels;
            short[][] columnPixelData = new short[imageLength * rowsPerColumn + totalYShift][]; //totalYShift is a negative value
            double angle;
            int xShift, yShift;

            for (int row = 0; row < imageLength * rowsPerColumn + totalYShift; row++)
            {
                columnPixelData[row] = new short[imageWidth];
            }

            for (int currentChannel = 2; currentChannel <3 /*totalChannels*/; currentChannel++)
            {
                int rowNumber = 1;
                int cumSumYShift = 0;
                int cumSumXShift = 0;
                GetFrameNumbers(columnNumber, currentChannel, columnFrames);
                foreach (short frameNumber in columnFrames)
                {
                    Tuple<int, int> colRow = new Tuple<int, int>(columnNumber, rowNumber);
                    angle = FOVShifts[colRow].Angle;
                    cumSumXShift = FOVShifts[colRow].CumsumXShift;
                    cumSumYShift = FOVShifts[colRow].CumsumYShift;
                    xShift = FOVShifts[colRow].XShift;
                    yShift = FOVShifts[colRow].YShift;
                    framePixels = FramePixelsAsShortArray(scanTiff, frameNumber);
                    framePixels = Rotate180AroundCenter(framePixels);
                    TranslateX(framePixels, cumSumXShift);
                    framePixels = RotateBilinear(framePixels, angle, (imageWidth - 1) / 2, (imageLength - 1) / 2);
                    MergeOnYOverlap(columnPixelData, framePixels, yShift, rowNumber, cumSumYShift);
                    rowNumber++;
                }

                IEnumerable<double[]> columnMoleculePixels = getMoleculesPixels(columnNumber, columnPixelData, moleculePositions, FOVShifts, (imageWidth - 1) / 2, (imageLength - 1) / 2, currentChannel);

                //using (StreamWriter sw = new StreamWriter(@"column2_rotate_180_cumsum_x_minus_rotate_angle_optical_axis.txt"))
                //{
                //    for (int row = 0; row < imageLength * rowsPerColumn + totalYShift; row++)
                //    {
                //        string print = String.Join("\t", Array.ConvertAll(columnPixelData[row], Convert.ToString));
                //        sw.WriteLine(print);
                //    }
                //}
            }

        }

        public delegate void UpdateBox(string s);

        public static void ProcessScanTiff(string scanTiffFilePath, UpdateBox updateBox)
        {
            using (Tiff scanImages = Tiff.Open(scanTiffFilePath, "r"))
            {
                FieldValue[] width = scanImages.GetField(TiffTag.IMAGEWIDTH);
                FieldValue[] height = scanImages.GetField(TiffTag.IMAGELENGTH);
                FieldValue[] bitsPerSample = scanImages.GetField(TiffTag.BITSPERSAMPLE);
                FieldValue[] samplesPerPixel = scanImages.GetField(TiffTag.SAMPLESPERPIXEL);
                imageLength = height[0].ToInt();
                imageWidth = width[0].ToInt();
                bitsPerPixel = bitsPerSample[0].ToShort();
                spp = samplesPerPixel[0].ToShort();
                scanlineSize = scanImages.ScanlineSize();

                Dictionary<Tuple<int, int>, FOV> FOVData = ParseFOVFile(@"X:\runs\2018-03\Pbmc_hmc_bspq1_6.3.17_fc2_2018-03-25_11_59\Detect Molecules\Stitch1.fov");

                for (int currColumn = 2; currColumn <=2 /*columnPerScan*/; currColumn++)
                {
                    
                    ProcessColumnImages(scanImages, currColumn, FOVData);
                    updateBox(currColumn.ToString());
                }
            }
        }
        
        public static string openImageLibtiff()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            using (Tiff image = Tiff.Open(@"X:\runs\2018-03\Pbmc_hmc_bspq1_6.3.17_fc2_2018-03-25_11_59\Pbmc_hmc_bspq1_6.3.17_fc2_2018-03-25_11_59_Scan001.tiff", "r"))
            {
                FieldValue[] width = image.GetField(TiffTag.IMAGEWIDTH);
                FieldValue[] height = image.GetField(TiffTag.IMAGELENGTH);
                FieldValue[] samplesPerPixel = image.GetField(TiffTag.SAMPLESPERPIXEL); //will be 1 for greyscale, 3 for RGB
                FieldValue[] bitsPerSample = image.GetField(TiffTag.BITSPERSAMPLE); //will be 16 if 16-bit image
                FieldValue[] orientation = image.GetField(TiffTag.ORIENTATION); //defines 0,0 pixel of image
                FieldValue[] planarConfig = image.GetField(TiffTag.PLANARCONFIG); // 0 = UNKNOWN; 1 = CONTIG (single image plane); 2 = SEPARATE (separate planes of data)
                FieldValue[] photometric = image.GetField(TiffTag.PHOTOMETRIC); //minvalue is black or white
                FieldValue[] rowsPerStrip = image.GetField(TiffTag.ROWSPERSTRIP);
                FieldValue[] compression = image.GetField(TiffTag.COMPRESSION);
                FieldValue[] fillOrder = image.GetField(TiffTag.FILLORDER); //which is the most significant bit

                int imageLength = height[0].ToInt();
                int imageWidth = width[0].ToInt();
                short[][] pixelData = new short[imageLength][];

                byte[] buf = new byte[image.ScanlineSize()];

                for (short i = 0; i < 12; i++)
                {
                    image.SetDirectory(i);
                    for (int row = 0; row < imageLength; row++)
                    {
                        image.ReadScanline(buf, row);
                        short[] rowPixels = new short[buf.Length / sizeof(Int16)];
                        Buffer.BlockCopy(buf, 0, rowPixels, 0, buf.Length);
                        pixelData[row] = rowPixels;
                    }
                }
                
                /*image.SetDirectory(1);
                for (int row = 0; row < imageLength; row++)
                {
                    image.ReadScanline(buf, row);
                    short[] rowPixels = new short[buf.Length / sizeof(Int16)];
                    Buffer.BlockCopy(buf, 0, rowPixels, 0, buf.Length);
                    pixelData[row] = rowPixels;
                } */
            }     

            return stopwatch.Elapsed.ToString();
        }

        
    }
}
