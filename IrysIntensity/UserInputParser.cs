using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace IrysIntensity
{
    class UserInputParser
    {
        public static int[] getMolIds(string idTxtBxText, string idFilePathText)
        {
            string[] stringMolIds;
            if (!String.IsNullOrEmpty(idTxtBxText)) //get from textbox
            {
                stringMolIds = idTxtBxText.Split('\n');
            }
            else //get from file
            {
                if (!File.Exists(idFilePathText))
                {
                    return null;
                }
                stringMolIds = File.ReadAllLines(idFilePathText);
            }
            return Array.ConvertAll(stringMolIds, int.Parse);
        }

        //return tuple of chromID, start, end. start, end = -1 if only chrom name
        private static Tuple<int, int, int> ParseLocation(string locationLine)
        {
            string locationPattern = @"(\w+)(:(\d*)-(\d*))?";
            Regex r = new Regex(locationPattern);
            Match m = r.Match(locationLine);

            if (m.Success)
            {
                if (m.Groups[1].Success && !CMAPParser.chromNamesToIds.ContainsKey(m.Groups[1].ToString())) //the chromosome name doesn't exist in the key file
                {
                    return null;
                }

                if (m.Groups[1].Success && !String.IsNullOrEmpty(m.Groups[3].Value) && !String.IsNullOrEmpty(m.Groups[4].Value)) //chrom, start, end
                {
                    return Tuple.Create(CMAPParser.chromNamesToIds[m.Groups[1].ToString()], int.Parse(m.Groups[3].ToString()), int.Parse(m.Groups[4].ToString()));
                }
                else if (m.Groups[1].Success && String.IsNullOrEmpty(m.Groups[3].Value) && String.IsNullOrEmpty(m.Groups[4].Value)) //only chrom
                {
                    return Tuple.Create(CMAPParser.chromNamesToIds[m.Groups[1].ToString()], -1, -1);
                }
                else //doesn't match pattern
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        private static Tuple<List<int>, List<Tuple<int, int, int>>> ParseAllLocations(string[] locationLines)
        {
            List<int> chromosomeNamesOnly = new List<int>();
            List<Tuple<int,int,int>> chromNameStartEnds = new List<Tuple<int,int,int>>();

            foreach (string locationLine in locationLines)
            {
                Tuple<int, int, int> newLocation = ParseLocation(locationLine);
                if (newLocation != null) //answers location format requirements
                {
                    if (newLocation.Item2 != -1) //chrom,start,end
                    {
                        chromNameStartEnds.Add(newLocation);
                    }
                    else
                    {
                        chromosomeNamesOnly.Add(newLocation.Item1);
                    }
                }
            }

            return Tuple.Create(chromosomeNamesOnly, chromNameStartEnds);
        }

        public static Tuple<List<int>, List<Tuple<int, int, int>>> getLocations(string locationTxtBxText, string locationFilePathText)
        {
            string[] stringMolLocations;
            if (!String.IsNullOrEmpty(locationTxtBxText)) //get from text box
            {
                stringMolLocations = locationTxtBxText.Split('\n');
            }
            else //get from file
            {
                if (!File.Exists(locationFilePathText))
                {
                    return null;
                }
                stringMolLocations = File.ReadAllLines(locationFilePathText);
            }
            return ParseAllLocations(stringMolLocations);
        }
    }
}
