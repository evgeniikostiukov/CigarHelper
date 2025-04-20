const axios = require('axios');
const cheerio = require('cheerio');
const puppeteer = require('puppeteer');
const fs = require('fs');
const path = require('path');

// URLs
const BASE_URL = 'https://cigarday.ru';
const CIGARS_URL = `${BASE_URL}/sigary/`;

// Main object to store all cigar data
const cigarData = {
  brands: []
};

/**
 * Main function to start the scraping process
 */
async function scrapeCigarData() {
  console.log('Starting cigar data scraping...');
  
  try {
    // Launch browser for JavaScript rendered content
    const browser = await puppeteer.launch({ headless: false });
    const page = await browser.newPage();
    
    // Set user agent to avoid being blocked
    await page.setUserAgent('Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36');
    
    // Navigate to the cigars page
    console.log(`Navigating to ${CIGARS_URL}...`);
    await page.goto(CIGARS_URL, { waitUntil: 'networkidle2' });
    
    // Accept age verification if present
    try {
      const ageButton = await page.$('input.modal__button button button--default button--lg button--primary');
      if (ageButton) {
        await ageButton.click();
        await page.waitForNavigation({ waitUntil: 'networkidle2' });
      }
    } catch (error) {
      console.log('No age verification popup or unable to click it');
    }
    
    // Get the page content
    const content = await page.content();
    const $ = cheerio.load(content);
    
    // Extract brand links from the sidebar
    console.log('Extracting brand information...');
    
    // Find all brand links in the dropdown menu
    const brandLinks = [];
    $('li.catalog__sidebar-menu-item a').each((index, element) => {
      const link = $(element).attr('href');
      const name = $(element).text().trim();
      
      if (link && link.includes('/sigary/') && !link.includes('?') && name) {
        brandLinks.push({
          name,
          url: link.startsWith('http') ? link : `${BASE_URL}${link}`
        });
      }
    });
    
    console.log(`Found ${brandLinks.length} potential brand links`);
    
    // Process the first 5 brands for testing (remove limit for production)
    const brandsToProcess = brandLinks.slice(0, 5);
    
    // Process each brand
    for (const brand of brandsToProcess) {
      console.log(`Processing brand: ${brand.name}`);
      const brandData = await scrapeBrandPage(browser, brand.url, brand.name);
      if (brandData && brandData.products.length > 0) {
        cigarData.brands.push(brandData);
      }
    }
    
    // Close browser
    await browser.close();
    
    // Save data to file
    saveDataToJson(cigarData);
    
    console.log(`Scraping completed. Scraped ${cigarData.brands.length} brands with a total of ${cigarData.brands.reduce((sum, brand) => sum + brand.products.length, 0)} products.`);
    
  } catch (error) {
    console.error('Error during scraping:', error);
  }
}

/**
 * Scrape a specific brand page
 */
async function scrapeBrandPage(browser, brandUrl, brandName) {
  try {
    const page = await browser.newPage();
    await page.setUserAgent('Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36');
    
    console.log(`Navigating to brand page: ${brandUrl}`);
    await page.goto(brandUrl, { waitUntil: 'networkidle2' });
    
    const content = await page.content();
    const $ = cheerio.load(content);
    
    // Extract brand information
    const brandData = {
      name: brandName,
      country: '',
      description: '',
      products: []
    };
    
    // Try to find the brand description if available
    const description = $('.catalog-section__description').text().trim();
    if (description) {
      brandData.description = description;
    }
    
    // Find all product items on the brand page
    const productItems = $('.catalog-section__item');
    console.log(`Found ${productItems.length} products for brand ${brandName}`);
    
    // Process each product
    for (let i = 0; i < productItems.length; i++) {
      const item = productItems[i];
      
      const productLink = $(item).find('.catalog-section__item-title').attr('href');
      if (!productLink) continue;
      
      const productUrl = productLink.startsWith('http') ? productLink : `${BASE_URL}${productLink}`;
      const productData = await scrapeProductPage(browser, productUrl);
      
      if (productData) {
        brandData.products.push(productData);
      }
      
      // Add a small delay to avoid overloading the server
      await new Promise(resolve => setTimeout(resolve, 1000));
    }
    
    await page.close();
    return brandData;
    
  } catch (error) {
    console.error(`Error scraping brand ${brandName}:`, error);
    return null;
  }
}

/**
 * Scrape a specific product page
 */
async function scrapeProductPage(browser, productUrl) {
  try {
    const page = await browser.newPage();
    await page.setUserAgent('Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36');
    
    console.log(`Navigating to product page: ${productUrl}`);
    await page.goto(productUrl, { waitUntil: 'networkidle2' });
    
    const content = await page.content();
    const $ = cheerio.load(content);
    
    // Extract product information
    const productData = {
      name: $('.product__title').text().trim(),
      vitola: '',
      length: null,
      ringGauge: null,
      strength: '',
      wrapper: '',
      binder: '',
      filler: '',
      description: $('.product__description').text().trim(),
      imageUrl: '',
      price: null
    };
    
    // Extract product image
    const imageElement = $('.product__photo img');
    if (imageElement.length > 0) {
      const imageSrc = $(imageElement).attr('src');
      if (imageSrc) {
        productData.imageUrl = imageSrc.startsWith('http') ? imageSrc : `${BASE_URL}${imageSrc}`;
      }
    }
    
    // Extract price
    const priceText = $('.product__price').text().trim();
    if (priceText) {
      // Extract numbers from the price text (e.g., "1 480 ₽" -> 1480)
      const priceMatch = priceText.match(/(\d+[\s\d]*,\d+|\d+[\s\d]*)/);
      if (priceMatch) {
        const cleanPrice = priceMatch[1].replace(/\s/g, '').replace(',', '.');
        productData.price = parseFloat(cleanPrice);
      }
    }
    
    // Extract product details
    $('.product__info-row').each((index, row) => {
      const title = $(row).find('.product__info-title').text().trim().toLowerCase();
      const value = $(row).find('.product__info-value').text().trim();
      
      if (!title || !value) return;
      
      if (title.includes('формат') || title.includes('витола')) {
        productData.vitola = value;
      } else if (title.includes('крепость')) {
        productData.strength = value;
      } else if (title.includes('покровный лист')) {
        productData.wrapper = value;
      } else if (title.includes('связующий лист')) {
        productData.binder = value;
      } else if (title.includes('наполнитель')) {
        productData.filler = value;
      } else if (title.includes('длина')) {
        // Extract the length (e.g., "123 мм" -> 123)
        const lengthMatch = value.match(/(\d+)/);
        if (lengthMatch) {
          productData.length = parseInt(lengthMatch[1]);
        }
      } else if (title.includes('диаметр') || title.includes('ринг') || title.includes('ring')) {
        // Extract the ring gauge (e.g., "52 RG" -> 52)
        const ringMatch = value.match(/(\d+)/);
        if (ringMatch) {
          productData.ringGauge = parseInt(ringMatch[1]);
        }
      } else if (title.includes('страна')) {
        // This is typically a brand-level attribute, but we'll extract it here
        // to potentially use it to update the brand data
        return value;
      }
    });
    
    await page.close();
    return productData;
    
  } catch (error) {
    console.error(`Error scraping product ${productUrl}:`, error);
    return null;
  }
}

/**
 * Save the collected data to a JSON file
 */
function saveDataToJson(data) {
  const filePath = path.join(__dirname, 'cigars-data.json');
  const jsonData = JSON.stringify(data, null, 2);
  
  fs.writeFileSync(filePath, jsonData, 'utf8');
  console.log(`Data saved to ${filePath}`);
}

// Run the scraper
scrapeCigarData(); 