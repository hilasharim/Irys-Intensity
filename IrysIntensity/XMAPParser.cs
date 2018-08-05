using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace IrysIntensity
{
    class XMAPParser
    {
        public static Dictionary<int, List<string>> moleculeData = new Dictionary<int, List<string>>();

        private static float getPercentAlignedMolLength(string QueryStartPos, string QueryEndPos, string QueryLen)
        {
            return (Math.Abs(float.Parse(QueryEndPos) - float.Parse(QueryStartPos))) / float.Parse(QueryLen) * 100;
        }

        //parse the XMAP and return number of molecules read, or -1 if file does not exit
        public static int ParseXmap(string xmapFilePath)
        {
            int totalMoleculesRead = 0;

            if (!(File.Exists(xmapFilePath)))
            {
                return -1;
            }

            using (var fileStream = File.OpenRead(xmapFilePath))
            {
                using (StreamReader streamReader = new StreamReader(fileStream))
                {
                    string line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        if (!line.StartsWith("#")) //if not a header line
                        {
                            string[] moleculeInfo = line.Split('\t');
                            //moleculeData has the following information: RefContigID, RefStartPos, RefEndPos, Orientation, Confidence, AlignmentString, AlignedMolLengthPercent; all as strings
                            moleculeData[int.Parse(moleculeInfo[1])] = new List<string> {moleculeInfo[2], moleculeInfo[5], moleculeInfo[6], moleculeInfo[7], moleculeInfo[8], moleculeInfo[13], 
                                                                                 getPercentAlignedMolLength(moleculeInfo[3], moleculeInfo[4], moleculeInfo[10]).ToString()};
                            totalMoleculesRead++;
                        }
                    }
                }
            }

            return totalMoleculesRead;
        }

        //parse the XMAP alignment string to return a list of tuples where item1 = reference position, item2 = label position
        public static List<Tuple<int, int>> ParseAlignmentString(string molAlignmentString)
        {
            List<Tuple<int,int>> matches = new List<Tuple<int,int>>();
            molAlignmentString = molAlignmentString.Substring(1, molAlignmentString.Length - 2);

            string[] stringSeperators = new string[] {")("};
            string[] stringMatchTuples = molAlignmentString.Split(stringSeperators, StringSplitOptions.None);
            foreach (string stringMatchTuple in stringMatchTuples)
            {
                string[] positions = stringMatchTuple.Split(',');
                Tuple<int, int> match = new Tuple<int, int>(int.Parse(positions[0]), int.Parse(positions[1]));
                matches.Add(match);
            }
            return matches;
        }
    }
}
