namespace TransactionManager
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.Json;
    using System.Windows.Forms;
    using System.Drawing;
    using Properties;
    using CRM;

    //transaction class
    class Transaction
    {
        public int TransactionID { get; set; }
        public string CustomerID { get; set; }
        public DateTime Date { get; set; }
        public string CustomerName { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public string CustomerEmail { get; set; }
        public string ProductName { get; set; }
        public decimal Quantity { get; set; }
        public float TotalCost { get; set; }

        public static int GenerateTransactionID()
        {
            // Generate a random transaction ID
            Random random = new();
            return random.Next(100000, 999999);
        }
    }
    // add transaction class
    class AddTransactionForm : Form //inheriting from windowsForm
    {
        private DateTimePicker date_TimePicker;
        private TextBox CustomerName_TextBox;
        private TextBox CustomerEmail_TextBox;
        private TextBox ProductName_TextBox;
        private TextBox quantity_TextBox;
        private Button add_Button;
        private Button cancel_Button;

        public AddTransactionForm()//constructor for initializing
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            Text = "Add Transaction";
            Size = new Size(300, 270);
            BackColor = GlobalProperties.BackgroundColor;

            CustomerName_TextBox = new TextBox
            {
                Location = new Point(40, 110),
                Size = new Size(200, 20),
                PlaceholderText = "Customer name",
                BackColor = GlobalProperties.BackgroundColor,
            };

            CustomerEmail_TextBox = new TextBox
            {
                Location = new Point(40, 140),
                Size = new Size(200, 20),
                PlaceholderText = "Customer email",
                BackColor = GlobalProperties.BackgroundColor,
            };
            // check if email is valid
            CustomerEmail_TextBox.Validating += (sender, e) =>
            {
                if (!CustomerEmail_TextBox.Text.Contains('@') || !CustomerEmail_TextBox.Text.Contains('.'))
                {
                    MessageBox.Show("Please enter a valid email address.");
                    e.Cancel = true;
                }
            };

            ProductName_TextBox = new TextBox
            {
                Location = new Point(40, 50),
                Size = new Size(200, 20),
                PlaceholderText = "Product",
                BackColor = GlobalProperties.BackgroundColor,
            };

            quantity_TextBox = new TextBox
            {
                Location = new Point(40, 80),
                Size = new Size(200, 20),
                PlaceholderText = "Transaction Quantity",
                BackColor = GlobalProperties.BackgroundColor,
            };

            date_TimePicker = new DateTimePicker
            {
                Location = new Point(40, 20),
                Size = new Size(200, 20),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today,
            };

            add_Button = new Button
            {
                Text = "Add",//title to be displyed on the button
                Location = new Point(50, 175),
                Size = new Size(80, 30),
                BackColor = GlobalProperties.PrimaryColor,//background color
                DialogResult = DialogResult.OK,//setting result of the button to 'ok'
                FlatStyle = FlatStyle.Flat,//setting the style of button to flat
            };
            add_Button.FlatAppearance.BorderSize = 0;//setting border of button to 0

            cancel_Button = new Button
            {
                Text = "Cancel",//display title
                Location = new Point(145, 175),//coordiantes
                Size = new Size(80, 30),//in pixels
                BackColor = GlobalProperties.SecondaryColor,//background
                DialogResult = DialogResult.Cancel,//result cancel
                FlatStyle = FlatStyle.Flat,//not 3D
            };
            cancel_Button.FlatAppearance.BorderSize = 0;

            Controls.Add(date_TimePicker);
            Controls.Add(ProductName_TextBox);
            Controls.Add(quantity_TextBox);
            Controls.Add(CustomerName_TextBox);
            Controls.Add(CustomerEmail_TextBox);
            Controls.Add(add_Button);
            Controls.Add(cancel_Button);
        }

        private bool ValidateInputs()
        {
            // Example validation - adjust according to your requirements
            if (string.IsNullOrWhiteSpace(CustomerName_TextBox.Text) ||
                string.IsNullOrWhiteSpace(CustomerEmail_TextBox.Text) ||
                string.IsNullOrWhiteSpace(ProductName_TextBox.Text))
            {
                MessageBox.Show("Please fill in all fields.");
                return false;
            }

            if (!decimal.TryParse(quantity_TextBox.Text, out _))
            {
                MessageBox.Show("Quantity must be a valid decimal number.");
                return false;
            }

            return true;
        }

        public Transaction GetTransaction()//function to get transaction
        {
            if (!ValidateInputs())
            {
                return null;
            }

            //Return a new Transaction object with the details entered in the form
            return new Transaction//object created
            {
                TransactionID = Transaction.GenerateTransactionID(),
                CustomerName = CustomerName_TextBox.Text,
                CustomerEmail = CustomerEmail_TextBox.Text,
                CustomerID = Customer.GenerateCustomerID(CustomerName_TextBox.Text, CustomerEmail_TextBox.Text),
                Date = date_TimePicker.Value,
                ProductName = ProductName_TextBox.Text,
                Quantity = decimal.Parse(quantity_TextBox.Text),
            };
        }
    }
    class EditTransactionForm : Form
    {
        private ComboBox TransactionComboBox;
        private DateTimePicker Date_DateTimePicker;
        private TextBox CustomerName_TextBox;
        private TextBox ProductName_TextBox;
        private TextBox quantity_TextBox;

        private Button edit_Button;
        private Button cancel_Button;

        private List<Transaction> Transactions;
        public Transaction SelectedTransaction { get; private set; }

        public EditTransactionForm(List<Transaction> transactions)
        {
            Transactions = transactions;
            InitializeComponents();
            PopulateTransactionIDsComboBox(TransactionComboBox, transactions);
        }

        private void InitializeComponents()
        {
            Text = "Edit Transaction";
            Size = new Size(300, 260);
            BackColor = GlobalProperties.BackgroundColor;

            TransactionComboBox = new ComboBox
            {
                Location = new Point(40, 20),
                Size = new Size(200, 20),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            //act according to the index selected
            TransactionComboBox.SelectedIndexChanged += TransactionComboBox_SelectedIndexChanged;

            //date
            Date_DateTimePicker = new DateTimePicker
            {
                Location = new Point(40, 50),
                Size = new Size(200, 20),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today,
            };

            CustomerName_TextBox = new TextBox
            {
                Location = new Point(40, 140),
                Size = new Size(200, 20),
                PlaceholderText = "Customer name",
                BackColor = GlobalProperties.BackgroundColor,
            };

            ProductName_TextBox = new TextBox
            {
                Location = new Point(40, 80),
                Size = new Size(200, 20),
                PlaceholderText = "New Transaction Product",
                BackColor = GlobalProperties.BackgroundColor,
            };

            quantity_TextBox = new TextBox
            {
                Location = new Point(40, 110),
                Size = new Size(200, 20),
                PlaceholderText = "New Transaction Quantity",
                BackColor = GlobalProperties.BackgroundColor,
            };

            edit_Button = new Button
            {
                Text = "Edit",//label of button
                Location = new Point(50, 180),
                Size = new Size(80, 30),
                BackColor = GlobalProperties.PrimaryColor,
                DialogResult = DialogResult.OK,
                FlatStyle = FlatStyle.Flat,//not 3D
            };
            edit_Button.FlatAppearance.BorderSize = 0;

            cancel_Button = new Button
            {
                Text = "Cancel",
                Location = new Point(145, 180),
                Size = new Size(80, 30),
                BackColor = GlobalProperties.SecondaryColor,
                DialogResult = DialogResult.Cancel,
                FlatStyle = FlatStyle.Flat,
            };
            cancel_Button.FlatAppearance.BorderSize = 0;

            Controls.Add(TransactionComboBox);
            Controls.Add(Date_DateTimePicker);
            Controls.Add(ProductName_TextBox);
            Controls.Add(CustomerName_TextBox);
            Controls.Add(quantity_TextBox);
            Controls.Add(edit_Button);
            Controls.Add(cancel_Button);
        }

        private void PopulateTransactionIDsComboBox(ComboBox comboBox, List<Transaction> transactions)
        {
            // sort
            transactions.Sort((transaction1, transaction2) =>
                transaction1.TransactionID.CompareTo(transaction2.TransactionID));

            comboBox.Items.Clear();
            foreach (var transaction in transactions)
            {
                comboBox.Items.Add(transaction.TransactionID);
            }
        }

        private void TransactionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Get the selected transaction index from the combo box
            int selectedIndex = TransactionComboBox.SelectedIndex;

            if (selectedIndex >= 0 && selectedIndex < Transactions.Count)
            {
                // Get the selected transaction
                SelectedTransaction = Transactions[selectedIndex];

                // Populate the text boxes with the selected transaction details
                Date_DateTimePicker.Value = SelectedTransaction.Date;
                CustomerName_TextBox.Text = SelectedTransaction.CustomerName;
                ProductName_TextBox.Text = SelectedTransaction.ProductName;
                quantity_TextBox.Text = SelectedTransaction.Quantity.ToString();
            }
            else
            {
                MessageBox.Show("Transaction not found.", "Edit Transaction");
            }
        }

        public Transaction GetEditedTransaction()
        {
            // Get the selected transaction index from the combo box
            int selectedIndex = TransactionComboBox.SelectedIndex;

            if (selectedIndex >= 0 && selectedIndex < Transactions.Count)
            {
                // Get the edited values (text)from the text boxes
                string editedDate = Date_DateTimePicker.Value.ToShortDateString();

                // Check if the product name is not null before adding
                string editedProductName = ProductName_TextBox.Text;
                if (editedProductName == null || editedProductName == "")
                {
                    MessageBox.Show("Product name cannot be empty.", "Edit Transaction");
                    return null;
                }

                // Check if the quantity is not null before adding
                decimal editedQuantity;
                if (!decimal.TryParse(quantity_TextBox.Text, out editedQuantity))
                {
                    MessageBox.Show("Quantity must be a number.", "Edit Transaction");
                    return null;
                }

                // Return a new Transaction object with the edited details
                return new Transaction
                {
                    TransactionID = SelectedTransaction.TransactionID, // Preserve original TransactionID
                    CustomerID = SelectedTransaction.CustomerID,       // Preserve original CustomerID
                    Date = DateTime.Parse(editedDate),
                    CustomerName = SelectedTransaction.CustomerName,   // Preserve original CustomerName
                    CustomerEmail = SelectedTransaction.CustomerEmail, // Preserve original CustomerEmail
                    ProductName = editedProductName,
                    Quantity = editedQuantity,
                    TotalCost = SelectedTransaction.TotalCost,
                };
            }
            else
            {
                MessageBox.Show("Transaction not found.", "Edit Transaction");
                return null;
            }
        }
    }

    class DeleteTransactionForm : Form
    {
        private ComboBox transactionComboBox;
        private Button deleteButton;
        private Button cancel_Button;
        private List<Transaction> transactions;

        public DeleteTransactionForm(List<Transaction> transactions)
        {
            this.transactions = transactions;
            InitializeComponents();
            PopulateTransactionIDsComboBox(transactionComboBox, transactions);
        }

        private void InitializeComponents()
        {
            Text = "Delete Transaction";//title
            Size = new Size(300, 150);
            BackColor = GlobalProperties.BackgroundColor;

            transactionComboBox = new ComboBox
            {
                Location = new Point(57, 20),//loc
                Size = new Size(170, 20),
                DropDownStyle = ComboBoxStyle.DropDownList//user cant enter data
            };

            deleteButton = new Button
            {
                Text = "Delete",
                Location = new Point(50, 60),
                Size = new Size(80, 30),
                BackColor = GlobalProperties.PrimaryColor,
                DialogResult = DialogResult.OK,//result dialogue
                FlatStyle = FlatStyle.Flat,//not 3D
            };
            deleteButton.FlatAppearance.BorderSize = 0;

            cancel_Button = new Button
            {
                Text = "Cancel",
                Location = new Point(154, 60),
                Size = new Size(80, 30),
                BackColor = GlobalProperties.SecondaryColor,
                DialogResult = DialogResult.Cancel,
                FlatStyle = FlatStyle.Flat,
            };
            cancel_Button.FlatAppearance.BorderSize = 0;

            Controls.Add(transactionComboBox);
            Controls.Add(deleteButton);
            Controls.Add(cancel_Button);
        }

        public string GetTransactionToDelete()
        {
            // Return the selected transaction 
            return transactionComboBox.Text.ToString();
        }

        private void PopulateTransactionIDsComboBox(ComboBox comboBox, List<Transaction> transactions)
        {
            // sort
            transactions.Sort((transaction1, transaction2) =>
                transaction1.TransactionID.CompareTo(transaction2.TransactionID));
                
            comboBox.Items.Clear();
            foreach (var transaction in transactions)
            {
                comboBox.Items.Add(transaction.TransactionID);
            }
        }
    }

    class TransactionManagerForm : Form//inheriting from form class
    {
        private static readonly string transactionsFilePath = GlobalProperties.TransactionsJSONFilePath;
        private List<Transaction> transactions = new();//declared and initialized

        private Button addTransaction_Button;//declaration of buttons
        private Button editTransaction_Button;
        private Button deleteTransaction_Button;
        private Button viewTransactions_Button;

        public TransactionManagerForm()//constructor
        {
            LoadTransactions();
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            Text = "Transaction Manager";
            Size = new Size(400, 370);
            BackColor = GlobalProperties.BackgroundColor;

            addTransaction_Button = new Button
            {
                Text = "Add Transaction",
                Size = new Size(150, 50),
                BackColor = GlobalProperties.SecondaryColor,//background color of button
                ForeColor = Color.Black,//text color
                FlatStyle = FlatStyle.Flat,//flat appearance
            };
            addTransaction_Button.FlatAppearance.BorderSize = 0;
            addTransaction_Button.Click += AddTransaction_Button_Click;

            editTransaction_Button = new Button
            {
                Text = "Edit Transaction",
                Size = new Size(150, 50),
                BackColor = GlobalProperties.SecondaryColor,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
            };
            editTransaction_Button.FlatAppearance.BorderSize = 0;
            editTransaction_Button.Click += EditTransaction_Button_Click;

            deleteTransaction_Button = new Button
            {
                Text = "Delete Transaction",
                Size = new Size(150, 50),
                BackColor = GlobalProperties.SecondaryColor,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
            };
            deleteTransaction_Button.FlatAppearance.BorderSize = 0;
            deleteTransaction_Button.Click += DeleteTransactionButton_Click;

            //view button
            viewTransactions_Button = new Button
            {
                Text = "View Transactions",
                Size = new Size(150, 50),
                BackColor = GlobalProperties.SecondaryColor,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
            };
            viewTransactions_Button.FlatAppearance.BorderSize = 0;
            viewTransactions_Button.Click += ViewTransactions_Button_Click;

            Resize += TransactionManagerForm_Resize;

            Controls.Add(addTransaction_Button);
            Controls.Add(editTransaction_Button);
            Controls.Add(deleteTransaction_Button);
            Controls.Add(viewTransactions_Button);

            UpdateButtonLocations();
        }

        private void UpdateButtonLocations()
        {
            int buttonTopMargin = 30;//for first button
            int buttonSpacing = 20;//vertical spacing b/w buttons, horizontal positon=client width of form- first button
            int buttonLeft = (ClientSize.Width - addTransaction_Button.Width) / 2;  //divide to center  button horizontally
            addTransaction_Button.Location = new Point(buttonLeft, buttonTopMargin);
            editTransaction_Button.Location = new Point(buttonLeft, addTransaction_Button.Bottom + buttonSpacing);
            deleteTransaction_Button.Location = new Point(buttonLeft, editTransaction_Button.Bottom + buttonSpacing);
            viewTransactions_Button.Location = new Point(buttonLeft, deleteTransaction_Button.Bottom + buttonSpacing);
        }

        private void TransactionManagerForm_Resize(object sender, EventArgs e)
        {
            UpdateButtonLocations();
        }

        private void AddTransaction_Button_Click(object sender, EventArgs e)
        {
            // Create a new form for adding transactions
            AddTransactionForm addTransactionForm = new AddTransactionForm()
            {
                StartPosition = FormStartPosition.CenterParent,
                BackColor = GlobalProperties.BackgroundColor,
            };
            // Show the form as a dialog
            DialogResult result = addTransactionForm.ShowDialog();

            // If the user clicked OK on the dialog
            if (result == DialogResult.OK)
            {
                // Get the transaction details from the form
                Transaction newTransaction = addTransactionForm.GetTransaction();

                // Add the new transaction to the list
                transactions.Add(newTransaction);

                // Save the updated transactions list
                SaveTransactions();
            }

        }

        private void EditTransaction_Button_Click(object sender, EventArgs e)
        {
            using (EditTransactionForm editTransactionForm = new EditTransactionForm(transactions))
            {
                DialogResult result = editTransactionForm.ShowDialog();

                if (result == DialogResult.OK)
                {
                    // Get the edited transaction from the form
                    Transaction editedTransaction = editTransactionForm.GetEditedTransaction();

                    if (editedTransaction != null)
                    {
                        // Get the transaction to edit
                        Transaction transactionToEdit = editTransactionForm.SelectedTransaction;

                        // Check if transactions is null or if something went wrong in the form
                        if (transactions == null || transactionToEdit == null)
                        {
                            MessageBox.Show("Transactions list or selected transaction is null.", "Error");
                            return; // Exit the method to avoid further processing
                        }

                        // Find the index of the transaction to be edited
                        int index = transactions.FindIndex(transaction =>
                            transaction.TransactionID == transactionToEdit.TransactionID);

                        if (index != -1)
                        {
                            // Update the transaction at the found index
                            transactions[index].Date = editedTransaction.Date;
                            transactions[index].ProductName = editedTransaction.ProductName;
                            transactions[index].Quantity = editedTransaction.Quantity;

                            // Save the updated transactions list
                            SaveTransactions();
                        }
                        else
                        {
                            MessageBox.Show("Transaction not found.", "Edit Transaction");
                        }
                    }
                }
            }
        }

        private void DeleteTransactionButton_Click(object sender, EventArgs e)
        {
            using (DeleteTransactionForm deleteTransactionForm = new DeleteTransactionForm(transactions))
            {
                DialogResult result = deleteTransactionForm.ShowDialog();

                if (result == DialogResult.OK)
                {
                    // Get the transaction description to be deleted from the form
                    string transactionDescriptionToDelete = deleteTransactionForm.GetTransactionToDelete();

                    // Check if transactions is null or if something went wrong in the form
                    if (transactions == null)
                    {
                        MessageBox.Show("Transactions list is null.", "Error");
                        return; // Exit the method to avoid further processing
                    }

                    // Find the index of the transaction to be deleted
                    int index = transactions.FindIndex(transaction =>
                        transaction.ToString() == transactionDescriptionToDelete);

                    if (index != -1)
                    {
                        // Remove the transaction at the found index
                        transactions.RemoveAt(index);

                        // Save the updated transactions list
                        SaveTransactions();
                    }
                    else
                    {
                        MessageBox.Show("Transaction not found.", "Delete Transaction");
                    }
                }
            }
        }

        private void ViewTransactions_Button_Click(object sender, EventArgs e)
        {
            // Create a new form for showing transactions
            ShowTransactionForm showTransactionForm = new ShowTransactionForm(transactions)
            {
                StartPosition = FormStartPosition.CenterParent,
                BackColor = GlobalProperties.BackgroundColor,
            };
            // Show the form as a dialog
            showTransactionForm.ShowDialog();
        }

        private void SaveTransactions()
        {
            string json = JsonSerializer.Serialize(transactions);
            File.WriteAllText(transactionsFilePath, json);
        }

        private void LoadTransactions()
        {
            if (File.Exists(transactionsFilePath))
            {
                try
                {
                    string json = File.ReadAllText(transactionsFilePath);
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true // Ensures case-insensitivity when matching JSON properties to C# properties
                    };
                    transactions = JsonSerializer.Deserialize<List<Transaction>>(json, options);

                    if (transactions == null)
                    {
                        transactions = new List<Transaction>();
                        Console.WriteLine("No transactions found in the file.");
                    }
                }
                catch (JsonException ex)
                {
                    MessageBox.Show($"Error parsing JSON data: {ex.Message}", "JSON Parse Error");
                    transactions = new List<Transaction>();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading transactions: {ex.Message}", "Load Error");
                    transactions = new List<Transaction>();
                }
            }
            else
            {
                MessageBox.Show("Transactions file not found.", "File Not Found");
                transactions = new List<Transaction>();
            }
        }

    }

    class ShowTransactionForm : Form
    {
        private DataGridView transactionsDataGridView;
        private Button okButton;

        public ShowTransactionForm(List<Transaction> transactions)
        {
            InitializeComponents();
            ViewTransactions(transactions);
        }

        private void InitializeComponents()
        {
            Text = "Transactions";
            Size = new Size(1200, 450);
            BackColor = GlobalProperties.BackgroundColor;

            transactionsDataGridView = new DataGridView
            {
                Location = new Point(20, 20),
                Size = new Size(1160, 320),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = GlobalProperties.PrimaryColor,
                    ForeColor = Color.White,
                    Font = new Font("Arial", 9, FontStyle.Bold),
                },
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                BackgroundColor = GlobalProperties.BackgroundColor,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = GlobalProperties.PrimaryColor,
                RowTemplate = new DataGridViewRow
                {
                    Height = 30,
                    MinimumHeight = 20,
                    DividerHeight = 1,
                    Resizable = DataGridViewTriState.False,
                },
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                ScrollBars = ScrollBars.Vertical,
            };

            okButton = new Button
            {
                Text = "OK",
                Location = new Point(550, 360),
                Size = new Size(80, 30),
                BackColor = GlobalProperties.PrimaryColor,
                DialogResult = DialogResult.OK,
                FlatStyle = FlatStyle.Flat,
            };
            okButton.FlatAppearance.BorderSize = 0;

            Controls.Add(transactionsDataGridView);
            Controls.Add(okButton);
        }

        private void ViewTransactions(List<Transaction> transactions)
        {
            // Clear existing columns and rows
            transactionsDataGridView.Columns.Clear();
            transactionsDataGridView.Rows.Clear();

            // Add columns
            transactionsDataGridView.Columns.Add("transactionID", "Transaction ID");
            transactionsDataGridView.Columns.Add("customerID", "Customer ID");
            transactionsDataGridView.Columns.Add("date", "Date");
            transactionsDataGridView.Columns.Add("customerName", "Customer Name");
            transactionsDataGridView.Columns.Add("productName", "Product Name");
            transactionsDataGridView.Columns.Add("quantity", "Quantity");
            transactionsDataGridView.Columns.Add("totalCost", "Total Cost");

            // Add rows
            foreach (var transaction in transactions)
            {
                transactionsDataGridView.Rows.Add(
                    transaction.TransactionID,
                    transaction.CustomerID,
                    transaction.Date.ToShortDateString(),
                    transaction.CustomerName,
                    transaction.ProductName,
                    transaction.Quantity,
                    transaction.TotalCost
                );
            }
        }
    }
}
