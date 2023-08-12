using MAUI_App_Tutorial.Models;
using System.Net.Http.Json;

namespace MAUI_App_Tutorial;

public partial class RoomsPage : ContentPage
{
	HttpClient httpClient;
	public List<Room> Rooms { get; set; }
	User user;

    public RoomsPage(User user)
	{
		InitializeComponent();
		this.httpClient = new HttpClient();
		this.Rooms = new List<Room>();
		this.user = user;
		HelloLabel.Text = $"Buna {user.NumeUtilizator}!";
		this.GetRooms();
    }

	private async void GetRooms()
	{
        string baseURL = Preferences.Get("BaseURL", "");
        HttpResponseMessage response = await this.httpClient
			.GetAsync($"{baseURL}/api/Sala?idUser={this.user.Id}");

		if (response.IsSuccessStatusCode)
		{
            this.Rooms = await response.Content.ReadFromJsonAsync<List<Room>>();
			listRooms.ItemsSource = this.Rooms;
        }
	}

	public void HandleRoomClicked(object sender, EventArgs args)
	{
        Room selectedRoom = listRooms.SelectedItem as Room;
        Navigation.PushAsync(new TablesPage(selectedRoom, this.user));
    }

	public void OnLogoutClicked(object sender, EventArgs e)
	{
        Application.Current.MainPage = new NavigationPage(new MainPage());
    }

	public async void OnSettingsClicked(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new SettingsPage());
	}
}