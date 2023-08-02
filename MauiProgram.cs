using Microsoft.Extensions.Logging;

namespace MAUI_App_Tutorial;

public static class LocalStorageConfig
{
	public static void ConfigureLocalStorageData()
	{
		string? baseUrl = Preferences.Get("BaseURL", null);
		if (baseUrl == null)
		{
			Preferences.Set("BaseURL", "http://192.168.0.105:8099");
		}
		var orderNumber = Preferences.Get("OrderNumber", -1);
		if (orderNumber == -1)
		{
			Preferences.Set("OrderNumber", 0);
		}
	}
}

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();

		LocalStorageConfig.ConfigureLocalStorageData();

		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
