using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IrysIntensity
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new IrysIntensity());
            }
            //process for genome alignment for single location from command line arguments, without opening the user interface
            //the following arguments are expected: key file path, r_cmap path, output directory, project id, min mol length, min confidence, min aligned length percent, location, saveChannel1 flag, saveChannel2 flag
            else
            {
                GenomeAlignmentFromCmd(args);
            }
        }

        static int GenomeAlignmentFromCmd(string[] args)
        {
            string keyFilePath = args[0];
            string rCmapFilePath = args[1];
            string outputDirPath = args[2];
            int projectId = int.Parse(args[3]);
            float lengthFilter = float.Parse(args[4]) * 1000;
            float confidenceFilter = float.Parse(args[5]);
            float alignedLenPercentFilter = float.Parse(args[6]);
            string queryLocation = args[7];
            bool saveChannel1 = int.Parse(args[8]) == 1 ? true : false;
            bool saveChannel2 = int.Parse(args[9]) == 1 ? true : false;

            int readKeyChroms = CMAPParser.ReadKeyFile(keyFilePath);
            Tuple<List<int>, List<Tuple<int, int, int>>> locations = UserInputParser.getLocations(queryLocation, null);
            List<int> chromIdsFilter = locations.Item1;
            List<Tuple<int, int, int>> chromStartEndFilter = locations.Item2;
            CMAPParser.rCmapPositions = CMAPParser.ParseCmap(rCmapFilePath, 1);

            List<Molecule> selectedMolecules = DatabaseManager.SelectMoleculesForGenomeAlignment(projectId, lengthFilter, confidenceFilter, alignedLenPercentFilter, null, chromIdsFilter,
                chromStartEndFilter);
            foreach (Molecule molecule in selectedMolecules)
            {
                CMAPParser.FitMoleculeToRef(molecule, saveChannel1, saveChannel2, outputDirPath);
            }

            return 0;
        }
    }
}
