using MAUI_App_Tutorial.Models;
using System.Net.Http.Json;

namespace MAUI_App_Tutorial;

public partial class TablesPage : ContentPage
{
    HttpClient httpClient;
    Room room;
    User user;
    public List<Table> parentTablesList { get; set; }
    public List<Table> childTablesList { get; set; }
    public List<Table> filteredChildTablesList { get; set; }
    public TablesPage(Room room, User user)
	{
		InitializeComponent();
        this.httpClient = new HttpClient();
        this.parentTablesList = new List<Table>();
        this.childTablesList = new List<Table>();
        this.filteredChildTablesList = new List<Table>();
        this.room = room;
        this.user = user;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        this.LoadTables();
    }

    private async void LoadTables()
    {
        string baseURL = Preferences.Get("BaseURL", "");
        HttpResponseMessage response = await this.httpClient
            .GetAsync($"{baseURL}/api/Mese?idSala={this.room.Id}&idUser={this.user.Id}");

        if (response.IsSuccessStatusCode)
        {
            var tables = await response.Content.ReadFromJsonAsync<List<Table>>();
            foreach (Table table in tables)
            {
                if (table.IdCopil == null)
                {
                    this.parentTablesList.Add(table);
                }
                else
                {
                    this.childTablesList.Add(table);
                }
            }
            this.filteredChildTablesList = this.childTablesList;
            parentTables.ItemsSource = this.parentTablesList;
            childTables.ItemsSource = this.filteredChildTablesList;
        }
    }

    public void HandleParentTableClicked(object sender, EventArgs args)
    {
        Table selectedTable = parentTables.SelectedItem as Table;
        this.filteredChildTablesList = new List<Table>();
        foreach (Table table in this.childTablesList)
        {
            if (table.IdCopil == selectedTable.Id)
            {
                this.filteredChildTablesList.Add(table);
            }
        }
        childTables.ItemsSource = this.filteredChildTablesList;
    }

    public void HandleChildTableClicked(object sender, EventArgs args)
    {
        if (childTables.SelectedItem == null)
        {
            return;
        }
        Table selectedTable = childTables.SelectedItem as Table;
        Navigation.PushAsync(new ProductsPage(selectedTable, user));
        childTables.SelectedItem = null;
    }
}