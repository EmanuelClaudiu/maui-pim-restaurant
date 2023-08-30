using MAUI_App_Tutorial.Models;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MAUI_App_Tutorial;

public partial class BillItemDetails : ContentPage
{
    HttpClient httpClient = new HttpClient();
    BillItem billItem;
    User user;
    Table table;
    public List<BillItem> billItems { get; set; }
	public BillItemDetails(BillItem billItem, List<BillItem> billItems, User user, Table table)
	{
        this.billItem = billItem;
        this.billItems = billItems;
        this.user = user;
        this.table = table;
		InitializeComponent();
        quantityEntryUI.Text = billItem.Quantity.ToString() ?? "";
        mentionsEntryUI.Text = billItem.Mention ?? "";
        Title = billItem.Product?.Denumire ?? "Produs";
        if (billItem.PredefinedQuantity != 1)
        {
            quantityEntryUI.IsReadOnly = true;
        }
    }

    public async void OnSaveClicked(object sender, EventArgs e)
    {
        if (this.CheckRightQuantityTextFormat() == false)
        {
            await DisplayAlert("Error", $"Cantitatea introdusa nu este in formatul corect (numar cu maxim 3 cifre dupa virgula).", "Ok");
            return;
        } else
        {
            this.billItem.Mention = mentionsEntryUI.Text;

            string baseURL = Preferences.Get("BaseURL", "");
            var endpoint = $"{baseURL}/api/BillItems/{this.billItem.Id}";

            var json = JsonSerializer.Serialize<BillItem>(this.billItem);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await this.httpClient
                .PutAsync(endpoint, content);

            if (response.IsSuccessStatusCode)
            {
                var newBillItem = await response.Content.ReadFromJsonAsync<BillItem>();
                await DisplayAlert("Update", "Produsul de pe nota a fost modificat cu succes!", "Ok");
                await Navigation.PopAsync();
            }
        }
    }

    public async void OnDeleteClicked(object sender, EventArgs e)
    {
        string baseURL = Preferences.Get("BaseURL", "");
        var endpoint = $"{baseURL}/api/BillItems/{this.billItem.Id}";
        
        HttpResponseMessage response = await this.httpClient
            .DeleteAsync(endpoint);

        if (response.IsSuccessStatusCode)
        {
            var deleteId = await response.Content.ReadFromJsonAsync<long>();
            await DisplayAlert("Stergere", "Produsul a fost sters de pe nota!", "Ok");
            await Navigation.PopAsync();
        }
    }

    public async void OnMinusClicked(object sender, EventArgs e)
    {
        if (this.table.IdUser != null && this.table.IdUser != this.user.Id)
        {
            await DisplayAlert("Error", $"Nu ai acces sa editezi comanda in curs la aceasta masa.", "Ok");
        }
        if (!this.CheckReturnsValidity())
        {
            await DisplayAlert("Error", $"Nu mai poti scadea {billItem.PredefinedQuantity} din {billItem.Product.Denumire}. " +
                $"Stornarea nu va mai fi corecta", "Ok");
        } else
        {
            decimal newQuantity
                = decimal.Round((decimal)billItem.Quantity, 3) - decimal.Round((decimal)billItem.PredefinedQuantity, 3);            
            billItem.Quantity = (double)newQuantity;
            quantityEntryUI.Text = billItem.Quantity.ToString() ?? "";
        }
    }

    public async void OnPlusClicked(object sender, EventArgs e)
    {
        if (this.table.IdUser != null && this.table.IdUser != this.user.Id)
        {
            await DisplayAlert("Error", $"Nu ai acces sa editezi comanda in curs la aceasta masa.", "Ok");
        }
        decimal newQuantity
                = decimal.Round((decimal)billItem.Quantity, 3) + decimal.Round((decimal)billItem.PredefinedQuantity, 3);
        billItem.Quantity = (double)newQuantity;
        quantityEntryUI.Text = billItem.Quantity.ToString() ?? "";
    }

    public bool CheckReturnsValidity()
    {
        if (this.user.PermisStornare == false)
        {
            return false;
        }
        double? total = 0;
        foreach (BillItem item in billItems)
        {
            if (item.Product.Id == billItem.Product.Id)
            {
                total += item.Quantity;
            }
        }
        if (total - billItem.PredefinedQuantity >= 0)
        {
            return true;
        }
        return false;
    }

    public bool CheckRightQuantityTextFormat()
    {
        string pattern = @"^\d+(\.\d{1,3})?$";
        return Regex.IsMatch(quantityEntryUI.Text, pattern);
    }
}