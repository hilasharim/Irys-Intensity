using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.IO;
using System.Diagnostics;
using BitMiracle.LibTiff.Classic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;
using System.Windows.Media;

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

        //Function receives an open scan's Tiff stream and a specific  frame number, and returns that frame's pixels as a byte array
        private static MemoryStream FramePixelsAsByteArray(Tiff scanTiff, short frameNumber)
        {
            //byte[][] framePixels = new byte[imageLength][];
            //scanTiff.SetDirectory(frameNumber);
            //for (int row = 0; row < imageLength; row++)
            //{
            //    framePixels[row] = new byte[scanlineSize];
            //    scanTiff.ReadScanline(framePixels[row], row);
            //}
            //return new MemoryStream(framePixels.SelectMany(a => a).ToArray());

            scanTiff.SetDirectory(frameNumber);
            MemoryStream stream = new MemoryStream();
            byte[] data = new byte[scanlineSize];
            for (int row = 0; row < imageLength; row++)
            {
                scanTiff.ReadScanline(data, row);
                stream.Write(data, 0, scanlineSize);
            }

            stream.Position = 0;
            return stream;
        }

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

            //int lowerLeft = pixelValues[yBase][xBase];
            //int lowerRight = pixelValues[yBase][xBase + 1];
            //int upperLeft = pixelValues[yBase + 1][xBase];
            //int upperRight = pixelValues[yBase + 1][xBase + 1];
            //double upperAverage, lowerAverage;
            //upperAverage = upperLeft + xFraction * (upperRight - upperLeft);
            //lowerAverage = lowerLeft + xFraction * (lowerRight - lowerLeft);
            //short newPixelValue = (short)(lowerAverage + yFraction * (upperAverage - lowerAverage) + 0.5);
            //return newPixelValue;
        }

        public static short[][] RotateBilinear(short[][] pixelValues, /*int pixelX, int pixelY,*/ double radians, int rotationCenterX, int rotationCenterY)
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


            //double rotatedX = (pixelX - rotationCenterX) * Math.Cos(radians) - (pixelY - rotationCenterY) * Math.Sin(radians) + rotationCenterX;
            //double rotatedY = (pixelX - rotationCenterX) * Math.Sin(radians) + (pixelY - rotationCenterY) * Math.Cos(radians) + rotationCenterY;
            //if (rotatedX < 0) rotatedX = 0;
            //if (rotatedX > imageWidth - 1) rotatedX = imageWidth - 1;
            //if (rotatedY < 0) rotatedY = 0;
            //if (rotatedY > imageLength) rotatedY = imageLength;
            //return PixelBilinearInterpolation(pixelValues, rotatedX, rotatedY);

            //short bottomLeftVal, bottomRightVal, upperLeftVal, upperRightVal;
            //double factorX, factorY, rotatedPixelVal;

            //int leftX = (int)Math.Floor(rotatedX);
            //int rightX = (int)Math.Ceiling(rotatedX);
            //int upperY = (int)Math.Ceiling(rotatedY);
            //int bottomY = (int)Math.Floor(rotatedY);

            //leftX = Math.Max(0, leftX);
            //bottomY = Math.Max(0, bottomY);
            //leftX = Math.Min(imageWidth - 1, leftX);
            //bottomY = Math.Min(imageLength - 1, bottomY);
            //rightX = Math.Min(imageWidth - 1, rightX);
            //rightX = Math.Max(0, rightX);
            //upperY = Math.Min(imageLength - 1, upperY);
            //upperY = Math.Max(0, upperY);

            //bottomLeftVal = pixelValues[bottomY][leftX];
            //bottomRightVal = pixelValues[bottomY][rightX];
            //upperRightVal = pixelValues[upperY][rightX];
            //upperLeftVal = pixelValues[upperY][leftX];

            //if (rightX == leftX)
            //{
            //    factorX = 1;
            //}
            //else
            //{
            //    factorX = (rightX - rotatedX) / (rightX - leftX); 
            //}

            //if (bottomY == upperY)
            //{
            //    factorY = 1;
            //}
            //else
            //{
            //    factorY = (bottomY - rotatedY) / (bottomY - upperY);
            //}

            //rotatedPixelVal = factorY * (factorX * upperLeftVal + (1 - factorX) * upperRightVal) + (1 - factorY) * (factorX * bottomLeftVal + (1 - factorX) * bottomRightVal);
            //return (short)rotatedPixelVal;
        }

        private static Tuple<int, int> RotatePixel(int pixelX, int pixelY, double radians, int rotationCenterX, int rotationCenterY)
        {
            int rotatedX = (int)(Math.Round((pixelX - rotationCenterX) * Math.Cos(radians) - (pixelY - rotationCenterY) * Math.Sin(radians))) + rotationCenterX;
            int rotatedY = (int)(Math.Round((pixelX - rotationCenterX) * Math.Sin(radians) + (pixelY - rotationCenterY) * Math.Cos(radians))) + rotationCenterY;
            return new Tuple<int, int>(rotatedX, rotatedY);
        }

        public static void RotateImage()
        {
            Tuple<int, int> result = RotatePixel(0, 0, 0.1 * Math.PI / 180, 256, 256);
            Tuple<int, int> newResult = RotatePixel(result.Item1, result.Item2, -0.1 * Math.PI / 180, 256, 256);
        }

        //private static BitmapImage GetFrameImage(Tiff scanTiff, short frameNumber)
        //{
        //    using (MemoryStream ms = FramePixelsAsByteArray(scanTiff, frameNumber))
        //    {
        //        BitmapImage image = new BitmapImage();
        //        image.BeginInit();
        //        image.Format
        //        image.CacheOption = BitmapCacheOption.OnLoad;
        //        image.StreamSource = ms;
        //        image.EndInit();
        //        return image;
        //    }
        //}

        private static Bitmap GetBitmap16Bit(Tiff tiff, short frameNumber)
        {
            Bitmap imageResult;
            if (bitsPerPixel != 16) {
                return null;
            }
            if (spp != 1)
            {
                return null;
            }
            tiff.SetDirectory(frameNumber);
            byte[] buffer = new byte[scanlineSize];
            imageResult = new Bitmap(imageWidth, imageLength, System.Drawing.Imaging.PixelFormat.Format16bppGrayScale);

            short[] buffer16Bit = null;
            for (int row = 0; row < imageLength; row++)
            {
                Rectangle imRect = new Rectangle(0, row, imageWidth, 1);
                BitmapData imgData = imageResult.LockBits(imRect, ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format16bppGrayScale);
                if (buffer16Bit == null)
                {
                    buffer16Bit = new short[scanlineSize / sizeof(Int16)];
                }
                else
                {
                    Array.Clear(buffer16Bit, 0, buffer16Bit.Length);
                }
                
                tiff.ReadScanline(buffer, row);
                Buffer.BlockCopy(buffer, 0, buffer16Bit, 0, buffer.Length);
                Marshal.Copy(buffer16Bit, 0, imgData.Scan0, buffer16Bit.Length);
                imageResult.UnlockBits(imgData);
            }

            return imageResult;
        }

        private static void SaveBitmapAsTiff(BitmapSource bitmap, string fileName)
        {
            //BitmapData imgdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
            //BitmapSource src = BitmapSource.Create(imgdata.Width, imgdata.Height, bitmap.HorizontalResolution, bitmap.VerticalResolution, System.Windows.Media.PixelFormats.Gray16, null,
              //  imgdata.Scan0, imgdata.Height * imgdata.Stride, imgdata.Stride);
            //bitmap.UnlockBits(imgdata);
            using (FileStream stream = new FileStream(fileName, FileMode.Create))
            {
                TiffBitmapEncoder encoder = new TiffBitmapEncoder();
                encoder.Compression = TiffCompressOption.Zip;
                //encoder.Frames.Add(BitmapFrame.Create(src));
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                encoder.Save(stream);
            }
        }

        private static double RadiansToDegrees(double radians)
        {
            return ((180 / Math.PI) * radians);
        }

        private static TransformedBitmap RotateBitmap(Bitmap bitmap, RotateTransform rotateTransform)
        {
            BitmapData imgdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
            BitmapSource src = BitmapSource.Create(imgdata.Width, imgdata.Height, bitmap.HorizontalResolution, bitmap.VerticalResolution, System.Windows.Media.PixelFormats.Gray16, null,
                imgdata.Scan0, imgdata.Height * imgdata.Stride, imgdata.Stride);
            bitmap.UnlockBits(imgdata);
            TransformedBitmap bmp = new TransformedBitmap(src, rotateTransform);
            return bmp;
        }

        private static Bitmap RotateImage(Bitmap bmp, double radians)
        {
            Bitmap rotatedImage = new Bitmap(bmp.Width, bmp.Height);
            GraphicsPath gp = new GraphicsPath();
            gp.AddPolygon(new Point[] { new Point(0, 0), new Point(bmp.Width, 0), new Point(0, bmp.Height) });
            System.Drawing.Drawing2D.Matrix rotateAtCenter = new System.Drawing.Drawing2D.Matrix();
            rotateAtCenter.RotateAt((float)RadiansToDegrees(radians), new PointF(bmp.Width / 2f, bmp.Height / 2f));
            gp.Transform(rotateAtCenter);
            PointF[] pts = gp.PathPoints;

            Bitmap rotImg = new Bitmap(bmp.Width, bmp.Height);
            Graphics g = Graphics.FromImage(rotImg);
            g.DrawImage(bmp, pts);

            //BitmapData bData = rotImg.LockBits(new Rectangle(new Point(), rotImg.Size), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format16bppGrayScale);
            //int byteCount = bData.Stride * (rotImg.Height);
            //short[] flatArr = new short[byteCount / sizeof(Int16)];

            //RotateTransform rotateAtCenter = new RotateTransform((float)RadiansToDegrees(radians), bmp.Width / 2, bmp.Height / 2);
            

            //Matrix rotateAtCenter = new Matrix();
            //rotateAtCenter.RotateAt((float)RadiansToDegrees(radians), new PointF(bmp.Width / 2f, bmp.Height / 2f));
            //using (Graphics gr = Graphics.FromImage(rotatedImage))
            //{
            //    gr.InterpolationMode = InterpolationMode.Bilinear;
            //    gr.Clear(Color.LightBlue);
            //    gr.Transform = rotateAtCenter;
            //    gr.DrawImage(bmp, 0, 0);
            //}

            //using (Graphics g = Graphics.FromImage(bmp))
            //{
            //    // Set the rotation point to the center of the matrix
            //    g.TranslateTransform(bmp.Width / 2, bmp.Height / 2);
            //    // Rotate
            //    g.RotateTransform((float)RadiansToDegrees(radians));
            //    // Restore rotation point in the matrix
            //    g.TranslateTransform(-bmp.Width / 2, -bmp.Height / 2);
            //    // Draw image on the bitmap
            //    g.DrawImage(bmp, new Point(0, 0));
            //}
            return rotatedImage;
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


        private static void ProcessColumnImages(Tiff scanTiff, int columnNumber)
        {
            short[][][] columnFramesPixelData = new short[rowsPerColumn][][];
            //Bitmap[] columnBitmaps = new Bitmap[rowsPerColumn];
            //RotateTransform rotateAtCenter = new RotateTransform(0.1, 256, 256);
            //BitmapImage[] columnBitmaps = new BitmapImage[rowsPerColumn];
            for (int currentChannel = 0; currentChannel <1 /*totalChannels*/; currentChannel++)
            {
                int currPos = 0;
                GetFrameNumbers(columnNumber, currentChannel, columnFrames);
                foreach (short frameNumber in columnFrames)
                {
                    columnFramesPixelData[currPos] = FramePixelsAsShortArray(scanTiff, frameNumber);
                    short[][] rotatedImage = RotateBilinear(columnFramesPixelData[currPos], 0.002, imageWidth / 2, imageLength / 2);
                    //short newVal = RotateBilinear(columnFramesPixelData[currPos], 0, 0, 0.0019, imageWidth / 2, imageLength / 2);
                    //columnBitmaps[currPos] = GetFrameImage(scanTiff, frameNumber);
                    //currPos++;
                    //columnBitmaps[currPos] = GetBitmap16Bit(scanTiff, frameNumber);

                    //Bitmap rotated = RotateImage(columnBitmaps[currPos], 0.0174533);
                    //TransformedBitmap bmp = RotateBitmap(columnBitmaps[currPos], rotateAtCenter);
                    //SaveBitmapAsTiff(bmp, "new" + currPos.ToString() + ".tif");
                    //SaveBitmapAsTiff(bmp, "new" + currPos.ToString() + ".tif");
                    currPos++;
                }
            }

        }

        public static void ProcessScanTiff(string scanTiffFilePath)
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
                for (int currColumn = 1; currColumn <=1 /*columnPerScan*/; currColumn++)
                {
                    ProcessColumnImages(scanImages, currColumn);
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
