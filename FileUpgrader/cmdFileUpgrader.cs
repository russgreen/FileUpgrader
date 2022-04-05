using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using System;
using System.Collections.Generic;

namespace FileUpgrader;

[Transaction(TransactionMode.Manual)]
public class cmdFileUpgrader : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        UIApplication UIApp = commandData.Application;
        UIApp.DialogBoxShowing += new EventHandler<DialogBoxShowingEventArgs>(OnDialogShowing);
        UIApp.Application.FailuresProcessing += new EventHandler<FailuresProcessingEventArgs>(OnFailuresProcessing);

        FormMain frm = new FormMain(commandData);
        frm.ShowDialog();

        UIApp.Application.FailuresProcessing -= OnFailuresProcessing;
        UIApp.DialogBoxShowing -= OnDialogShowing;

        return Result.Succeeded;
    }

    private static void OnFailuresProcessing(object sender, Autodesk.Revit.DB.Events.FailuresProcessingEventArgs e)
    {
        FailuresAccessor failuresAccessor = e.GetFailuresAccessor();
        IEnumerable<FailureMessageAccessor> failureMessages = failuresAccessor.GetFailureMessages();
        foreach (FailureMessageAccessor failureMessage in failureMessages)
        {
            if (failureMessage.GetSeverity() == FailureSeverity.Warning)
            {
                failuresAccessor.DeleteWarning(failureMessage);
            }
        }
        e.SetProcessingResult(FailureProcessingResult.Continue);
    }

    private static void OnDialogShowing(object o, DialogBoxShowingEventArgs e)
    {
        if (e.Cancellable)
        {
            e.Cancel();
        }
        if (e.DialogId == "TaskDialog_Unresolved_References")
        {
            //Worry about this later - 1002 = cancel
            e.OverrideResult(1002);
        }
        if (e.DialogId == "TaskDialog_Local_Changes_Not_Synchronized_With_Central")
        {
            //Don't sync newly created files. 1003 = close
            e.OverrideResult(1003);
        }
        if (e.DialogId == "TaskDialog_Save_Changes_To_Local_File")
        {
            //Relinquish unmodified elements and worksets
            e.OverrideResult(1001);
        }
        if (e.DialogId == "TaskDialog_Save_File")
        {
            //Confirm save changes
            e.OverrideResult(1001);
        }


    }
}
