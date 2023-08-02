using MAUI_App_Tutorial.Models;
using Mopups.Services;
using System.Net.Http.Json;

namespace MAUI_App_Tutorial;

public partial class BillItemDetails : ContentPage
{
    HttpClient httpClient = new HttpClient();
    BillItem billItem;
    public List<BillItem> billItems { get; set; }
	public BillItemDetails(BillItem billItem, List<BillItem> billItems)
	{
        this.billItem = billItem;
        this.billItems = billItems;
		InitializeComponent();
        quantityEntryUI.Text = billItem.Quantity.ToString() ?? "";
        mentionsEntryUI.Text = billItem.Mention ?? "";
    }

    public void OnSaveClicked(object sender, EventArgs e)
    {
        // send put request
        // check quantity entry validity
        Navigation.PopAsync();
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
        // check quantity entry validity
        // do operations with doubles rounded to 2 decimals
        if (!this.CheckReturnsValidity())
        {
            await DisplayAlert("Error", $"Nu mai poti scadea {billItem.PredefinedQuantity} din {billItem.Product.Denumire}. " +
                $"Stornarea nu va mai fi corecta", "Ok");
        } else
        {
            billItem.Quantity -= billItem.PredefinedQuantity;
            quantityEntryUI.Text = billItem.Quantity.ToString() ?? "";
        }
    }

    public void OnPlusClicked(object sender, EventArgs e)
    {
        billItem.Quantity += billItem.PredefinedQuantity;
        quantityEntryUI.Text = billItem.Quantity.ToString() ?? "";
    }

    public bool CheckReturnsValidity()
    {
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
}