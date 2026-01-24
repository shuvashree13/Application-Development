namespace Journal_Entry
{
    public partial class App : Application
    {
        public App()
        {
            try
            {
                InitializeComponent();

                // Add unhandled exception handler
                AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                {
                    var ex = e.ExceptionObject as Exception;
                    System.Diagnostics.Debug.WriteLine($"UNHANDLED EXCEPTION: {ex?.Message}");
                    System.Diagnostics.Debug.WriteLine($"STACK: {ex?.StackTrace}");
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"APP INIT ERROR: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"STACK: {ex.StackTrace}");
                throw;
            }
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            try
            {
                return new Window(new MainPage()) { Title = "Journal Entry" };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WINDOW CREATE ERROR: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"STACK: {ex.StackTrace}");
                throw;
            }
        }
    }
}