using Velopack;

namespace DnDInitiativeTracker;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // Velopack must be the very first thing to run in Main.
        // It handles install, uninstall, and update hooks, then exits
        // if the process was started by the Velopack installer.
        VelopackApp.Build().Run();

        var app = new App();
        app.InitializeComponent();
        app.Run();
    }
}

