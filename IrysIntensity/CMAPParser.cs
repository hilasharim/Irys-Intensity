using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using MathNet.Numerics;
using MathNet.Numerics.Interpolation;

namespace IrysIntensity
{
    class CMAPParser
    {
        public static Dictionary<string, int> chromNamesToIds = new Dictionary<string,int>();
        private static Dictionary<string, string> chromIdsToNames = new Dictionary<string,string>();
        private static Dictionary<string, int> chromLengths = new Dictionary<string,int>(); //map chromosome names to their length
        public static Dictionary<int, List<double>> rCmapPositions;
        public static Dictionary<int, List<double>> qCmapPositions;
        private const double bpPerPixel = 575.8;

        //returns -1 if file does not exist, otherwise reads into dictionaries chromosme IDs, names and lengths, and returns number of chromosomes read
        public static int ReadKeyFile(string keyFilePath)
        {
            int totalChromosomesRead = 0;

            if (!File.Exists(keyFilePath))
            {
                return -1;               
            }

            using (var fileStream = File.OpenRead(keyFilePath))
            {
                using (StreamReader streamReader = new StreamReader(fileStream))
                {
                    string line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        if (Char.IsNumber(line[0])) //if not header line
                        {
                            string[] chromInfo = line.Split('\t');
                            chromIdsToNames[chromInfo[0]] = chromInfo[1];
                            chromNamesToIds[chromInfo[1]] = int.Parse(chromInfo[0]);
                            chromLengths[chromInfo[1]] = int.Parse(chromInfo[2]);
                            totalChromosomesRead++;
                        }
                    }
                }
            }

            return totalChromosomesRead;
        }

        public static Dictionary<int, List<double>> ParseCmap(string cmapFilePath, int channel)
        {
            Dictionary<int, List<double>> cmapPositions = new Dictionary<int, List<double>>();
            int prevId = 0;
            int currId, labelChannel;
            double currPosition;
            if (!File.Exists(cmapFilePath))
            {
                return null;
            }
            using (var fileStream = File.OpenRead(cmapFilePath))
            {
                using (StreamReader streamReader = new StreamReader(fileStream))
                {
                    string line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        if (!line.StartsWith("#"))
                        {
                            string[] labelInfo = line.Split('\t');
                            currId = int.Parse(labelInfo[0]);
                            labelChannel = int.Parse(labelInfo[4]);
                            currPosition = double.Parse(labelInfo[5]);
                            if (currId != prevId)
                            {
                                cmapPositions[currId] = new List<double>();
                                prevId = currId;
                            }
                            if (labelChannel == channel)
                            {
                                cmapPositions[currId].Add(currPosition);
                            }
                        }
                    }
                }
            }
            return cmapPositions;
        }

        public static void FitMoleculeToRef(Molecule molecule, double[] moleculePixels)
        {
            List<Tuple<int, int>> labelIdAlignmentPositions = XMAPParser.ParseAlignmentString(molecule.AlignmentString); //in each tuple item1 = ref, item2 = molecule
            string[] molAlignmentLabelPositions = molecule.AlignmentChannelLabelPositions.Split('\t');
            List<double> refPositions = new List<double>();
            List<double> molPositions = new List<double>();
            Dictionary<int, double> molFitPixelPositions = new Dictionary<int, double>();
            double currRefPosition, currMolPosition;
            foreach (Tuple<int, int> labelIdAlignment in labelIdAlignmentPositions)
            {
                currRefPosition = rCmapPositions[molecule.ChromId][labelIdAlignment.Item1 - 1]; //position along chromosome in bp
                currMolPosition = double.Parse(molAlignmentLabelPositions[labelIdAlignment.Item2 - 1]) / bpPerPixel; //position along molecule in pixels
                if (!refPositions.Contains(currRefPosition) && !molPositions.Contains(currMolPosition))
                {
                    refPositions.Add(currRefPosition);
                    molPositions.Add(currMolPosition);
                }
            }
            if (molecule.Orientation == "-")
            {
                refPositions.Reverse();
                molPositions.Reverse();
            }
            IInterpolation linearInterpolation = Interpolate.Linear(molPositions, refPositions);
            for (int pixelPosition = 0; pixelPosition < moleculePixels.Length; pixelPosition++)
            {
                double fittedPosition = linearInterpolation.Interpolate(pixelPosition);
                molFitPixelPositions[(int)(Math.Round(fittedPosition))] = moleculePixels[pixelPosition];
            }
            using (StreamWriter sw = new StreamWriter(@"moleculeIntensitiesZeroBased" + molecule.MoleculeId.ToString() + ".txt"))
            {
                foreach (KeyValuePair<int, double> kvp in molFitPixelPositions)
                {
                    string print = String.Format("{0}\t{1}\t{2}", "chr"+molecule.ChromId, kvp.Key, kvp.Value);
                    sw.WriteLine(print);
                }
            }
        }
    }
}
