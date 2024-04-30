using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace FileUpgrader;

public class Availability : IExternalCommandAvailability
{
    public bool IsCommandAvailable(UIApplication a, CategorySet b)
    {
        return true;
    }
}
