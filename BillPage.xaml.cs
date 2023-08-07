using MAUI_App_Tutorial.Models;
using Mopups.Services;
using System.Net.Http.Json;

namespace MAUI_App_Tutorial;

public partial class BillPage : ContentPage
{
	Table table;
    User user;
    HttpClient httpClient;
    public List<BillItem> BillItemsList { get; set; }
    bool firstLoadFlag = true;
	public BillPage(Table table, User user)
	{
		InitializeComponent();
        this.httpClient = new HttpClient();
		this.table = table;
        this.user = user;
        pageTitleUI.Text = $"Nota de plata masa nr. {table.Id}";
        /*this.LoadBill();*/
        this.billItemsUI.ItemSelected += OnBillItemClicked;

    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        this.LoadBill();
    }

    private async void LoadBill()
	{
        string baseURL = Preferences.Get("BaseURL", "");
        HttpResponseMessage response = await this.httpClient
            .GetAsync($"{baseURL}/api/BillItems?idTable={this.table.Id}");

        if (response.IsSuccessStatusCode)
        {
            var billItems = await response.Content.ReadFromJsonAsync<List<BillItem>>();
            BillItemsList = billItems;
            billItemsUI.ItemsSource = this.BillItemsList;
            // compute total
        }
    }

    public async void OnBillItemClicked(object sender, EventArgs e)
    {
        if (billItemsUI.SelectedItem == null)
        {
            return;
        }
        BillItem billItem = billItemsUI.SelectedItem as BillItem;
        if (billItem.orderSent == true)
        {
            await DisplayAlert("Interzis", "Nu puteti edita un produs a carui comanda a fost deja plasata!", "Ok");
        } else
        {
            await Navigation.PushAsync(new BillItemDetails(billItem, BillItemsList, this.user, this.table));
        }
        billItemsUI.SelectedItem = null;
    }

    public void OnHomeButtonClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new RoomsPage(this.user));
    }
}