using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace IrysIntensity
{
    class CMAPParser
    {
        public static Dictionary<string, int> chromNamesToIds = new Dictionary<string,int>();
        private static Dictionary<string, string> chromIdsToNames = new Dictionary<string,string>();
        private static Dictionary<string, int> chromLengths = new Dictionary<string,int>(); //map chromosome names to their length

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
    }
}
