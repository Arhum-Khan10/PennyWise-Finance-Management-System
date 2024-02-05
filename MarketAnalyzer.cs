namespace MarketAnalyzer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using System.Windows.Forms;
    using System.Drawing;
    using Properties;

    class MarketAnalyzerForm : Form
    {
        private readonly string filePath = GlobalProperties.MarketJSONFilePath;

        private Button analyzeMarket_Button;
        private RichTextBox output_TextBox;

        public MarketAnalyzerForm()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            Text = "Market Analyzer";
            Size = new Size(600, 500);
            BackColor = GlobalProperties.BackgroundColor;

            analyzeMarket_Button = new Button
            {
                Text = "Analyze Market",
                Size = new Size(150, 50),
                Location = new Point(225, 25),
                BackColor = GlobalProperties.SecondaryColor,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
            };
            analyzeMarket_Button.FlatAppearance.BorderSize = 0;
            analyzeMarket_Button.Click += AnalyzeMarket_Button_Click;

            output_TextBox = new RichTextBox
            {
                Size = new Size(550, 350),
                Location = new Point(20, 100),
                ReadOnly = true,
                BackColor = GlobalProperties.BackgroundColor,
            };

            Controls.Add(analyzeMarket_Button);
            Controls.Add(output_TextBox);
        }

        private void AnalyzeMarket_Button_Click(object sender, EventArgs e)
        {
            output_TextBox.Clear();

            // Load sample market data from a file
            List<Product> products = LoadSampleMarketData();

            // Perform analysis (for example, finding the highest priced product)
            var highestPricedProduct = FindHighestPricedProduct(products);

            // Identify opportunities and threats
            IdentifyOpportunitiesAndThreats(products);

            if (highestPricedProduct != null)
            {
                output_TextBox.SelectionFont = new Font(output_TextBox.Font, FontStyle.Bold);
                output_TextBox.AppendText("Highest Priced Product:\n");
                output_TextBox.SelectionFont = new Font(output_TextBox.Font, FontStyle.Regular);
                output_TextBox.AppendText("Name:" + highestPricedProduct.Name);
                output_TextBox.SelectionFont = new Font(output_TextBox.Font, FontStyle.Regular);
                output_TextBox.AppendText("\nPrice: ");
                output_TextBox.SelectionFont = new Font(output_TextBox.Font, FontStyle.Regular);
                output_TextBox.AppendText($"{highestPricedProduct.Price:C}");            
            }
        }

        private List<Product> LoadSampleMarketData()
        {
            // Load sample market data from a JSON file

            try
            {
                string json = File.ReadAllText(filePath);
                List<Product> products = JsonSerializer.Deserialize<List<Product>>(json);
                return products;
            }
            catch (FileNotFoundException)
            {
                DisplayResult("Sample market data file not found. Please make sure the file exists.");
            }
            catch (Exception ex)
            {
                DisplayResult($"An error occurred while loading sample market data: {ex.Message}");
            }

            return new List<Product>();
        }

        private Product FindHighestPricedProduct(List<Product> products)
        {
            // Find the product with the highest price
            if (products.Any())
            {
                return products.OrderByDescending(p => p.Price).First();
            }

            DisplayResult("No products found for analysis. Please make sure the sample data is loaded.");
            return null;
        }

        private void IdentifyOpportunitiesAndThreats(List<Product> products)
        {
            // Identify opportunities and threats based on specific criteria
            if (!products.Any())
            {
                DisplayResult("No products found for analysis. Please make sure the sample data is loaded.");
                return;
            }

            int totalQuantity = products.Sum(p => p.Quantity);
            double averageQuantity = (double)totalQuantity / products.Count;

            // Identify opportunities (e.g., products with quantity above 1.2 times the average)
            var opportunities = products.Where(p => p.Quantity > 1.2 * averageQuantity);
            DisplayResults("Opportunities", opportunities);

            // Identify threats (e.g., products with quantity below 0.8 times the average)
            var threats = products.Where(p => p.Quantity < 0.8 * averageQuantity);
            DisplayResults("Threats", threats);

            // Identify products within a specific range (e.g., between 0.8 and 1.2 times the average quantity)
            var bestProducts = products.Where(p => p.Quantity >= 0.8 * averageQuantity && p.Quantity <= 1.2 * averageQuantity);

            // Display best products as opportunities
            DisplayResults("Best Products to Buy (Opportunities)", bestProducts);
        }

        private void DisplayResults(string category, IEnumerable<Product> products)
        {
            // Display the category only once in bold
            output_TextBox.AppendText($"{category}:\n");
            int start = output_TextBox.Text.Length - category.Length - 2; // Adjust the position for bolding
            int length = category.Length;

            // Select the category and apply bold style
            output_TextBox.Select(start, length);
            output_TextBox.SelectionFont = new Font(output_TextBox.Font, FontStyle.Bold);

            // Reset the font style to regular
            output_TextBox.SelectionLength = 0;
            output_TextBox.SelectionFont = new Font(output_TextBox.Font, FontStyle.Regular);

            foreach (var product in products)
            {
                DisplayResult($"{product.Name}: Quantity: {product.Quantity}");
            }

            DisplayResult("\n");

        }


        private void DisplayResult(string result)
        {
            output_TextBox.AppendText(result + "\n");
        }
    }

    class Product
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}