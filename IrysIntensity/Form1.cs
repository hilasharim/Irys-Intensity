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
using System.Diagnostics;

namespace IrysIntensity
{
    public partial class IrysIntensity : Form
    {

        int projectID = 1;

        public IrysIntensity()
        {
            InitializeComponent();
            if (File.Exists("MoleculeData.db"))
            {
                DatabaseManager.updateComboBox(projectsCmbBox, "name", "id", "projects");
            }
            DatabaseManager.setUpDBOnStartUp();
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

        private void NewProject_Click(object sender, EventArgs e)
        {
            string new_project_name = NewProjectNameTextBox.Text;
            DatabaseManager.AddProject(new_project_name);
            DatabaseManager.updateComboBox(projectsCmbBox, "name", "id", "projects");
        }

        private void AddNewMols_Click(object sender, EventArgs e)
        {
            string xmapFilePath = XMAP_path_txtbox.Text;
            string bnxFilePath = BNX_path_txtbox.Text;
            if (String.IsNullOrEmpty(xmapFilePath))
            {
                MessageBox.Show("Must provide path to XMAP file", "Missing file path");
            }
            else if (String.IsNullOrEmpty(bnxFilePath))
            {
                MessageBox.Show("Must provide path to BNX file", "Missing file path");
            }
            else
            {
                int totalALignedMolecules = XMAPParser.ParseXmap(xmapFilePath);
                if (totalALignedMolecules == -1)
                {
                    MessageBox.Show("Can't open specified XMAP file", "Error opening file");
                }
                else
                {
                    int totalMolecules = BNXParser.ParseBNX(bnxFilePath, projectID);
                    if (totalMolecules == -1)
                    {
                        MessageBox.Show("Can't open specified BNX file", "Error opening file");
                    }
                }
            }
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
            float lengthFilter = String.IsNullOrEmpty(min_len_txtbx.Text) ? 0 : float.Parse(min_len_txtbx.Text);
            float confidenceFilter = String.IsNullOrEmpty(min_conf_txtbx.Text) ? 0 : float.Parse(min_conf_txtbx.Text);
            float alignedLenPercentFilter = String.IsNullOrEmpty(min_aligned_len_txtbx.Text) ? 0 : float.Parse(min_aligned_len_txtbx.Text);
            int[] molIdsFilterArray = null;

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
                    }
                }
            }
        }
    }
}
