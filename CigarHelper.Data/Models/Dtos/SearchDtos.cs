namespace CigarHelper.Data.Models.Dtos;

public class GlobalSearchResultDto
{
    public List<SearchCigarDto> Cigars { get; set; } = new();
    public List<SearchHumidorDto> Humidors { get; set; } = new();
    public List<SearchCigarBaseDto> CigarBases { get; set; } = new();
    public List<SearchBrandDto> Brands { get; set; } = new();
}

public class SearchCigarDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public string? HumidorName { get; set; }
}

public class SearchHumidorDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class SearchCigarBaseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
}

public class SearchBrandDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Country { get; set; }
}
