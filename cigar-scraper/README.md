# Cigar Scraper

A Node.js application for scraping cigar data from [cigarday.ru](https://cigarday.ru/sigary/) and saving it to a JSON file.

## Features

- Extracts cigar brand information
- Extracts detailed product information including:
  - Name
  - Vitola (shape/format)
  - Length and ring gauge
  - Strength
  - Wrapper, binder, and filler tobacco
  - Description
  - Price
  - Product image URL
- Saves all data to a structured JSON file
- Includes a mock data generator for testing

## Prerequisites

- Node.js (v14 or higher recommended)
- npm (comes with Node.js)

## Installation

1. Clone this repository or download the source code
2. Navigate to the project directory
3. Install dependencies:

```bash
npm install
```

## Usage

### Live Scraping

To run the scraper and fetch data from cigarday.ru:

```bash
npm run scrape
```

This will:
1. Launch a headless browser
2. Navigate to the cigar catalog
3. Extract brand and product data
4. Save the results to `cigars-data.json`

### Generate Mock Data

If you just need sample data without making web requests:

```bash
npm run mock
```

This will generate a `mock-cigars-data.json` file with realistic sample data.

## Data Structure

The generated JSON files follow this structure:

```json
{
  "brands": [
    {
      "name": "Brand Name",
      "country": "Country of Origin",
      "description": "Brand description",
      "products": [
        {
          "name": "Product Name",
          "vitola": "Cigar Shape/Format",
          "length": 156,
          "ringGauge": 52,
          "strength": "Medium-Full",
          "wrapper": "Wrapper leaf type",
          "binder": "Binder leaf type",
          "filler": "Filler leaf type",
          "description": "Product description",
          "imageUrl": "URL to cigar image",
          "price": 2500
        }
      ]
    }
  ]
}
```

## Notes

- The scraper includes a rate limit to avoid overloading the target server.
- By default, the live scraper only processes the first 5 brands for testing purposes. Modify the `brandLinks.slice(0, 5)` line in `index.js` to process more or all brands.
- The age verification popup is automatically accepted if present.

## Customization

You can modify the scraper behavior by editing the following files:

- `index.js`: Main scraper logic
- `mockData.js`: Mock data generation

## Legal Considerations

This scraper is intended for educational purposes only. Always respect the target website's robots.txt file and terms of service. 