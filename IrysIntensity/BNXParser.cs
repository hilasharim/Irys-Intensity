using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace IrysIntensity
{
    class BNXParser
    {
        private const string molFilePrefix = "Molecules";
        private const string molFileSuffix = ".mol";
        private const string scanFilesSubDir = "Detect Molecules";

        private static Dictionary<int,string> runIdsToNames = new Dictionary<int, string>(); //key - run ID, value - run name
        private static Dictionary<int, string> runIdsToMonths = new Dictionary<int, string>(); //key - run ID, value - run month
        public static Dictionary<int, int> runDBIds = new Dictionary<int, int>(); //key - run ID in file, value - run ID in DB
        public static List<List<Molecule>> moleculeListByRun = new List<List<Molecule>>();

        //returns run ID, run name (without the word 'swap' in the beginning) and month
        private static Tuple<int, string, string> ParseRunData(string runDataLine)
        {
            string runName = "";
            string runMonth = "";

            string runNamePattern = @"\\(.*(\d{4}-\d{2})-\d{2}_\d{2}_\d{2})";
            Regex r = new Regex(runNamePattern);

            string[] runInfo = runDataLine.Split('\t');
            Match m = r.Match(runInfo[1]);
            if (m.Success)
            {
                runName = m.Groups[1].ToString();
                runMonth = m.Groups[2].ToString();
                if (runName.StartsWith("swap")) {
                    runName = runName.Remove(0, 4);
                }
                runIdsToNames[int.Parse(runInfo[13])] = runName;
                runIdsToMonths[int.Parse(runInfo[13])] = runMonth;
            }

            return Tuple.Create(int.Parse(runInfo[13]), runName, runMonth);
        }

        //reads the BNX file, adds new molecules and runs to database, returns total number of molecules read or -1 if can't open file
        public static int ParseBNX(string bnx_file_path, int projectId, int alignmentChannel)
        {
            int totalReadMolecules = 0;
             
            if (!File.Exists(bnx_file_path))
            {
                return -1;
            }

            DatabaseManager.SetConnection();
            DatabaseManager.sql_con.Open();

            using (DatabaseManager.sql_con)
            {
                using (var transaction = DatabaseManager.sql_con.BeginTransaction())
                {
                    using (var fileStream = File.OpenRead(bnx_file_path))
                    {
                        using (StreamReader streamReader = new StreamReader(fileStream))
                        {
                            string line;
                            while ((line = streamReader.ReadLine()) != null)
                            {
                                if (line.StartsWith("# Run Data")) //run data header file
                                {
                                    Tuple<int, string, string> runData = ParseRunData(line);
                                    //add run to database if the combination (projectId, run name, run month) doesn't exist
                                    DatabaseManager.AddRun(projectId, runData.Item2, runData.Item3);
                                    runDBIds[runData.Item1] = DatabaseManager.FindRunId(projectId, runData.Item2); //save the DB run id
                                    moleculeListByRun.Add(new List<Molecule>());
                                }
                                else if (line.StartsWith("0")) //all molecule information lines start with '0'
                                {
                                    //process molecule line, including information from XMAP
                                    //BNX lines contain following information: label_channel, molecule_ID (same as XMAP), length, avg_intensity, SNR, num_labels, original_mol_id, scan_number,
                                    //scan_direction, chip_id, flowcell, run_id, global_scan_number
                                    string[] bnxMolInfo = line.Split('\t');
                                    Molecule mol = new Molecule(int.Parse(bnxMolInfo[1]), float.Parse(bnxMolInfo[2]), int.Parse(bnxMolInfo[11]), int.Parse(bnxMolInfo[7]), int.Parse(bnxMolInfo[6]));
                                    do { line = streamReader.ReadLine(); } while (!line.StartsWith(alignmentChannel.ToString()));
                                    if (line.LastIndexOf('\t') - line.IndexOf('\t') - 1 > 0) 
                                    {
                                        mol.AlignmentChannelLabelPositions = line.Substring(line.IndexOf('\t') + 1, line.LastIndexOf('\t') - line.IndexOf('\t') - 1);
                                    }
                                    else
                                    {
                                        mol.AlignmentChannelLabelPositions = "";
                                    }
                                    moleculeListByRun[int.Parse(bnxMolInfo[11]) - 1].Add(mol);
                                    totalReadMolecules++;
                                }
                            }
                        }
                    }
                    transaction.Commit();
                }

            }

            //sort the molecules by scan and original id for parsing of .mol files
            for (int run = 0; run < moleculeListByRun.Count; run++)
            {
                moleculeListByRun[run] = moleculeListByRun[run].OrderBy(mol => mol.Scan).ThenBy(mol => mol.OriginalId).ToList();
            }

            return totalReadMolecules;
        }

        //function receives a run id and the path to the .mol files of the run, and adds the image positions information to the relevant molecules
        //the molecules are sorted so there is no need to go back in the file to a line that's already been read
        private static void ParseRunMolFiles(string molFilesDirPath, int runId)
        {
            int prev_scan = 0;
            FileStream fileStream = null;
            StreamReader streamReader = null;
            string line;
            string[] molInfo;

            foreach (Molecule mol in moleculeListByRun[runId])
            {
                if (mol.Scan != prev_scan) //new scan - open the relevant .mol file
                {
                    if (fileStream != null)
                    {
                        fileStream.Close();
                        streamReader.Close();
                    }
                    prev_scan = mol.Scan;
                    string molFileName = molFilePrefix + mol.Scan.ToString() + molFileSuffix;
                    fileStream = File.OpenRead(Path.Combine(molFilesDirPath, molFileName));
                    streamReader = new StreamReader(fileStream);
                }
                
                do //read lines in file until you find the molecule's line
                {
                    line = streamReader.ReadLine();
                    molInfo = line.Split('\t');
                } while (!Char.IsNumber(line[0]) || int.Parse(molInfo[0]) != mol.OriginalId);
                                
                mol.Column = int.Parse(molInfo[5]);
                mol.RowStart = int.Parse(molInfo[6]);
                mol.RowEnd = int.Parse(molInfo[7]);
                mol.XStart = float.Parse(molInfo[8]);
                mol.XEnd = float.Parse(molInfo[10]);
                mol.YStart = float.Parse(molInfo[9]);
                mol.YEnd = float.Parse(molInfo[11]);
            }
            fileStream.Close();
            streamReader.Close();
        }

        //look for the run directory and return the position of the root directory it was found in. if more than one root directory exists - return -2. if not found - return -1.
        private static int FindRunPath(string[] rootFilesDirPaths, int runIndex)
        {
            int count = 0;
            int foundIndex = 0;
            foreach (string rootDir in rootFilesDirPaths)
            {
                if (Directory.Exists(Path.Combine(rootDir.Trim(), runIdsToMonths[runIndex + 1], runIdsToNames[runIndex + 1])))
                {
                    count++;
                    foundIndex = Array.IndexOf(rootFilesDirPaths, rootDir);
                }
            }
            if (count == 0)
            {
                return -1;
            }
            else if (count > 1)
            {
                return -2;
            }
            else
            {
                return foundIndex;
            }
        }

        public static void ParseAllMolFiles(string[] rootFilesDirPath)
        {
            for (int run = 0; run < moleculeListByRun.Count; run++)
            {
                int rootDirIdx = FindRunPath(rootFilesDirPath, run);
                if (rootDirIdx >= 0)
                {
                    string FilesDirLocation = Path.Combine(rootFilesDirPath[rootDirIdx].Trim(), runIdsToMonths[run + 1], runIdsToNames[run + 1], scanFilesSubDir);
                    ParseRunMolFiles(FilesDirLocation, run);
                }
            }
        }
    }
}
