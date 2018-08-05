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


        //Function receives an open scan's Tiff stream and a specific  frame number, and returns that frame's pixels as a byte array
        private static MemoryStream FramePixelsAsByteArray(Tiff scanTiff, short frameNumber, int imageLength)
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

        //private static void GetBitmap(Tiff tiff, short frameNubmer, int imageLength)
        //{
        //    Bitmap bitmap = new Bitmap(scanlineSize / 2, imageLength, System.Drawing.Imaging.PixelFormat.Format16bppGrayScale);

        //    byte[] buf = new byte[tiff.ScanlineSize()];
        //    for (int row = 0; row < imageLength; row++)
        //    {
        //        tiff.ReadScanline(buf, row);
        //        for (int col = 0; col < scanlineSize / 2; col+=2)
        //        {
        //            int color = BitConverter.ToInt16(buf, col);
        //            bitmap.SetPixel(col, row, (Color) color);
        //        }
        //    }

        //    bitmap.Save(@"C:\Users\Hila\Downloads\CCITT_1.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
        //}

        private static Bitmap GetBitmap16Bit(Tiff tiff, short frameNumber, string name)
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
            imageResult = new Bitmap(imageWidth, imageLength, PixelFormat.Format16bppGrayScale);

            short[] buffer16Bit = null;
            for (int row = 0; row < imageLength; row++)
            {
                Rectangle imRect = new Rectangle(0, row, imageWidth, 1);
                BitmapData imgData = imageResult.LockBits(imRect, ImageLockMode.WriteOnly, PixelFormat.Format16bppGrayScale);
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

            //BitmapData imgdata = imageResult.LockBits(new Rectangle(0, 0, imageResult.Width, imageResult.Height), ImageLockMode.ReadOnly, imageResult.PixelFormat);
            //BitmapSource src = BitmapSource.Create(imgdata.Width, imgdata.Height, imageResult.HorizontalResolution, imageResult.VerticalResolution, System.Windows.Media.PixelFormats.Gray16, null,
            //    imgdata.Scan0, imgdata.Height * imgdata.Stride, imgdata.Stride);
            //imageResult.UnlockBits(imgdata);
            //using (FileStream stream = new FileStream(name, FileMode.Create))
            //{
            //    TiffBitmapEncoder encoder = new TiffBitmapEncoder();
            //    encoder.Compression = TiffCompressOption.Zip;
            //    encoder.Frames.Add(BitmapFrame.Create(src));
            //    encoder.Save(stream);
            //}

            return imageResult;
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
            Bitmap[] columnBitmaps = new Bitmap[rowsPerColumn];
            for (int currentChannel = 0; currentChannel < totalChannels; currentChannel++)
            {
                int currPos = 0;
                GetFrameNumbers(columnNumber, currentChannel, columnFrames);
                foreach (short frameNumber in columnFrames)
                {
                    columnBitmaps[currPos] = GetBitmap16Bit(scanTiff, frameNumber, "new"+currPos.ToString()+".tif");
                    currPos++;
                    //GetBitmap(scanTiff, frameNumber, imageLength);
                    //FramePixelsAsByteArray(scanTiff, frameNumber, imageLength);
                    //MemoryStream frameStream = FramePixelsAsByteArray(scanTiff, frameNumber, imageLength);
                    
                    //using (Image frame = Image.FromStream(frameStream))
                    //{
                    //    string name = "try" + frameNumber.ToString() + ".gif";
                    //    frame.Save(name);
                    //}
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
