using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

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
    }
}
