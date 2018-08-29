namespace IrysIntensity
{
    partial class IrysIntensity
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.manageProjects = new System.Windows.Forms.Label();
            this.NewProject = new System.Windows.Forms.Button();
            this.NewProjectNameTextBox = new System.Windows.Forms.TextBox();
            this.AddNewMols = new System.Windows.Forms.Button();
            this.projectsCmbBox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.openXmap = new System.Windows.Forms.Button();
            this.XMAP_path_txtbox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.BNX_path_txtbox = new System.Windows.Forms.TextBox();
            this.openBNX = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.aligned_filter_ckbx = new System.Windows.Forms.CheckBox();
            this.label8 = new System.Windows.Forms.Label();
            this.min_len_txtbx = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.min_conf_txtbx = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.min_aligned_len_txtbx = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.mols_ids_txtbx = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.mol_ids_file_path_txtbx = new System.Windows.Forms.TextBox();
            this.openMolIdsFile = new System.Windows.Forms.Button();
            this.label13 = new System.Windows.Forms.Label();
            this.mol_locations_txtbx = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.mol_locations_file_path_txtbx = new System.Windows.Forms.TextBox();
            this.openMolLocationsFile = new System.Windows.Forms.Button();
            this.label15 = new System.Windows.Forms.Label();
            this.key_file_path_txtbx = new System.Windows.Forms.TextBox();
            this.openKeyFile = new System.Windows.Forms.Button();
            this.filterMolecules = new System.Windows.Forms.Button();
            this.label16 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.r_cmap_file_path_txtbx = new System.Windows.Forms.TextBox();
            this.openRcmapFile = new System.Windows.Forms.Button();
            this.label19 = new System.Windows.Forms.Label();
            this.key_file_path_txtbx2 = new System.Windows.Forms.TextBox();
            this.openKeyFile2 = new System.Windows.Forms.Button();
            this.label20 = new System.Windows.Forms.Label();
            this.runs_paths_txtbx2 = new System.Windows.Forms.TextBox();
            this.openRunLocations = new System.Windows.Forms.Button();
            this.label22 = new System.Windows.Forms.Label();
            this.runs_paths_txtbx = new System.Windows.Forms.TextBox();
            this.openRunLocationsAdd = new System.Windows.Forms.Button();
            this.get_pixels_button = new System.Windows.Forms.Button();
            this.column_counter_txtbx = new System.Windows.Forms.TextBox();
            this.label23 = new System.Windows.Forms.Label();
            this.mol_upload_alignment_ch_txtbx = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            this.get_output_button = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // manageProjects
            // 
            this.manageProjects.AutoSize = true;
            this.manageProjects.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.manageProjects.Location = new System.Drawing.Point(12, 9);
            this.manageProjects.Name = "manageProjects";
            this.manageProjects.Size = new System.Drawing.Size(102, 13);
            this.manageProjects.TabIndex = 0;
            this.manageProjects.Text = "Manage Projects";
            // 
            // NewProject
            // 
            this.NewProject.Location = new System.Drawing.Point(15, 59);
            this.NewProject.Name = "NewProject";
            this.NewProject.Size = new System.Drawing.Size(107, 26);
            this.NewProject.TabIndex = 2;
            this.NewProject.Text = "Add new project";
            this.NewProject.UseVisualStyleBackColor = true;
            this.NewProject.Click += new System.EventHandler(this.NewProject_Click);
            // 
            // NewProjectNameTextBox
            // 
            this.NewProjectNameTextBox.Location = new System.Drawing.Point(15, 33);
            this.NewProjectNameTextBox.Name = "NewProjectNameTextBox";
            this.NewProjectNameTextBox.Size = new System.Drawing.Size(141, 20);
            this.NewProjectNameTextBox.TabIndex = 1;
            this.NewProjectNameTextBox.Text = "New Project";
            // 
            // AddNewMols
            // 
            this.AddNewMols.Location = new System.Drawing.Point(620, 101);
            this.AddNewMols.Name = "AddNewMols";
            this.AddNewMols.Size = new System.Drawing.Size(79, 28);
            this.AddNewMols.TabIndex = 17;
            this.AddNewMols.Text = "Upload";
            this.AddNewMols.UseVisualStyleBackColor = true;
            this.AddNewMols.Click += new System.EventHandler(this.AddNewMols_Click);
            // 
            // projectsCmbBox
            // 
            this.projectsCmbBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.projectsCmbBox.FormattingEnabled = true;
            this.projectsCmbBox.Location = new System.Drawing.Point(278, 32);
            this.projectsCmbBox.Name = "projectsCmbBox";
            this.projectsCmbBox.Size = new System.Drawing.Size(121, 21);
            this.projectsCmbBox.TabIndex = 4;
            this.projectsCmbBox.SelectedIndexChanged += new System.EventHandler(this.projectsCmbBox_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(194, 35);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(78, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Choose project";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.label2.Location = new System.Drawing.Point(12, 101);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(181, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Upload Molecules to Database";
            // 
            // openXmap
            // 
            this.openXmap.Location = new System.Drawing.Point(465, 123);
            this.openXmap.Name = "openXmap";
            this.openXmap.Size = new System.Drawing.Size(75, 23);
            this.openXmap.TabIndex = 8;
            this.openXmap.Text = "Browse";
            this.openXmap.UseVisualStyleBackColor = true;
            this.openXmap.Click += new System.EventHandler(this.openXmap_Click);
            // 
            // XMAP_path_txtbox
            // 
            this.XMAP_path_txtbox.Location = new System.Drawing.Point(97, 126);
            this.XMAP_path_txtbox.Name = "XMAP_path_txtbox";
            this.XMAP_path_txtbox.Size = new System.Drawing.Size(362, 20);
            this.XMAP_path_txtbox.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(14, 128);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "XMAP file path";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(14, 152);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(69, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "BNX file path";
            // 
            // BNX_path_txtbox
            // 
            this.BNX_path_txtbox.Location = new System.Drawing.Point(97, 152);
            this.BNX_path_txtbox.Name = "BNX_path_txtbox";
            this.BNX_path_txtbox.Size = new System.Drawing.Size(362, 20);
            this.BNX_path_txtbox.TabIndex = 10;
            // 
            // openBNX
            // 
            this.openBNX.Location = new System.Drawing.Point(465, 150);
            this.openBNX.Name = "openBNX";
            this.openBNX.Size = new System.Drawing.Size(75, 23);
            this.openBNX.TabIndex = 11;
            this.openBNX.Text = "Browse";
            this.openBNX.UseVisualStyleBackColor = true;
            this.openBNX.Click += new System.EventHandler(this.openBNX_Click);
            // 
            // label5
            // 
            this.label5.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label5.Location = new System.Drawing.Point(-1, 93);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(700, 2);
            this.label5.TabIndex = 34;
            // 
            // label6
            // 
            this.label6.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label6.Location = new System.Drawing.Point(-1, 280);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(700, 2);
            this.label6.TabIndex = 35;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.label7.Location = new System.Drawing.Point(12, 293);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(104, 13);
            this.label7.TabIndex = 13;
            this.label7.Text = "Select Molecules";
            // 
            // aligned_filter_ckbx
            // 
            this.aligned_filter_ckbx.AutoSize = true;
            this.aligned_filter_ckbx.Checked = true;
            this.aligned_filter_ckbx.CheckState = System.Windows.Forms.CheckState.Checked;
            this.aligned_filter_ckbx.Location = new System.Drawing.Point(12, 319);
            this.aligned_filter_ckbx.Name = "aligned_filter_ckbx";
            this.aligned_filter_ckbx.Size = new System.Drawing.Size(83, 17);
            this.aligned_filter_ckbx.TabIndex = 18;
            this.aligned_filter_ckbx.Text = "Aligned only";
            this.aligned_filter_ckbx.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(127, 319);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(77, 13);
            this.label8.TabIndex = 19;
            this.label8.Text = "Min length (kb)";
            // 
            // min_len_txtbx
            // 
            this.min_len_txtbx.Location = new System.Drawing.Point(130, 335);
            this.min_len_txtbx.Name = "min_len_txtbx";
            this.min_len_txtbx.Size = new System.Drawing.Size(53, 20);
            this.min_len_txtbx.TabIndex = 20;
            this.min_len_txtbx.Text = "150";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(247, 319);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(80, 13);
            this.label9.TabIndex = 21;
            this.label9.Text = "Min confidence";
            // 
            // min_conf_txtbx
            // 
            this.min_conf_txtbx.Location = new System.Drawing.Point(250, 335);
            this.min_conf_txtbx.Name = "min_conf_txtbx";
            this.min_conf_txtbx.Size = new System.Drawing.Size(53, 20);
            this.min_conf_txtbx.TabIndex = 22;
            this.min_conf_txtbx.Text = "12";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(358, 318);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(149, 13);
            this.label10.TabIndex = 23;
            this.label10.Text = "Min aligned length percent (%)";
            // 
            // min_aligned_len_txtbx
            // 
            this.min_aligned_len_txtbx.Location = new System.Drawing.Point(361, 335);
            this.min_aligned_len_txtbx.Name = "min_aligned_len_txtbx";
            this.min_aligned_len_txtbx.Size = new System.Drawing.Size(53, 20);
            this.min_aligned_len_txtbx.TabIndex = 24;
            this.min_aligned_len_txtbx.Text = "60";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(12, 371);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(69, 13);
            this.label11.TabIndex = 25;
            this.label11.Text = "Molecule IDs";
            // 
            // mols_ids_txtbx
            // 
            this.mols_ids_txtbx.Location = new System.Drawing.Point(12, 387);
            this.mols_ids_txtbx.Multiline = true;
            this.mols_ids_txtbx.Name = "mols_ids_txtbx";
            this.mols_ids_txtbx.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.mols_ids_txtbx.Size = new System.Drawing.Size(153, 169);
            this.mols_ids_txtbx.TabIndex = 26;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(14, 559);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(55, 13);
            this.label12.TabIndex = 27;
            this.label12.Text = "or from file";
            // 
            // mol_ids_file_path_txtbx
            // 
            this.mol_ids_file_path_txtbx.Location = new System.Drawing.Point(12, 575);
            this.mol_ids_file_path_txtbx.Name = "mol_ids_file_path_txtbx";
            this.mol_ids_file_path_txtbx.Size = new System.Drawing.Size(223, 20);
            this.mol_ids_file_path_txtbx.TabIndex = 28;
            // 
            // openMolIdsFile
            // 
            this.openMolIdsFile.Location = new System.Drawing.Point(12, 601);
            this.openMolIdsFile.Name = "openMolIdsFile";
            this.openMolIdsFile.Size = new System.Drawing.Size(75, 23);
            this.openMolIdsFile.TabIndex = 29;
            this.openMolIdsFile.Text = "Browse";
            this.openMolIdsFile.UseVisualStyleBackColor = true;
            this.openMolIdsFile.Click += new System.EventHandler(this.openMolIdsFile_Click);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(298, 371);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(149, 13);
            this.label13.TabIndex = 30;
            this.label13.Text = "Location (chr:start-end, or chr)";
            // 
            // mol_locations_txtbx
            // 
            this.mol_locations_txtbx.Location = new System.Drawing.Point(301, 387);
            this.mol_locations_txtbx.Multiline = true;
            this.mol_locations_txtbx.Name = "mol_locations_txtbx";
            this.mol_locations_txtbx.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.mol_locations_txtbx.Size = new System.Drawing.Size(176, 169);
            this.mol_locations_txtbx.TabIndex = 31;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(298, 559);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(55, 13);
            this.label14.TabIndex = 32;
            this.label14.Text = "or from file";
            // 
            // mol_locations_file_path_txtbx
            // 
            this.mol_locations_file_path_txtbx.Location = new System.Drawing.Point(301, 575);
            this.mol_locations_file_path_txtbx.Name = "mol_locations_file_path_txtbx";
            this.mol_locations_file_path_txtbx.Size = new System.Drawing.Size(223, 20);
            this.mol_locations_file_path_txtbx.TabIndex = 33;
            // 
            // openMolLocationsFile
            // 
            this.openMolLocationsFile.Location = new System.Drawing.Point(530, 573);
            this.openMolLocationsFile.Name = "openMolLocationsFile";
            this.openMolLocationsFile.Size = new System.Drawing.Size(75, 23);
            this.openMolLocationsFile.TabIndex = 34;
            this.openMolLocationsFile.Text = "Browse";
            this.openMolLocationsFile.UseVisualStyleBackColor = true;
            this.openMolLocationsFile.Click += new System.EventHandler(this.openMolLocationsFile_Click);
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(298, 606);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(151, 13);
            this.label15.TabIndex = 35;
            this.label15.Text = "requires chromosomes key file:";
            // 
            // key_file_path_txtbx
            // 
            this.key_file_path_txtbx.Location = new System.Drawing.Point(301, 622);
            this.key_file_path_txtbx.Name = "key_file_path_txtbx";
            this.key_file_path_txtbx.Size = new System.Drawing.Size(223, 20);
            this.key_file_path_txtbx.TabIndex = 36;
            // 
            // openKeyFile
            // 
            this.openKeyFile.Location = new System.Drawing.Point(530, 620);
            this.openKeyFile.Name = "openKeyFile";
            this.openKeyFile.Size = new System.Drawing.Size(75, 23);
            this.openKeyFile.TabIndex = 37;
            this.openKeyFile.Text = "Browse";
            this.openKeyFile.UseVisualStyleBackColor = true;
            this.openKeyFile.Click += new System.EventHandler(this.openKeyFile_Click);
            // 
            // filterMolecules
            // 
            this.filterMolecules.Location = new System.Drawing.Point(620, 293);
            this.filterMolecules.Name = "filterMolecules";
            this.filterMolecules.Size = new System.Drawing.Size(79, 28);
            this.filterMolecules.TabIndex = 38;
            this.filterMolecules.Text = "Apply filters";
            this.filterMolecules.UseVisualStyleBackColor = true;
            this.filterMolecules.Click += new System.EventHandler(this.filterMolecules_Click);
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.label16.Location = new System.Drawing.Point(780, 269);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(51, 13);
            this.label16.TabIndex = 37;
            this.label16.Text = "Analyze";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(780, 301);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(91, 13);
            this.label18.TabIndex = 42;
            this.label18.Text = "R_CMAP file path";
            // 
            // r_cmap_file_path_txtbx
            // 
            this.r_cmap_file_path_txtbx.Location = new System.Drawing.Point(919, 298);
            this.r_cmap_file_path_txtbx.Name = "r_cmap_file_path_txtbx";
            this.r_cmap_file_path_txtbx.Size = new System.Drawing.Size(362, 20);
            this.r_cmap_file_path_txtbx.TabIndex = 43;
            // 
            // openRcmapFile
            // 
            this.openRcmapFile.Location = new System.Drawing.Point(1287, 296);
            this.openRcmapFile.Name = "openRcmapFile";
            this.openRcmapFile.Size = new System.Drawing.Size(75, 23);
            this.openRcmapFile.TabIndex = 44;
            this.openRcmapFile.Text = "Browse";
            this.openRcmapFile.UseVisualStyleBackColor = true;
            this.openRcmapFile.Click += new System.EventHandler(this.openRcmapFile_Click);
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(780, 330);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(133, 13);
            this.label19.TabIndex = 45;
            this.label19.Text = "Chromosomes key file path";
            // 
            // key_file_path_txtbx2
            // 
            this.key_file_path_txtbx2.Location = new System.Drawing.Point(919, 328);
            this.key_file_path_txtbx2.Name = "key_file_path_txtbx2";
            this.key_file_path_txtbx2.Size = new System.Drawing.Size(362, 20);
            this.key_file_path_txtbx2.TabIndex = 46;
            // 
            // openKeyFile2
            // 
            this.openKeyFile2.Location = new System.Drawing.Point(1287, 325);
            this.openKeyFile2.Name = "openKeyFile2";
            this.openKeyFile2.Size = new System.Drawing.Size(75, 23);
            this.openKeyFile2.TabIndex = 47;
            this.openKeyFile2.Text = "Browse";
            this.openKeyFile2.UseVisualStyleBackColor = true;
            this.openKeyFile2.Click += new System.EventHandler(this.openKeyFile2_Click);
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(776, 59);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(61, 13);
            this.label20.TabIndex = 48;
            this.label20.Text = "Runs paths";
            // 
            // runs_paths_txtbx2
            // 
            this.runs_paths_txtbx2.Location = new System.Drawing.Point(843, 32);
            this.runs_paths_txtbx2.Multiline = true;
            this.runs_paths_txtbx2.Name = "runs_paths_txtbx2";
            this.runs_paths_txtbx2.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.runs_paths_txtbx2.Size = new System.Drawing.Size(362, 76);
            this.runs_paths_txtbx2.TabIndex = 49;
            // 
            // openRunLocations
            // 
            this.openRunLocations.Location = new System.Drawing.Point(1211, 59);
            this.openRunLocations.Name = "openRunLocations";
            this.openRunLocations.Size = new System.Drawing.Size(105, 23);
            this.openRunLocations.TabIndex = 50;
            this.openRunLocations.Text = "Add run locations";
            this.openRunLocations.UseVisualStyleBackColor = true;
            this.openRunLocations.Click += new System.EventHandler(this.openRunLocations_Click);
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(14, 186);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(61, 13);
            this.label22.TabIndex = 12;
            this.label22.Text = "Runs paths";
            // 
            // runs_paths_txtbx
            // 
            this.runs_paths_txtbx.Location = new System.Drawing.Point(97, 183);
            this.runs_paths_txtbx.Multiline = true;
            this.runs_paths_txtbx.Name = "runs_paths_txtbx";
            this.runs_paths_txtbx.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.runs_paths_txtbx.Size = new System.Drawing.Size(362, 61);
            this.runs_paths_txtbx.TabIndex = 13;
            // 
            // openRunLocationsAdd
            // 
            this.openRunLocationsAdd.Location = new System.Drawing.Point(465, 198);
            this.openRunLocationsAdd.Name = "openRunLocationsAdd";
            this.openRunLocationsAdd.Size = new System.Drawing.Size(105, 23);
            this.openRunLocationsAdd.TabIndex = 14;
            this.openRunLocationsAdd.Text = "Add run locations";
            this.openRunLocationsAdd.UseVisualStyleBackColor = true;
            this.openRunLocationsAdd.Click += new System.EventHandler(this.openRunLocationsAdd_Click);
            // 
            // get_pixels_button
            // 
            this.get_pixels_button.Location = new System.Drawing.Point(936, 120);
            this.get_pixels_button.Name = "get_pixels_button";
            this.get_pixels_button.Size = new System.Drawing.Size(75, 28);
            this.get_pixels_button.TabIndex = 55;
            this.get_pixels_button.Text = "Get Pixels";
            this.get_pixels_button.UseVisualStyleBackColor = true;
            this.get_pixels_button.Click += new System.EventHandler(this.button1_Click);
            // 
            // column_counter_txtbx
            // 
            this.column_counter_txtbx.Location = new System.Drawing.Point(1049, 125);
            this.column_counter_txtbx.Name = "column_counter_txtbx";
            this.column_counter_txtbx.Size = new System.Drawing.Size(100, 20);
            this.column_counter_txtbx.TabIndex = 56;
            this.column_counter_txtbx.Text = "0";
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(14, 253);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(119, 13);
            this.label23.TabIndex = 15;
            this.label23.Text = "Alignment label channel";
            // 
            // mol_upload_alignment_ch_txtbx
            // 
            this.mol_upload_alignment_ch_txtbx.Location = new System.Drawing.Point(139, 250);
            this.mol_upload_alignment_ch_txtbx.Name = "mol_upload_alignment_ch_txtbx";
            this.mol_upload_alignment_ch_txtbx.Size = new System.Drawing.Size(30, 20);
            this.mol_upload_alignment_ch_txtbx.TabIndex = 16;
            this.mol_upload_alignment_ch_txtbx.Text = "1";
            this.mol_upload_alignment_ch_txtbx.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.label17.Location = new System.Drawing.Point(774, 9);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(89, 13);
            this.label17.TabIndex = 57;
            this.label17.Text = "Get Pixel Data";
            // 
            // get_output_button
            // 
            this.get_output_button.Location = new System.Drawing.Point(1202, 398);
            this.get_output_button.Name = "get_output_button";
            this.get_output_button.Size = new System.Drawing.Size(160, 28);
            this.get_output_button.TabIndex = 58;
            this.get_output_button.Text = "Get Genome Alignment";
            this.get_output_button.UseVisualStyleBackColor = true;
            this.get_output_button.Click += new System.EventHandler(this.get_output_button_Click);
            // 
            // IrysIntensity
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1405, 661);
            this.Controls.Add(this.get_output_button);
            this.Controls.Add(this.label17);
            this.Controls.Add(this.mol_upload_alignment_ch_txtbx);
            this.Controls.Add(this.label23);
            this.Controls.Add(this.column_counter_txtbx);
            this.Controls.Add(this.get_pixels_button);
            this.Controls.Add(this.openRunLocationsAdd);
            this.Controls.Add(this.runs_paths_txtbx);
            this.Controls.Add(this.label22);
            this.Controls.Add(this.openRunLocations);
            this.Controls.Add(this.runs_paths_txtbx2);
            this.Controls.Add(this.label20);
            this.Controls.Add(this.openKeyFile2);
            this.Controls.Add(this.key_file_path_txtbx2);
            this.Controls.Add(this.label19);
            this.Controls.Add(this.openRcmapFile);
            this.Controls.Add(this.r_cmap_file_path_txtbx);
            this.Controls.Add(this.label18);
            this.Controls.Add(this.label16);
            this.Controls.Add(this.filterMolecules);
            this.Controls.Add(this.openKeyFile);
            this.Controls.Add(this.key_file_path_txtbx);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.openMolLocationsFile);
            this.Controls.Add(this.mol_locations_file_path_txtbx);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.mol_locations_txtbx);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.openMolIdsFile);
            this.Controls.Add(this.mol_ids_file_path_txtbx);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.mols_ids_txtbx);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.min_aligned_len_txtbx);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.min_conf_txtbx);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.min_len_txtbx);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.aligned_filter_ckbx);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.openBNX);
            this.Controls.Add(this.BNX_path_txtbox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.XMAP_path_txtbox);
            this.Controls.Add(this.openXmap);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.projectsCmbBox);
            this.Controls.Add(this.AddNewMols);
            this.Controls.Add(this.NewProjectNameTextBox);
            this.Controls.Add(this.NewProject);
            this.Controls.Add(this.manageProjects);
            this.Name = "IrysIntensity";
            this.Text = "IrysIntensity";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label manageProjects;
        private System.Windows.Forms.Button NewProject;
        private System.Windows.Forms.TextBox NewProjectNameTextBox;
        private System.Windows.Forms.Button AddNewMols;
        private System.Windows.Forms.ComboBox projectsCmbBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button openXmap;
        private System.Windows.Forms.TextBox XMAP_path_txtbox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox BNX_path_txtbox;
        private System.Windows.Forms.Button openBNX;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.CheckBox aligned_filter_ckbx;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox min_len_txtbx;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox min_conf_txtbx;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox min_aligned_len_txtbx;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox mols_ids_txtbx;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox mol_ids_file_path_txtbx;
        private System.Windows.Forms.Button openMolIdsFile;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox mol_locations_txtbx;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox mol_locations_file_path_txtbx;
        private System.Windows.Forms.Button openMolLocationsFile;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TextBox key_file_path_txtbx;
        private System.Windows.Forms.Button openKeyFile;
        private System.Windows.Forms.Button filterMolecules;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.TextBox r_cmap_file_path_txtbx;
        private System.Windows.Forms.Button openRcmapFile;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.TextBox key_file_path_txtbx2;
        private System.Windows.Forms.Button openKeyFile2;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.TextBox runs_paths_txtbx2;
        private System.Windows.Forms.Button openRunLocations;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.TextBox runs_paths_txtbx;
        private System.Windows.Forms.Button openRunLocationsAdd;
        private System.Windows.Forms.Button get_pixels_button;
        private System.Windows.Forms.TextBox column_counter_txtbx;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.TextBox mol_upload_alignment_ch_txtbx;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Button get_output_button;
    }
}

