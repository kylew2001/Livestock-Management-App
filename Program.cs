using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Odbc;
using System.Linq;
using System.Numerics;

namespace AppDev2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var app = new App();

            while (true)
            {
                Console.WriteLine("Choose an option:");
                Console.WriteLine("1. List all farm animals");
                Console.WriteLine("2. Query farm livestock");
                Console.WriteLine("3. Insert new farm animal");
                Console.WriteLine("4. Delete farm animal");
                Console.WriteLine("5. Print Metrics");
                Console.WriteLine("6. Edit Record");
                Console.WriteLine("7. Clear Console");
                Console.WriteLine("8. Exit");

                int choice;
                if (int.TryParse(Console.ReadLine(), out choice))
                {
                    switch (choice)
                    {
                        case 1:
                            app.PrintConsole();
                            break;
                        case 2:
                            app.QueryFarmLivestock();
                            break;
                        case 3:
                            app.ConsoleInsertAnimal();
                            break;
                        case 4:
                            app.ConsoleDeleteAnimalByID();
                            break;
                        case 5:
                            app.CalculateAndPrintMetrics();
                            break;
                        case 6:
                            app.EditLivestockRecord();
                            break;
                        case 7:
                            Console.Clear();
                            break;
                        case 8:
                            return;
                        default:
                            Console.WriteLine("Invalid choice. Please select a valid option.");
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter a valid option.");
                }
            }
        }
    }

    internal class FarmAnimal
    {

        public int ID { get; set; }
        public double Water { get; set; }
        public double Cost { get; set; }
        public double Weight { get; set; }
        public string Colour { get; set; }


        public FarmAnimal(int id, double water, double cost, double weight, string colour)
        {
            ID = id;
            Water = water;
            Cost = cost;
            Weight = weight;
            Colour = colour;
            
        }

        

        public static int GetNewAnimalID(List<FarmAnimal> farmAnimals)
        {
            if (farmAnimals.Count == 0)
            {
                return 1;
            }

            int maxExistingID = farmAnimals.Max(animal => animal.ID);

            return maxExistingID + 1;
        }

        public override string ToString()
        {
            return $"ID: {ID}, Water: {Water} KG, Cost: ${Cost:F2}, Weight: {Weight} KG, Colour: {Colour}";
        }
    }

    internal class App
    {
        public List<FarmAnimal> FarmAnimals { get; set; }
        public static OdbcConnection Conn { get; private set; }
        private int lastAssignedID = 0;

        public App()
        {
            Conn = Util.GetConn();
            FarmAnimals = new List<FarmAnimal>();
            ReadFarmAnimalData();
        }

        public double GetCommodityPrice(string commodityName)
        {
            double price = 0.0;

            using (var cmd = Conn.CreateCommand())
            {
                string sql = "SELECT [Price] FROM Commodity WHERE [Item] = ?";
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@p1", commodityName); 

                try
                {
                    object result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        price = Convert.ToDouble(result);
                    }
                }
                catch (OdbcException ex)
                {
                    Console.WriteLine("Database Error: " + ex.Message);
                }
            }

            return price;
        }

        public void CalculateAndPrintMetrics()
        {
            if (FarmAnimals.Count == 0)
            {
                Console.WriteLine("No farm animals to calculate metrics for.");
                return;
            }

            double goatMilkPrice = GetCommodityPrice("GoatMilk");
            double cowMilkPrice = GetCommodityPrice("CowMilk");
            double sheepWoolPrice = GetCommodityPrice("SheepWool");
            double waterPrice = GetCommodityPrice("Water");
            double livestockWeightTax = GetCommodityPrice("LivestockWeightTax");

            double totalIncomePerDay = 0.0;
            double totalCostPerDay = 0.0;

            foreach (var animal in FarmAnimals)
            {
                double income = 0.0;
                double cost = 0.0;

                if (animal is Cow cow)
                {
                    income = cow.Milk * cowMilkPrice;
                }
                else if (animal is Goat goat)
                {
                    income = goat.Milk * goatMilkPrice;
                }
                else if (animal is Sheep sheep)
                {
                    income = sheep.Wool * sheepWoolPrice;
                }

                cost = animal.Cost + (animal.Water * waterPrice) + (animal.Weight * livestockWeightTax);

                totalIncomePerDay += income;
                totalCostPerDay += cost;
            }

            Console.WriteLine("Metrics Report:");
            Console.WriteLine($"LivestockWeightTax: ${livestockWeightTax:F2}");
            Console.WriteLine($"WaterPrice: ${waterPrice:F2}");
            Console.WriteLine($"CowMilkPrice: ${cowMilkPrice:F2}");
            Console.WriteLine($"GoatMilkPrice: ${goatMilkPrice:F2}");
            Console.WriteLine($"SheepWoolPrice: ${sheepWoolPrice:F2}");
            Console.WriteLine($"Total Income Per Day: ${totalIncomePerDay:F2}");
            Console.WriteLine($"Total Cost Per Day: ${totalCostPerDay:F2}");

            double profitOrLoss = totalIncomePerDay - totalCostPerDay;

            if (profitOrLoss > 0)
            {
                Console.WriteLine($"Profit: ${profitOrLoss:F2}");
            }
            else if (profitOrLoss < 0)
            {
                Console.WriteLine($"Loss: ${Math.Abs(profitOrLoss):F2}");
            }
            else
            {
                Console.WriteLine("No Profit or Loss.");
            }

            double averageWeight = FarmAnimals.Average(animal => animal.Weight);
            Console.WriteLine($"Average Weight of all livestock: {averageWeight:F2} KG");
        }

        public void PrintConsole()
        {
            if (FarmAnimals.Count == 0)
            {
                Console.WriteLine("No farm animals to display.");
                return;
            }

            var sortedFarmAnimals = FarmAnimals.OrderBy(animal => animal.ID).ToList();

            Console.WriteLine($"{"Type",-10} {"ID",-5} {"Water",-12} {"Cost",-12} {"Weight",-15} {"Colour",-12} {"Milk/Wool",-12}");

            sortedFarmAnimals.ForEach(animal =>
            {
                string additionalInfo = "";

                if (animal is Goat goat)
                {
                    additionalInfo = $"{goat.Milk:F2}";
                }
                else if (animal is Cow cow)
                {
                    additionalInfo = $"{cow.Milk:F2}";
                }
                else if (animal is Sheep sheep)
                {
                    additionalInfo = $"{sheep.Wool:F2}";
                }

                Console.WriteLine($"{animal.GetType().Name,-10} {animal.ID,-5} {animal.Water,-12:F2} {animal.Cost,-12:F2} {animal.Weight,-15:F2} {animal.Colour,-12} {additionalInfo,-12}");
            });
        }

        internal void ReadFarmAnimalData()
        {
            using (var cmd = Conn.CreateCommand())
            {
                cmd.Connection = Conn;

                string sql;
                OdbcDataReader reader;

                sql = "SELECT * FROM Cow";
                cmd.CommandText = sql;
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int id = Util.GetInt(reader["ID"]);
                    double water = Util.GetDouble(reader["Water"]);
                    double cost = Util.GetDouble(reader["Cost"]);
                    double weight = Util.GetDouble(reader["Weight"]);
                    string colour = reader["Colour"].ToString();
                    double milk = Util.GetDouble(reader["Milk"]);
                    var cow = new Cow(id, water, cost, weight, colour, milk);
                    FarmAnimals.Add(cow);
                }
                reader.Close();

                sql = "SELECT * FROM Goat";
                cmd.CommandText = sql;
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int id = Util.GetInt(reader["ID"]);
                    double water = Util.GetDouble(reader["Water"]);
                    double cost = Util.GetDouble(reader["Cost"]);
                    double weight = Util.GetDouble(reader["Weight"]);
                    string colour = reader["Colour"].ToString();
                    double milk = Util.GetDouble(reader["Milk"]);
                    var goat = new Goat(id, water, cost, weight, colour, milk);
                    FarmAnimals.Add(goat);
                }
                reader.Close();

                sql = "SELECT * FROM Sheep";
                cmd.CommandText = sql;
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int id = Util.GetInt(reader["ID"]);
                    double water = Util.GetDouble(reader["Water"]);
                    double cost = Util.GetDouble(reader["Cost"]);
                    double weight = Util.GetDouble(reader["Weight"]);
                    string colour = reader["Colour"].ToString();
                    double wool = Util.GetDouble(reader["Wool"]);
                    var sheep = new Sheep(id, water, cost, weight, colour, wool);
                    FarmAnimals.Add(sheep);
                }
                reader.Close();
            }
        }

        public void ConsoleInsertAnimal()
        {
            Console.WriteLine("Choose an animal type to insert:");
            Console.WriteLine("1. Cow");
            Console.WriteLine("2. Goat");
            Console.WriteLine("3. Sheep");

            int animalTypeChoice;
            if (int.TryParse(Console.ReadLine(), out animalTypeChoice))
            {
                switch (animalTypeChoice)
                {
                    case 1:
                        ConsoleInsertCow();
                        break;
                    case 2:
                        ConsoleInsertGoat();
                        break;
                    case 3:
                        ConsoleInsertSheep();
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Please select a valid option.");
                        break;
                }
            }
            else
            {
                Console.WriteLine("Invalid input. Please enter a valid option.");
            }
        }

        public void ConsoleInsertCow()
        {
            Console.WriteLine("Enter water (KG):");
            double water;
            if (!double.TryParse(Console.ReadLine(), out water))
            {
                Console.WriteLine("Invalid input for water. Please enter a valid number.");
                return;
            }

            Console.WriteLine("Enter cost ($):");
            double cost;
            if (!double.TryParse(Console.ReadLine(), out cost))
            {
                Console.WriteLine("Invalid input for cost. Please enter a valid number.");
                return;
            }

            Console.WriteLine("Enter weight (KG):");
            double weight;
            if (!double.TryParse(Console.ReadLine(), out weight))
            {
                Console.WriteLine("Invalid input for weight. Please enter a valid number.");
                return;
            }

            Console.WriteLine("Enter Milk ($):");
            double milk;
            if (!double.TryParse(Console.ReadLine(), out milk))
            {
                Console.WriteLine("Invalid input for milk. Please enter a valid number.");
                return;
            }

            Console.WriteLine("Enter colour:");
            string colour = Console.ReadLine();

            var cow = new Cow(0, water, cost, weight, colour, milk); 

            int maxExistingID = FarmAnimals.Max(animal => animal.ID);

            int newID = maxExistingID + 1;

            using (var cmd = Conn.CreateCommand())
            {
                string sql = "INSERT INTO Cow ([ID], [Water], [Cost], [Weight], [Colour], [Milk]) VALUES (?, ?, ?, ?, ?, ?)";
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@p1", newID); 
                cmd.Parameters.AddWithValue("@p2", cow.Water);
                cmd.Parameters.AddWithValue("@p3", cow.Cost);
                cmd.Parameters.AddWithValue("@p4", cow.Weight);
                cmd.Parameters.AddWithValue("@p5", cow.Colour);
                cmd.Parameters.AddWithValue("@p6", cow.Milk);

                try
                {
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        cow.ID = newID; 
                        FarmAnimals.Add(cow);
                        Console.WriteLine("Cow added successfully!");
                    }
                    else
                    {
                        Console.WriteLine("Insertion didn't go through.");
                    }
                }
                catch (OdbcException ex)
                {
                    Console.WriteLine("Database Error: " + ex.Message);
                }
            }
        }

        public void ConsoleInsertGoat()
        {
            Console.WriteLine("Enter water (KG):");
            double water;
            if (!double.TryParse(Console.ReadLine(), out water))
            {
                Console.WriteLine("Invalid input for water. Please enter a valid number.");
                return;
            }

            Console.WriteLine("Enter cost ($):");
            double cost;
            if (!double.TryParse(Console.ReadLine(), out cost))
            {
                Console.WriteLine("Invalid input for cost. Please enter a valid number.");
                return;
            }

            Console.WriteLine("Enter weight (KG):");
            double weight;
            if (!double.TryParse(Console.ReadLine(), out weight))
            {
                Console.WriteLine("Invalid input for weight. Please enter a valid number.");
                return;
            }

            Console.WriteLine("Enter milk produced (L):");
            double milk;
            if (!double.TryParse(Console.ReadLine(), out milk))
            {
                Console.WriteLine("Invalid input for milk produced. Please enter a valid number.");
                return;
            }

            Console.WriteLine("Enter colour:");
            string colour = Console.ReadLine();

            var goat = new Goat(0, water, cost, weight, colour, milk); 

            int maxExistingID = FarmAnimals.Max(animal => animal.ID);

            int newID = maxExistingID + 1;

            using (var cmd = Conn.CreateCommand())
            {
                string sql = "INSERT INTO Goat ([ID], [Water], [Cost], [Weight], [Colour], [Milk]) VALUES (?, ?, ?, ?, ?, ?)";
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@p1", newID); 
                cmd.Parameters.AddWithValue("@p2", goat.Water);
                cmd.Parameters.AddWithValue("@p3", goat.Cost);
                cmd.Parameters.AddWithValue("@p4", goat.Weight);
                cmd.Parameters.AddWithValue("@p5", goat.Colour);
                cmd.Parameters.AddWithValue("@p6", goat.Milk);

                try
                {
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        goat.ID = newID;
                        FarmAnimals.Add(goat);
                        Console.WriteLine("Goat added successfully!");
                    }
                    else
                    {
                        Console.WriteLine("Insertion didn't go through.");
                    }
                }
                catch (OdbcException ex)
                {
                    Console.WriteLine("Database Error: " + ex.Message);
                }
            }
        }

        public void ConsoleInsertSheep()
        {
            Console.WriteLine("Enter water (KG):");
            double water;
            if (!double.TryParse(Console.ReadLine(), out water))
            {
                Console.WriteLine("Invalid input for water. Please enter a valid number.");
                return;
            }

            Console.WriteLine("Enter cost ($):");
            double cost;
            if (!double.TryParse(Console.ReadLine(), out cost))
            {
                Console.WriteLine("Invalid input for cost. Please enter a valid number.");
                return;
            }

            Console.WriteLine("Enter weight (KG):");
            double weight;
            if (!double.TryParse(Console.ReadLine(), out weight))
            {
                Console.WriteLine("Invalid input for weight. Please enter a valid number.");
                return;
            }

            Console.WriteLine("Enter wool produced (KG):");
            double wool;
            if (!double.TryParse(Console.ReadLine(), out wool))
            {
                Console.WriteLine("Invalid input for wool produced. Please enter a valid number.");
                return;
            }

            Console.WriteLine("Enter colour:");
            string colour = Console.ReadLine();
            colour = char.ToUpper(colour[0]) + colour.Substring(1).ToLower();



            var sheep = new Sheep(0, water, cost, weight, colour, wool); 

            int maxExistingID = FarmAnimals.Max(animal => animal.ID);

            int newID = maxExistingID + 1;

            using (var cmd = Conn.CreateCommand())
            {
                string sql = "INSERT INTO Sheep ([ID], [Water], [Cost], [Weight], [Colour], [Wool]) VALUES (?, ?, ?, ?, ?, ?)";
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@p1", newID); 
                cmd.Parameters.AddWithValue("@p2", sheep.Water);
                cmd.Parameters.AddWithValue("@p3", sheep.Cost);
                cmd.Parameters.AddWithValue("@p4", sheep.Weight);
                cmd.Parameters.AddWithValue("@p5", sheep.Colour);
                cmd.Parameters.AddWithValue("@p6", sheep.Wool);

                try
                {
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        sheep.ID = newID; 
                        FarmAnimals.Add(sheep);
                        Console.WriteLine("Sheep added successfully!");
                    }
                    else
                    {
                        Console.WriteLine("Insertion didn't go through.");
                    }
                }
                catch (OdbcException ex)
                {
                    Console.WriteLine("Database Error: " + ex.Message);
                }
            }
        }

        public void QueryFarmLivestock()
        {
            Console.WriteLine("Choose a query option:");
            Console.WriteLine("1. Query by ID");
            Console.WriteLine("2. Query by colour");
            Console.WriteLine("3. Query by type");
            Console.WriteLine("4. Query by weight threshold");

            int queryChoice;
            if (int.TryParse(Console.ReadLine(), out queryChoice))
            {
                switch (queryChoice)
                {
                    case 1:
                        ConsoleQueryByID();
                        break;
                    case 2:
                        ConsoleQueryByColour();
                        break;
                    case 3:
                        ConsoleQueryByType();
                        break;
                    case 4:
                        ConsoleQueryByWeightThreshold();
                        break;
                    default:
                        Console.WriteLine("Invalid query choice. Please select a valid option.");
                        break;
                }
            }
            else
            {
                Console.WriteLine("Invalid input. Please enter a valid option.");
            }
        }

        public void ConsoleQueryByID()
        {
            Console.WriteLine("Enter the ID of the animal you want to query:");

            int id;
            if (int.TryParse(Console.ReadLine(), out id))
            {
                FarmAnimal animal = FarmAnimals.FirstOrDefault(a => a.ID == id);
                if (animal != null)
                {
                    string animalInfo = $"{animal.ID,-12}{animal.Water,-12}{animal.Cost,-12}{animal.Weight,-12}{animal.Colour,-12}";

                    if (animal is Cow cow)
                    {
                        animalInfo += $"{cow.Milk,-12}";
                    }
                    else if (animal is Goat goat)
                    {
                        animalInfo += $"{goat.Milk,-12}";
                    }
                    else if (animal is Sheep sheep)
                    {
                        animalInfo += $"{sheep.Wool,-12}";
                    }

                    Console.WriteLine($"Animal type: {animal.GetType().Name}");
                    Console.WriteLine($"Animal information:");
                    Console.WriteLine(animalInfo);
                }
                else
                {
                    Console.WriteLine($"No animal found with ID {id}.");
                }
            }
            else
            {
                Console.WriteLine("Invalid input. Please enter a valid ID.");
            }
        }

        public void ConsoleQueryByColour()
        {
            Console.WriteLine("Enter the color of the animals you want to query:");

            string colour = Console.ReadLine();

            List<FarmAnimal> matchingAnimals = FarmAnimals.Where(a => a.Colour.Equals(colour, StringComparison.OrdinalIgnoreCase)).ToList();
            int totalAnimals = FarmAnimals.Count;

            if (matchingAnimals.Count > 0)
            {
                int colourCount = matchingAnimals.Count;
                double totalIncomePerDay = 0.0;
                double totalCostPerDay = 0.0;
                double colourPercentage = (colourCount / (double)totalAnimals) * 100.0;
                double taxPerDay = 0.0; // Initialize tax per day

                foreach (var animal in matchingAnimals)
                {
                    double income = 0.0;
                    double cost = 0.0;

                    if (animal is Cow cow)
                    {
                        income = cow.Milk * GetCommodityPrice("CowMilk");
                    }
                    else if (animal is Goat goat)
                    {
                        income = goat.Milk * GetCommodityPrice("GoatMilk");
                    }
                    else if (animal is Sheep sheep)
                    {
                        income = sheep.Wool * GetCommodityPrice("SheepWool");
                    }

                    cost = animal.Cost + (animal.Water * GetCommodityPrice("Water")) + (animal.Weight * GetCommodityPrice("LivestockWeightTax"));

                    // Calculate tax per day for animals with the specified color
                    if (animal.Colour.Equals(colour, StringComparison.OrdinalIgnoreCase))
                    {
                        taxPerDay += animal.Weight * GetCommodityPrice("LivestockWeightTax");
                    }

                    totalIncomePerDay += income;
                    totalCostPerDay += cost;

                    string animalInfo = $"{animal.ID,-12}{animal.Water,-12}{animal.Cost,-12}{animal.Weight,-12}{animal.Colour,-12}";

                    if (animal is Cow cowAnimal)
                    {
                        animalInfo += $"{cowAnimal.Milk,-12}";
                    }
                    else if (animal is Goat goatAnimal)
                    {
                        animalInfo += $"{goatAnimal.Milk,-12}";
                    }
                    else if (animal is Sheep sheepAnimal)
                    {
                        animalInfo += $"{sheepAnimal.Wool,-12}";
                    }

                    Console.WriteLine(animalInfo);
                }

                Console.WriteLine($"Number of livestock in {colour}: {matchingAnimals.Count}");
                Console.WriteLine($"Percentage of {colour} livestock: {colourPercentage:F2}%");
                Console.WriteLine($"Tax per day for {colour} animals: ${taxPerDay:F2}");
                Console.WriteLine($"Total Income Per Day for {colour} animals: ${totalIncomePerDay:F2}");
                Console.WriteLine($"Total Cost Per Day for {colour} animals: ${totalCostPerDay:F2}");

                double profitOrLoss = totalIncomePerDay - totalCostPerDay;
                if (profitOrLoss > 0)
                {
                    Console.WriteLine($"Profit for {colour} animals: ${profitOrLoss:F2}");
                }
                else if (profitOrLoss < 0)
                {
                    Console.WriteLine($"Loss for {colour} animals: ${Math.Abs(profitOrLoss):F2}");
                }
                else
                {
                    Console.WriteLine($"No Profit or Loss for {colour} animals.");
                }
            }
            else
            {
                Console.WriteLine($"No animals found with color '{colour}'.");
            }
        }

        public void ConsoleQueryByType()
        {
            Console.WriteLine("Enter the animal type you want to query (Cow, Goat, or Sheep):");

            string type = Console.ReadLine();

            List<FarmAnimal> matchingAnimals = FarmAnimals
                .Where(a => a.GetType().Name.Equals(type, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (matchingAnimals.Count > 0)
            {
                double totalProduce = 0.0;
                double totalWaterConsumed = matchingAnimals.Sum(a => a.Water);
                double totalTax = 0.0;

                foreach (var animal in matchingAnimals)
                {
                    if (type.Equals("Cow", StringComparison.OrdinalIgnoreCase))
                    {
                        totalProduce += ((Cow)animal).Milk;
                        totalTax += animal.Weight * GetCommodityPrice("LivestockWeightTax");
                    }
                    else if (type.Equals("Goat", StringComparison.OrdinalIgnoreCase))
                    {
                        totalProduce += ((Goat)animal).Milk;
                        totalTax += animal.Weight * GetCommodityPrice("LivestockWeightTax");
                    }
                    else if (type.Equals("Sheep", StringComparison.OrdinalIgnoreCase))
                    {
                        totalProduce += ((Sheep)animal).Wool;
                        totalTax += animal.Weight * GetCommodityPrice("LivestockWeightTax");
                    }
                }

                Console.WriteLine($"Total {type} produce for the day: {totalProduce}");
                Console.WriteLine($"Total water consumption for {type} for the day: {totalWaterConsumed}");
                Console.WriteLine($"Total tax for {type} for the day: {totalTax}");
            }
            else
            {
                Console.WriteLine($"No animals found of type '{type}'.");
            }
        }

        public void ConsoleQueryByWeightThreshold()
        {
            Console.WriteLine("Enter the weight threshold (in KG):");
            double weightThreshold;

            if (double.TryParse(Console.ReadLine(), out weightThreshold))
            {
                if (weightThreshold <= 0)
                {
                    Console.WriteLine("Invalid weight threshold. Please enter a positive value.");
                    return;
                }

                var aboveThresholdAnimals = FarmAnimals.Where(animal => animal.Weight > weightThreshold).ToList();

                if (aboveThresholdAnimals.Count == 0)
                {
                    Console.WriteLine("No animals found above the entered weight threshold.");
                    return;
                }

                double averageWeight = aboveThresholdAnimals.Average(animal => animal.Weight);

                double totalOperationCost = aboveThresholdAnimals.Sum(animal => animal.Cost);

                double goatMilkPrice = GetCommodityPrice("GoatMilk");
                double cowMilkPrice = GetCommodityPrice("CowMilk");
                double sheepWoolPrice = GetCommodityPrice("SheepWool");
                double waterPrice = GetCommodityPrice("Water");
                double livestockWeightTax = GetCommodityPrice("LivestockWeightTax");

                double totalIncomePerDay = 0.0;
                double totalCostPerDay = 0.0;

                foreach (var animal in aboveThresholdAnimals)
                {
                    double income = 0.0;
                    double cost = 0.0;

                    if (animal is Cow animalCow)
                    {
                        income = animalCow.Milk * cowMilkPrice;
                    }
                    else if (animal is Goat animalGoat)
                    {
                        income = animalGoat.Milk * goatMilkPrice;
                    }
                    else if (animal is Sheep animalSheep)
                    {
                        income = animalSheep.Wool * sheepWoolPrice;
                    }

                    cost = animal.Cost + (animal.Water * waterPrice) + (animal.Weight * livestockWeightTax);

                    totalIncomePerDay += income;
                    totalCostPerDay += cost;


                    string animalInfo = $"{animal.ID,-12}{animal.Water,-12}{animal.Cost,-12}{animal.Weight,-12}{animal.Colour,-12}";

                    if (animal is Cow cow)
                    {
                        animalInfo += $"{cow.Milk,-12}";
                    }
                    else if (animal is Goat goat)
                    {
                        animalInfo += $"{goat.Milk,-12}";
                    }
                    else if (animal is Sheep sheep)
                    {
                        animalInfo += $"{sheep.Wool,-12}";
                    }
                    Console.WriteLine(animalInfo);
                }
            

            double profitOrLoss = totalIncomePerDay - totalCostPerDay;

                Console.WriteLine($"Average Weight of animals above {weightThreshold} KG: {averageWeight:F2} KG");
                Console.WriteLine($"Total Operation Cost Per Day: ${totalOperationCost:F2}");
                Console.WriteLine($"Total Income Per Day: ${totalIncomePerDay:F2}");
                Console.WriteLine($"Total Cost Per Day: ${totalCostPerDay:F2}");

                if (profitOrLoss > 0)
                {
                    Console.WriteLine($"Profit: ${profitOrLoss:F2}");
                }
                else if (profitOrLoss < 0)
                {
                    Console.WriteLine($"Loss: ${Math.Abs(profitOrLoss):F2}");
                }
                else
                {
                    Console.WriteLine("No Profit or Loss.");
                }
            }
            else
            {
                Console.WriteLine("Invalid input for weight threshold. Please enter a valid number.");
            }
        }

        public void EditLivestockRecord()
        {
            Console.WriteLine("===Update database record===");
            Console.WriteLine("Enter livestock ID:");
            int livestockID;

            if (int.TryParse(Console.ReadLine(), out livestockID) && livestockID > 0)
            {
                

                var livestockToEdit = FarmAnimals.FirstOrDefault(animal => animal.ID == livestockID);

                if (livestockToEdit != null)
                {
                    int originalID = livestockToEdit.ID; // Store the original ID
                    Console.WriteLine("Original Livestock Record:");
                    PrintLivestockRecord(livestockToEdit);

                    Console.Write("Enter new ID: ");
                    int newID;
                    if (int.TryParse(Console.ReadLine(), out newID) && newID > 0 && !FarmAnimals.Any(animal => animal.ID == newID))
                    {
                        livestockToEdit.ID = newID;
                    }
                    else
                    {
                        Console.WriteLine("Invalid input for new ID. Please enter a valid, unique ID greater than 0.");
                        livestockToEdit.ID = originalID;
                    }

                    Console.Write("Enter Water: ");
                    double newWater;
                    if (double.TryParse(Console.ReadLine(), out newWater))
                    {
                        livestockToEdit.Water = newWater;
                    }

                    Console.Write("Enter Cost: ");
                    double newCost;
                    if (double.TryParse(Console.ReadLine(), out newCost))
                    {
                        livestockToEdit.Cost = newCost;
                    }

                    Console.Write("Enter Weight: ");
                    double newWeight;
                    if (double.TryParse(Console.ReadLine(), out newWeight))
                    {
                        livestockToEdit.Weight = newWeight;
                    }

                    Console.WriteLine("Enter colour:");
                    string colour = Console.ReadLine();
                    colour = char.ToUpper(colour[0]) + colour.Substring(1).ToLower();

                    if (livestockToEdit is Cow)
                    {
                        Console.Write("Enter Milk: ");
                        double newMilk;
                        if (double.TryParse(Console.ReadLine(), out newMilk))
                        {
                            ((Cow)livestockToEdit).Milk = newMilk;
                        }
                    }
                    else if (livestockToEdit is Goat)
                    {
                        Console.Write("Enter Milk: ");
                        double newMilk;
                        if (double.TryParse(Console.ReadLine(), out newMilk))
                        {
                            ((Goat)livestockToEdit).Milk = newMilk;
                        }
                    }
                    else if (livestockToEdit is Sheep)
                    {
                        Console.Write("Enter Wool: ");
                        double newWool;
                        if (double.TryParse(Console.ReadLine(), out newWool))
                        {
                            ((Sheep)livestockToEdit).Wool = newWool;
                        }
                    }

                    

                    Console.WriteLine("Livestock record updated successfully.");

                    string animalInfo = $"{livestockToEdit.ID,-12}{livestockToEdit.Water,-12}{livestockToEdit.Cost,-12}{livestockToEdit.Weight,-12}{livestockToEdit.Colour,-12}";

                    if (livestockToEdit is Cow cow)
                    {
                        animalInfo += $"{cow.Milk,-12}";
                    }
                    else if (livestockToEdit is Goat goat)
                    {
                        animalInfo += $"{goat.Milk,-12}";
                    }
                    else if (livestockToEdit is Sheep sheep)
                    {
                        animalInfo += $"{sheep.Wool,-12}";
                    }
                    Console.WriteLine($"Record updated to:");
                    Console.WriteLine(animalInfo);
                }
                else
                {
                    Console.WriteLine("Livestock with the specified ID not found.");
                }
            }
            else
            {
                Console.WriteLine("Invalid input for livestock ID. Please enter a valid number greater than 0.");
            }
        }

        private void PrintLivestockRecord(FarmAnimal livestock)
        {
            string animalInfo = $"{livestock.ID,-12}{livestock.Water,-12}{livestock.Cost,-12}{livestock.Weight,-12}{livestock.Colour,-12}";

            if (livestock is Cow cow)
            {
                animalInfo += $"{cow.Milk,-12}";
            }
            else if (livestock is Goat goat)
            {
                animalInfo += $"{goat.Milk,-12}";
            }
            else if (livestock is Sheep sheep)
            {
                animalInfo += $"{sheep.Wool,-12}";
            }

            Console.WriteLine(animalInfo);
        }

        public void ConsoleDeleteAnimalByID()
        {
            Console.WriteLine("===Delete record from database===");
            Console.WriteLine("Enter livestock ID:");

            int id;
            if (int.TryParse(Console.ReadLine(), out id))
            {
                FarmAnimal animalToDelete = FarmAnimals.FirstOrDefault(a => a.ID == id);
                if (animalToDelete != null)
                {
                    FarmAnimals.Remove(animalToDelete);
                    DeleteAnimalFromDatabase(animalToDelete);

                    string animalInfo = $"{animalToDelete.ID,-12}{animalToDelete.Water,-12}{animalToDelete.Cost,-12}{animalToDelete.Weight,-12}{animalToDelete.Colour,-12}";

                    if (animalToDelete is Cow cow)
                    {
                        animalInfo += $"{cow.Milk,-12}";
                    }
                    else if (animalToDelete is Goat goat)
                    {
                        animalInfo += $"{goat.Milk,-12}";
                    }
                    else if (animalToDelete is Sheep sheep)
                    {
                        animalInfo += $"{sheep.Wool,-12}";
                    }

                    Console.WriteLine($"Deleting the following animal record:");
                    Console.WriteLine(animalInfo);
                }
                else
                {
                    Console.WriteLine($"No animal found with ID {id}.");
                }
            }
            else
            {
                Console.WriteLine("Invalid input. Please enter a valid ID.");
            }
        }

        private void DeleteAnimalFromDatabase(FarmAnimal animal)
        {
            using (var cmd = Conn.CreateCommand())
            {
                string tableName = GetTableNameForType(animal.GetType());
                string sql = $"DELETE FROM {tableName} WHERE ID = ?";
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@p1", animal.ID);
                int rowsAffected = cmd.ExecuteNonQuery();
                
            }
        }

        private string GetTableNameForType(Type type)
        {
            if (type == typeof(Cow)) return "Cow";
            if (type == typeof(Goat)) return "Goat";
            if (type == typeof(Sheep)) return "Sheep";
            return string.Empty; 
        }

    }

    internal class Cow : FarmAnimal
    {
        public double Milk { get; set; }

        public Cow(int id, double water, double cost, double weight, string colour, double milk)
            : base(id, water, cost, weight, colour)
        {
            Milk = milk;
        }
    }

    internal class Goat : FarmAnimal
    {
        public double Milk { get; set; }

        public Goat(int id, double water, double cost, double weight, string colour, double milk) : base(id, water, cost, weight, colour)
        {
            Milk = milk;
        }

        
    }

    internal class Sheep : FarmAnimal
    {
        public double Wool { get; set; }

        public Sheep(int id, double water, double cost, double weight, string colour, double wool) : base(id, water, cost, weight, colour)
        {
            Wool = wool;
        }

        
    }

    internal static class Util
    {
        internal static OdbcConnection GetConn()
        {
            string? dbstr = ConfigurationManager.AppSettings.Get("odbcString");
            string fpath = @"C:\Users\kswal\Downloads\FarmData.accdb"; 
            string connstr = dbstr + fpath;
            var conn = new OdbcConnection(connstr);
            conn.Open();
            return conn;
        }

        

        public static int GetInt(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return 0;
            }
            return Convert.ToInt32(value);
        }

        public static double GetDouble(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return 0.0;
            }
            return Convert.ToDouble(value);
        }

        public static int InsertAndGetGeneratedID(OdbcCommand cmd, OdbcConnection conn)
        {
            cmd.ExecuteNonQuery();

            cmd.CommandText = "SELECT @@IDENTITY";
            object result = cmd.ExecuteScalar();

            return Convert.ToInt32(result);
        }
    }
}

