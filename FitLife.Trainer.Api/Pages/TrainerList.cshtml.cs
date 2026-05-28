using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FitLife.Trainer.Api.Pages;

// Razor Page der henter og viser en liste af trænere via Nginx-gatewayen.
// Bruges til Page Decomposition-mønstret (M14.01) — serveres statisk af Nginx på /trainers/list.
public class TrainerListModel : PageModel
{
    private readonly IHttpClientFactory? _clientFactory = null;
    public List<TrainerItemDTO>? Trainers { get; set; }
    public string Hostname { get; set; } = System.Net.Dns.GetHostName();

    public TrainerListModel(IHttpClientFactory clientFactory)
        => _clientFactory = clientFactory;

    public void OnGet()
    {
        using HttpClient? client = _clientFactory?.CreateClient("FitLifeGateway");
        try
        {
            Trainers = client?.GetFromJsonAsync<List<TrainerItemDTO>>("api/trainers").Result;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}

// DTO til at modtage trænerdata fra API'et i Razor Page-visningen
public class TrainerItemDTO
{
    public Guid Id { get; set; }
    public string? FullName { get; set; }
    public string? Specialty { get; set; }
    public double Rating { get; set; }
    public int SessionCount { get; set; }
}
