using MAUI_App_Tutorial.Models;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Web;

namespace MAUI_App_Tutorial;

public class AddProductPostPayload
{
    public int? IdUser { get; set; }
    public long? IdProdusCantitatePredefinita { get; set; }
    public string? NumarComanda { get; set; }
}

public partial class ProductsPage : ContentPage
{
    Table table;
    User user;
	HttpClient httpClient;
	public List<PimLocation> LocationsList { get; set; } = new List<PimLocation>();
    public List<Group> GroupsList { get; set; } = new List<Group>();
    public List<Product> ProductsList { get; set; } = new List<Product>();
    public List<BillItem> BillItemList { get; set; } = new List<BillItem>();
    PimLocation selectedLocation = null;
    Group selectedGroup = null;
    string searchString = null;
    public ProductsPage(Table table, User user)
	{
        this.table = table;
        this.user = user;
        this.httpClient = new HttpClient();
		InitializeComponent();
        this.LoadLocations();
        this.LoadGroups();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await this.LoadProducts();
        await LoadBillForTable();
    }

    private async void LoadLocations()
	{
        string baseURL = Preferences.Get("BaseURL", "");
        HttpResponseMessage response = await this.httpClient
            .GetAsync($"{baseURL}/api/Location");

        if (response.IsSuccessStatusCode)
        {
            var locations = await response.Content.ReadFromJsonAsync<List<PimLocation>>();
            LocationsList = locations;
            LocationsList.Insert(0, new PimLocation
            {
                Id = null,
                Denumire = "TOATE",
                Departament = null
            });
            locationsUI.ItemsSource = this.LocationsList;
        }
    }

    private async void LoadGroups()
    {
        string baseURL = Preferences.Get("BaseURL", "");
        var endpoint = $"{baseURL}/api/Group";
        if (this.selectedLocation != null && this.selectedLocation.Id != null)
        {
            // first load or TOATE selected
            endpoint = $"http://192.168.0.105:8099/api/Group?idLocation={this.selectedLocation.Id}";
        }
        HttpResponseMessage response = await this.httpClient
            .GetAsync(endpoint);

        if (response.IsSuccessStatusCode)
        {
            var groups = await response.Content.ReadFromJsonAsync<List<Group>>();
            GroupsList = groups;
            groupsUI.ItemsSource = this.GroupsList;
        }
    }

    private async Task LoadProducts()
    {
        var endpoint = this.GetProductsEndpoint();
        HttpResponseMessage response = await this.httpClient
            .GetAsync(endpoint);

        if (response.IsSuccessStatusCode)
        {
            var products = await response.Content.ReadFromJsonAsync<List<Product>>();
            ProductsList = products;
            productsUI.ItemsSource = this.ProductsList;
        }
    }

    private async Task LoadBillForTable()
    {
        string baseURL = Preferences.Get("BaseURL", "");
        var endpoint = $"{baseURL}/api/BillItems$idTable={this.table.Id}";

        HttpResponseMessage response = await this.httpClient
            .GetAsync(endpoint);

        if (response.IsSuccessStatusCode)
        {
            var billItems = await response.Content.ReadFromJsonAsync<List<BillItem>>();
            BillItemList = billItems;
        }
    }
    public void OnLocationClick(object sender, EventArgs e) {
        PimLocation location = locationsUI.SelectedItem as PimLocation;
        searchBarUI.Text = "";
        this.selectedLocation = location;
        this.selectedGroup = null;
        this.searchString = null;
        this.LoadGroups();
        this.LoadProducts();
    }

    public void OnGroupClick(object sender, EventArgs e)
    {
        Group group = groupsUI.SelectedItem as Group;
        searchBarUI.Text = "";
        this.selectedGroup = group;
        this.searchString = null;
        this.LoadProducts();
    }

    public async void OnProductClick(object sender, EventArgs e)
    {
        if (productsUI.SelectedItem == null)
        {
            return;
        }
        Product product = productsUI.SelectedItem as Product;
        if (this.table.Status == TableStatus.NOTA_EMISA)
        {
            await DisplayAlert("Nota de PLata Emisa", "S-a emis nota de plata pentru aceasta masa. Nu mai poti adauga produse.", "Ok");
            return;
        }
        if (this.table.IdUser.HasValue && this.table.IdUser != this.user.Id)
        {
            await DisplayAlert("Eroare Adaugare Produs", "Nu ai acces sa editezi comanda in curs la aceasta masa.", "Ok");
            return;
        }

        if (product.CantitatiPredefinite.Count != 0)
        {
            // move to predefined quantity details
            await Navigation.PushAsync(new PredefinedQuantityPage(product, this.table, this.user, this.BillItemList.Count == 0));
        }
        else
        {
            // add product on bill
            this.AddProductToBill(product);
        }
        productsUI.SelectedItem = null;
        this.LoadProducts();
    }
    public void OnSearchCompleted(object sender, EventArgs e) {
        string query = searchBarUI.Text;
        this.searchString = query;
        this.selectedLocation = null;
        this.selectedGroup = null;
        this.LoadProducts();
    }

    public void OnBillButtonClick(object sender, EventArgs e)
    {
        Navigation.PushAsync(new BillPage(this.table, this.user));
    }

    public string GetProductsEndpoint()
    {
        string baseURL = Preferences.Get("BaseURL", "");
        string baseUri = $"{baseURL}/api/Product";
        Dictionary<string, string> queryParams = new Dictionary<string, string>();
        if (this.selectedLocation != null && this.selectedLocation.Id != null)
        {
            queryParams.Add("idLocation", this.selectedLocation.Id.ToString());
        }
        if (this.selectedGroup != null)
        {
            queryParams.Add("idGroup", this.selectedGroup.Id.ToString());
        }
        if (this.searchString != null)
        {
            queryParams.Add("searchPrompt", this.searchString);
        }

        var queryBuilder = new UriBuilder(baseUri);
        var query = HttpUtility.ParseQueryString(queryBuilder.Query);
        foreach (var kvp in queryParams)
        {
            query[kvp.Key] = kvp.Value;
        }

        queryBuilder.Query = query.ToString();
        string fullUri = queryBuilder.ToString();
        return fullUri;
    }

    public string GetOrderNumber()
    {
        string waiterName = this.user.NumeUtilizator;
        int orderNumber = Preferences.Get("OrderNumber", 0);
        if (this.BillItemList.Count == 0)
        {
            Preferences.Set("OrderNumber", orderNumber + 1);
            return $"{waiterName}-mobile-{orderNumber + 1}";
        }
        return $"{waiterName}-mobile-{orderNumber}";
    }

    public async void AddProductToBill(Product product)
    {
        string baseURL = Preferences.Get("BaseURL", "");
        var endpoint = $"{baseURL}/api/Product/{product.Id}/AddToBill/{this.table.Id}";

        string jsonPayload = JsonSerializer.Serialize(new AddProductPostPayload
        {
            IdUser = this.user.Id,
            IdProdusCantitatePredefinita = null,
            NumarComanda = GetOrderNumber()
        });
        StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await httpClient.PostAsync(endpoint, content);

        if (response.IsSuccessStatusCode)
        {
            List<BillItem> billItems = await response.Content.ReadFromJsonAsync<List<BillItem>>();
            this.BillItemList = billItems;
            await DisplayAlert("Succes", $"Produsul {product.Denumire} a fost adaugat pe nota", "Ok");
        }
        else
        {
            await DisplayAlert("Eroare", $"Produsul {product.Denumire} nu a putut fi adaugat pe nota", "Ok");
        }
    }
}