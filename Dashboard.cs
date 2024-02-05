namespace Dashboard
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Text.Json;
    using System.Windows.Forms;
    using MarketAnalyzer;
    using ReportGenerator;
    using ItemManager;
    using Properties;
    using CRM;
    using TransactionManager;

    class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    class LoginForm : Form
    {
        private static readonly string loginUsersPath = GlobalProperties.UsersJSONFilePath;
        private static readonly string logoPath = GlobalProperties.LogoFilePath;

        static List<User> users = new();

        private Button login_Button;
        private Button signup_Button;
        private Label username_Label;
        private TextBox username_TextBox;
        private Label password_Label;
        private TextBox password_TextBox;
        private bool isNotificationDisplayed = false;

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            isNotificationDisplayed = true;
        }

        public LoginForm()
        {
            LoadUsers();
            InitializeComponents();
            PictureBox pictureBox = new PictureBox
            {
                Image = Image.FromFile(logoPath),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Size = new Size(100, 100),
            };
            pictureBox.Location = new Point((ClientSize.Width - pictureBox.Width) / 2, 5);
            Controls.Add(pictureBox);
            VisibleChanged += LoginForm_VisibleChanged;
            FormClosed += LoginForm_FormClosed;
        }
        private void LoginForm_VisibleChanged(object sender, EventArgs e)
        {
            if (Visible)
            {
                username_TextBox.Text = "Example";
                password_TextBox.Text = "Password";
            }
        }

        private void LoginForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void Password_TextBox_KeyUp(object sender, KeyEventArgs e)
        { 
            if (isNotificationDisplayed)
            {
                isNotificationDisplayed = false;
                return;
            }
            if (e.KeyCode == Keys.Enter)
            {
                Login_Button_Click(sender, e);
            }    
        }
        private void InitializeComponents()
        {
            Text = "Welcome to PennyWise!";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            BackColor = GlobalProperties.BackgroundColor;

            Size = new Size(400, 350);

            login_Button = new Button
            {
                Text = "Login",
                Location = new Point(94, 260),
                Size = new Size(90, 40),
                BackColor = GlobalProperties.PrimaryColor,
                FlatStyle = FlatStyle.Flat,
            };
            login_Button.FlatAppearance.BorderSize = 0;
            login_Button.Click += Login_Button_Click;

            signup_Button = new Button
            {
                Text = "Sign up",
                Location = new Point(200, 260),
                Size = new Size(90, 40),
                BackColor = GlobalProperties.SecondaryColor,
                FlatStyle = FlatStyle.Flat,
            };
            signup_Button.FlatAppearance.BorderSize = 0;
            signup_Button.Click += Signup_Button_Click;

            username_Label = new Label
            {
                Text = "Username:",
                Location = new Point(35, 130),
                Font = new Font("Arial", 14),
                AutoSize = true,
                ForeColor = Color.Black
            };

            username_TextBox = new TextBox
            {
                Location = new Point(145, 130),
                Size = new Size(200, 30),
                BackColor = GlobalProperties.BackgroundColor,
            };
            username_TextBox.Enter += Username_TextBox_Enter;
            username_TextBox.Leave += Username_TextBox_Leave;
            username_TextBox.Text = "Example";

            password_Label = new Label
            {
                Text = "Password:",
                Location = new Point(35, 180),
                Font = new Font("Arial", 14),
                AutoSize = true,
                ForeColor = Color.Black
            };

            password_TextBox = new TextBox
            {
                Location = new Point(145, 180),
                Size = new Size(200, 30),
                PasswordChar = '*',
                BackColor = GlobalProperties.BackgroundColor
            };
            password_TextBox.Enter += Password_TextBox_Enter;
            password_TextBox.Leave += Password_TextBox_Leave;
            password_TextBox.KeyUp += Password_TextBox_KeyUp;
            password_TextBox.Text = "Password";

            Controls.Add(login_Button);
            Controls.Add(signup_Button);
            Controls.Add(username_Label);
            Controls.Add(username_TextBox);
            Controls.Add(password_Label);
            Controls.Add(password_TextBox);
        }

        private void Username_TextBox_Enter(object sender, EventArgs e)
        {
            if (username_TextBox.Text == "Example")
            {
                username_TextBox.Text = "";
            }
        }

        private void Username_TextBox_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(username_TextBox.Text))
            {
                username_TextBox.Text = "Example";
            }
        }

        public void Password_TextBox_Enter(object sender, EventArgs e)
        {
            if (password_TextBox.Text == "Password")
            {
                password_TextBox.Text = "";
            }
        }

        private void Password_TextBox_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(password_TextBox.Text))
            {
                password_TextBox.Text = "Password";
            }
        }

        private Dashboard dashboard;

        private void Login_Button_Click(object sender, EventArgs e)
        {
            string username = username_TextBox.Text ?? string.Empty;
            string password = password_TextBox.Text ?? string.Empty;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ShowError("Username or password cannot be empty.");
                return;
            }

            if (username.Length < 5 || password.Length < 5)
            {
                ShowError("Username and password must be at least 5 characters long.");
                return;
            }

            User user = users.Find(u => u.Username == username && u.Password == password);
            if (user != null)
            {
                dashboard = new Dashboard(this);
                Hide();
                dashboard.Show();
            }
            else
            {
                ShowError("Invalid username or password.");
            }
        }

        private void Logout_Button_Click(object sender, EventArgs e)
        {
            // Close the current form
            Close();
        }

        private void SaveUsers()
        {
            string json = JsonSerializer.Serialize(users);
            File.WriteAllText(loginUsersPath, json);
        }

        private void LoadUsers()
        {
            if (!File.Exists(loginUsersPath))
            {
                users = new();
                return;
            }

            string json = File.ReadAllText(loginUsersPath);
            users = JsonSerializer.Deserialize<List<User>>(json);
        }

        private void Signup_Button_Click(object sender, EventArgs e)
        {
            string username = username_TextBox.Text ?? string.Empty;
            string password = password_TextBox.Text ?? string.Empty;

            User existingUser = users.Find(u => u.Username == username);

            if (username.Length < 5)
            {
                MessageBox.Show("Username must be at least 5 characters long.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (password.Length < 5)
            {
                MessageBox.Show("Password must be at least 5 characters long.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!password.Any(char.IsDigit))
            {
                MessageBox.Show("Password must contain at least one number.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!password.Any(c => char.IsSymbol(c) || char.IsPunctuation(c)))
            {
                MessageBox.Show("Password must contain at least one special character.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (existingUser != null)
            {
                MessageBox.Show("Username already taken. Please choose a different one.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            users.Add(new User { Username = username, Password = password });
            SaveUsers();
            MessageBox.Show("Sign up successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        [STAThread]
        public static void RunLoginForm()
        {
            LoginForm loginForm = new LoginForm();
            Application.Run(loginForm);
        }
    }

    class Dashboard : Form
    {
        private static readonly string logoPath = GlobalProperties.LogoFilePath;
        private Form loginForm;

        public Dashboard(Form loginForm)
        {
            this.loginForm = loginForm;
            InitializeComponents();
            this.FormClosed += Dashboard_FormClosed;
        }

        private void InitializeComponents()
        {
            BackColor = GlobalProperties.BackgroundColor;
            Text = "Dashboard";
            Size = new Size(800, 600);

            // Header with name and menu icon
            Panel header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
            };

            // Add a PictureBox for your logo
            PictureBox pictureBox = new PictureBox
            {
                Image = Image.FromFile(logoPath),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Size = new Size(100, 100),
            };
            pictureBox.Location = new Point((ClientSize.Width - pictureBox.Width) / 2, 5);
            Controls.Add(pictureBox);
            Controls.Add(pictureBox);

            // Key financial metrics and charts
            // You can add charts and financial metrics here

            Button[] quickButtons = new Button[6];

            string[] buttonLabels = { "Sale Reports", "Item Manager", "Transaction Manager", "Market Analyzer", "Customer Analyzer", "Logout" };
            for (int i = 0; i < 6; i++)
            {
                int horizontalPosition;
                int verticalPosition;

                if (i == 0)
                {
                    horizontalPosition = 302;
                    verticalPosition = 175;
                }
                else if (i == 5)
                {
                    horizontalPosition = 302;
                    verticalPosition = 445;
                    quickButtons[i] = new Button
                    {
                        Text = buttonLabels[i],
                        Location = new Point(horizontalPosition, verticalPosition),
                        Size = new Size(180, 70),
                        BackColor = GlobalProperties.PrimaryColor,
                        Font = new Font("Arial", 10),
                        FlatStyle = FlatStyle.Flat,
                    };
                    quickButtons[i].FlatAppearance.BorderSize = 0;
                    quickButtons[i].Click += Logout_Button_Click;
                    Controls.Add(quickButtons[i]);
                    break;
                }
                else if (i % 2 == 0)
                {
                    horizontalPosition = 202;
                    verticalPosition = 220 + (45 * (i - 1));
                }
                else
                {
                    horizontalPosition = 402;
                    verticalPosition = 220 + (45 * i);
                }
                quickButtons[i] = new Button
                {
                    Text = buttonLabels[i],
                    Location = new Point(horizontalPosition, verticalPosition),
                    Size = new Size(180, 70),
                    BackColor = GlobalProperties.PrimaryColor,
                    Font = new Font("Arial", 10),
                    FlatStyle = FlatStyle.Flat,
                };
                quickButtons[i].FlatAppearance.BorderSize = 0;
                quickButtons[i].Click += Button_Click;
                Controls.Add(quickButtons[i]);
            }

            // Notification Section
            Panel notificationSection = new Panel
            {
                Dock = DockStyle.Fill,
            };

            Controls.Add(header);
            Controls.Add(notificationSection);
        }

        private void Button_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            if (button.Text == "Market Analyzer")
            {
                MarketAnalyzerForm marketAnalyzerForm = new();
                marketAnalyzerForm.Show();
            }
            else if (button.Text == "Sale Reports")
            {
                ReportGeneratorForm saleReportsForm = new();
                saleReportsForm.Show();
            }
            else if (button.Text == "Item Manager")
            {
                ItemManagerForm itemManagerForm = new();
                itemManagerForm.Show();
            }
            else if (button.Text == "Transaction Manager")
            {
                TransactionManagerForm transactionManagerForm = new();
                transactionManagerForm.Show();
            }
            else if (button.Text == "Customer Analyzer")
            {
                CRMForm customerAnalyzerForm = new();
                customerAnalyzerForm.Show();
            }
        }

        private void Logout_Button_Click(object sender, EventArgs e)
        {
            // Close the current form
            Close();

            // Show the login form
            loginForm.Show();
        }

        private void Dashboard_FormClosed(object sender, FormClosedEventArgs e)
        {
            loginForm.Show();
        }
    }
}