using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FitLife.Trainer.Api.Pages;

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

public class TrainerItemDTO
{
    public Guid Id { get; set; }
    public string? FullName { get; set; }
    public string? Specialty { get; set; }
    public double Rating { get; set; }
    public int SessionCount { get; set; }
}
