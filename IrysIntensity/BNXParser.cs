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
        private static Dictionary<string,string> runIds = new Dictionary<String, String>(); //key - run ID, value - run name
        private static Dictionary<string, int> runDBIds = new Dictionary<string, int>(); //key - run ID in file, value - run ID in DB

        //returns run ID, run name (without the word 'swap' in the beginning) and month
        private static Tuple<string, string, string> ParseRunData(string runDataLine)
        {
            string runName = "";
            string runMonth = "";

            string runNamePattern = @"\w*(\d{4}-\d{2})-\d{2}_\d{2}_\d{2}";
            Regex r = new Regex(runNamePattern);

            string[] runInfo = runDataLine.Split('\t');
            Match m = r.Match(runInfo[1]);
            if (m.Success)
            {
                runName = m.ToString();
                runMonth = m.Groups[1].ToString();
                if (runName.StartsWith("swap")) {
                    runName = runName.Remove(0, 4);
                }
                runIds.Add(runInfo[13], runName);
            }

            return Tuple.Create(runInfo[13], runName, runMonth);
        }

        //reads the BNX file, adds new molecules and runs to database, returns total number of molecules read or -1 if can't open file
        public static int ParseBNX(string bnx_file_path, int projectId)
        {
            int totalReadMolecules = 0;

            if (!File.Exists(bnx_file_path))
            {
                return -1;
            }

            DatabaseManager.SetConnection();
            DatabaseManager.sql_con.Open();

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
                                Tuple<string, string, string> runData = ParseRunData(line);
                                //add run to database if the combination (projectId, run name, run month) doesn't exist
                                DatabaseManager.AddRun(projectId, runData.Item2, runData.Item3);
                                runDBIds.Add(runData.Item1, DatabaseManager.FindRunId(projectId, runData.Item2));
                            }
                            else if (line.StartsWith("0")) //all molecule information lines start with '0'
                            {
                                //process molecule line, including information from XMAP
                                //BNX lines contain following information: label_channel, molecule_ID (same as XMAP), length, avg_intensity, SNR, num_labels, original_mol_id, scan_number,
                                //scan_direction, chip_id, flowcell, run_id, global_scan_number
                                string[] bnxMolInfo = line.Split('\t');
                                //int runId = DatabaseManager.FindRunId(projectId, runIds[bnxMolInfo[11]]);
                                if (XMAPParser.moleculeData.ContainsKey(bnxMolInfo[1])) //if molecule was aligned - add to database alignment information
                                {
                                    DatabaseManager.AddMolecule(projectId, runDBIds[bnxMolInfo[11]], int.Parse(bnxMolInfo[1]), int.Parse(bnxMolInfo[7]), int.Parse(bnxMolInfo[6]), float.Parse(bnxMolInfo[2]), 1,
                                    int.Parse(XMAPParser.moleculeData[bnxMolInfo[1]][0]), float.Parse(XMAPParser.moleculeData[bnxMolInfo[1]][1]), float.Parse(XMAPParser.moleculeData[bnxMolInfo[1]][2]),
                                    XMAPParser.moleculeData[bnxMolInfo[1]][3], float.Parse(XMAPParser.moleculeData[bnxMolInfo[1]][4]), XMAPParser.moleculeData[bnxMolInfo[1]][5],
                                    float.Parse(XMAPParser.moleculeData[bnxMolInfo[1]][6]));
                                }
                                else //otherwise add only information from BNX
                                {
                                    DatabaseManager.AddMolecule(projectId, runDBIds[bnxMolInfo[11]], int.Parse(bnxMolInfo[1]), int.Parse(bnxMolInfo[7]), int.Parse(bnxMolInfo[6]), float.Parse(bnxMolInfo[2]), 0,
                                    0, 0, 0, "null", 0, "null", 0);
                                }

                                totalReadMolecules++;
                            }
                        }
                    }
                }
                transaction.Commit();
            }

            DatabaseManager.sql_con.Close();
            
            return totalReadMolecules;
        }
    }
}
