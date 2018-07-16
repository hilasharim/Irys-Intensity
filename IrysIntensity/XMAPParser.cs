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
        public static Dictionary<string, List<string>> moleculeData = new Dictionary<string, List<string>>();

        private static float getPercentAlignedMolLength(string QueryStartPos, string QueryEndPos, string QueryLen)
        {
            return (Math.Abs(float.Parse(QueryEndPos) - float.Parse(QueryStartPos))) / float.Parse(QueryLen) * 100;
        }

        //parse the XMAP and return number of molecules read, or -1 if file does not exit
        public static int ParseXmap(string xmap_file_path)
        {
            int totalMoleculesRead = 0;

            if (!(File.Exists(xmap_file_path)))
            {
                return -1;
            }

            using (var fileStream = File.OpenRead(xmap_file_path))
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
                            moleculeData[moleculeInfo[1]] = new List<string> { moleculeInfo[2], moleculeInfo[5], moleculeInfo[6], moleculeInfo[7], moleculeInfo[8], moleculeInfo[13], 
                                                                                 getPercentAlignedMolLength(moleculeInfo[3], moleculeInfo[4], moleculeInfo[10]).ToString()};
                            totalMoleculesRead++;
                        }
                    }
                }
            }

            return totalMoleculesRead;
        }
    }
}
