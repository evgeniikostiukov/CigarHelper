using CigarHelper.Scrapers.Models;

namespace CigarHelper.Scrapers.Services;

public class MockCigarDataService
{
    private readonly Random _random = new Random();
    
    public CigarData GenerateMockData()
    {
        var result = new CigarData
        {
            Brands = new List<CigarBrand>
            {
                CreateCubanBrand(),
                CreateDominicanBrand(),
                CreateNicaraguanBrand(),
                CreateHondurasBrand()
            }
        };
        
        return result;
    }
    
    private CigarBrand CreateCubanBrand()
    {
        return new CigarBrand
        {
            Name = "Cohiba",
            Country = "Cuba",
            Description = "Cohiba is the flagship brand of Habanos S.A. and Cuba. Created in 1966 for Fidel Castro himself and was made at the El Laguito factory in Havana.",
            Products = new List<CigarProduct>
            {
                new CigarProduct
                {
                    Name = "Cohiba Robusto",
                    Vitola = "Robusto",
                    Length = 4.9f,
                    RingGauge = 50,
                    Strength = "Medium to Full",
                    Wrapper = "Cuban",
                    Binder = "Cuban",
                    Filler = "Cuban",
                    Description = "The Cohiba Robusto is a classic Cuban cigar, offering a rich, complex flavor profile with notes of wood, coffee, and subtle spice.",
                    ImageUrl = "https://cigarday.ru/images/cigars/cohiba-robusto.jpg",
                    Price = 35.50m
                },
                new CigarProduct
                {
                    Name = "Cohiba Esplendidos",
                    Vitola = "Churchill",
                    Length = 7.0f,
                    RingGauge = 47,
                    Strength = "Medium to Full",
                    Wrapper = "Cuban",
                    Binder = "Cuban",
                    Filler = "Cuban",
                    Description = "The flagship of the Cohiba line, the Esplendidos is a classic Churchill-sized cigar with a perfect balance of flavors.",
                    ImageUrl = "https://cigarday.ru/images/cigars/cohiba-esplendidos.jpg",
                    Price = 45.75m
                },
                new CigarProduct
                {
                    Name = "Cohiba Siglo VI",
                    Vitola = "Canonazo",
                    Length = 5.9f,
                    RingGauge = 52,
                    Strength = "Medium",
                    Wrapper = "Cuban",
                    Binder = "Cuban",
                    Filler = "Cuban",
                    Description = "The Siglo VI is part of the Línea 1492 and is considered one of the finest expressions of the Cohiba brand.",
                    ImageUrl = "https://cigarday.ru/images/cigars/cohiba-siglo-vi.jpg",
                    Price = 40.25m
                }
            }
        };
    }
    
    private CigarBrand CreateDominicanBrand()
    {
        return new CigarBrand
        {
            Name = "Arturo Fuente",
            Country = "Dominican Republic",
            Description = "The Arturo Fuente brand is one of the most respected names in the cigar industry, known for their commitment to quality and consistency.",
            Products = new List<CigarProduct>
            {
                new CigarProduct
                {
                    Name = "Arturo Fuente Hemingway Short Story",
                    Vitola = "Perfecto",
                    Length = 4.0f,
                    RingGauge = 49,
                    Strength = "Medium",
                    Wrapper = "Cameroon",
                    Binder = "Dominican",
                    Filler = "Dominican",
                    Description = "A small but mighty perfecto with a distinctive shape and exceptional construction. Notes of cedar, coffee, and natural sweetness.",
                    ImageUrl = "https://cigarday.ru/images/cigars/arturo-fuente-hemingway-short-story.jpg",
                    Price = 9.99m
                },
                new CigarProduct
                {
                    Name = "Arturo Fuente Opus X",
                    Vitola = "Robusto",
                    Length = 5.2f,
                    RingGauge = 50,
                    Strength = "Full",
                    Wrapper = "Dominican",
                    Binder = "Dominican",
                    Filler = "Dominican",
                    Description = "The legendary Opus X is the first successful Dominican puro and is considered one of the finest cigars in the world.",
                    ImageUrl = "https://cigarday.ru/images/cigars/arturo-fuente-opus-x.jpg",
                    Price = 30.50m
                },
                new CigarProduct
                {
                    Name = "Arturo Fuente Don Carlos",
                    Vitola = "Double Robusto",
                    Length = 5.75f,
                    RingGauge = 52,
                    Strength = "Medium",
                    Wrapper = "Cameroon",
                    Binder = "Dominican",
                    Filler = "Dominican",
                    Description = "Named after the company's founder, the Don Carlos line is known for its smoothness and complexity.",
                    ImageUrl = "https://cigarday.ru/images/cigars/arturo-fuente-don-carlos.jpg",
                    Price = 14.50m
                }
            }
        };
    }
    
    private CigarBrand CreateNicaraguanBrand()
    {
        return new CigarBrand
        {
            Name = "Padron",
            Country = "Nicaragua",
            Description = "Family-owned Padron is known for their box-pressed cigars and consistent quality, offering some of the highest-rated cigars in the world.",
            Products = new List<CigarProduct>
            {
                new CigarProduct
                {
                    Name = "Padron 1964 Anniversary Series",
                    Vitola = "Torpedo",
                    Length = 6.0f,
                    RingGauge = 52,
                    Strength = "Medium-Full",
                    Wrapper = "Nicaraguan",
                    Binder = "Nicaraguan",
                    Filler = "Nicaraguan",
                    Description = "The 1964 Anniversary Series commemorates the founding of the company and is available in both natural and maduro wrappers.",
                    ImageUrl = "https://cigarday.ru/images/cigars/padron-1964.jpg",
                    Price = 19.99m
                },
                new CigarProduct
                {
                    Name = "Padron 1926 Series",
                    Vitola = "No. 9",
                    Length = 5.25f,
                    RingGauge = 56,
                    Strength = "Full",
                    Wrapper = "Nicaraguan",
                    Binder = "Nicaraguan",
                    Filler = "Nicaraguan",
                    Description = "Released to commemorate Jose O. Padron's 75th birthday, the 1926 Series is a full-bodied, complex smoke with well-aged tobacco.",
                    ImageUrl = "https://cigarday.ru/images/cigars/padron-1926.jpg",
                    Price = 24.50m
                },
                new CigarProduct
                {
                    Name = "Padron Family Reserve",
                    Vitola = "No. 45",
                    Length = 6.0f,
                    RingGauge = 52,
                    Strength = "Full",
                    Wrapper = "Nicaraguan",
                    Binder = "Nicaraguan",
                    Filler = "Nicaraguan",
                    Description = "The Family Reserve uses tobacco aged for a minimum of 10 years, creating an extraordinarily smooth and complex smoking experience.",
                    ImageUrl = "https://cigarday.ru/images/cigars/padron-family-reserve.jpg",
                    Price = 29.99m
                }
            }
        };
    }
    
    private CigarBrand CreateHondurasBrand()
    {
        return new CigarBrand
        {
            Name = "Camacho",
            Country = "Honduras",
            Description = "Known for their bold and powerful cigars, Camacho is a Honduran brand that specializes in full-bodied smokes using authentic Corojo tobacco.",
            Products = new List<CigarProduct>
            {
                new CigarProduct
                {
                    Name = "Camacho Corojo",
                    Vitola = "Toro",
                    Length = 6.0f,
                    RingGauge = 50,
                    Strength = "Full",
                    Wrapper = "Honduran Corojo",
                    Binder = "Honduran",
                    Filler = "Honduran",
                    Description = "The Camacho Corojo is a Honduran puro featuring authentic Corojo tobacco for a bold, spicy experience.",
                    ImageUrl = "https://cigarday.ru/images/cigars/camacho-corojo.jpg",
                    Price = 8.50m
                },
                new CigarProduct
                {
                    Name = "Camacho Triple Maduro",
                    Vitola = "Robusto",
                    Length = 5.0f,
                    RingGauge = 50,
                    Strength = "Full",
                    Wrapper = "Maduro",
                    Binder = "Maduro",
                    Filler = "Maduro",
                    Description = "The world's first all-maduro cigar, featuring maduro tobacco in the wrapper, binder, and filler for an intense experience.",
                    ImageUrl = "https://cigarday.ru/images/cigars/camacho-triple-maduro.jpg",
                    Price = 12.25m
                },
                new CigarProduct
                {
                    Name = "Camacho Connecticut",
                    Vitola = "Churchill",
                    Length = 7.0f,
                    RingGauge = 48,
                    Strength = "Medium",
                    Wrapper = "Ecuador Connecticut",
                    Binder = "Honduran",
                    Filler = "Dominican, Honduran",
                    Description = "The Connecticut offers a milder approach to the typically bold Camacho profile, with a smooth Ecuador Connecticut wrapper.",
                    ImageUrl = "https://cigarday.ru/images/cigars/camacho-connecticut.jpg",
                    Price = 9.99m
                }
            }
        };
    }
} 