namespace ItemManager
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.Json;
    using System.Windows.Forms;
    using System.Drawing;
    using Properties;

    class Item
    {
        public string Name { get; set; }
        public double Price { get; set; }
    }

    class ItemManagerForm : Form
    {
        private static readonly string itemsFilePath = GlobalProperties.ItemsJSONFilePath;
        private List<Item> items = new();

        private Button add_Button;
        private Button showAll_Button;
        private Button editItem_Button;
        private Button deleteSpecific_Button;

        public ItemManagerForm()
        {
            LoadItems();
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            Text = "Item Manager";
            Size = new Size(400, 370);
            BackColor = GlobalProperties.BackgroundColor;

            add_Button = new Button
            {
                Text = "Add Items",
                Size = new Size(150, 50),
                BackColor = GlobalProperties.SecondaryColor,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
            };
            add_Button.FlatAppearance.BorderSize = 0;
            add_Button.Click += AddButton_Click;

            showAll_Button = new Button
            {
                Text = "Show All Items",
                Size = new Size(150, 50),
                BackColor = GlobalProperties.SecondaryColor,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
            };
            showAll_Button.FlatAppearance.BorderSize = 0;
            showAll_Button.Click += ShowAll_Button_Click;

            editItem_Button = new Button
            {
                Text = "Edit Item",
                Size = new Size(150, 50),
                BackColor = GlobalProperties.SecondaryColor,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
            };
            editItem_Button.FlatAppearance.BorderSize = 0;
            editItem_Button.Click += EditItem_Button_Click;

            deleteSpecific_Button = new Button
            {
                Text = "Delete Specific Item",
                Size = new Size(150, 50),
                BackColor = GlobalProperties.SecondaryColor,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
            };
            deleteSpecific_Button.FlatAppearance.BorderSize = 0;
            deleteSpecific_Button.Click += DeleteSpecific_Button_Click;

            Resize += ItemManagerForm_Resize;

            Controls.Add(add_Button);
            Controls.Add(showAll_Button);
            Controls.Add(editItem_Button);
            Controls.Add(deleteSpecific_Button);

            UpdateButtonLocations();
        }

        private void UpdateButtonLocations()
        {
            int buttonTopMargin = 30;
            int buttonSpacing = 20;
            int buttonLeft = (ClientSize.Width - add_Button.Width) / 2;

            add_Button.Location = new Point(buttonLeft, buttonTopMargin);
            showAll_Button.Location = new Point(buttonLeft, add_Button.Bottom + buttonSpacing);
            editItem_Button.Location = new Point(buttonLeft, showAll_Button.Bottom + buttonSpacing);
            deleteSpecific_Button.Location = new Point(buttonLeft, editItem_Button.Bottom + buttonSpacing);
        }

        private void ItemManagerForm_Resize(object sender, EventArgs e)
        {
            UpdateButtonLocations();
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            // Create a new form for adding items
            AddItemForm addItemForm = new AddItemForm()
            {
                StartPosition = FormStartPosition.CenterParent,
                BackColor = GlobalProperties.BackgroundColor,
            };
            // Show the form as a dialog
            DialogResult result = addItemForm.ShowDialog();

            // If the user clicked OK on the dialog
            if (result == DialogResult.OK && addItemForm.GetItem() != null)
            {
                // Get the item details from the form
                Item newItem = addItemForm.GetItem();

                // Add the new item to the list
                items.Add(newItem);

                // Save the updated items list
                SaveItems();
            }
        }

        private void Delete_Button_Click(object sender, EventArgs e)
        {
            // Implementation for deleting items
            // You can open a new form for deleting items and update the items list and file
            // For simplicity, I'll just delete the first item in the list
            if (items.Count > 0)
            {
                items.RemoveAt(0);
                SaveItems();
            }
        }

        private void ShowAll_Button_Click(object sender, EventArgs e)
        {
            // Create a new form for showing items
            ShowItemForm showItemForm = new ShowItemForm(items)
            {
                StartPosition = FormStartPosition.CenterParent,
                BackColor = GlobalProperties.BackgroundColor,
            };
            // Show the form as a dialog
            showItemForm.ShowDialog();
        }

        private void EditItem_Button_Click(object sender, EventArgs e)
        {
            using (EditItemForm editItemForm = new EditItemForm(items))
            {
                DialogResult result = editItemForm.ShowDialog();

                if (result == DialogResult.OK)
                {
                    // Get the edited item from the form
                    Item editedItem = editItemForm.GetEditedItem();

                    if (editedItem != null)
                    {
                        // Find the index of the item to be edited (case-insensitive)
                        int index = items.FindIndex(item =>
                            string.Compare(item.Name, editItemForm.SelectedName, StringComparison.OrdinalIgnoreCase) == 0);

                        if (index != -1)
                        {
                            // Update the item at the found index
                            items[index].Name = editedItem.Name;
                            items[index].Price = editedItem.Price;

                            // Save the updated items list
                            SaveItems();
                        }
                        else
                        {
                            MessageBox.Show("Item not found.", "Edit Item");
                        }
                    }
                }
            }
        }

        private void DeleteSpecific_Button_Click(object sender, EventArgs e)
        {
            using (DeleteItemForm deleteItemForm = new DeleteItemForm(items))
            {
                DialogResult result = deleteItemForm.ShowDialog();

                if (result == DialogResult.OK)
                {
                    // Get the item name to be deleted from the form
                    string itemNameToDelete = deleteItemForm.GetItemNameToDelete();

                    // Find the index of the item to be deleted
                    int index = items.FindIndex(item => item.Name.Equals(itemNameToDelete, StringComparison.OrdinalIgnoreCase));

                    if (index != -1)
                    {
                        // Remove the item at the found index
                        items.RemoveAt(index);

                        // Save the updated items list
                        SaveItems();
                    }
                    else
                    {
                        MessageBox.Show("Item not found.", "Delete Item");
                    }
                }
            }
        }

        private void SaveItems()
        {
            string json = JsonSerializer.Serialize(items);
            File.WriteAllText(itemsFilePath, json);
        }

        private void LoadItems()
        {
            if (File.Exists(itemsFilePath))
            {
                string json = File.ReadAllText(itemsFilePath);
                items = JsonSerializer.Deserialize<List<Item>>(json);
            }
        }
    }

    class AddItemForm : Form
    {
        private TextBox itemName_TextBox;
        private TextBox itemPrice_TextBox;
        private Button ok_Button;
        private Button cancel_Button;

        public AddItemForm()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            Text = "Add Item";
            Size = new Size(300, 200);
            BackColor = GlobalProperties.BackgroundColor;

            itemName_TextBox = new TextBox
            {
                Location = new Point(40, 20),
                Size = new Size(200, 20),
                PlaceholderText = "Item Name"
            };

            // checking for invalid
            itemName_TextBox.TextChanged += (sender, e) =>
            {
                if (itemName_TextBox.Text.Length > 5)
                {
                    itemName_TextBox.BackColor = Color.White;
                }
                else
                {
                    itemName_TextBox.BackColor = Color.Red;
                }
            };

            itemPrice_TextBox = new TextBox
            {
                Location = new Point(40, 60),
                Size = new Size(200, 20),
                PlaceholderText = "Item Price"
            };

            // checking for invalid
            itemPrice_TextBox.TextChanged += (sender, e) =>
            {
                if (itemPrice_TextBox.Text.Length > 0 || double.TryParse(itemPrice_TextBox.Text, out _))
                {
                    itemPrice_TextBox.BackColor = Color.White;
                }
                else
                {
                    itemPrice_TextBox.BackColor = Color.Red;
                }
            };

            ok_Button = new Button
            {
                Text = "OK",
                Location = new Point(50, 110),
                Size = new Size(80, 30),
                BackColor = GlobalProperties.PrimaryColor,
                DialogResult = DialogResult.OK,
                FlatStyle = FlatStyle.Flat,
            };
            ok_Button.FlatAppearance.BorderSize = 0;

            cancel_Button = new Button
            {
                Text = "Cancel",
                Location = new Point(145, 110),
                Size = new Size(80, 30),
                BackColor = GlobalProperties.SecondaryColor,
                DialogResult = DialogResult.Cancel,
                FlatStyle = FlatStyle.Flat,
            };
            cancel_Button.FlatAppearance.BorderSize = 0;

            Controls.Add(itemName_TextBox);
            Controls.Add(itemPrice_TextBox);
            Controls.Add(ok_Button);
            Controls.Add(cancel_Button);
        }

        public Item GetItem()
        {
            if (itemName_TextBox.BackColor == Color.Red || string.IsNullOrWhiteSpace(itemName_TextBox.Text))
            {
                MessageBox.Show("Item name cannot be less than 5 characters.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            if (string.IsNullOrWhiteSpace(itemPrice_TextBox.Text))
            {
                MessageBox.Show("Item price cannot be blank.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            // Check if item price is a valid number
            if (!double.TryParse(itemPrice_TextBox.Text, out double price))
            {
                MessageBox.Show("Item price must be a valid number.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            // Return a new Item object with the details entered in the form
            return new Item
            {
                Name = itemName_TextBox.Text,
                Price = double.Parse(itemPrice_TextBox.Text)
            };
        }
    }

    class ShowItemForm : Form
    {
        private DataGridView items_DataGridView;
        private Button ok_Button;

        public ShowItemForm(List<Item> items)
        {
            InitializeComponents();
            LoadItems(items);
        }

        private void InitializeComponents()
        {
            Text = "Items";
            Size = new Size(300, 451);
            BackColor = GlobalProperties.BackgroundColor;

            items_DataGridView = new DataGridView
            {
                Location = new Point(42, 20),
                Size = new Size(200, 303),
                BorderStyle = BorderStyle.None,
                BackColor = GlobalProperties.BackgroundColor,
                ForeColor = Color.Black,
                Font = new Font("Arial", 10),
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,

                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    Font = new Font("Arial", 10, FontStyle.Bold)
                },

                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false
            };

            ok_Button = new Button
            {
                Text = "OK",
                Location = new Point(100, 361),
                Size = new Size(80, 30),
                BackColor = GlobalProperties.PrimaryColor,
                DialogResult = DialogResult.OK,
                FlatStyle = FlatStyle.Flat,
            };
            ok_Button.FlatAppearance.BorderSize = 0;

            Controls.Add(items_DataGridView);
            Controls.Add(ok_Button);
        }

        private void LoadItems(List<Item> items)
        {
            // Populate the DataGridView with item names and prices
            items_DataGridView.Columns.Add("Name", "Name");
            items_DataGridView.Columns.Add("Price", "Price");

            foreach (var item in items)
            {
                items_DataGridView.Rows.Add(item.Name, item.Price);
            }
        }
    }

    class EditItemForm : Form
    {
        private ComboBox item_ComboBox;
        private TextBox itemName_TextBox;
        private TextBox itemPrice_TextBox;
        private Button ok_Button;
        private Button cancel_Button;

        private List<Item> items;

        public EditItemForm(List<Item> items)
        {
            this.items = items;
            InitializeComponents();
            LoadItemNames();
        }

        private void InitializeComponents()
        {
            Text = "Edit Item";
            Size = new Size(300, 200);
            BackColor = GlobalProperties.BackgroundColor;

            item_ComboBox = new ComboBox
            {
                Location = new Point(40, 20),
                Size = new Size(200, 20),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            item_ComboBox.SelectedIndexChanged += Item_ComboBox_SelectedIndexChanged;

            itemName_TextBox = new TextBox
            {
                Location = new Point(40, 50),
                Size = new Size(200, 20),
                PlaceholderText = "New Item Name"
            };

            itemPrice_TextBox = new TextBox
            {
                Location = new Point(40, 80),
                Size = new Size(200, 20),
                PlaceholderText = "New Item Price"
            };

            ok_Button = new Button
            {
                Text = "OK",
                Location = new Point(50, 110),
                Size = new Size(80, 30),
                DialogResult = DialogResult.OK,
                BackColor = GlobalProperties.PrimaryColor,
                FlatStyle = FlatStyle.Flat,
            };
            ok_Button.FlatAppearance.BorderSize = 0;

            cancel_Button = new Button
            {
                Text = "Cancel",
                Location = new Point(145, 110),
                Size = new Size(80, 30),
                DialogResult = DialogResult.Cancel,
                BackColor = GlobalProperties.SecondaryColor,
                FlatStyle = FlatStyle.Flat,
            };
            cancel_Button.FlatAppearance.BorderSize = 0;

            Controls.Add(item_ComboBox);
            Controls.Add(itemName_TextBox);
            Controls.Add(itemPrice_TextBox);
            Controls.Add(ok_Button);
            Controls.Add(cancel_Button);
        }

        private void LoadItemNames()
        {
            // Populate the combo box with item names
            foreach (var item in items)
            {
                item_ComboBox.Items.Add(item.Name);
            }
        }

        public string SelectedName { get; private set; }

        private void Item_ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // When an item is selected from the combo box, store its name
            int selectedIndex = item_ComboBox.SelectedIndex;

            if (selectedIndex >= 0 && selectedIndex < items.Count)
            {
                SelectedName = items[selectedIndex].Name;
                itemName_TextBox.Text = SelectedName;
                itemPrice_TextBox.Text = items[selectedIndex].Price.ToString();
            }
        }

        public Item GetEditedItem()
        {
            // Get the selected item index from the combo box
            int selectedIndex = item_ComboBox.SelectedIndex;

            if (selectedIndex >= 0 && selectedIndex < items.Count)
            {
                // Get the edited values from the text boxes
                string editedName = itemName_TextBox.Text;

                // Check if item name is blank
                if (string.IsNullOrWhiteSpace(editedName))
                {
                    MessageBox.Show("Item name cannot be blank.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }

                // Check if item price is a valid number
                if (!double.TryParse(itemPrice_TextBox.Text, out double editedPrice))
                {
                    MessageBox.Show("Item price must be a valid number.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }

                // Return a new Item object with the corrected details
                return new Item
                {
                    Name = editedName,
                    Price = editedPrice
                };
            }

            // If the selected index is out of range or no item is selected, return null
            return null;
        }
    }

    class DeleteItemForm : Form
    {
        private ComboBox item_ComboBox;
        private Button ok_Button;
        private Button cancel_Button;
        private List<Item> items;

        public DeleteItemForm(List<Item> items)
        {
            this.items = items;
            InitializeComponents();
            LoadItemNames();
        }

        private void InitializeComponents()
        {
            Text = "Delete Item";
            Size = new Size(300, 150);
            BackColor = GlobalProperties.BackgroundColor;

            item_ComboBox = new ComboBox
            {
                Location = new Point(57, 20),
                Size = new Size(170, 20),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            ok_Button = new Button
            {
                Text = "Delete",
                Location = new Point(50, 60),
                Size = new Size(80, 30),
                BackColor = GlobalProperties.PrimaryColor,
                DialogResult = DialogResult.OK,
                FlatStyle = FlatStyle.Flat,
            };
            ok_Button.FlatAppearance.BorderSize = 0;

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

            Controls.Add(item_ComboBox);
            Controls.Add(ok_Button);
            Controls.Add(cancel_Button);
        }

        private void LoadItemNames()
        {
            // Populate the combo box with item names
            foreach (var item in items)
            {
                item_ComboBox.Items.Add(item.Name);
            }
        }

        public string GetItemNameToDelete()
        {
            // Return the selected item name for deletion
            return item_ComboBox.SelectedItem?.ToString();
        }
    }
}