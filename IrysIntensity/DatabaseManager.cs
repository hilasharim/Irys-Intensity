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
            string create_table_command = "CREATE TABLE IF NOT EXISTS projects (id INTEGER PRIMARY KEY, name TEXT NOT NULL UNIQUE)";
            string exists_project_name_command = "SELECT COUNT(id) from projects where name = '" + projectName + "'";
            string add_project_command = "INSERT INTO projects (name) VALUES (\"" + projectName + "\")";
            ExecuteNonQueryCmd(create_table_command);
            if (ExecuteScalarCmd(exists_project_name_command) > 0)
            {
                MessageBox.Show("Project name already exists");
            }
            else
            {
                ExecuteNonQueryCmd(add_project_command);
            }
            sql_con.Close();
        }

        public static void AddRun(int projectId, string runName, string runMonth)
        {
            string create_table_command = "CREATE TABLE IF NOT EXISTS runs (id INTEGER PRIMARY KEY, projectId INTEGER NOT NULL, name TEXT NOT NULL, month TEXT NOT NULL, UNIQUE(projectId, name, month))";
            string add_run_command = "INSERT OR IGNORE INTO runs (projectId, name, month) VALUES (@param1, @param2, @param3)";
            ExecuteNonQueryCmd(create_table_command);

            sql_cmd = new SQLiteCommand(add_run_command, sql_con);
            sql_cmd.Parameters.Add(new SQLiteParameter("@param1", projectId));
            sql_cmd.Parameters.Add(new SQLiteParameter("@param2", runName));
            sql_cmd.Parameters.Add(new SQLiteParameter("@param3", runMonth));
            sql_cmd.ExecuteNonQuery();
        }

        public static int FindRunId(int projectId, string runName)
        {
            string select_run_command = "SELECT id FROM runs WHERE projectId = @param1 AND name = @param2";
            sql_cmd = new SQLiteCommand(select_run_command, sql_con);
            sql_cmd.Parameters.Add(new SQLiteParameter("@param1", projectId));
            sql_cmd.Parameters.Add(new SQLiteParameter("@param2", runName));
            SQLiteDataReader reader = sql_cmd.ExecuteReader();
            reader.Read();
            return Convert.ToInt32(reader["id"]);
        }

        public static void AddMolecule(int projectId, int runId, int molId, int scan, int originalId, float length, int mapped, int chromId, float start, float end, string orientation, float confidence,
            string alignmentString, float percentAligned)
        {
            string create_table_command = @"CREATE TABLE IF NOT EXISTS molecules (id INTEGER PRIMARY KEY, projectId INTEGER NOT NULL, runId INTEGER NOT NULL, molId INTEGER NOT NULL,
                                            scan INTEGER NOT NULL, originalID INTEGER NOT NULL, length REAL NOT NULL, mapped INTEGER NOT NULL, chromId INTEGER, start REAL, end REAL,
                                            orientation TEXT, confidence REAL, alignmentString TEXT, percentAligned REAL, UNIQUE(projectId, runId, scan, originalId))";
            string[] create_index_commands = {"CREATE INDEX IF NOT EXISTS molecule_ids ON molecules (projectId, molId)",
                                              "CREATE INDEX IF NOT EXISTS aligned ON molecules (mapped)",
                                              "CREATE INDEX IF NOT EXISTS lengths ON molecules (length)",
                                              "CREATE INDEX IF NOT EXISTS conf ON molecules (confidence)",
                                              "CREATE INDEX IF NOT EXISTS percent_alignment ON molecules (percentAligned)",
                                              "CREATE INDEX IF NOT EXISTS chromosome ON molecules (chromId)",
                                              "CREATE INDEX IF NOT EXISTS alignment_pos ON molecules (chromId, start, end)"};
            
            ExecuteNonQueryCmd(create_table_command);
            foreach(string command_text in create_index_commands) {
                ExecuteNonQueryCmd(command_text);
            }

            string add_molecule_command = @"INSERT OR IGNORE INTO molecules (projectId, runId, molId, scan, originalId, length, mapped, chromId, start, end, orientation, confidence,
                                            alignmentString, percentAligned) VALUES (@param1, @param2, @param3, @param4, @param5, @param6, @param7, @param8, @param9, @param10, @param11,
                                            @param12, @param13, @param14)";
            sql_cmd = new SQLiteCommand(add_molecule_command, sql_con);
            sql_cmd.Parameters.Add(new SQLiteParameter("@param1", projectId));
            sql_cmd.Parameters.Add(new SQLiteParameter("@param2", runId));
            sql_cmd.Parameters.Add(new SQLiteParameter("@param3", molId));
            sql_cmd.Parameters.Add(new SQLiteParameter("@param4", scan));
            sql_cmd.Parameters.Add(new SQLiteParameter("@param5", originalId));
            sql_cmd.Parameters.Add(new SQLiteParameter("@param6", length));
            sql_cmd.Parameters.Add(new SQLiteParameter("@param7", mapped));
            sql_cmd.Parameters.Add(new SQLiteParameter("@param8", chromId));
            sql_cmd.Parameters.Add(new SQLiteParameter("@param9", start));
            sql_cmd.Parameters.Add(new SQLiteParameter("@param10", end));
            sql_cmd.Parameters.Add(new SQLiteParameter("@param11", orientation));
            sql_cmd.Parameters.Add(new SQLiteParameter("@param12", confidence));
            sql_cmd.Parameters.Add(new SQLiteParameter("@param13", alignmentString));
            sql_cmd.Parameters.Add(new SQLiteParameter("@param14", percentAligned));
            sql_cmd.ExecuteNonQuery();
        }
    }
}
