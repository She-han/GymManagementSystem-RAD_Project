using System.Configuration;
using System.Data;
using System.Windows;
using GymManagementSystem.DAL;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            // Initialize database with WAL mode
            DatabaseHelper.InitializeDatabase();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Database initialization failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        // Clear connection pool on exit
        DatabaseHelper.ClearConnectionPool();
        base.OnExit(e);
    }
}