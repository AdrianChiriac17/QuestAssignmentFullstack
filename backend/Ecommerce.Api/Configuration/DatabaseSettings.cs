namespace Ecommerce.Api.Configuration;

public class DatabaseSettings
{
    public const string SectionName = "ConnectionStrings";

    public string DefaultConnection { get; set; } = string.Empty;
}
