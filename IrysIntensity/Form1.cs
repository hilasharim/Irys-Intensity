﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Diagnostics;
using System.Threading;

namespace IrysIntensity
{
    public partial class IrysIntensity : Form
    {

        int projectID = 1;
        private readonly SynchronizationContext synchronizationContext;

        public IrysIntensity()
        {
            InitializeComponent();
            if (File.Exists("MoleculeData.db"))
            {
                DatabaseManager.updateComboBox(projectsCmbBox, "name", "id", "projects");
            }
            DatabaseManager.setUpDBOnStartUp();
            synchronizationContext = SynchronizationContext.Current;
        }

        private void LoadNewFile(TextBox txtBox, String fileType)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = fileType + " files | *." + fileType;
            DialogResult dr = ofd.ShowDialog();
            if (dr == DialogResult.OK)
            {
                txtBox.Text = ofd.FileName;
            }
        }

        private void LoadDirectories(TextBox txtBox)
        {
            CommonOpenFileDialog fd = new CommonOpenFileDialog();
            fd.IsFolderPicker = true;
            fd.Multiselect = true;
            CommonFileDialogResult dr = fd.ShowDialog();
            if (dr == CommonFileDialogResult.Ok)
            {
                txtBox.Text = String.Join("\r\n",fd.FileNames);
            }
        }

        private void NewProject_Click(object sender, EventArgs e)
        {
            string new_project_name = NewProjectNameTextBox.Text;
            DatabaseManager.AddProject(new_project_name);
            DatabaseManager.updateComboBox(projectsCmbBox, "name", "id", "projects");
        }

        private void AddNewMols_Click(object sender, EventArgs e)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            string xmapFilePath = XMAP_path_txtbox.Text;
            string bnxFilePath = BNX_path_txtbox.Text;
            string[] runRootDirs = runs_paths_txtbx.Text.Split('\n');
            if (String.IsNullOrEmpty(xmapFilePath))
            {
                MessageBox.Show("Must provide path to XMAP file", "Missing file path");
                return;
            }
            if (String.IsNullOrEmpty(bnxFilePath))
            {
                MessageBox.Show("Must provide path to BNX file", "Missing file path");
                return;
            }
            if (String.IsNullOrEmpty(runRootDirs[0]))
            {
                MessageBox.Show("Must provide path to root directory of run files", "Missing file path");
                return;
            }

            int totalALignedMolecules = XMAPParser.ParseXmap(xmapFilePath);
            if (totalALignedMolecules == -1)
            {
                MessageBox.Show("Can't open specified XMAP file", "Error opening file");
                return;
            }
            int totalMolecules = BNXParser.ParseBNX(bnxFilePath, projectID);
            if (totalMolecules == -1)
            {
                MessageBox.Show("Can't open specified BNX file", "Error opening file");
                return;
            }
            BNXParser.ParseAllMolFiles(runRootDirs);
            DatabaseManager.AddAllMolecules(BNXParser.moleculeListByRun, projectID);
            MessageBox.Show(String.Format("time elapsed: {0}", stopWatch.Elapsed.ToString()));
        }
         

        private void projectsCmbBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            projectID = int.Parse(projectsCmbBox.SelectedValue.ToString());
        }

        private void openXmap_Click(object sender, EventArgs e)
        {
            LoadNewFile(XMAP_path_txtbox, "XMAP");
        }

        private void openBNX_Click(object sender, EventArgs e)
        {
            LoadNewFile(BNX_path_txtbox, "BNX");
        }

        private void openMolIdsFile_Click(object sender, EventArgs e)
        {
            LoadNewFile(mol_ids_file_path_txtbx, "txt");
        }

        private void openMolLocationsFile_Click(object sender, EventArgs e)
        {
            LoadNewFile(mol_locations_file_path_txtbx, "txt");
        }

        private void openKeyFile_Click(object sender, EventArgs e)
        {
            LoadNewFile(key_file_path_txtbx, "txt");
        }

        private void filterMolecules_Click(object sender, EventArgs e)
        {
            int alignmentFilter = aligned_filter_ckbx.Checked ? 1 : 0;
            float lengthFilter = String.IsNullOrEmpty(min_len_txtbx.Text) ? 0 : float.Parse(min_len_txtbx.Text) * 1000;
            float confidenceFilter = String.IsNullOrEmpty(min_conf_txtbx.Text) ? 0 : float.Parse(min_conf_txtbx.Text);
            float alignedLenPercentFilter = String.IsNullOrEmpty(min_aligned_len_txtbx.Text) ? 0 : float.Parse(min_aligned_len_txtbx.Text);
            int[] molIdsFilterArray = null;
            List<int> chromIdsFilter = null;
            List<Tuple<int, int, int>> chromStartEndFilter = null;

            if (!String.IsNullOrEmpty(mols_ids_txtbx.Text) && !String.IsNullOrEmpty(mol_ids_file_path_txtbx.Text))
            {
                MessageBox.Show("For molecule IDs filter either input into the text box, or upload a file, but not both", "Error parsing filter options");
                return;
            }

            if (!String.IsNullOrEmpty(mols_ids_txtbx.Text) || !String.IsNullOrEmpty(mol_ids_file_path_txtbx.Text))
            {
                molIdsFilterArray = UserInputParser.getMolIds(mols_ids_txtbx.Text, mol_ids_file_path_txtbx.Text);
                if (molIdsFilterArray == null)
                {
                    MessageBox.Show("Can't open molecule IDs file", "Error parsing filter options");
                    return;
                } 
            }

            if (!String.IsNullOrEmpty(mol_locations_txtbx.Text) && !String.IsNullOrEmpty(mol_locations_file_path_txtbx.Text))
            {
                MessageBox.Show("For molecule locations filter either input into the text box, or upload a file, but not both", "Error parsing filter options");
                return;
            }

            if (!String.IsNullOrEmpty(mol_locations_txtbx.Text) || !String.IsNullOrEmpty(mol_locations_file_path_txtbx.Text))
            {
                if (String.IsNullOrEmpty(key_file_path_txtbx.Text))
                {
                    MessageBox.Show("For molecule locations filter you must supply a key file", "Error parsing filter options");
                    return;
                }
                else //file and lines are present, parse the input
                {
                    int readKeyChroms = CMAPParser.ReadKeyFile(key_file_path_txtbx.Text); //reading the key file to translate chromosome names to IDs, becaues chromosomes are saved as IDs in DB
                    if (readKeyChroms == -1)
                    {
                        MessageBox.Show("Can't open specified key file", "Error opening file");
                        return;
                    }
                    else
                    {
                        //get the locations from user input
                        Tuple<List<int>, List<Tuple<int, int, int>>> locations = UserInputParser.getLocations(mol_locations_txtbx.Text, mol_locations_file_path_txtbx.Text);
                        chromIdsFilter = locations.Item1;
                        chromStartEndFilter = locations.Item2;
                    }
                }
            }

            int count = DatabaseManager.SelectMolecules(projectID, alignmentFilter, lengthFilter, confidenceFilter, alignedLenPercentFilter, molIdsFilterArray, chromIdsFilter, chromStartEndFilter);
        }

        private void openQcmapFile_Click(object sender, EventArgs e)
        {
            LoadNewFile(q_cmap_file_path_txtbx, "CMAP");
        }

        private void openRcmapFile_Click(object sender, EventArgs e)
        {
            LoadNewFile(r_cmap_file_path_txtbx, "CMAP");
        }

        private void openKeyFile2_Click(object sender, EventArgs e)
        {
            LoadNewFile(key_file_path_txtbx2, "txt");
        }

        private void openRunLocations_Click(object sender, EventArgs e)
        {
            LoadDirectories(runs_paths_txtbx2);
        }

        private void openRunLocationsAdd_Click(object sender, EventArgs e)
        {
            LoadDirectories(runs_paths_txtbx);
        }

        public void updateBox(string text) {
            //column_counter_txtbx.Text = text;
            synchronizationContext.Post(new SendOrPostCallback(value =>
            {
                column_counter_txtbx.Text = value.ToString();
            }), text);
        }        

        private void button1_Click(object sender, EventArgs e)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            //TiffImages.ProcessScanTiff(@"C:\Users\Hila\Downloads\CCITT_1.TIF");
            Task newTask = Task.Factory.StartNew(() =>
            {
                //TiffImages.ProcessScanTiff(@"X:\runs\2018-03\Pbmc_hmc_bspq1_6.3.17_fc2_2018-03-25_11_59\Pbmc_hmc_bspq1_6.3.17_fc2_2018-03-25_11_59_Scan001.tiff", updateBox);
                DatabaseManager.SelectFOVsBackgroundSubtraction(1, 1);
            });
            newTask.ContinueWith(_ => MessageBox.Show(stopWatch.Elapsed.ToString()));
            //TiffImages.ProcessScanTiff(@"X:\runs\2018-03\Pbmc_hmc_bspq1_6.3.17_fc2_2018-03-25_11_59\Pbmc_hmc_bspq1_6.3.17_fc2_2018-03-25_11_59_Scan001.tiff", column_counter_txtbx);
            //TiffImages.RotateBilinear(null, 0, 0, 0.002, 256, 256);
            //TiffImages.RotateImage();
           
            //TiffImages.ParseFOVFile(@"X:\runs\2018-03\Pbmc_hmc_bspq1_6.3.17_fc2_2018-03-25_11_59\Detect Molecules\Stitch1.fov");
            //MessageBox.Show(stopWatch.Elapsed.ToString());
            //string elapsedTime = TiffImages.openImageLibtiff();
            //string alignmentString = "(39,20)(40,19)(42,18)(43,17)(45,16)(46,15)(47,13)(48,12)(49,11)(50,10)(51,9)(52,9)(53,8)(54,7)(55,5)(56,4)(57,3)(58,2)(60,1)";
            //List<Tuple<int, int>> result = XMAPParser.ParseAlignmentString(alignmentString);
        }
                
    }
}
