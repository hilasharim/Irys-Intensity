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
                int slashPosition = runName.LastIndexOf('\\');
                runName = slashPosition < 0 ? runName : runName.Substring(slashPosition + 1);
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
        public static int ParseBNX(string bnx_file_path, int projectId, int alignmentChannel, string[] rootFilesDirPath)
        {
            int totalReadMolecules = 0;
            List<Molecule> runMolecules = new List<Molecule>();

            if (!File.Exists(bnx_file_path))
            {
                return -1;
            }

            DatabaseManager.SetConnection();
            DatabaseManager.sql_con.Open();

            using (DatabaseManager.sql_con)
            {
                using (var fileStream = File.OpenRead(bnx_file_path))
                {
                    using (StreamReader streamReader = new StreamReader(fileStream))
                    {
                        string line = ReadRunData(streamReader, projectId);
                        foreach (int runId in runDBIds.Keys)
                        {
                            Tuple<int, string> readRunOutput = ReadAllRunMolecules(line, streamReader, runMolecules, runId, alignmentChannel);
                            totalReadMolecules += readRunOutput.Item1; //molecules ordered in BNX by runId, read until first molecule of next run
                            runMolecules = runMolecules.OrderBy(mol => mol.Scan).ThenBy(mol => mol.OriginalId).ToList(); //sort run molecules by scan and molecule ID
                            ParseRunMolFiles(rootFilesDirPath, runId, runMolecules); //read .Mol files of specific run
                            DatabaseManager.AddRunMolecules(runMolecules, projectId, runId); //transaction to save to DB all molecules of specific run
                            runMolecules.Clear(); //clear the memory of the molecule list to make room for next run
                            line = readRunOutput.Item2;
                        }
                    }
                }
            }

            return totalReadMolecules;
        }

        //function to Parse #Run Data lines from the BNX file. adds new runs to DB. returns the last line not corresponding to comment in the file.
        private static string ReadRunData(StreamReader bnxFile, int projectId)
        {
            using (var transaction = DatabaseManager.sql_con.BeginTransaction())
            {
                string line;
                while (((line = bnxFile.ReadLine()) != null) && (line.StartsWith("#")))
                {
                    if (line.StartsWith("# Run Data"))
                    {
                        Tuple<int, string, string> runData = ParseRunData(line);
                        DatabaseManager.AddRun(projectId, runData.Item2, runData.Item3);
                        runDBIds[runData.Item1] = DatabaseManager.FindRunId(projectId, runData.Item2); //save the DB run id
                    }
                }
                transaction.Commit();
                return line;
            }
        }

        //function to go over BNX lines of molecules corresponding to specific run. returns the number of molecules read, and the last line read, corresponding to the first line of the next run. 
        //updates the runMolecules list with found molecules.
        private static Tuple<int,string> ReadAllRunMolecules(string line, StreamReader bnxFile, List<Molecule> runMolecules, int currRunID, int alignmentChannel)
        {
            //BNX lines contain following information: label_channel, molecule_ID (same as XMAP), length, avg_intensity, SNR, num_labels, original_mol_id, scan_number,
            //scan_direction, chip_id, flowcell, run_id, global_scan_number
            int totalRunMols = 0;
            while ((line != null) && (int.Parse(line.Split('\t')[11]) == currRunID))
            {
                string[] bnxMolInfo = line.Split('\t');
                Molecule mol = new Molecule(int.Parse(bnxMolInfo[1]), float.Parse(bnxMolInfo[2]), int.Parse(bnxMolInfo[11]), int.Parse(bnxMolInfo[7]), int.Parse(bnxMolInfo[6]));
                do { line = bnxFile.ReadLine(); } while (!line.StartsWith(alignmentChannel.ToString()));
                if (line.LastIndexOf('\t') - line.IndexOf('\t') - 1 > 0)
                {
                    mol.AlignmentChannelLabelPositions = line.Substring(line.IndexOf('\t') + 1, line.LastIndexOf('\t') - line.IndexOf('\t') - 1);
                }
                else
                {
                    mol.AlignmentChannelLabelPositions = "";
                }
                runMolecules.Add(mol);
                totalRunMols++;
                do { line = bnxFile.ReadLine(); } while ((line != null) && (!line.StartsWith("0")));
            }
            return new Tuple<int,string>(totalRunMols, line);
        }

        //function receives a run id and the path to the .mol files of the run, and adds the image positions information to the relevant molecules
        //the molecules are sorted so there is no need to go back in the file to a line that's already been read
        private static void ParseRunMolFiles(/*string molFilesDirPath*/string[] rootFilesDirPath, int runId, List<Molecule> runMolecules)
        {
            int rootDirIdx = FindRunPath(rootFilesDirPath, runId);
            if (rootDirIdx < 0)
            {
                return;
            }
            string FilesDirLocation = Path.Combine(rootFilesDirPath[rootDirIdx].Trim(), runIdsToMonths[runId], runIdsToNames[runId], scanFilesSubDir);
            int prev_scan = 0;
            FileStream fileStream = null;
            StreamReader streamReader = null;
            string line;
            string[] molInfo;

            foreach (Molecule mol in runMolecules)
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
                    if (!File.Exists(Path.Combine(FilesDirLocation, molFileName)))
                    {
                        return;
                    }
                    fileStream = File.OpenRead(Path.Combine(FilesDirLocation, molFileName));
                    streamReader = new StreamReader(fileStream);
                }
                
                do //read lines in file until you find the molecule's line
                {
                    line = streamReader.ReadLine();
                    molInfo = null;
                    if (line != null)
                    {
                        molInfo = line.Split('\t');
                    }
                } while ((line != null) && (!Char.IsNumber(line[0]) || int.Parse(molInfo[0]) != mol.OriginalId));

                if (molInfo != null)
                {
                    mol.Column = int.Parse(molInfo[5]);
                    mol.RowStart = int.Parse(molInfo[6]);
                    mol.RowEnd = int.Parse(molInfo[7]);
                    mol.XStart = float.Parse(molInfo[8]);
                    mol.XEnd = float.Parse(molInfo[10]);
                    mol.YStart = float.Parse(molInfo[9]);
                    mol.YEnd = float.Parse(molInfo[11]);
                }
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
                if (Directory.Exists(Path.Combine(rootDir.Trim(), runIdsToMonths[runIndex], runIdsToNames[runIndex])))
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
    }
}
