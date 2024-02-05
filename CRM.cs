namespace CRM
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Windows.Forms;
    using Properties;
    using ItemManager;

    public class Feedback
    {
        public string CustomerName { get; set; }
        public string CustomerID { get; set; }
        public int TransactionID { get; set; }
        public string ItemPurchased { get; set; }
        public string FeedbackText { get; set; }
        public int Rating { get; set; }

        public Feedback()
        {
            CustomerName = "";
            CustomerID = "";
            TransactionID = 0;
            ItemPurchased = "";
            FeedbackText = "";
            Rating = 0;
        }

        public Feedback(string customerName, string customerID, int transactionID, string itemPurchased, string feedbackText, int rating)
        {
            CustomerName = customerName;
            CustomerID = customerID;
            TransactionID = transactionID;
            ItemPurchased = itemPurchased;
            FeedbackText = feedbackText;
            Rating = rating;
        }
    }


    public class Order
    {
        public string ProductName { get; set; }
        public int TransactionID { get; set; }
        public DateTime DateBought { get; set; }
        [JsonIgnore]
        public string Email { get; set; }
    }

    public class Customer
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string CustomerID { get; set; }
        public List<Order> Orders { get; set; } = new List<Order>();

        [JsonIgnore]
        public List<Feedback> FeedbackList { get; set; } = new List<Feedback>();

        public static string GenerateCustomerID(string username, string email)
        {
            string combined = username + email;

            // Convert the combined string to bytes
            byte[] bytes = Encoding.UTF8.GetBytes(combined);

            // Create a new SHA256 hash object
            SHA256 sha256 = SHA256.Create();

            // Hash the combined string
            byte[] hashBytes = sha256.ComputeHash(bytes);

            // Convert the byte array to a hexadecimal string
            string hashHex = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

            return hashHex;
        }

        public Customer(string name, string email)
        {
            Name = name;
            Email = email;
            CustomerID = GenerateCustomerID(name, email);
        }

        public Customer()
        {
            Name = "";
            Email = "";
            CustomerID = "";
        }

        public class Transaction
        {
            public string CustomerID { get; set; }
            public string ProductName { get; set; }
            public int TransactionID { get; set; }
        }

        public static int FindTransaction(string customerID, string ProductName)
        {
            // Read the JSON file
            string json = File.ReadAllText(GlobalProperties.TransactionsJSONFilePath);

            // Parse the JSON file
            var transactions = JsonSerializer.Deserialize<List<Transaction>>(json);

            // Loop through each transaction
            foreach (var transaction in transactions)
            {
                // Check if the customerID and ProductName match
                if (transaction.CustomerID == customerID && transaction.ProductName == ProductName)
                {
                    // If they match, return the transactionID
                    return transaction.TransactionID;
                }
            }

            // If no matching transaction is found, return -1
            return -1;
        }

        public static int GenerateTransactionID()
        {
            // random number generator 6 digits
            Random rnd = new Random();
            return rnd.Next(100000, 999999);
        }
    }

    public partial class CRMForm : Form
    {
        private List<Customer> customers;

        private Button viewCustomers_Button;
        private Button addCustomer_Button;
        private Button enterFeedback_Button;
        private Button viewFeedback_Button;

        public CRMForm()
        {
            InitializeComponents();
            LoadCustomers();

            foreach (Customer customer in customers)
            {
                customer.FeedbackList = LoadFeedback(customer.CustomerID);
            }
        }

        private static List<Feedback> LoadFeedback(string customerID)
        {
            try
            {
                string feedbackFilePath = GlobalProperties.FeedbackJSONFilePath;

                if (File.Exists(feedbackFilePath))
                {
                    string json = File.ReadAllText(feedbackFilePath);

                    if (!string.IsNullOrEmpty(json))
                    {
                        var allFeedback = JsonSerializer.Deserialize<List<Feedback>>(json);
                        var customerFeedback = allFeedback.Where(f => f.CustomerID == customerID).ToList();
                        return customerFeedback;
                    }
                }
            }
            catch (JsonException ex)
            {
                MessageBox.Show($"Error loading feedback: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (IOException ex)
            {
                MessageBox.Show($"Error accessing feedback file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return new List<Feedback>();
        }

        private void LoadCustomers()
        {
            try
            {
                if (File.Exists(GlobalProperties.CustomersJSONFilePath))
                {
                    string json = File.ReadAllText(GlobalProperties.CustomersJSONFilePath);
                    customers = JsonSerializer.Deserialize<List<Customer>>(json);
                }
            }
            catch (JsonException ex)
            {
                MessageBox.Show($"Error loading customers: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (IOException ex)
            {
                MessageBox.Show($"Error accessing file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeComponents()
        {
            Text = "Customer Relationship Manager";
            Size = new Size(400, 370);
            BackColor = GlobalProperties.BackgroundColor;

            viewCustomers_Button = new Button
            {
                Text = "View Customers",
                Size = new Size(150, 50),
                BackColor = GlobalProperties.SecondaryColor,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
            };
            viewCustomers_Button.FlatAppearance.BorderSize = 0;
            viewCustomers_Button.Click += ViewCustomers_Button_Click;

            addCustomer_Button = new Button
            {
                Text = "Add Customer",
                Size = new Size(150, 50),
                BackColor = GlobalProperties.SecondaryColor,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
            };
            addCustomer_Button.FlatAppearance.BorderSize = 0;
            addCustomer_Button.Click += addCustomerButton_Click;

            enterFeedback_Button = new Button
            {
                Text = "Add Feedback",
                Size = new Size(150, 50),
                BackColor = GlobalProperties.SecondaryColor,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
            };
            enterFeedback_Button.FlatAppearance.BorderSize = 0;
            enterFeedback_Button.Click += enterFeedback_Button_Click;

            viewFeedback_Button = new Button
            {
                Text = "View Feedback",
                Size = new Size(150, 50),
                BackColor = GlobalProperties.SecondaryColor,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
            };
            viewFeedback_Button.FlatAppearance.BorderSize = 0;
            viewFeedback_Button.Click += ViewFeedback_Button_Click;

            Resize += CRMForm_Resize;

            Controls.Add(viewCustomers_Button);
            Controls.Add(addCustomer_Button);
            Controls.Add(enterFeedback_Button);
            Controls.Add(viewFeedback_Button);

            UpdateButtonLocations();
        }
        private void UpdateButtonLocations()
        {
            int buttonTopMargin = 30;
            int buttonSpacing = 20;
            int buttonLeft = (ClientSize.Width - viewCustomers_Button.Width) / 2;

            addCustomer_Button.Location = new Point(buttonLeft, buttonTopMargin);
            viewCustomers_Button.Location = new Point(buttonLeft, addCustomer_Button.Bottom + buttonSpacing);
            enterFeedback_Button.Location = new Point(buttonLeft, viewCustomers_Button.Bottom + buttonSpacing);
            viewFeedback_Button.Location = new Point(buttonLeft, enterFeedback_Button.Bottom + buttonSpacing);
        }
        private void CRMForm_Resize(object sender, EventArgs e)
        {
            UpdateButtonLocations();
        }

        private void addCustomerButton_Click(object sender, EventArgs e)
        {
            using (var addCustomerForm = new AddCustomerForm())
            {
                if (addCustomerForm.ShowDialog() == DialogResult.OK)
                {
                    if (string.IsNullOrEmpty(addCustomerForm.CustomerName))
                    {
                        MessageBox.Show("Customer name is not set.");
                        return;
                    }
                    Customer newCustomer = new Customer(addCustomerForm.CustomerName, addCustomerForm.CustomerEmail);

                    Order newOrder = new Order
                    {
                        ProductName = addCustomerForm.ProductName,
                        TransactionID = addCustomerForm.TransactionID,
                        DateBought = addCustomerForm.DateBought,
                        Email = addCustomerForm.CustomerEmail
                    };

                    // Checking if the customer already exists
                    var existingCustomer = customers.FirstOrDefault(c => c.CustomerID == newCustomer.CustomerID);
                    if (existingCustomer != null)
                    {
                        // Check if the order already exists for the customer
                        if (!existingCustomer.Orders.Any(o => o.TransactionID == newOrder.TransactionID))
                        {
                            existingCustomer.Orders.Add(newOrder);
                        }
                        // Update the customer record
                        SaveCustomers();
                    }
                    else
                    {
                        // Add the new customer
                        newCustomer.Orders.Add(newOrder);
                        customers.Add(newCustomer);
                        SaveCustomers();
                    }
                }
            }

        }

        private void ViewCustomers_Button_Click(object sender, EventArgs e)
        {
            using (var customerViewerForm = new CustomerViewerForm(customers))
            {
                customerViewerForm.ShowDialog();
            }
        }

        private void enterFeedback_Button_Click(object sender, EventArgs e)
        {
            using (var feedbackForm = new FeedbackForm(customers[0]))
            {
                feedbackForm.ShowDialog();
            }
        }

        private void ViewFeedback_Button_Click(object sender, EventArgs e)
        {
            using (var feedbackForm = new FeedbackForm(customers[0]))
            {
                var feedbackViewerForm = new FeedbackViewerForm(customers);
                feedbackViewerForm.ShowDialog();
            }
        }

        private void SaveCustomers()
        {
            string json = JsonSerializer.Serialize(customers);
            File.WriteAllText(GlobalProperties.CustomersJSONFilePath, json);
        }
    }

    public class CustomerViewerForm : Form
    {
        private List<Customer> customers;

        public CustomerViewerForm(List<Customer> customers)
        {
            this.customers = customers;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            Text = "Customer Viewer";
            Size = new Size(800, 600);

            var customerGridView = new DataGridView
            {
                Parent = this,
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                Columns =
                {
                    new DataGridViewTextBoxColumn { Name = "Name", DataPropertyName = "Name" },
                    new DataGridViewTextBoxColumn { Name = "Product", DataPropertyName = "ProductName" },
                    new DataGridViewTextBoxColumn { Name = "Transaction ID", DataPropertyName = "TransactionID" },
                    new DataGridViewTextBoxColumn { Name = "Date Bought", DataPropertyName = "DateBought" },
                },
            };

            var customerOrderList = new List<dynamic>();

            foreach (var customer in customers)
            {
                foreach (var order in customer.Orders)
                {
                    customerOrderList.Add(new
                    {
                        Name = customer.Name,
                        ProductName = order.ProductName,
                        TransactionID = order.TransactionID,
                        DateBought = order.DateBought.ToString("yyyy-MM-dd"),
                        Email = order.Email
                    });
                }

                if (customer.Orders.Count == 0)
                {
                    customerOrderList.Add(new
                    {
                        Name = customer.Name,
                        ProductName = "No orders.",
                        TransactionID = "",
                        DateBought = "",
                        Email = ""
                    });
                }
            }

            customerGridView.DataSource = customerOrderList;
        }
    }

    public class AddCustomerForm : Form
    {
        private TextBox CustomerName_TextBox;
        private TextBox TransactionID_TextBox;
        private TextBox Product_TextBox;
        private DateTimePicker DateBought_DateTimePicker;
        private TextBox CustomerEmail_TextBox;
        private Button Add_Button;

        private Customer customer;
        public string CustomerName { get; private set; }
        public new string ProductName { get; private set; }
        public int TransactionID { get; private set; }
        public DateTime DateBought { get; private set; }
        public string CustomerEmail { get; private set; }

        public AddCustomerForm()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            Text = "Add Customer";
            Size = new Size(400, 350);
            BackColor = GlobalProperties.BackgroundColor;

            CustomerName_TextBox = new TextBox
            {
                Location = new Point(20, 20),
                Size = new Size(300, 20),
                PlaceholderText = "Customer Name",
                BackColor = GlobalProperties.BackgroundColor,
            };
            // checking if the name exists
            CustomerName_TextBox.Validating += (s, e) =>
            {
                if (string.IsNullOrEmpty(CustomerName_TextBox.Text))
                {
                    MessageBox.Show("Please enter a valid name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            CustomerEmail_TextBox = new TextBox
            {
                Location = new Point(20, 60),
                Size = new Size(300, 20),
                PlaceholderText = "Customer Email",
                BackColor = GlobalProperties.BackgroundColor,
            };

            // checking if the email is valid
            CustomerEmail_TextBox.Validating += (s, e) =>
            {
                if (!CustomerEmail_TextBox.Text.Contains('@') || !CustomerEmail_TextBox.Text.Contains('.'))
                {
                    MessageBox.Show("Please enter a valid email address.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            TransactionID_TextBox = new TextBox
            {
                Location = new Point(20, 180),
                Size = new Size(300, 20),
                PlaceholderText = "Transaction ID",
                BackColor = GlobalProperties.BackgroundColor,
            };

            Product_TextBox = new TextBox
            {
                Location = new Point(20, 100),
                Size = new Size(300, 20),
                PlaceholderText = "Product",
                BackColor = GlobalProperties.BackgroundColor,
            };

            // checking if the product is valid
            Product_TextBox.Validating += (s, e) =>
            {
                // reading items.json and comparing
                string itemsFilePath = GlobalProperties.ItemsJSONFilePath;
                string json = File.ReadAllText(itemsFilePath);
                List<Item> items = JsonSerializer.Deserialize<List<Item>>(json);

                bool itemExists = false;
                foreach (var item in items)
                {
                    if (item.Name == Product_TextBox.Text)
                    {
                        itemExists = true;
                    }
                }

                if (!itemExists)
                {
                    MessageBox.Show("Please enter a valid product.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            DateBought_DateTimePicker = new DateTimePicker
            {
                Location = new Point(20, 140),
                Size = new Size(200, 20),
                Format = DateTimePickerFormat.Short,
                CustomFormat = "yyyy-MM-dd",
                Value = DateTime.Now,
                BackColor = GlobalProperties.BackgroundColor,
                ForeColor = GlobalProperties.PrimaryColor,
                CalendarForeColor = Color.Black,
            };

            Add_Button = new Button
            {
                Text = "Add Customer",
                Location = new Point(130, 250),
                Size = new Size(124, 40),
                BackColor = GlobalProperties.PrimaryColor,
                FlatStyle = FlatStyle.Flat,
            };
            Add_Button.FlatAppearance.BorderSize = 0;
            Add_Button.Click += Add_Button_Click;

            Controls.Add(CustomerName_TextBox);
            Controls.Add(CustomerEmail_TextBox);
            Controls.Add(Product_TextBox);
            Controls.Add(DateBought_DateTimePicker);
            Controls.Add(TransactionID_TextBox);
            Controls.Add(Add_Button);
        }

        private void Add_Button_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(CustomerName_TextBox.Text) && !string.IsNullOrEmpty(Product_TextBox.Text) && !string.IsNullOrEmpty(CustomerEmail_TextBox.Text))
            {
                CustomerName = CustomerName_TextBox.Text;

                customer = new Customer(CustomerName, CustomerEmail_TextBox.Text);
                ProductName = Product_TextBox.Text;
                TransactionID = TransactionID_TextBox.Text == "" ? Customer.GenerateTransactionID() : int.Parse(TransactionID_TextBox.Text);
                DateBought = DateBought_DateTimePicker.Value;
                CustomerEmail = CustomerEmail_TextBox.Text;

                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show("Please enter valid data for all fields.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    public class FeedbackViewerForm : Form
    {
        private List<Feedback> feedback;
        private DataGridView feedbackGridView;
        public FeedbackViewerForm(List<Customer> customers)
        {
            // harvesting customer data from the CRMForm
            feedback = new List<Feedback>();
            foreach (var customer in customers)
            {
                foreach (var feedbackItem in customer.FeedbackList)
                {
                    feedback.Add(feedbackItem);
                }
            }

            InitializeComponents();
        }

        private void FeedbackGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex == feedbackGridView.Columns["Rating"].Index && e.Value != null)
            {
                int rating = (int)e.Value;
                e.Value = new string('★', rating);
                e.CellStyle.ForeColor = Color.Gold;
            }
        }

        private void InitializeComponents()
        {
            Text = "Feedback View";
            Size = new Size(800, 600);

            feedbackGridView = new DataGridView
            {
                Parent = this,
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                Size = new Size(800, 600),
                Columns =
                {
                    new DataGridViewTextBoxColumn { Name = "Customer Name", DataPropertyName = "CustomerName" },
                    new DataGridViewTextBoxColumn { Name = "Customer ID", DataPropertyName = "CustomerID" },
                    new DataGridViewTextBoxColumn { Name = "Transaction ID", DataPropertyName = "TransactionID" },
                    new DataGridViewTextBoxColumn { Name = "Item Purchased", DataPropertyName = "ItemPurchased" },
                    new DataGridViewTextBoxColumn { Name = "Feedback", DataPropertyName = "FeedbackText", AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells },
                    new DataGridViewTextBoxColumn { Name = "Rating", DataPropertyName = "Rating" },
                },
            };

            feedbackGridView.CellFormatting += FeedbackGridView_CellFormatting;

            var feedbackList = new List<dynamic>();

            try
            {
                // Iterate over feedback store
                foreach (var feedback in feedback)
                {
                    feedbackList.Add(new
                    {
                        CustomerName = feedback.CustomerName,
                        CustomerID = feedback.CustomerID,
                        TransactionID = feedback.TransactionID,
                        ItemPurchased = feedback.ItemPurchased,
                        FeedbackText = feedback.FeedbackText,
                        Rating = feedback.Rating
                    });
                }

                feedbackGridView.DataSource = feedbackList;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading feedback: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    public class FeedbackForm : Form
    {
        private TextBox CustomerName_TextBox;
        private TextBox CustomerEmail_TextBox;
        private TextBox productName_TextBox;
        private RichTextBox feedback_RichTextBox;
        private NumericUpDown rating_NumericUpDown;
        private Button submitFeedback_Button;

        private Customer customer;

        public FeedbackForm(Customer customer)
        {
            this.customer = customer;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            Text = "Customer Feedback";
            Size = new Size(400, 400);
            BackColor = GlobalProperties.BackgroundColor;

            CustomerName_TextBox = new TextBox
            {
                Location = new Point(20, 20),
                Size = new Size(300, 20),
                PlaceholderText = "Your Name",
                BackColor = GlobalProperties.BackgroundColor
            };

            CustomerEmail_TextBox = new TextBox
            {
                Location = new Point(20, 60),
                Size = new Size(300, 20),
                PlaceholderText = "Your Email",
                BackColor = GlobalProperties.BackgroundColor
            };

            // checking if the email is valid
            CustomerEmail_TextBox.Validating += (s, e) =>
            {
                if (!CustomerEmail_TextBox.Text.Contains("@") || !CustomerEmail_TextBox.Text.Contains("."))
                {
                    MessageBox.Show("Please enter a valid email address.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            productName_TextBox = new TextBox
            {
                Location = new Point(20, 100),
                Size = new Size(300, 20),
                PlaceholderText = "Product Name",
                BackColor = GlobalProperties.BackgroundColor
            };

            // checking if the product is valid
            productName_TextBox.Validating += (s, e) =>
            {
                // reading items.json and comparing
                string itemsFilePath = GlobalProperties.ItemsJSONFilePath;
                string json = File.ReadAllText(itemsFilePath);
                List<Item> items = JsonSerializer.Deserialize<List<Item>>(json);

                bool itemExists = false;
                foreach (var item in items)
                {
                    if (item.Name == productName_TextBox.Text)
                    {
                        itemExists = true;
                    }
                }

                if (!itemExists)
                {
                    MessageBox.Show("Please enter a valid product.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            feedback_RichTextBox = new RichTextBox
            {
                Location = new Point(20, 140),
                Size = new Size(300, 100),
                BackColor = GlobalProperties.BackgroundColor,
            };
            feedback_RichTextBox.Enter += (s, e) => { if (feedback_RichTextBox.Text == "Enter your feedback here") feedback_RichTextBox.Text = ""; };
            feedback_RichTextBox.Leave += (s, e) => { if (string.IsNullOrWhiteSpace(feedback_RichTextBox.Text)) feedback_RichTextBox.Text = "Enter your feedback here"; };
            feedback_RichTextBox.Text = "Enter your feedback here";

            Label rating_Label = new Label
            {
                Text = "Stars:",
                Location = new Point(20, 255),
                Size = new Size(40, 20),
                BackColor = GlobalProperties.BackgroundColor,
            };

            rating_NumericUpDown = new NumericUpDown
            {
                Location = new Point(60, 250),
                Size = new Size(50, 20),
                BackColor = GlobalProperties.SecondaryColor,
                ForeColor = Color.Black,
                Minimum = 1,
                Maximum = 5
            };

            submitFeedback_Button = new Button
            {
                Text = "Submit Feedback",
                Location = new Point(120, 300),
                Size = new Size(150, 30),
                BackColor = GlobalProperties.PrimaryColor,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
            };
            submitFeedback_Button.FlatAppearance.BorderSize = 0;
            submitFeedback_Button.Click += SubmitFeedback_Button_Click;

            Controls.Add(CustomerName_TextBox);
            Controls.Add(CustomerEmail_TextBox);
            Controls.Add(productName_TextBox);
            Controls.Add(feedback_RichTextBox);
            Controls.Add(rating_Label);
            Controls.Add(rating_NumericUpDown);
            Controls.Add(submitFeedback_Button);
        }
        private void SubmitFeedback_Button_Click(object sender, EventArgs e)
        {

            // Extracting feedback details from the form
            string customerID = Customer.GenerateCustomerID(CustomerName_TextBox.Text, CustomerEmail_TextBox.Text);
            int transactionID = Customer.FindTransaction(customerID, productName_TextBox.Text);
            if (transactionID == -1)
            {
                MessageBox.Show("This customer has not bought this product.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string feedbackText = feedback_RichTextBox.Text;
            int rating = (int)rating_NumericUpDown.Value;

            // Checking if the customer exists
            var existingCustomer = customer.FeedbackList.FirstOrDefault(c => c.CustomerID == customerID && c.TransactionID == transactionID);
            if (existingCustomer != null)
            {
                MessageBox.Show("This customer has already submitted feedback for this product.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Adding the feedback to the customer
            customer.FeedbackList.Add(new Feedback(CustomerName_TextBox.Text, customerID, transactionID, productName_TextBox.Text, feedbackText, rating));

            // Saving the feedback
            SaveFeedback();

            // CLose the form
            DialogResult = DialogResult.OK;

        }

        public List<Feedback> LoadFeedback()
        {
            try
            {
                string feedbackFilePath = GlobalProperties.FeedbackJSONFilePath;

                if (File.Exists(feedbackFilePath))
                {
                    string json = File.ReadAllText(feedbackFilePath);

                    if (!string.IsNullOrEmpty(json))
                    {
                        return JsonSerializer.Deserialize<List<Feedback>>(json);
                    }
                }
            }
            catch (JsonException ex)
            {
                MessageBox.Show($"Error loading feedback: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (IOException ex)
            {
                MessageBox.Show($"Error accessing feedback file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return new List<Feedback>();
        }

        private void SaveFeedback()
        {
            List<Feedback> existingFeedback;

            try
            {
                if (File.Exists(GlobalProperties.FeedbackJSONFilePath))
                {
                    string existingJson = File.ReadAllText(GlobalProperties.FeedbackJSONFilePath);
                    existingFeedback = JsonSerializer.Deserialize<List<Feedback>>(existingJson);
                }
                else
                {
                    existingFeedback = new List<Feedback>();
                }

                existingFeedback.RemoveAll(f => f.TransactionID == -1);
                existingFeedback.RemoveAll(f => f.CustomerName == null);

                foreach (var feedback in customer.FeedbackList)
                {
                    if (!existingFeedback.Any(f => f.CustomerID == feedback.CustomerID && f.TransactionID == feedback.TransactionID))
                    {
                        existingFeedback.Add(feedback);
                    }
                }

                string updatedJson = JsonSerializer.Serialize(existingFeedback, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(GlobalProperties.FeedbackJSONFilePath, updatedJson);
            }
            catch (JsonException ex)
            {
                MessageBox.Show($"Error saving feedback: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (IOException ex)
            {
                MessageBox.Show($"Error accessing feedback file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

}