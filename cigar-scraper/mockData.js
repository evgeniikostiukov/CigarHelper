const fs = require('fs');
const path = require('path');

/**
 * Generate mock cigar data for testing
 */
function generateMockData() {
  const mockData = {
    brands: [
      {
        name: "Cohiba",
        country: "Cuba",
        description: "Cohiba is the flagship brand of Habanos S.A. and Cuba. Created in 1966 for Fidel Castro himself and was made at the El Laguito factory in Havana.",
        products: [
          {
            name: "Cohiba Robusto",
            vitola: "Robusto",
            length: 124,
            ringGauge: 50,
            strength: "Medium to Full",
            wrapper: "Cuban",
            binder: "Cuban",
            filler: "Cuban",
            description: "The Cohiba Robusto is a classic Cuban cigar, offering a rich, complex flavor profile with notes of wood, coffee, and subtle spice.",
            imageUrl: "https://cigarday.ru/images/cigars/cohiba-robusto.jpg",
            price: 3500
          },
          {
            name: "Cohiba Esplendidos",
            vitola: "Churchill",
            length: 178,
            ringGauge: 47,
            strength: "Medium to Full",
            wrapper: "Cuban",
            binder: "Cuban",
            filler: "Cuban",
            description: "The flagship of the Cohiba line, the Esplendidos is a classic Churchill-sized cigar with a perfect balance of flavors.",
            imageUrl: "https://cigarday.ru/images/cigars/cohiba-esplendidos.jpg",
            price: 4500
          }
        ]
      },
      {
        name: "Romeo y Julieta",
        country: "Cuba",
        description: "Romeo y Julieta is one of the most recognized cigar brands in the world, known for their balanced flavor and consistent construction.",
        products: [
          {
            name: "Romeo y Julieta Churchill",
            vitola: "Churchill",
            length: 178,
            ringGauge: 47,
            strength: "Medium",
            wrapper: "Cuban",
            binder: "Cuban",
            filler: "Cuban",
            description: "The Churchill is Romeo y Julieta's most famous vitola, offering a smooth, balanced smoke with notes of cedar, coffee and light spice.",
            imageUrl: "https://cigarday.ru/images/cigars/romeo-churchill.jpg",
            price: 2500
          },
          {
            name: "Romeo y Julieta Mille Fleurs",
            vitola: "Petit Corona",
            length: 129,
            ringGauge: 42,
            strength: "Medium",
            wrapper: "Cuban",
            binder: "Cuban",
            filler: "Cuban",
            description: "A smaller format that delivers the classic Romeo y Julieta flavor profile in a shorter smoking time.",
            imageUrl: "https://cigarday.ru/images/cigars/romeo-mille-fleurs.jpg",
            price: 1480
          }
        ]
      },
      {
        name: "Montecristo",
        country: "Cuba",
        description: "Montecristo is the most popular and arguably the most recognized cigar brand in the world, offering consistent quality and flavor.",
        products: [
          {
            name: "Montecristo No. 2",
            vitola: "Torpedo",
            length: 156,
            ringGauge: 52,
            strength: "Medium-Full",
            wrapper: "Cuban",
            binder: "Cuban",
            filler: "Cuban",
            description: "Perhaps the most famous torpedo in the world, delivering a rich blend of cocoa, coffee, and cedar with a creamy texture.",
            imageUrl: "https://cigarday.ru/images/cigars/montecristo-no2.jpg",
            price: 2900
          },
          {
            name: "Montecristo No. 4",
            vitola: "Petit Corona",
            length: 129,
            ringGauge: 42,
            strength: "Medium",
            wrapper: "Cuban",
            binder: "Cuban",
            filler: "Cuban",
            description: "The most popular Montecristo vitola, offering the brand's characteristic tangy flavor in a convenient size.",
            imageUrl: "https://cigarday.ru/images/cigars/montecristo-no4.jpg",
            price: 1800
          }
        ]
      }
    ]
  };
  
  // Save the mock data to a JSON file
  const filePath = path.join(__dirname, 'mock-cigars-data.json');
  const jsonData = JSON.stringify(mockData, null, 2);
  
  fs.writeFileSync(filePath, jsonData, 'utf8');
  console.log(`Mock data saved to ${filePath}`);
  
  return mockData;
}

// Run the mock data generator if this file is executed directly
if (require.main === module) {
  generateMockData();
}

module.exports = { generateMockData }; 