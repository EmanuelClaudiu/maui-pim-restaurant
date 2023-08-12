using MAUI_App_Tutorial.Models;
using Mopups.Services;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;

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
        this.billItemsUI.SelectionChanged += OnBillItemClicked;

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
            double? totalBillPrice = 0;
            var garbageData = false;
            billItems.ForEach(item =>
            {
                if (item.Quantity != null && item.Product.Pret != null)
                {
                    totalBillPrice += (item.Quantity * item.Product.Pret);
                } else
                {
                    garbageData = true;
                }
            });
            totalUI.Text = $"Total: {decimal.Round((decimal)totalBillPrice, 3)} RON";
            if (garbageData)
            {
                totalUI.Text += "; Unele produse au cantitate sau pret setate incorect";
            }
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

    public async void OnSendButtonClicked(object sender, EventArgs e)
    {
        var unsentBillItems = BillItemsList.Where(billItem => billItem.orderSent == false).ToList();

        string baseURL = Preferences.Get("BaseURL", "");
        var endpoint = $"{baseURL}/api/BillItems/saveTableBill/{this.table.Id}?waiterName={this.user.NumeUtilizator}";

        var json = JsonSerializer.Serialize<List<BillItem>>(unsentBillItems);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await this.httpClient
            .PutAsync(endpoint, content);

        if (response.IsSuccessStatusCode)
        {
            var tableId = await response.Content.ReadFromJsonAsync<long>();
            await DisplayAlert("Succes", "Comanda a fost trimisa cu succes!", "Ok");
            await Navigation.PopAsync();
        } else
        {
            await DisplayAlert("Eroare", "Ceva nu a mers bine!", "Ok");
        }
    }

    public void OnHomeButtonClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new RoomsPage(this.user));
    }
}