using Microsoft.Extensions.Logging;
using Journal_Entry.Components.Services;

namespace Journal_Entry
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            try
            {
                // Move SQLite initialization inside the method
                SQLitePCL.Batteries.Init();

                var builder = MauiApp.CreateBuilder();
                builder
                    .UseMauiApp<App>()
                    .ConfigureFonts(fonts =>
                    {
                        fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    });

                builder.Services.AddMauiBlazorWebView();
                builder.Services.AddSingleton<JournalService>();
                builder.Services.AddSingleton<PdfExportService>();

#if DEBUG
                builder.Services.AddBlazorWebViewDeveloperTools();
                builder.Logging.AddDebug();
#endif
                return builder.Build();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"STARTUP ERROR: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"STACK TRACE: {ex.StackTrace}");
                throw;
            }
        }
    }
}