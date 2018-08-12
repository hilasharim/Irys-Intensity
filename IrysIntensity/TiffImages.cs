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
        private const int columnPerScan = 95;
        static short[] columnFrames = new short[rowsPerColumn];
        static int imageLength;
        static int imageWidth;
        static int scanlineSize;
        static short bitsPerPixel; //16 for 16-bit tiff
        static short spp; //tiff samples per pixel: 1 for greyscale, 3 for RGB


        //Parse a scan's FOV file to return a dictionary where the keys are a tuple (col, row) and the values are a tuple (angle, xShift, yShift). returns null if FOV file not found
        private static Dictionary<Tuple<int, int>, Tuple<float, int, int>> ParseFOVFile(string fovFilePath)
        {
            if (!File.Exists(fovFilePath))
            {
                return null;
            }

            using (var fileStream = File.OpenRead(fovFilePath))
            {
                using (StreamReader streamReader = new StreamReader(fileStream))
                {
                    Dictionary<Tuple<int, int>, Tuple<float, int, int>> fovAnglesAndShifts = new Dictionary<Tuple<int, int>, Tuple<float, int, int>>();
                    string line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        if (Char.IsNumber(line[0]))
                        {
                            string[] fovInfo = line.Split('\t');
                            fovAnglesAndShifts[new Tuple<int, int>(int.Parse(fovInfo[2]), int.Parse(fovInfo[1]))] = new Tuple<float, int, int>(float.Parse(fovInfo[3]), int.Parse(fovInfo[4]), int.Parse(fovInfo[5]));
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

        /*Function to rotate an image represented as a short[][] array around a given point, using bilinear interpolation without resizing*/
        private static short[][] RotateBilinear(short[][] pixelValues, double radians, int rotationCenterX, int rotationCenterY)
        {
            short[][] rotatedPixels = new short[imageLength][];
            double rotationAngle = -radians;
            double angleCos = Math.Cos(rotationAngle);
            double angleSin = Math.Sin(rotationAngle);
            double rotatedX, rotatedY;
            for (int row = 0; row < imageLength; row++)
            {
                rotatedPixels[row] = new short[imageWidth];
                for (int col = 0; col < imageWidth; col++)
                {
                    rotatedX = (col - rotationCenterX) * angleCos - (row - rotationCenterY) * angleSin + rotationCenterX;
                    rotatedY = (col - rotationCenterX) * angleSin + (row - rotationCenterY) * angleCos + rotationCenterY;
                    if ((rotatedX >= -0.01) && (rotatedX < imageWidth) && (rotatedY >= -0.01) && (rotatedY < imageLength))
                    {
                        if (rotatedX < 0) rotatedX = 0;
                        if (rotatedX > imageWidth - 1) rotatedX = imageWidth - 1.001;
                        if (rotatedY < 0) rotatedY = 0;
                        if (rotatedY > imageLength - 1) rotatedY = imageLength - 1.001;
                        rotatedPixels[row][col] = (short)(PixelBilinearInterpolation(pixelValues, rotatedX, rotatedY) + 0.5);
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

        //private static Bitmap GetBitmap16Bit(Tiff tiff, short frameNumber)
        //{
        //    Bitmap imageResult;
        //    if (bitsPerPixel != 16) {
        //        return null;
        //    }
        //    if (spp != 1)
        //    {
        //        return null;
        //    }
        //    tiff.SetDirectory(frameNumber);
        //    byte[] buffer = new byte[scanlineSize];
        //    imageResult = new Bitmap(imageWidth, imageLength, System.Drawing.Imaging.PixelFormat.Format16bppGrayScale);

        //    short[] buffer16Bit = null;
        //    for (int row = 0; row < imageLength; row++)
        //    {
        //        Rectangle imRect = new Rectangle(0, row, imageWidth, 1);
        //        BitmapData imgData = imageResult.LockBits(imRect, ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format16bppGrayScale);
        //        if (buffer16Bit == null)
        //        {
        //            buffer16Bit = new short[scanlineSize / sizeof(Int16)];
        //        }
        //        else
        //        {
        //            Array.Clear(buffer16Bit, 0, buffer16Bit.Length);
        //        }
                
        //        tiff.ReadScanline(buffer, row);
        //        Buffer.BlockCopy(buffer, 0, buffer16Bit, 0, buffer.Length);
        //        Marshal.Copy(buffer16Bit, 0, imgData.Scan0, buffer16Bit.Length);
        //        imageResult.UnlockBits(imgData);
        //    }

        //    return imageResult;
        //}

        //private static void SaveBitmapAsTiff(BitmapSource bitmap, string fileName)
        //{
        //    //BitmapData imgdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
        //    //BitmapSource src = BitmapSource.Create(imgdata.Width, imgdata.Height, bitmap.HorizontalResolution, bitmap.VerticalResolution, System.Windows.Media.PixelFormats.Gray16, null,
        //      //  imgdata.Scan0, imgdata.Height * imgdata.Stride, imgdata.Stride);
        //    //bitmap.UnlockBits(imgdata);
        //    using (FileStream stream = new FileStream(fileName, FileMode.Create))
        //    {
        //        TiffBitmapEncoder encoder = new TiffBitmapEncoder();
        //        encoder.Compression = TiffCompressOption.Zip;
        //        //encoder.Frames.Add(BitmapFrame.Create(src));
        //        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        //        encoder.Save(stream);
        //    }
        //}


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


        private static IEnumerable<double[]> getMoleculesPixels(short[][] columnPixelData, IEnumerable<Molecule> moleculePositions)
        {
            List<double[]> moleculesPixels = new List<double[]>();
            foreach (Molecule molecule in moleculePositions)
            {
                int molRowStart = (int)Math.Floor(molecule.YStart);
                int molRowEnd = (int)Math.Ceiling(molecule.YEnd);
                int molColStart = (int)Math.Floor(molecule.XStart);
                int molColEnd = (int)Math.Ceiling(molecule.XEnd);
                int molLength = molRowEnd - molRowStart;
                int molWidth = molColEnd - molColStart;

                double[] moleculePixelData = new double[molLength];
                for (int row = molRowStart; row < molRowEnd; row++)
                {
                    double rowSum = 0;
                    for (int col = molColStart; col < molColEnd; col++)
                    {
                        rowSum += columnPixelData[row][col];
                    }
                    moleculePixelData[row - molRowStart] = (rowSum / molWidth);
                }
                moleculesPixels.Add(moleculePixelData);
            }
            return moleculesPixels;
        }


        private static void ProcessColumnImages(Tiff scanTiff, int columnNumber, Dictionary<Tuple<int, int>, Tuple<float, int, int>> FOVShifts)
        {
            //short[][][] columnFramesPixelData = new short[rowsPerColumn][][];
            IEnumerable<Molecule> moleculePositions = DatabaseManager.SelectColumnMolecules(1, 1, 1, columnNumber);
            short[][] framePixels;
            short[][] columnPixelData = new short[imageLength * rowsPerColumn /*+ totalYShift*/][]; //totalYShift is a negative value
            float angle;
            int xShift, yShift;

            for (int row = 0; row < imageLength * rowsPerColumn /*+ totalYShift*/; row++)
            {
                columnPixelData[row] = new short[imageWidth];
            }

            for (int currentChannel = 0; currentChannel <1 /*totalChannels*/; currentChannel++)
            {
                int rowNumber = 1;
                int cumSumYShift = 0;
                int cumSumXShift = 0;
                float cumsumAngle = 0;
                GetFrameNumbers(columnNumber, currentChannel, columnFrames);
                foreach (short frameNumber in columnFrames)
                {
                    Tuple<int, int> colRow = new Tuple<int, int>(columnNumber, rowNumber);
                    angle = FOVShifts[colRow].Item1;
                    xShift = FOVShifts[colRow].Item2;
                    yShift = FOVShifts[colRow].Item3;
                    cumSumYShift += /*(int)(Math.Floor(Math.Sin(Math.PI / 2 - angle)* yShift))*/ yShift;
                    cumSumXShift -= (int)Math.Ceiling(Math.Cos(angle)*xShift) /*xShift*/;
                    framePixels = FramePixelsAsShortArray(scanTiff, frameNumber);
                    TranslateX(framePixels, cumSumXShift /*-xShift*/);
                    //framePixels = RotateBilinear(framePixels, angle, (imageWidth - 1) / 2, (imageLength - 1) / 2);
                    framePixels = RotateBilinear(framePixels, (Math.PI - angle), (imageWidth - 1) / 2, (imageLength - 1) / 2);
                    //framePixels = Rotate180AroundCenter(framePixels);
                    //TranslateX(framePixels, cumSumXShift /*xShift*/);
                    MergeOnYOverlap(columnPixelData, framePixels, yShift, rowNumber, cumSumYShift);
                    rowNumber++;
                }

                //IEnumerable<double[]> columnMoleculePixels = getMoleculesPixels(columnPixelData, moleculePositions);

                using (StreamWriter sw = new StreamWriter(@"column2_cumsum_x_cos_angle_rotate_180_minus_angle.txt"))
                {
                    for (int row = 0; row < imageLength * rowsPerColumn; row++)
                    {
                        string print = String.Join("\t", Array.ConvertAll(columnPixelData[row], Convert.ToString));
                        sw.WriteLine(print);
                    }
                }
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

                Dictionary<Tuple<int, int>, Tuple<float, int, int>> FOVData = ParseFOVFile(@"X:\runs\2018-03\Pbmc_hmc_bspq1_6.3.17_fc2_2018-03-25_11_59\Detect Molecules\Stitch1.fov");

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
