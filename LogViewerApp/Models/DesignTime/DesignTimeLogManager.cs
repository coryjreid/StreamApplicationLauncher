namespace LogViewerApp.Models.DesignTime;

public class DesignTimeLogManager : LogManager
{
    public DesignTimeLogManager()
    {
        StartSimulatedLogging();
    }
}