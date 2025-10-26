const fs = require('fs');
const csv = require('csv-parser');

const brands = new Set();

// Читаем CSV файл
fs.createReadStream('CigarHelper.Import/cigarday.csv')
  .pipe(csv())
  .on('data', (row) => {
    // Извлекаем бренд из URL
    const url = row['card__link href'];
    if (url) {
      // Извлекаем бренд из URL вида: https://cigarday.ru/sigary/BRAND_NAME/...
      const match = url.match(/\/sigary\/([^/]+)\//);
      if (match) {
        const brand = match[1];
        // Преобразуем slug в читаемое название бренда
        const brandName = brand
          .split('-')
          .map(word => word.charAt(0).toUpperCase() + word.slice(1))
          .join(' ');
        brands.add(brandName);
      }
    }
  })
  .on('end', () => {
    console.log('Список уникальных брендов:');
    console.log('========================');
    const sortedBrands = Array.from(brands).sort();
    sortedBrands.forEach((brand, index) => {
      console.log(`${index + 1}. ${brand}`);
    });
    console.log(`\nВсего уникальных брендов: ${brands.size}`);
  }); 