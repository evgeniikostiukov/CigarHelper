# CigarHelper.Scrapers

This project contains tools for extracting cigar data from cigarday.ru and generating mock cigar data for the CigarHelper application.

## Components

### 1. Web Scraper (`CigarDayScraperService.cs`)

A web scraper built with HtmlAgilityPack to extract cigar data from cigarday.ru. The scraper:

- Navigates through the cigar catalog pages
- Extracts brand information (name, country, description)
- Extracts product details for each brand (name, vitola, length, ring gauge, strength, wrapper, binder, filler, description, price)

Due to potential website access issues, this scraper is currently disabled in the main program, but the code is preserved for future use when the website becomes accessible.

### 2. Mock Data Generator (`MockCigarDataService.cs`)

A service that generates realistic mock cigar data based on well-known cigar brands and products. The mock data includes:

- Multiple brands (Cohiba, Arturo Fuente, Padron, Camacho)
- Various products for each brand
- Realistic cigar specifications and descriptions

### 3. Program Entry Point (`Program.cs`)

The main program that:
1. Generates the cigar data (currently using mock data)
2. Serializes the data to JSON
3. Saves the data to the output directory
4. Copies the JSON file to the CigarHelper.Data project for use in the main application

## Usage

To run the scraper/generator:

```bash
dotnet run
```

This will generate a `scraped_cigars.json` file in the output directory and copy it to the CigarHelper.Data project.

## Data Structure

The extracted/generated data follows this structure:

```json
{
  "Brands": [
    {
      "Name": "Brand Name",
      "Country": "Country of Origin",
      "Description": "Brand description",
      "Products": [
        {
          "Name": "Product Name",
          "Vitola": "Cigar Shape/Format",
          "Length": 5.0,
          "RingGauge": 50,
          "Strength": "Medium",
          "Wrapper": "Wrapper leaf type",
          "Binder": "Binder leaf type",
          "Filler": "Filler leaf type",
          "Description": "Product description",
          "ImageUrl": "URL to cigar image",
          "Price": 10.99
        }
      ]
    }
  ]
}
```

## Future Improvements

- Implement error retry mechanisms for the web scraper
- Add pagination support for brand catalog pages
- Enhance mock data with more brands and product variations
- Add support for cigar ratings and reviews 