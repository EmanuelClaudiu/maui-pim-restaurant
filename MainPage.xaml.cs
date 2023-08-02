using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using MAUI_App_Tutorial.Models;
using Microsoft.Maui.Controls;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;

namespace MAUI_App_Tutorial;

public class LoginPostPayload
{
	public string Code { get; set; }
}

public partial class MainPage : ContentPage
{
	public MainPage()
	{
        InitializeComponent();
	}

	async void OnAccessButtonClicked(object sender, EventArgs e)
	{
        HttpClient httpClient = new HttpClient();

        string jsonPayload = JsonSerializer.Serialize(new LoginPostPayload
        {
            Code = CodeInput.Text.Trim(),
        });
        StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
        string baseURL = Preferences.Get("BaseURL", "");
        HttpResponseMessage response = await httpClient.PostAsync(
            $"{baseURL}/Login", content);

        if (response.IsSuccessStatusCode)
        {
            User user = await response.Content.ReadFromJsonAsync<User>();
            Application.Current.MainPage = new NavigationPage(new RoomsPage(user));
        }
    }

    public async void OnSettingsButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SettingsPage());
    }
}

