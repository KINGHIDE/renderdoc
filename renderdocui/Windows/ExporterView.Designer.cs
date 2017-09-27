namespace renderdocui.Windows
{
    partial class ExporterView
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.typeToExport = new System.Windows.Forms.ComboBox();
            this.exportSwapUV = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.texSlotToExport = new System.Windows.Forms.ComboBox();
            this.exportWorldXForms = new System.Windows.Forms.CheckBox();
            this.exportFrustums = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.uvChannelToExport = new System.Windows.Forms.ComboBox();
            this.exportTextures = new System.Windows.Forms.CheckBox();
            this.doExport = new System.Windows.Forms.Button();
            this.drawcallsToExport = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader9 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader10 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader11 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader12 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader13 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader14 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.addDrawCallsButton = new System.Windows.Forms.Button();
            this.clearDrawCalls = new System.Windows.Forms.Button();
            this.removeSelected = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.lblCount = new System.Windows.Forms.Label();
            this.lblTriCount = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.synchronize = new System.Windows.Forms.Button();
            this.analysisProgress = new System.Windows.Forms.ProgressBar();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.typeToExport);
            this.groupBox1.Controls.Add(this.exportSwapUV);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.texSlotToExport);
            this.groupBox1.Controls.Add(this.exportWorldXForms);
            this.groupBox1.Controls.Add(this.exportFrustums);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.uvChannelToExport);
            this.groupBox1.Controls.Add(this.exportTextures);
            this.groupBox1.Location = new System.Drawing.Point(594, 7);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(221, 396);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Export Options";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(132, 173);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(62, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "Export Filter";
            // 
            // typeToExport
            // 
            this.typeToExport.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.typeToExport.FormattingEnabled = true;
            this.typeToExport.Items.AddRange(new object[] {
            "Skip Depth only",
            "Depth only",
            "Color only",
            "All"});
            this.typeToExport.Location = new System.Drawing.Point(11, 169);
            this.typeToExport.Name = "typeToExport";
            this.typeToExport.Size = new System.Drawing.Size(119, 21);
            this.typeToExport.TabIndex = 8;
            // 
            // exportSwapUV
            // 
            this.exportSwapUV.AutoSize = true;
            this.exportSwapUV.Checked = true;
            this.exportSwapUV.CheckState = System.Windows.Forms.CheckState.Checked;
            this.exportSwapUV.Location = new System.Drawing.Point(11, 90);
            this.exportSwapUV.Name = "exportSwapUV";
            this.exportSwapUV.Size = new System.Drawing.Size(107, 17);
            this.exportSwapUV.TabIndex = 7;
            this.exportSwapUV.Text = "Vertical swap UV";
            this.exportSwapUV.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(132, 146);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(64, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Texture Slot";
            // 
            // texSlotToExport
            // 
            this.texSlotToExport.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.texSlotToExport.FormattingEnabled = true;
            this.texSlotToExport.Items.AddRange(new object[] {
            "Slot0",
            "Slot1",
            "Slot2",
            "Slot3",
            "Slot4",
            "Slot5",
            "Slot6",
            "Slot7"});
            this.texSlotToExport.Location = new System.Drawing.Point(11, 142);
            this.texSlotToExport.Name = "texSlotToExport";
            this.texSlotToExport.Size = new System.Drawing.Size(119, 21);
            this.texSlotToExport.TabIndex = 5;
            // 
            // exportWorldXForms
            // 
            this.exportWorldXForms.AutoSize = true;
            this.exportWorldXForms.Checked = true;
            this.exportWorldXForms.CheckState = System.Windows.Forms.CheckState.Checked;
            this.exportWorldXForms.Location = new System.Drawing.Point(11, 67);
            this.exportWorldXForms.Name = "exportWorldXForms";
            this.exportWorldXForms.Size = new System.Drawing.Size(207, 17);
            this.exportWorldXForms.TabIndex = 4;
            this.exportWorldXForms.Text = "Caclulate World XForms (experimental)";
            this.exportWorldXForms.UseVisualStyleBackColor = true;
            // 
            // exportFrustums
            // 
            this.exportFrustums.AutoSize = true;
            this.exportFrustums.Checked = true;
            this.exportFrustums.CheckState = System.Windows.Forms.CheckState.Checked;
            this.exportFrustums.Location = new System.Drawing.Point(11, 44);
            this.exportFrustums.Name = "exportFrustums";
            this.exportFrustums.Size = new System.Drawing.Size(169, 17);
            this.exportFrustums.TabIndex = 3;
            this.exportFrustums.Text = "Export Frustums (experimental)";
            this.exportFrustums.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(132, 119);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "UV Channel";
            // 
            // uvChannelToExport
            // 
            this.uvChannelToExport.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.uvChannelToExport.FormattingEnabled = true;
            this.uvChannelToExport.Items.AddRange(new object[] {
            "UV0",
            "UV1",
            "UV2",
            "UV3"});
            this.uvChannelToExport.Location = new System.Drawing.Point(11, 115);
            this.uvChannelToExport.Name = "uvChannelToExport";
            this.uvChannelToExport.Size = new System.Drawing.Size(119, 21);
            this.uvChannelToExport.TabIndex = 1;
            // 
            // exportTextures
            // 
            this.exportTextures.AutoSize = true;
            this.exportTextures.Checked = true;
            this.exportTextures.CheckState = System.Windows.Forms.CheckState.Checked;
            this.exportTextures.Location = new System.Drawing.Point(11, 21);
            this.exportTextures.Name = "exportTextures";
            this.exportTextures.Size = new System.Drawing.Size(100, 17);
            this.exportTextures.TabIndex = 0;
            this.exportTextures.Text = "Export Textures";
            this.exportTextures.UseVisualStyleBackColor = true;
            // 
            // doExport
            // 
            this.doExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.doExport.Location = new System.Drawing.Point(713, 428);
            this.doExport.Name = "doExport";
            this.doExport.Size = new System.Drawing.Size(102, 23);
            this.doExport.TabIndex = 1;
            this.doExport.Text = "Export Selected...";
            this.doExport.UseVisualStyleBackColor = true;
            this.doExport.Click += new System.EventHandler(this.doExport_Click);
            // 
            // drawcallsToExport
            // 
            this.drawcallsToExport.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.drawcallsToExport.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6,
            this.columnHeader7,
            this.columnHeader8,
            this.columnHeader9,
            this.columnHeader10,
            this.columnHeader11,
            this.columnHeader12,
            this.columnHeader13,
            this.columnHeader14});
            this.drawcallsToExport.FullRowSelect = true;
            this.drawcallsToExport.HideSelection = false;
            this.drawcallsToExport.Location = new System.Drawing.Point(8, 7);
            this.drawcallsToExport.Name = "drawcallsToExport";
            this.drawcallsToExport.Size = new System.Drawing.Size(575, 396);
            this.drawcallsToExport.TabIndex = 2;
            this.drawcallsToExport.UseCompatibleStateImageBehavior = false;
            this.drawcallsToExport.View = System.Windows.Forms.View.Details;
            this.drawcallsToExport.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.drawcallsToExport_ColumnClick);
            this.drawcallsToExport.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.drawcallsToExport_ItemSelectionChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "EID";
            this.columnHeader1.Width = 40;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Name";
            this.columnHeader2.Width = 210;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Num Tri\'s";
            this.columnHeader3.Width = 90;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Num Instances";
            this.columnHeader4.Width = 90;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Num RT\'s";
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "Depth ID";
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "Color ID\'s";
            this.columnHeader7.Width = 120;
            // 
            // columnHeader8
            // 
            this.columnHeader8.Text = "Export Color";
            this.columnHeader8.Width = 90;
            // 
            // columnHeader9
            // 
            this.columnHeader9.Text = "Width";
            // 
            // columnHeader10
            // 
            this.columnHeader10.Text = "Height";
            // 
            // columnHeader11
            // 
            this.columnHeader11.Text = "Num textures";
            // 
            // columnHeader12
            // 
            this.columnHeader12.Text = "WVP";
            // 
            // columnHeader13
            // 
            this.columnHeader13.Text = "NRM";
            // 
            // columnHeader14
            // 
            this.columnHeader14.Text = "Status";
            this.columnHeader14.Width = 120;
            // 
            // addDrawCallsButton
            // 
            this.addDrawCallsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.addDrawCallsButton.Enabled = false;
            this.addDrawCallsButton.Location = new System.Drawing.Point(8, 428);
            this.addDrawCallsButton.Name = "addDrawCallsButton";
            this.addDrawCallsButton.Size = new System.Drawing.Size(187, 23);
            this.addDrawCallsButton.TabIndex = 3;
            this.addDrawCallsButton.Text = "Add Selected EID to FBX Exporter";
            this.addDrawCallsButton.UseVisualStyleBackColor = true;
            this.addDrawCallsButton.Click += new System.EventHandler(this.addDrawCallsButton_Click);
            // 
            // clearDrawCalls
            // 
            this.clearDrawCalls.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.clearDrawCalls.Location = new System.Drawing.Point(310, 428);
            this.clearDrawCalls.Name = "clearDrawCalls";
            this.clearDrawCalls.Size = new System.Drawing.Size(100, 23);
            this.clearDrawCalls.TabIndex = 4;
            this.clearDrawCalls.Text = "Clear";
            this.clearDrawCalls.UseVisualStyleBackColor = true;
            this.clearDrawCalls.Click += new System.EventHandler(this.clearDrawCalls_Click);
            // 
            // removeSelected
            // 
            this.removeSelected.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.removeSelected.Enabled = false;
            this.removeSelected.Location = new System.Drawing.Point(202, 428);
            this.removeSelected.Name = "removeSelected";
            this.removeSelected.Size = new System.Drawing.Size(102, 23);
            this.removeSelected.TabIndex = 5;
            this.removeSelected.Text = "Remove Selected";
            this.removeSelected.UseVisualStyleBackColor = true;
            this.removeSelected.Click += new System.EventHandler(this.removeSelected_Click);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 405);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Count :";
            // 
            // lblCount
            // 
            this.lblCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblCount.Location = new System.Drawing.Point(46, 406);
            this.lblCount.Name = "lblCount";
            this.lblCount.Size = new System.Drawing.Size(149, 15);
            this.lblCount.TabIndex = 7;
            this.lblCount.Text = "0";
            // 
            // lblTriCount
            // 
            this.lblTriCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblTriCount.Location = new System.Drawing.Point(279, 406);
            this.lblTriCount.Name = "lblTriCount";
            this.lblTriCount.Size = new System.Drawing.Size(149, 15);
            this.lblTriCount.TabIndex = 9;
            this.lblTriCount.Text = "0";
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(227, 405);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(56, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Tri Count :";
            // 
            // synchronize
            // 
            this.synchronize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.synchronize.Enabled = false;
            this.synchronize.Location = new System.Drawing.Point(416, 428);
            this.synchronize.Name = "synchronize";
            this.synchronize.Size = new System.Drawing.Size(112, 23);
            this.synchronize.TabIndex = 10;
            this.synchronize.Text = "Select EID";
            this.synchronize.UseVisualStyleBackColor = true;
            this.synchronize.Click += new System.EventHandler(this.synchronize_Click);
            // 
            // analysisProgress
            // 
            this.analysisProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.analysisProgress.Location = new System.Drawing.Point(0, 457);
            this.analysisProgress.Name = "analysisProgress";
            this.analysisProgress.Size = new System.Drawing.Size(825, 5);
            this.analysisProgress.TabIndex = 11;
            // 
            // ExporterView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(825, 462);
            this.Controls.Add(this.analysisProgress);
            this.Controls.Add(this.synchronize);
            this.Controls.Add(this.lblTriCount);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.lblCount);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.removeSelected);
            this.Controls.Add(this.clearDrawCalls);
            this.Controls.Add(this.addDrawCallsButton);
            this.Controls.Add(this.drawcallsToExport);
            this.Controls.Add(this.doExport);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinimumSize = new System.Drawing.Size(626, 295);
            this.Name = "ExporterView";
            this.Text = "FBX Exporter";
            this.Load += new System.EventHandler(this.ExporterView_Load);
            this.VisibleChanged += new System.EventHandler(this.ExporterView_VisibleChanged);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox uvChannelToExport;
        private System.Windows.Forms.CheckBox exportTextures;
        private System.Windows.Forms.Button doExport;
        private System.Windows.Forms.ListView drawcallsToExport;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.Button addDrawCallsButton;
        private System.Windows.Forms.Button clearDrawCalls;
        private System.Windows.Forms.Button removeSelected;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblCount;
        private System.Windows.Forms.Label lblTriCount;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.ColumnHeader columnHeader9;
        private System.Windows.Forms.ColumnHeader columnHeader10;
        private System.Windows.Forms.Button synchronize;
        private System.Windows.Forms.ColumnHeader columnHeader11;
        private System.Windows.Forms.ColumnHeader columnHeader12;
        private System.Windows.Forms.ColumnHeader columnHeader13;
        private System.Windows.Forms.ColumnHeader columnHeader14;
        private System.Windows.Forms.ProgressBar analysisProgress;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox texSlotToExport;
        private System.Windows.Forms.CheckBox exportWorldXForms;
        private System.Windows.Forms.CheckBox exportFrustums;
        private System.Windows.Forms.CheckBox exportSwapUV;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox typeToExport;
    }
}