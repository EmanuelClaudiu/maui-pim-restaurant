namespace MAUI_App_Tutorial;

public partial class SettingsPage : ContentPage
{
	public SettingsPage()
	{
		// placeholder existing values to the entries
        InitializeComponent();
		this.LoadBaseURL();
	}

    public async void OnSaveClicked(object sender, EventArgs e)
	{
        // get values from entries and save to Preferences
		this.SetBaseURL();
        await DisplayAlert("Succes", "Setarile au fost salvate cu succes", "Ok");
        await Navigation.PopAsync();
	}

	private void LoadBaseURL()
	{
		string baseUrl = Preferences.Get("BaseURL", "");
		this.baseUrlEntryUI.Text = baseUrl;
	}

	private async void SetBaseURL()
	{
		string baseURL = this.baseUrlEntryUI?.Text ?? string.Empty;
		Preferences.Set("BaseURL", baseURL);
	}
}