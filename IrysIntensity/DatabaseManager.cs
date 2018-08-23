using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Data;
using System.Windows.Forms;

namespace IrysIntensity
{
    class DatabaseManager
    {
        public static SQLiteConnection sql_con;
        private static SQLiteCommand sql_cmd;

        public static void SetConnection()
        {
            sql_con = new SQLiteConnection("Data Source=MoleculeData.db; Version=3;New=False;");
        }

        private static void ExecuteNonQueryCmd(string txtQuery)
        {
            sql_cmd = new SQLiteCommand(txtQuery, sql_con);
            sql_cmd.ExecuteNonQuery();
        }

        private static int ExecuteScalarCmd(string txtQuery)
        {
            int count = 0;
            sql_cmd = new SQLiteCommand(txtQuery, sql_con);
            count = Convert.ToInt32(sql_cmd.ExecuteScalar());
            return count;
        }

        public static void setUpDBOnStartUp()
        {
            SetConnection();
            sql_con.Open();
            string create_projects_table_command = "CREATE TABLE IF NOT EXISTS projects (id INTEGER PRIMARY KEY, name TEXT NOT NULL UNIQUE)";
            string create_run_table_command = "CREATE TABLE IF NOT EXISTS runs (id INTEGER PRIMARY KEY, projectId INTEGER NOT NULL, name TEXT NOT NULL, month TEXT NOT NULL, UNIQUE(projectId, name, month))";
            string create_molecules_table_command = @"CREATE TABLE IF NOT EXISTS molecules (id INTEGER PRIMARY KEY, projectId INTEGER NOT NULL, runId INTEGER NOT NULL, molId INTEGER NOT NULL,
                                                    scan INTEGER NOT NULL, originalID INTEGER NOT NULL, length REAL NOT NULL, col INTEGER NOT NULL, rowStart INTEGER NOT NULL, rowEnd INTEGER NOT NULL,
                                                    xStart READL NOT NULL, yStart REAL NOT NULL, xEnd REAL NOT NULL, yEnd REAL NOT NULL, alignmentChannelPositions TEXT NOT NULL,
                                                    mapped INTEGER NOT NULL, chromId INTEGER, molStart REAL, molEnd REAL,
                                                    orientation TEXT, confidence REAL, alignmentString TEXT, percentAligned REAL, UNIQUE(projectId, runId, scan, originalId))";

            string[] create_index_commands = {"CREATE INDEX IF NOT EXISTS molecule_ids ON molecules (projectId, molId)",
                                              "CREATE INDEX IF NOT EXISTS aligned ON molecules (mapped)",
                                              "CREATE INDEX IF NOT EXISTS lengths ON molecules (length)",
                                              "CREATE INDEX IF NOT EXISTS conf ON molecules (confidence)",
                                              "CREATE INDEX IF NOT EXISTS percent_alignment ON molecules (percentAligned)",
                                              "CREATE INDEX IF NOT EXISTS alignment_pos ON molecules (chromId, molStart, molEnd)"};

            using (sql_con)
            {
                ExecuteNonQueryCmd(create_projects_table_command);
                ExecuteNonQueryCmd(create_run_table_command);
                ExecuteNonQueryCmd(create_molecules_table_command);
                foreach (string command_text in create_index_commands)
                {
                    ExecuteNonQueryCmd(command_text);
                }
            }
        }

        public static void updateComboBox(ComboBox cmbBoxToUpdate, string display, string value, string tableName)
        {
            DataTable dt = new DataTable();
            SetConnection();
            using (sql_con)
            {
                using (SQLiteDataAdapter da = new SQLiteDataAdapter("SELECT * FROM " + tableName, sql_con))
                {
                    dt.Clear();
                    da.Fill(dt);
                    cmbBoxToUpdate.DisplayMember = display;
                    cmbBoxToUpdate.ValueMember = value;
                    cmbBoxToUpdate.DataSource = dt;
                }
            }            
        }

        public static void AddProject(string projectName)
        {
            SetConnection();
            sql_con.Open();
            using (sql_con)
            {
                string exists_project_name_command = "SELECT COUNT(id) from projects where name = '" + projectName + "'";
                string add_project_command = "INSERT INTO projects (name) VALUES (\"" + projectName + "\")";

                if (ExecuteScalarCmd(exists_project_name_command) > 0)
                {
                    MessageBox.Show("Project name already exists");
                }
                else
                {
                    ExecuteNonQueryCmd(add_project_command);
                }
            }
        }

        public static void AddRun(int projectId, string runName, string runMonth)
        {
            string add_run_command = "INSERT OR IGNORE INTO runs (projectId, name, month) VALUES (@param1, @param2, @param3)";

            sql_cmd = new SQLiteCommand(add_run_command, sql_con);
            sql_cmd.Parameters.Add(new SQLiteParameter("@param1", projectId));
            sql_cmd.Parameters.Add(new SQLiteParameter("@param2", runName));
            sql_cmd.Parameters.Add(new SQLiteParameter("@param3", runMonth));
            sql_cmd.ExecuteNonQuery();
        }

        public static int FindRunId(int projectId, string runName)
        {
            string select_run_command = "SELECT id FROM runs WHERE projectId = @param1 AND name = @param2";
            using (sql_cmd = new SQLiteCommand(select_run_command, sql_con))
            {
                sql_cmd.Parameters.Add(new SQLiteParameter("@param1", projectId));
                sql_cmd.Parameters.Add(new SQLiteParameter("@param2", runName));
                using (SQLiteDataReader reader = sql_cmd.ExecuteReader())
                {
                    reader.Read();
                    return Convert.ToInt32(reader["id"]);
                }
            }
        }

        private static void AddMolecule(int projectId, int runId, int molId, int scan, int originalId, float length, int column, int rowStart, int rowEnd, double xStart, double yStart, double xEnd, double yEnd,
            int mapped, int chromId, float start, float end, string orientation, float confidence, string alignmentString, float percentAligned, string alignmentChannelPositions)
        {
            string add_molecule_command = @"INSERT OR IGNORE INTO molecules (projectId, runId, molId, scan, originalID, length, col, rowStart, rowEnd, xStart, yStart, xEnd, yEnd, mapped, 
                                            chromId, molStart, molEnd, orientation, confidence, alignmentString, percentAligned, alignmentChannelPositions) 
                                            VALUES (@param1, @param2, @param3, @param4, @param5, @param6, @param7, @param8, @param9, @param10, @param11, @param12, @param13, @param14, @param15,
                                            @param16, @param17, @param18, @param19, @param20, @param21, @param22)";

            using (sql_cmd = new SQLiteCommand(add_molecule_command, sql_con))
            {
                sql_cmd.Parameters.Add(new SQLiteParameter("@param1", projectId));
                sql_cmd.Parameters.Add(new SQLiteParameter("@param2", runId));
                sql_cmd.Parameters.Add(new SQLiteParameter("@param3", molId));
                sql_cmd.Parameters.Add(new SQLiteParameter("@param4", scan));
                sql_cmd.Parameters.Add(new SQLiteParameter("@param5", originalId));
                sql_cmd.Parameters.Add(new SQLiteParameter("@param6", length));
                sql_cmd.Parameters.Add(new SQLiteParameter("@param7", column));
                sql_cmd.Parameters.Add(new SQLiteParameter("@param8", rowStart));
                sql_cmd.Parameters.Add(new SQLiteParameter("@param9", rowEnd));
                sql_cmd.Parameters.Add(new SQLiteParameter("@param10", xStart));
                sql_cmd.Parameters.Add(new SQLiteParameter("@param11", yStart));
                sql_cmd.Parameters.Add(new SQLiteParameter("@param12", xEnd));
                sql_cmd.Parameters.Add(new SQLiteParameter("@param13", yEnd));
                sql_cmd.Parameters.Add(new SQLiteParameter("@param14", mapped));
                sql_cmd.Parameters.Add(new SQLiteParameter("@param15", chromId));
                sql_cmd.Parameters.Add(new SQLiteParameter("@param16", start));
                sql_cmd.Parameters.Add(new SQLiteParameter("@param17", end));
                sql_cmd.Parameters.Add(new SQLiteParameter("@param18", orientation));
                sql_cmd.Parameters.Add(new SQLiteParameter("@param19", confidence));
                sql_cmd.Parameters.Add(new SQLiteParameter("@param20", alignmentString));
                sql_cmd.Parameters.Add(new SQLiteParameter("@param21", percentAligned));
                sql_cmd.Parameters.Add(new SQLiteParameter("@param22", alignmentChannelPositions));
                sql_cmd.ExecuteNonQuery();
            }
        }

        public static void AddAllMolecules(List<List<Molecule>> moleculeListByRun, int projectId)
        {
            SetConnection();
            sql_con.Open();
            using (sql_con)
            {
                using (var transaction = sql_con.BeginTransaction())
                {
                    for (int run = 0; run < moleculeListByRun.Count; run++)
                    {
                        foreach (Molecule mol in moleculeListByRun[run])
                        {
                            if (XMAPParser.moleculeData.ContainsKey(mol.MoleculeId)) //molecule aligned - add it to DB with alignment data
                            {
                                AddMolecule(projectId, BNXParser.runDBIds[run + 1], mol.MoleculeId, mol.Scan, mol.OriginalId, mol.Length, mol.Column, mol.RowStart, mol.RowEnd, mol.XStart, mol.YStart, mol.XEnd,
                                    mol.YEnd, 1, int.Parse(XMAPParser.moleculeData[mol.MoleculeId][0]), float.Parse(XMAPParser.moleculeData[mol.MoleculeId][1]),
                                    float.Parse(XMAPParser.moleculeData[mol.MoleculeId][2]), XMAPParser.moleculeData[mol.MoleculeId][3], float.Parse(XMAPParser.moleculeData[mol.MoleculeId][4]),
                                    XMAPParser.moleculeData[mol.MoleculeId][5], float.Parse(XMAPParser.moleculeData[mol.MoleculeId][6]), mol.AlignmentChannelLabelPositions);
                            }
                            else //add without alignment data, only information from BNX and MOL files
                            {
                                AddMolecule(projectId, BNXParser.runDBIds[run + 1], mol.MoleculeId, mol.Scan, mol.OriginalId, mol.Length, mol.Column, mol.RowStart, mol.RowEnd, mol.XStart, mol.YStart, mol.XEnd,
                                    mol.YEnd, 0, 0, 0, 0, "null", 0, "null", 0, mol.AlignmentChannelLabelPositions);
                            }
                        }
                    }
                    transaction.Commit();
                }
            }
        }

        /*called when sql_con is already open and inside a using statement*/
        private static int CountProjectRuns(int projectId)
        {
            string countRunsCommand = "SELECT COUNT(DISTINCT runId) FROM molecules WHERE projectId = @param1";
            using (sql_cmd = new SQLiteCommand(countRunsCommand, sql_con))
            {
                sql_cmd.Parameters.Add(new SQLiteParameter("@param1", projectId));
                int count = Convert.ToInt32(sql_cmd.ExecuteScalar());
                return count;
            }
        }

        /*called when sql_con is already open and inside a using statement*/
        private static int GetMaxScanNumber(int projectId)
        {
            string maxScanCommand = "SELECT MAX(DISTINCT scan) FROM molecules WHERE projectId = @param1";
            using (sql_cmd = new SQLiteCommand(maxScanCommand, sql_con))
            {
                sql_cmd.Parameters.Add(new SQLiteParameter("@param1", projectId));
                int maxScan = Convert.ToInt32(sql_cmd.ExecuteScalar());
                return maxScan;
            }
        }

        private static void AddListToINCmd(int[] valuesArray, int startParamNum)
        {
            List<string> paramNames = new List<string>();
            foreach (int value in valuesArray)
            {
                string newParamName = String.Format("@param{0}", startParamNum++);
                paramNames.Add(newParamName);
                sql_cmd.Parameters.Add(new SQLiteParameter(newParamName, value));
            }

            sql_cmd.CommandText += ("("+ String.Join(",", paramNames)+ ")");
        }

        /*called when sql_con is open and inside a using statement*/
        private static void buildSelectMoleculesCommand(int projectId, int mappedFilter, float lengthFilter, float confidenceFilter, float percentAlignedFilter, int[] molIdsFilter,
            List<int> chromIdsFilter, List<Tuple<int, int, int>> chromStartEndsFilter)
        {
            string selectMoleculesCommand = "SELECT molId, runId, scan, col, rowStart, rowEnd, xStart, yStart, xEnd, yEnd, mapped, chromId, alignmentString, orientation, alignmentChannelPositions FROM molecules WHERE projectId = @param1 AND ";
            int optParamStartVal = 5;
            if (mappedFilter == 1)
            {
                selectMoleculesCommand += "mapped = 1 AND ";
            }
            selectMoleculesCommand += "length >= @param2 AND confidence >= @param3 AND percentAligned >= @param4";
            sql_cmd = new SQLiteCommand(selectMoleculesCommand, sql_con);
            sql_cmd.Parameters.Add(new SQLiteParameter("@param1", projectId));
            sql_cmd.Parameters.Add(new SQLiteParameter("@param2", lengthFilter));
            sql_cmd.Parameters.Add(new SQLiteParameter("@param3", confidenceFilter));
            sql_cmd.Parameters.Add(new SQLiteParameter("@param4", percentAlignedFilter));
            if (molIdsFilter != null || (chromIdsFilter != null && chromIdsFilter.Count > 0) || (chromStartEndsFilter != null && chromStartEndsFilter.Count > 0))
            {
                sql_cmd.CommandText += " OR (";
                if (molIdsFilter != null) //add mol IDs filter as parameters to SELECT IN
                {
                    sql_cmd.CommandText += " (molId IN ";
                    AddListToINCmd(molIdsFilter, optParamStartVal);
                    sql_cmd.CommandText += ")";
                    optParamStartVal += molIdsFilter.Length;
                }
                if (chromIdsFilter != null && chromIdsFilter.Count > 0) //add chrom IDs filter as parameters to SELECT IN
                {
                    sql_cmd.CommandText += " OR (chromId IN ";
                    AddListToINCmd(chromIdsFilter.ToArray(), optParamStartVal);
                    sql_cmd.CommandText += ")";
                    optParamStartVal += chromIdsFilter.Count;
                }
                if (chromStartEndsFilter != null && chromStartEndsFilter.Count > 0) //add chrom, start, end as parameters to SELECT, separated by OR
                {
                    List<string> newLocationParamStrings = new List<string>();
                    sql_cmd.CommandText += " OR (";
                    foreach (Tuple<int, int, int> molLocation in chromStartEndsFilter)
                    {
                        string newChromParam = String.Format("@param{0}", optParamStartVal++);
                        string newStartParam = String.Format("@param{0}", optParamStartVal++);
                        string newEndParam = String.Format("@param{0}", optParamStartVal++);
                        sql_cmd.Parameters.Add(new SQLiteParameter(newChromParam, molLocation.Item1));
                        sql_cmd.Parameters.Add(new SQLiteParameter(newStartParam, molLocation.Item2));
                        sql_cmd.Parameters.Add(new SQLiteParameter(newEndParam, molLocation.Item3));
                        string newLocationString = String.Format("(chromId = {0} AND ((molStart BETWEEN {1} AND {2}) OR (molEnd BETWEEN {1} AND {2})))", newChromParam, newStartParam, newEndParam);
                        newLocationParamStrings.Add(newLocationString);
                    }
                    sql_cmd.CommandText += String.Join("OR ", newLocationParamStrings);
                    sql_cmd.CommandText += ")";
                }
                sql_cmd.CommandText += ")";
            }
        }

        public static IEnumerable<Molecule>[][][] SelectMolecules(int projectId, int mappedFilter, float lengthFilter, float confidenceFilter, float percentAlignedFilter, int[] molIdsFilter,
            List<int> chromIdsFilter, List<Tuple<int, int, int>> chromStartEndsFilter)
        {
            SetConnection();
            sql_con.Open();

            using (sql_con)
            {
                int numRuns = CountProjectRuns(projectId);
                int maxScans = GetMaxScanNumber(projectId);
                List<Molecule>[][][] selectedMolecules = new List<Molecule>[numRuns][][];
                for (int run = 0; run < numRuns; run++)
                {
                    selectedMolecules[run] = new List<Molecule>[maxScans][];
                    for (int scan = 0; scan < maxScans; scan++)
                    {
                        selectedMolecules[run][scan] = new List<Molecule>[TiffImages.columnPerScan];
                        for (int col = 0; col < TiffImages.columnPerScan; col++)
                        {
                            selectedMolecules[run][scan][col] = new List<Molecule>();
                        }
                    }
                }
                buildSelectMoleculesCommand(projectId, mappedFilter, lengthFilter, confidenceFilter, percentAlignedFilter, molIdsFilter, chromIdsFilter, chromStartEndsFilter);
                using (sql_cmd)
                {
                    SQLiteDataReader dataReader = sql_cmd.ExecuteReader();
                    while (dataReader.Read())
                    {
                        Molecule mol = new Molecule(Convert.ToInt32(dataReader["molId"]), Convert.ToInt32(dataReader["runId"]), Convert.ToInt32(dataReader["scan"]), Convert.ToInt32(dataReader["col"]),
                            Convert.ToInt32(dataReader["rowStart"]), Convert.ToInt32(dataReader["rowEnd"]), Convert.ToDouble(dataReader["xStart"]), Convert.ToDouble(dataReader["xEnd"]),
                            Convert.ToDouble(dataReader["yStart"]), Convert.ToDouble(dataReader["yEnd"]), Convert.ToInt32(dataReader["mapped"]), Convert.ToInt32(dataReader["chromId"]), 
                            dataReader["alignmentString"].ToString(), dataReader["orientation"].ToString(), dataReader["alignmentChannelPositions"].ToString());
                        selectedMolecules[mol.RunId - 1][mol.Scan - 1][mol.Column - 1].Add(mol);
                    }
                }
                return selectedMolecules;
            }
        }

        public static IEnumerable<Molecule> SelectColumnMolecules(int projectId, int runId, int scan, int column)
        {
            List<Molecule> columnMolecules = new List<Molecule>();
            SetConnection();
            sql_con.Open();

            using (sql_con)
            {
                string selectMoleculesCommand = "SELECT rowStart, rowEnd, xStart, yStart, xEnd, yEnd FROM molecules WHERE projectId = @param1 AND runId = @param2 AND scan = @param3 AND col = @param4";
                using (sql_cmd = new SQLiteCommand(selectMoleculesCommand, sql_con))
                {
                    sql_cmd.Parameters.Add(new SQLiteParameter("@param1", projectId));
                    sql_cmd.Parameters.Add(new SQLiteParameter("@param2", runId));
                    sql_cmd.Parameters.Add(new SQLiteParameter("@param3", scan));
                    sql_cmd.Parameters.Add(new SQLiteParameter("@param4", column));
                    SQLiteDataReader dataReader = sql_cmd.ExecuteReader();
                    while (dataReader.Read())
                    {
                        columnMolecules.Add(new Molecule(Convert.ToInt32(dataReader["rowStart"]), Convert.ToInt32(dataReader["rowEnd"]),
                                                Convert.ToDouble(dataReader["xStart"]), Convert.ToDouble(dataReader["xEnd"]), 
                                                Convert.ToDouble(dataReader["yStart"]), Convert.ToDouble(dataReader["yEnd"])));
                    }
                }
            }

            return columnMolecules;
        }

        public static IEnumerable<int> SelectColumnRows(int projectId, int runId, int scan, int column)
        {
            HashSet<int> columnRows = new HashSet<int>();
            SetConnection();
            sql_con.Open();
            using (sql_con)
            {
                string selectMoleculesCommand = "SELECT DISTINCT rowStart, rowEnd FROM molecules WHERE projectId = @param1 AND runID = @param2 AND scan = @param3 AND col = @param4";
                using (sql_cmd = new SQLiteCommand(selectMoleculesCommand, sql_con))
                {
                    sql_cmd.Parameters.Add(new SQLiteParameter("@param1", projectId));
                    sql_cmd.Parameters.Add(new SQLiteParameter("@param2", runId));
                    sql_cmd.Parameters.Add(new SQLiteParameter("@param3", scan));
                    sql_cmd.Parameters.Add(new SQLiteParameter("@param4", column));
                    using (SQLiteDataReader dataReader = sql_cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            columnRows.Add(Convert.ToInt32(dataReader["rowStart"]));
                            columnRows.Add(Convert.ToInt32(dataReader["rowEnd"]));
                        }
                    }
                }
            }
            return columnRows;
        }
    }
}
