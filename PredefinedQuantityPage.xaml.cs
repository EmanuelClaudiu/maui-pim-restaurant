using MAUI_App_Tutorial.Models;
using System.Net.Http.Json;
using System.Net.Http;
using System.Text.Json;
using System.Text;

namespace MAUI_App_Tutorial;

public partial class PredefinedQuantityPage : ContentPage
{
    Product product;
    Table table;
    User user;
    HttpClient httpClient;
    bool newBill;
    public List<PredefinedQuantity> predefinedQuantities = new List<PredefinedQuantity>();
	public PredefinedQuantityPage(Product product, Table table, User user, bool newBill)
	{
		this.product = product;
        this.table = table;
        this.user = user;
        this.newBill = newBill;
        httpClient = new HttpClient();
        Title = $"Cantitati predefinite {product.Denumire ?? ""}";
        InitializeComponent();
        if (product.CantitatiPredefinite.Count > 0)
        {
            this.predefinedQuantitiesUI.ItemsSource = product.CantitatiPredefinite;
        }
    }

	public async void OnPredefinedQuantityClick(object sender, EventArgs e)
	{
		PredefinedQuantity predefinedQuantity = this.predefinedQuantitiesUI.SelectedItem as PredefinedQuantity;
		await this.AddProductToBillAsync(predefinedQuantity);
        await Navigation.PopAsync();
	}

    public async Task AddProductToBillAsync(PredefinedQuantity predefinedQuantity)
	{
        string baseURL = Preferences.Get("BaseURL", "");
        var endpoint = $"{baseURL}/api/Product/{product.Id}/AddToBill/{this.table.Id}";

        string jsonPayload = JsonSerializer.Serialize(new AddProductPostPayload
        {
            IdUser = this.user.Id,
            IdProdusCantitatePredefinita = predefinedQuantity.Id,
            NumarComanda = GetOrderNumber()
        });
        StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await httpClient.PostAsync(endpoint, content);

        if (response.IsSuccessStatusCode)
        {
            List<BillItem> billItems = await response.Content.ReadFromJsonAsync<List<BillItem>>();
            await DisplayAlert("Succes", $"Produsul {product.Denumire} - {predefinedQuantity.Alias} a fost adaugat pe nota", "Ok");
        }
        else
        {
            await DisplayAlert("Eroare", $"Produsul {product.Denumire} - {predefinedQuantity.Alias} nu a putut fi adaugat pe nota", "Ok");
        }
    }

    public string GetOrderNumber()
    {
        string waiterName = this.user.NumeUtilizator;
        int orderNumber = Preferences.Get("OrderNumber", 0);
        if (this.newBill)
        {
            Preferences.Set("OrderNumber", orderNumber + 1);
            return $"{waiterName}-mobile-{orderNumber + 1}";
        }
        return $"{waiterName}-mobile-{orderNumber}";
    }
}