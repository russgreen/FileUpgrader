
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Syncfusion.WinForms.DataGrid;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace FileUpgrader;

public partial class FormMain : System.Windows.Forms.Form
{
    private ExternalCommandData _commandData;
    private UIDocument _uiDoc;
    private UIApplication _uiApp;
    private Document _doc;

    private IList<FileInfo> _files = new List<FileInfo>();
    private IList<String> _failures = new List<String>();

    private StreamWriter _writer = null;

    private bool _cancelled = false;
    private bool _addInfo = false;

    private int _success;
    private int _failed;

    private UIDocument _previousDocument = null;


    public FormMain(ExternalCommandData commandData)
    {
        InitializeComponent();

        this.sfDataGrid1.Columns.Clear();
        this.sfDataGrid1.Columns.Add(new GridTextColumn() { MappingName = "Name", HeaderText = "File name", MinimumWidth = 200 });
        this.sfDataGrid1.Columns.Add(new GridCheckBoxColumn() { MappingName = "IsReadOnly", HeaderText = "Readonly" });
        this.sfDataGrid1.Columns.Add(new GridTextColumn() { MappingName = "FullName", HeaderText = "File path", Width = 200 });


        _commandData = commandData;
    }

    private void sfDataGrid1_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effect = DragDropEffects.Copy;
        }
    }

    private void sfDataGrid1_DragDrop(object sender, DragEventArgs e)
    {
        string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
        foreach (var file in files)
        {
            if(file.EndsWith(".rvt") || file.EndsWith(".rfa"))
            {
                AddFileToGrid(file);
            }
        }
    }

    private void AddFileToGrid(string filePath)
    {

        _files.Add(new FileInfo(filePath));
        WireUpDocuments();
    }

    private void WireUpDocuments()
    {
        this.sfDataGrid1.DataSource = _files;
        this.sfDataGrid1.View.Refresh();

        this.toolStripProgressBar1.Maximum = _files.Count - 1;
        this.toolStripProgressBar1.Step = 1;
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        _cancelled = true;
        this.Close();
    }

    private void btnUpgrade_Click(object sender, EventArgs e)
    {
        this.btnUpgrade.Enabled = false;

        foreach (var file in _files)
        {
            if (_cancelled == false && file.IsReadOnly == false)
            {
               Upgrade(file);
               Application.DoEvents();
            }
        }

        this.btnCancel.Enabled = false;
    }

    // Method which upgrades each file
    private void Upgrade(FileInfo file)
    {
        _addInfo = false;
        
        try
        {
            // Open a Revit file as an active document. 
            OpenOptions openOpt = new OpenOptions();
            openOpt.DetachFromCentralOption = DetachFromCentralOption.DetachAndPreserveWorksets;

            ModelPath modelPath = new FilePath(file.FullName);

            _uiApp = _commandData.Application;
            _uiDoc = _uiApp.OpenAndActivateDocument(modelPath, openOpt, true);
            _doc = _uiDoc.Document;

            // Try closing the previously opened document after 
            // another one is opened. We are doing this because we 
            // cannot explicitely close an active document
            //  at a moment.  

            if(_previousDocument != null)
            {
                _previousDocument.SaveAndClose();
            }

            // Save the Revit file to the target destination.
            // Since we are opening a file as an active document, 
            // it takes care of preview. 

            if (_doc.IsWorkshared)
            {
                WorksharingSaveAsOptions wsOpt = new WorksharingSaveAsOptions();
                wsOpt.SaveAsCentral = true;

                SaveAsOptions opt = new SaveAsOptions();
                opt.OverwriteExistingFile = true;
                opt.SetWorksharingOptions(wsOpt);
                    
                _doc.SaveAs(file.FullName, opt);
            }
            else
            {
                //String destinationFile = destPath + "\\" + file.Name;
                _doc.Save(); //  .SaveAs(file.Name);
            }         

            // Saving the current document to close it later.   
            // If we had a method to close an active document, 
            // we want to close it here. However, since we opened 
            // it as an active document, we cannot do so.
            // We'll close it after the next file is opened.
            _previousDocument = _uiDoc;
           
            // Set variable to know if upgrade 
            // was successful - for status updates

            _addInfo = true;
            
            // Log file and user interface updates
            lstBxUpdates.Items.Add($"\n {file.Name} has been upgraded");
            lstBxUpdates.TopIndex = lstBxUpdates.Items.Count - 1;

            toolStripProgressBar1.PerformStep();
            ++_success;
            
        }
        catch (Exception ex)
        {
            _failures.Add(file.FullName
              + " could not be upgraded: "
              + ex.Message);

            toolStripProgressBar1.PerformStep();

            ++_failed;
        }

    }

    private void statusStrip1_Resize(object sender, EventArgs e)
    {
        toolStripProgressBar1.Width = statusStrip1.Width - 20;
    }

}
