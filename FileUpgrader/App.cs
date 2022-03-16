using System.Reflection;
using System.Windows.Media.Imaging;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace FileUpgrader;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
class App : IExternalApplication
{
    public static UIControlledApplication cachedUiCtrApp;

    public Result OnShutdown(UIControlledApplication application)
    {
        return Result.Succeeded;
    }

    public Result OnStartup(UIControlledApplication application)
    {
        cachedUiCtrApp = application;
        var ribbonPanel = CreateRibbonPanel();

        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("NDI5MTU0QDMxMzkyZTMxMmUzMFRYNXJ4UkFXd24waElSUGZwTWszc20yaldkWk0vaHZRdTlpbktHU0dzSTg9");

        return Result.Succeeded;
    }

    private RibbonPanel CreateRibbonPanel()
    {
        RibbonPanel panel;

        // Check if "Archisoft Tools already exists and use if its there
        try
        {
            panel = cachedUiCtrApp.CreateRibbonPanel("Archisoft Tools", Guid.NewGuid().ToString());
            panel.Name = "ARBG_FileUpgrader_ExtApp";
            panel.Title = "File Upgrader";
        }
        catch
        {
            var archisoftPanel = false;
            var pluginPath = @"C:\ProgramData\Autodesk\ApplicationPlugins";
            if (System.IO.Directory.Exists(pluginPath) == true)
            {
                foreach (var folder in System.IO.Directory.GetDirectories(pluginPath))
                {
                    if (folder.ToLower().Contains("archisoft") == true & folder.ToLower().Contains("archisoft fileupgrader") == false)
                    {
                        archisoftPanel = true;
                        break;
                    }
                }
            }

            if (archisoftPanel == true)
            {
                cachedUiCtrApp.CreateRibbonTab("Archisoft Tools");
                panel = cachedUiCtrApp.CreateRibbonPanel("Archisoft Tools", Guid.NewGuid().ToString());
                panel.Name = "ARBG_FileUpgrader_ExtApp";
                panel.Title = "File Upgrader";
            }
            else
            {
                panel = cachedUiCtrApp.CreateRibbonPanel("File Upgrader");
            }
        }

        PushButtonData pbData = new PushButtonData("File Upgrader", "File Upgrader", Assembly.GetExecutingAssembly().Location, "FileUpgrader.cmdFileUpgrader");
        pbData.AvailabilityClassName = "FileUpgrader.Availability";

        PushButton pb = (PushButton)panel.AddItem(pbData);
        pb.ToolTip = "Upgrade Revit files to the latest version";
        pb.LargeImage = PngImageSource("FileUpgrader.Images.FileUpgrader32.png");


        //ContextualHelp contextHelp = new ContextualHelp(ContextualHelpType.Url, @"C:\ProgramData\Autodesk\ApplicationPlugins\Archisoft ExportSchedule.bundle\Contents\ExportSchedule.htm");
        //pbExpSched.SetContextualHelp(contextHelp);

        return panel;
    }

    private System.Windows.Media.ImageSource PngImageSource(string embeddedPath)
    {
        var stream = GetType().Assembly.GetManifestResourceStream(embeddedPath);
        System.Windows.Media.ImageSource imageSource;
        try
        {
            imageSource = BitmapFrame.Create(stream);
        }
        catch
        {
            imageSource = null;
        }

        return imageSource;
    }
}
