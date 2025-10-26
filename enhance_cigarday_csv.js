const fs = require('fs');
const csv = require('csv-parser');

// Данные о брендах с информацией о странах и описаниях
const brandInfo = {
    // Кубинские бренды
    "Cohiba": {
        country: "Cuba",
        description: "Премиальный кубинский бренд сигар, созданный в 1966 году. Известен своим исключительным качеством и сложными вкусами."
    },
    "Montecristo": {
        country: "Cuba",
        description: "Один из самых известных кубинских брендов сигар, основанный в 1935 году. Назван в честь романа Александра Дюма."
    },
    "Romeo Y Julieta": {
        country: "Cuba",
        description: "Классический кубинский бренд, основанный в 1875 году. Известен своими мягкими и сбалансированными сигарами."
    },
    "Partagas": {
        country: "Cuba",
        description: "Кубинский бренд, основанный в 1845 году. Известен своими крепкими и ароматными сигарами."
    },
    "Trinidad": {
        country: "Cuba",
        description: "Элитный кубинский бренд, созданный в 1969 году. Производит ограниченные серии высококачественных сигар."
    },
    "Hoyo De Monterrey": {
        country: "Cuba",
        description: "Кубинский бренд, основанный в 1865 году. Известен своими мягкими и элегантными сигарами."
    },
    "H.Upmann": {
        country: "Cuba",
        description: "Кубинский бренд, основанный в 1844 году. Известен своими сложными и изысканными вкусами."
    },
    "Bolivar": {
        country: "Cuba",
        description: "Кубинский бренд, основанный в 1902 году. Известен своими крепкими и ароматными сигарами."
    },
    "Diplomaticos": {
        country: "Cuba",
        description: "Кубинский бренд, созданный в 1966 году. Производит сигары среднего размера с классическими кубинскими вкусами."
    },
    "Fonseca": {
        country: "Cuba",
        description: "Кубинский бренд, основанный в 1906 году. Известен своими мягкими и ароматными сигарами."
    },
    "Quintero": {
        country: "Cuba",
        description: "Кубинский бренд, основанный в 1924 году. Производит доступные сигары с классическими кубинскими вкусами."
    },
    "Quai d'Orsay": {
        country: "Cuba",
        description: "Кубинский бренд, созданный в 1973 году специально для французского рынка. Известен своими элегантными сигарами."
    },
    "Ramon Allones": {
        country: "Cuba",
        description: "Кубинский бренд, основанный в 1837 году. Один из старейших брендов с богатой историей."
    },
    "Rey Del Mundo": {
        country: "Cuba",
        description: "Кубинский бренд, основанный в 1848 году. Известен своими традиционными методами производства."
    },
    "Sancho Panza": {
        country: "Cuba",
        description: "Кубинский бренд, основанный в 1848 году. Назван в честь персонажа из романа Сервантеса."
    },
    "Vegas Robaina": {
        country: "Cuba",
        description: "Кубинский бренд, созданный в 1997 году. Назван в честь известного производителя табака."
    },
    "Vegueros": {
        country: "Cuba",
        description: "Кубинский бренд, созданный в 1997 году. Производит доступные сигары для местного рынка."
    },
    "Juan Lopez": {
        country: "Cuba",
        description: "Кубинский бренд, основанный в 1876 году. Известен своими традиционными кубинскими вкусами."
    },
    "Por Larranaga": {
        country: "Cuba",
        description: "Кубинский бренд, основанный в 1834 году. Один из старейших брендов с богатой историей."
    },
    "La Gloria Cubana": {
        country: "Cuba",
        description: "Кубинский бренд, основанный в 1885 году. Известен своими классическими кубинскими сигарами."
    },
    "La Flor De Cano": {
        country: "Cuba",
        description: "Кубинский бренд, основанный в 1884 году. Производит традиционные кубинские сигары."
    },
    "La Unica": {
        country: "Cuba",
        description: "Кубинский бренд, известный своими доступными сигарами с классическими вкусами."
    },
    "La Instructora": {
        country: "Cuba",
        description: "Кубинский бренд, производящий сигары для местного рынка."
    },
    "La Ley": {
        country: "Cuba",
        description: "Кубинский бренд с традиционными методами производства."
    },
    "La Estrella": {
        country: "Cuba",
        description: "Кубинский бренд, производящий классические сигары."
    },
    "La Galera": {
        country: "Cuba",
        description: "Кубинский бренд с богатой историей производства сигар."
    },
    "La Aurora": {
        country: "Dominican Republic",
        description: "Доминиканский бренд, основанный в 1903 году. Старейший производитель сигар в Доминиканской Республике."
    },
    "La Aroma Del Caribe": {
        country: "Dominican Republic",
        description: "Доминиканский бренд, известный своими ароматными сигарами с карибскими вкусами."
    },
    "La Flor Dominicana": {
        country: "Dominican Republic",
        description: "Доминиканский бренд, основанный в 1994 году. Известен своими крепкими и ароматными сигарами."
    },
    "S.Cristobal": {
        country: "Cuba",
        description: "Кубинский бренд, созданный в 1999 году. Назван в честь покровителя Гаваны."
    },
    "Siglo de Oro": {
        country: "Cuba",
        description: "Кубинский бренд, производящий премиальные сигары с золотым веком кубинских сигар."
    },
    "Saint Luis Rey": {
        country: "Cuba",
        description: "Кубинский бренд, основанный в 1940-х годах. Назван в честь романа Сент-Экзюпери."
    },
    "Samana": {
        country: "Dominican Republic",
        description: "Доминиканский бренд, производящий сигары с табаком из региона Самана."
    },
    "Santa Damiana": {
        country: "Dominican Republic",
        description: "Доминиканский бренд, известный своими традиционными методами производства."
    },
    "Sicario": {
        country: "Nicaragua",
        description: "Никарагуанский бренд, производящий крепкие и ароматные сигары."
    },
    "Toreo": {
        country: "Dominican Republic",
        description: "Доминиканский бренд, вдохновленный испанской корридой."
    },
    "Torres": {
        country: "Dominican Republic",
        description: "Доминиканский бренд с традиционными методами производства."
    },
    "Toscano": {
        country: "Italy",
        description: "Итальянский бренд, основанный в 1818 году. Производит традиционные итальянские сигары."
    },
    "Total Flame": {
        country: "Dominican Republic",
        description: "Доминиканский бренд с яркими и интенсивными вкусами."
    },
    "Vegafina": {
        country: "Dominican Republic",
        description: "Доминиканский бренд, производящий доступные сигары с классическими вкусами."
    },
    "Villa Vieja": {
        country: "Honduras",
        description: "Гондурасский бренд, производящий традиционные сигары."
    },
    "Villa Zamorano": {
        country: "Honduras",
        description: "Гондурасский бренд от Maya Selva, известный своими качественными сигарами."
    },
    "Vintage": {
        country: "Dominican Republic",
        description: "Доминиканский бренд, производящий сигары с выдержанным табаком."
    },
    "XO": {
        country: "Dominican Republic",
        description: "Доминиканский бренд, производящий премиальные сигары с эксклюзивными вкусами."
    },
    "Yoruba": {
        country: "Dominican Republic",
        description: "Доминиканский бренд, вдохновленный африканской культурой."
    },
    "Zapata": {
        country: "Dominican Republic",
        description: "Доминиканский бренд, названный в честь мексиканского революционера."
    },
    "Zino": {
        country: "Dominican Republic",
        description: "Доминиканский бренд от Davidoff, производящий элегантные сигары."
    },
    "Zino Platinum": {
        country: "Dominican Republic",
        description: "Премиальная линия бренда Zino от Davidoff с эксклюзивными сигарами."
    },
    "AJ Fernandez": {
        country: "Nicaragua",
        description: "Никарагуанский бренд, основанный A.J. Fernandez. Известен своими крепкими и ароматными сигарами."
    },
    "Alec Bradley": {
        country: "Honduras",
        description: "Американский бренд, производящий сигары в Гондурасе. Известен своими инновационными смесями."
    },
    "Alfambra": {
        country: "Dominican Republic",
        description: "Доминиканский бренд с традиционными методами производства."
    },
    "Ararat": {
        country: "Armenia",
        description: "Армянский бренд, производящий сигары с местными традициями."
    },
    "Aristocrat": {
        country: "Dominican Republic",
        description: "Доминиканский бренд, производящий элегантные сигары."
    },
    "Ashton": {
        country: "Dominican Republic",
        description: "Американский бренд, производящий сигары в Доминиканской Республике. Известен своим качеством."
    },
    "Asylum": {
        country: "Nicaragua",
        description: "Никарагуанский бренд, производящий крепкие и интенсивные сигары."
    },
    "Atabey": {
        country: "Dominican Republic",
        description: "Доминиканский бренд, производящий премиальные сигары с уникальными вкусами."
    },
    "AVO": {
        country: "Dominican Republic",
        description: "Доминиканский бренд от Davidoff, основанный Avo Uvezian. Известен своими элегантными сигарами."
    },
    "Balmoral": {
        country: "Netherlands",
        description: "Голландский бренд, производящий сигары с европейскими традициями."
    },
    "Bentley": {
        country: "Dominican Republic",
        description: "Доминиканский бренд, вдохновленный роскошными автомобилями."
    },
    "Bossner": {
        country: "Germany",
        description: "Немецкий бренд, производящий сигары с европейскими традициями."
    },
    "Brick House": {
        country: "Nicaragua",
        description: "Никарагуанский бренд, производящий доступные сигары с классическими вкусами."
    },
    "Buena Vista": {
        country: "Dominican Republic",
        description: "Доминиканский бренд, производящий сигары с карибскими вкусами."
    },
    "Cain": {
        country: "Nicaragua",
        description: "Никарагуанский бренд, производящий крепкие сигары с интенсивными вкусами."
    },
    "Caldwell": {
        country: "Dominican Republic",
        description: "Доминиканский бренд, основанный Robert Caldwell. Известен своими инновационными смесями."
    },
    "Camacho": {
        country: "Honduras",
        description: "Гондурасский бренд, основанный в 1961 году. Известен своими крепкими сигарами."
    },
    "CAO": {
        country: "Nicaragua",
        description: "Американский бренд, производящий сигары в Никарагуа. Известен своими инновационными смесями."
    },
    "Capadura": {
        country: "Dominican Republic",
        description: "Доминиканский бренд с традиционными методами производства."
    },
    "Carlos Andre": {
        country: "Dominican Republic",
        description: "Доминиканский бренд, производящий сигары с классическими вкусами."
    },
    "Casa 1910": {
        country: "Mexico",
        description: "Мексиканский бренд, основанный в 1910 году. Производит сигары с мексиканскими традициями."
    },
    "Casa Turrent": {
        country: "Mexico",
        description: "Мексиканский бренд, основанный в 1880 году. Известен своими традиционными методами производства."
    },
    "Cherokee": {
        country: "Dominican Republic",
        description: "Доминиканский бренд, вдохновленный индейской культурой."
    },
    "CLE": {
        country: "Nicaragua",
        description: "Никарагуанский бренд, основанный Christian Eiroa. Известен своими качественными сигарами."
    },
    "Cuaba": {
        country: "Cuba",
        description: "Кубинский бренд, созданный в 1996 году. Известен своими фигурированными сигарами."
    },
    "Cuba Aliados": {
        country: "Nicaragua",
        description: "Никарагуанский бренд, основанный в 1927 году. Производит сигары с кубинскими традициями."
    },
    "Cumpay": {
        country: "Dominican Republic",
        description: "Доминиканский бренд с традиционными методами производства."
    },
    "Cusano": {
        country: "Dominican Republic",
        description: "Доминиканский бренд, основанный Michael Chiusano. Известен своими качественными сигарами."
    },
    "Dardanelles": {
        country: "Turkey",
        description: "Турецкий бренд, производящий сигары с восточными традициями."
    },
    "Davidoff": {
        country: "Dominican Republic",
        description: "Швейцарский бренд, производящий сигары в Доминиканской Республике. Известен своим премиальным качеством."
    },
    "Davtian": {
        country: "Armenia",
        description: "Армянский бренд, производящий сигары с местными традициями."
    },
    "Diesel": {
        country: "Nicaragua",
        description: "Никарагуанский бренд, производящий крепкие сигары с интенсивными вкусами."
    },
    "Don Tomas": {
        country: "Dominican Republic",
        description: "Доминиканский бренд, основанный в 1975 году. Производит доступные сигары с классическими вкусами."
    },
    "Don Diego": {
        country: "Dominican Republic",
        description: "Доминиканский бренд, производящий элегантные сигары с классическими вкусами."
    },
    "Eiroa": {
        country: "Honduras",
        description: "Гондурасский бренд, основанный Julio Eiroa. Известен своими традиционными методами производства."
    },
    "Ernesto Perez": {
        country: "Dominican Republic",
        description: "Доминиканский бренд, названный в честь известного производителя сигар."
    },
    "Flor De Copan": {
        country: "Honduras",
        description: "Гондурасский бренд, производящий сигары с табаком из региона Копан."
    },
    "Flor De Selva": {
        country: "Honduras",
        description: "Гондурасский бренд, производящий сигары с табаком из тропических лесов."
    },
    "Furia": {
        country: "Dominican Republic",
        description: "Доминиканский бренд, производящий крепкие сигары с интенсивными вкусами."
    },
    "God Of Fire": {
        country: "Dominican Republic",
        description: "Доминиканский бренд, производящий премиальные сигары с эксклюзивными вкусами."
    },
    "Great Wall": {
        country: "China",
        description: "Китайский бренд, производящий сигары с восточными традициями."
    },
    "Griffins": {
        country: "Dominican Republic",
        description: "Доминиканский бренд, производящий сигары с классическими вкусами."
    },
    "Guantanamera": {
        country: "Cuba",
        description: "Кубинский бренд, названный в честь популярной кубинской песни. Производит доступные сигары."
    },
    "Gurkha": {
        country: "Dominican Republic",
        description: "Американский бренд, производящий сигары в Доминиканской Республике. Известен своими премиальными сигарами."
    },
    "Horacio": {
        country: "Dominican Republic",
        description: "Доминиканский бренд, названный в честь известного производителя сигар."
    },
    "Humo Jaguar": {
        country: "Nicaragua",
        description: "Никарагуанский бренд, вдохновленный майянской культурой."
    },
    "Jose L.Piedra": {
        country: "Cuba",
        description: "Кубинский бренд, производящий доступные сигары для местного рынка."
    },
    "Leon Jimenes": {
        country: "Dominican Republic",
        description: "Доминиканский бренд, основанный в 1903 году. Один из старейших производителей в Доминиканской Республике."
    },
    "Luis Martinez": {
        country: "Dominican Republic",
        description: "Доминиканский бренд, названный в честь известного производителя сигар."
    },
    "Macanudo": {
        country: "Dominican Republic",
        description: "Американский бренд, производящий сигары в Доминиканской Республике. Известен своими мягкими сигарами."
    },
    "Miro": {
        country: "Dominican Republic",
        description: "Доминиканский бренд, производящий сигары с классическими вкусами."
    },
    "Montosa": {
        country: "Dominican Republic",
        description: "Доминиканский бренд с традиционными методами производства."
    },
    "My Father": {
        country: "Nicaragua",
        description: "Никарагуанский бренд, основанный Jose Pepin Garcia. Известен своими крепкими и ароматными сигарами."
    },
    "Nicarao": {
        country: "Nicaragua",
        description: "Никарагуанский бренд, названный в честь древнего народа Никарао."
    },
    "NUB": {
        country: "Nicaragua",
        description: "Никарагуанский бренд, производящий короткие сигары с интенсивными вкусами."
    },
    "Oliva": {
        country: "Nicaragua",
        description: "Никарагуанский бренд, основанный в 1886 году. Известен своими качественными сигарами."
    },
    "ORISHAS": {
        country: "Cuba",
        description: "Кубинский бренд, вдохновленный афро-кубинской религией. Производит сигары с уникальными вкусами."
    },
    "Other": {
        country: "Unknown",
        description: "Бренд с неизвестным происхождением."
    },
    "Padron": {
        country: "Nicaragua",
        description: "Никарагуанский бренд, основанный Jose Padron. Известен своими премиальными сигарами."
    },
    "Parcero": {
        country: "Dominican Republic",
        description: "Доминиканский бренд с традиционными методами производства."
    },
    "PDR": {
        country: "Dominican Republic",
        description: "Доминиканский бренд, производящий сигары с классическими вкусами."
    },
    "Pelo de Oro": {
        country: "Cuba",
        description: "Кубинский бренд, производящий сигары с золотистым табаком."
    },
    "Perdomo": {
        country: "Nicaragua",
        description: "Никарагуанский бренд, основанный Nick Perdomo. Известен своими качественными сигарами."
    },
    "Perla Del Mar": {
        country: "Nicaragua",
        description: "Никарагуанский бренд, производящий сигары с морскими вкусами."
    },
    "Plasensia": {
        country: "Nicaragua",
        description: "Никарагуанский бренд, основанный Nestor Plasencia. Известен своими качественными сигарами."
    },
    "Pogarskaya Fabrika": {
        country: "Russia",
        description: "Российский бренд, производящий сигары с местными традициями."
    },
    "Rocky Patel": {
        country: "Honduras",
        description: "Американский бренд, производящий сигары в Гондурасе. Известен своими качественными сигарами."
    },
    "San Lotano": {
        country: "Nicaragua",
        description: "Никарагуанский бренд, основанный A.J. Fernandez. Известен своими крепкими сигарами."
    },
    "Stanislaw": {
        country: "Poland",
        description: "Польский бренд, производящий сигары с европейскими традициями."
    },
    "Te Amo": {
        country: "Mexico",
        description: "Мексиканский бренд, основанный в 1960-х годах. Производит сигары с мексиканскими традициями."
    },
    "Toreo": {
        country: "Dominican Republic",
        description: "Доминиканский бренд, вдохновленный испанской корридой."
    },
    "Torres": {
        country: "Dominican Republic",
        description: "Доминиканский бренд с традиционными методами производства."
    },
    "Vintage": {
        country: "Dominican Republic",
        description: "Доминиканский бренд, производящий сигары с выдержанным табаком."
    },
    "Евгений Онегин": {
        country: "Russia",
        description: "Российский бренд, названный в честь романа Пушкина. Производит сигары с русскими традициями."
    }
};

// Функция для извлечения бренда из названия сигары (аналогично C# коду)
function extractBrandName(cigarName) {
    const parts = cigarName.split(' ');
    if (parts.length >= 2) {
        // Кубинские бренды (составные названия)
        if (parts[0].toLowerCase() === "hoyo" && parts[1].toLowerCase() === "de")
            return "Hoyo De Monterrey";
        if (parts[0].toLowerCase() === "romeo" && parts[1].toLowerCase() === "y")
            return "Romeo Y Julieta";
        if (parts[0].toLowerCase() === "h." && parts[1].toLowerCase() === "upmann")
            return "H.Upmann";
        if (parts[0].toLowerCase() === "quai" && parts[1].toLowerCase() === "d'orsay")
            return "Quai d'Orsay";
        if (parts[0].toLowerCase() === "s." && parts[1].toLowerCase() === "cristobal")
            return "S.Cristobal";
        if (parts[0].toLowerCase() === "zino" && parts[1].toLowerCase() === "platinum")
            return "Zino Platinum";
        if (parts[0].toLowerCase() === "casa" && parts[1].toLowerCase() === "turrent")
            return "Casa Turrent";
        if (parts[0].toLowerCase() === "cuesta" && parts[1].toLowerCase() === "rey")
            return "Cuesta Rey";
        if (parts[0].toLowerCase() === "por" && parts[1].toLowerCase() === "larranaga")
            return "Por Larranaga";
        if (parts[0].toLowerCase() === "ramon" && parts[1].toLowerCase() === "allones")
            return "Ramon Allones";
        if (parts[0].toLowerCase() === "rey" && parts[1].toLowerCase() === "del")
            return "Rey Del Mundo";
        if (parts[0].toLowerCase() === "siglo" && parts[1].toLowerCase() === "de")
            return "Siglo de Oro";
        if (parts[0].toLowerCase() === "vegas" && parts[1].toLowerCase() === "robaina")
            return "Vegas Robaina";
        if (parts[0].toLowerCase() === "juan" && parts[1].toLowerCase() === "lopez")
            return "Juan Lopez";
        if (parts[0].toLowerCase() === "aj" && parts[1].toLowerCase() === "fernandez")
            return "AJ Fernandez";
        if (parts[0].toLowerCase() === "evgenij" && parts[1].toLowerCase() === "onegin")
            return "Евгений Онегин";
        if (parts[0].toLowerCase() === "pelo" && parts[1].toLowerCase() === "de")
            return "Pelo de Oro";
        if (parts[0].toLowerCase() === "don" && parts[1].toLowerCase() === "tomas")
            return "Don Tomas";
        if (parts[0].toLowerCase() === "jose" && parts[1].toLowerCase() === "l.piedra")
            return "Jose L.Piedra";
        if (parts[0].toLowerCase() === "quintero")
            return "Quintero";
        if (parts[0].toLowerCase() === "orishas")
            return "ORISHAS";
        if (parts[0].toLowerCase() === "hidalgo")
            return "Hidalgo";
        if (parts[0].toLowerCase() === "punch")
            return "Punch";
        if (parts[0].toLowerCase() === "bolivar")
            return "Bolivar";
        if (parts[0].toLowerCase() === "diplomaticos")
            return "Diplomaticos";
        if (parts[0].toLowerCase() === "paradiso")
            return "Paradiso";
        if (parts[0].toLowerCase() === "stanislaw")
            return "Stanislaw";
        if (parts[0].toLowerCase() === "avo")
            return "AVO";
        
        // Обработка La Flor
        if (parts[0].toLowerCase() === "la" && parts[1].toLowerCase() === "flor" && parts.length >= 3) {
            if (parts[2].toLowerCase() === "de")
                return "La Flor De Cano";
            if (parts[2].toLowerCase() === "dominicana")
                return "La Flor Dominicana";
            return "La Flor De Cano";
        }
        
        // Остальные составные названия
        if (parts[0].toLowerCase() === "la" && parts[1].toLowerCase() === "aroma" && parts.length >= 4) {
            if (parts[2].toLowerCase() === "del" && parts[3].toLowerCase() === "caribe")
                return "La Aroma Del Caribe";
        }
        if (parts[0].toLowerCase() === "hiram" && parts[1].toLowerCase() === "solomon" && parts.length >= 3) {
            if (parts[2].toLowerCase() === "cigars")
                return "Hiram Solomon Cigars";
        }
        if (parts[0].toLowerCase() === "jm" && parts[1].toLowerCase() === "tobacco" && parts.length >= 3) {
            if (parts[2].toLowerCase() === "cigars")
                return "JM Tobacco Cigars";
        }
        if (parts[0].toLowerCase() === "tatuaje" && parts[1].toLowerCase() === "cigars")
            return "Tatuaje Cigars";
        if (parts[0].toLowerCase() === "total" && parts[1].toLowerCase() === "flame")
            return "Total Flame";
        if (parts[0].toLowerCase() === "villa" && parts[1].toLowerCase() === "vieja")
            return "Villa Vieja";
        if (parts[0].toLowerCase() === "villa" && parts[1].toLowerCase() === "zamorano")
            return "Villa Zamorano";
        if (parts[0].toLowerCase() === "arturo" && parts[1].toLowerCase() === "fuente")
            return "Arturo Fuente";
        if (parts[0].toLowerCase() === "diamond" && parts[1].toLowerCase() === "crown")
            return "Diamond Crown";
        if (parts[0].toLowerCase() === "dominican" && parts[1].toLowerCase() === "estates")
            return "Dominican Estates";
        if (parts[0].toLowerCase() === "don" && parts[1].toLowerCase() === "diego")
            return "Don Diego";
        if (parts[0].toLowerCase() === "drew" && parts[1].toLowerCase() === "estate")
            return "Drew Estate";
        if (parts[0].toLowerCase() === "el" && parts[1].toLowerCase() === "baton")
            return "El Baton";
        if (parts[0].toLowerCase() === "ernesto" && parts[1].toLowerCase() === "perez")
            return "Ernesto Perez";
        if (parts[0].toLowerCase() === "flor" && parts[1].toLowerCase() === "de")
            return "Flor De Copan";
        if (parts[0].toLowerCase() === "god" && parts[1].toLowerCase() === "of")
            return "God Of Fire";
        if (parts[0].toLowerCase() === "great" && parts[1].toLowerCase() === "wall")
            return "Great Wall";
        if (parts[0].toLowerCase() === "perla" && parts[1].toLowerCase() === "del")
            return "Perla Del Mar";
        if (parts[0].toLowerCase() === "saint" && parts[1].toLowerCase() === "luis")
            return "Saint Luis Rey";
        
        // Одиночные бренды
        const firstWord = parts[0].toLowerCase();
        const brandMap = {
            "cohiba": "Cohiba",
            "montecristo": "Montecristo",
            "partagas": "Partagas",
            "trinidad": "Trinidad",
            "fonseca": "Fonseca",
            "quintero": "Quintero",
            "orishas": "ORISHAS",
            "hidalgo": "Hidalgo",
            "punch": "Punch",
            "paradiso": "Paradiso",
            "vegueros": "Vegueros",
            "aj": "AJ Fernandez",
            "alec": "Alec Bradley",
            "alfambra": "Alfambra",
            "ararat": "Ararat",
            "aristocrat": "Aristocrat",
            "ashton": "Ashton",
            "asylum": "Asylum",
            "atabey": "Atabey",
            "avo": "AVO",
            "balmoral": "Balmoral",
            "bentley": "Bentley",
            "bossner": "Bossner",
            "brick": "Brick House",
            "buena": "Buena Vista",
            "cain": "Cain",
            "caldwell": "Caldwell",
            "camacho": "Camacho",
            "cao": "CAO",
            "capadura": "Capadura",
            "carlos": "Carlos Andre",
            "casa": "Casa 1910",
            "cherokee": "Cherokee",
            "cle": "CLE",
            "cuaba": "Cuaba",
            "cuba": "Cuba Aliados",
            "cumpay": "Cumpay",
            "cusano": "Cusano",
            "dardanelles": "Dardanelles",
            "davidoff": "Davidoff",
            "davtian": "Davtian",
            "diesel": "Diesel",
            "eiroa": "Eiroa",
            "evgenij": "Евгений Онегин",
            "furia": "Furia",
            "griffins": "Griffins",
            "guantanamera": "Guantanamera",
            "gurkha": "Gurkha",
            "horacio": "Horacio",
            "humo": "Humo Jaguar",
            "jose": "Jose L.Piedra",
            "juan": "Juan Lopez",
            "leon": "Leon Jimenes",
            "luis": "Luis Martinez",
            "macanudo": "Macanudo",
            "miro": "Miro",
            "montosa": "Montosa",
            "nicarao": "Nicarao",
            "nub": "NUB",
            "oliva": "Oliva",
            "other": "Other",
            "padron": "Padron",
            "parcero": "Parcero",
            "pdr": "PDR",
            "pelo": "Pelo de Oro",
            "perdomo": "Perdomo",
            "plasensia": "Plasensia",
            "pogarskaya": "Pogarskaya Fabrika",
            "por": "Por Larranaga",
            "ramon": "Ramon Allones",
            "rey": "Rey Del Mundo",
            "rocky": "Rocky Patel",
            "romeo": "Romeo Y Julieta",
            "s": "S.Cristobal",
            "samana": "Samana",
            "san": "San Lotano",
            "sancho": "Sancho Panza",
            "santa": "Santa Damiana",
            "sicario": "Sicario",
            "siglo": "Siglo de Oro",
            "stanislaw": "Stanislaw",
            "te": "Te Amo",
            "toreo": "Toreo",
            "torres": "Torres",
            "toscano": "Toscano",
            "vegas": "Vegas Robaina",
            "vegafina": "Vegafina",
            "vintage": "Vintage",
            "xo": "XO",
            "yoruba": "Yoruba",
            "zapata": "Zapata",
            "zino": "Zino"
        };
        
        return brandMap[firstWord] || parts[0];
    }
    
    return cigarName;
}

// Читаем CSV файл и дополняем его
const enhancedData = [];

fs.createReadStream('CigarHelper.Import/cigarday.csv')
  .pipe(csv())
  .on('data', (row) => {
    const cigarName = row['card__title'];
    const brandName = extractBrandName(cigarName);
    const brandData = brandInfo[brandName] || { country: "Unknown", description: "Информация о бренде недоступна" };
    
    enhancedData.push({
      ...row,
      'brand_name': brandName,
      'brand_country': brandData.country,
      'brand_description': brandData.description
    });
  })
  .on('end', () => {
    // Записываем обновленный CSV файл
    const csvHeader = 'card__link href,card__image src,card__size,card__title,card__button,brand_name,brand_country,brand_description\n';
    const csvContent = enhancedData.map(row => {
      return `"${row['card__link href']}","${row['card__image src']}","${row['card__size']}","${row['card__title']}","${row['card__button']}","${row.brand_name}","${row.brand_country}","${row.brand_description}"`;
    }).join('\n');
    
    fs.writeFileSync('CigarHelper.Import/cigarday_enhanced.csv', csvHeader + csvContent);
    
    console.log('CSV файл успешно дополнен!');
    console.log(`Обработано ${enhancedData.length} записей`);
    console.log('Новый файл: cigarday_enhanced.csv');
    
    // Статистика по странам
    const countryStats = {};
    enhancedData.forEach(row => {
      const country = row.brand_country;
      countryStats[country] = (countryStats[country] || 0) + 1;
    });
    
    console.log('\nСтатистика по странам:');
    Object.entries(countryStats).sort((a, b) => b[1] - a[1]).forEach(([country, count]) => {
      console.log(`${country}: ${count} сигар`);
    });
  }); 