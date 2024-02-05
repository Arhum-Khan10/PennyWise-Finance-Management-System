namespace ReportGenerator
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using System.Windows.Forms;
    using System.Drawing;
    using System.Windows.Forms.DataVisualization.Charting;
    using Properties;

    class YearlyReport
    {
        public string Month { get; set; }
        public int Year { get; set; }
        public double TotalSales { get; set; }
    }

    class ReportGeneratorForm : Form
    {
        private static readonly string YearlyReportsFilePath = GlobalProperties.ReportsJSONFilePath;
        private List<YearlyReport> YearlyReports = new();

        private Button AddYearlyReport_Button;
        private Button viewYearlyReports_Button; // button for viewing sales reports
        private Button deleteYearlyReports_Button; // button for deleting sales reports
        private TextBox month_TextBox;
        private TextBox year_TextBox;
        private TextBox totalSales_TextBox;
        private Chart sales_Chart; // Use Chart for displaying sales reports

        public ReportGeneratorForm()
        {
            LoadYearlyReports();
            InitializeComponents();
            this.BackColor = GlobalProperties.BackgroundColor;

            // Update the chart
            UpdateSales_Chart();
        }

        private void InitializeComponents()
        {
            Text = "Yearly Sales Report Generator";
            Size = new Size(800, 700);

            AddYearlyReport_Button = new Button
            {
                Text = "Add New Report",
                Size = new Size(200, 50),
                BackColor = GlobalProperties.SecondaryColor,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat
            };
            AddYearlyReport_Button.FlatAppearance.BorderSize = 0;
            AddYearlyReport_Button.Click += AddYearlyReport_Button_Click;

            viewYearlyReports_Button = new Button
            {
                Text = "View Yearly Sales",
                Size = new Size(200, 50),
                BackColor = GlobalProperties.SecondaryColor,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat
            };
            viewYearlyReports_Button.FlatAppearance.BorderSize = 0;
            viewYearlyReports_Button.Click += ViewYearlyReports_Button_Click;

            deleteYearlyReports_Button = new Button
            {
                Text = "Delete Reports",
                Size = new Size(200, 50),
                BackColor = GlobalProperties.SecondaryColor,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat
            };
            deleteYearlyReports_Button.FlatAppearance.BorderSize = 0;
            deleteYearlyReports_Button.Click += DeleteYearlyReports_Button_Click;

            month_TextBox = new TextBox
            {
                Location = new Point(20, 80),
                Size = new Size(100, 20),
                PlaceholderText = "New Month"
            };

            year_TextBox = new TextBox
            {
                Location = new Point(140, 80),
                Size = new Size(100, 20),
                PlaceholderText = "New Year"
            };

            totalSales_TextBox = new TextBox
            {
                Location = new Point(20, 120),
                Size = new Size(220, 20),
                PlaceholderText = "Total New Sales"
            };

            sales_Chart = new Chart
            {
                Location = new Point(20, 160),
                Size = new Size(700, 450),
                BorderlineColor = Color.Black,
                BorderlineWidth = 2,
                BorderlineDashStyle = ChartDashStyle.Solid,
            };
            sales_Chart.ChartAreas.Add(new ChartArea());
            sales_Chart.Series.Add(new Series("TotalSales") { ChartType = SeriesChartType.Bar, Color = GlobalProperties.PrimaryColor });

            Controls.Add(AddYearlyReport_Button);
            Controls.Add(viewYearlyReports_Button);
            Controls.Add(deleteYearlyReports_Button);
            Controls.Add(month_TextBox);
            Controls.Add(year_TextBox);
            Controls.Add(totalSales_TextBox);
            Controls.Add(sales_Chart);

            UpdateControlLocations();
        }

        private void UpdateControlLocations()
        {
            int controlTopMargin = 30;
            int controlSpacing = 20;

            AddYearlyReport_Button.Location = new Point(287, controlTopMargin);
            viewYearlyReports_Button.Location = new Point(40, controlTopMargin);
            deleteYearlyReports_Button.Location = new Point(534, controlTopMargin);

            month_TextBox.Location = new Point(40, AddYearlyReport_Button.Bottom + controlSpacing);
            year_TextBox.Location = new Point(month_TextBox.Right + 20, AddYearlyReport_Button.Bottom + controlSpacing);

            totalSales_TextBox.Location = new Point(40, month_TextBox.Bottom + controlSpacing);
            sales_Chart.Location = new Point(40, totalSales_TextBox.Bottom + controlSpacing);
        }

        private void AddYearlyReport_Button_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(month_TextBox.Text) && !string.IsNullOrEmpty(year_TextBox.Text) && !string.IsNullOrEmpty(totalSales_TextBox.Text))
            {
                // Validate input
                if (!int.TryParse(year_TextBox.Text, out int parsedYear) ||
                    !double.TryParse(totalSales_TextBox.Text, out double parsedTotalSales) ||
                    parsedTotalSales < 0) 
                {
                    MessageBox.Show("Invalid input. Please enter valid numeric values for Year and Total Sales.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                // if in shortened, fix
                else if (GlobalProperties.ShortenedMonths.Contains(month_TextBox.Text))
                {
                    month_TextBox.Text = GlobalProperties.Months[Array.IndexOf(GlobalProperties.ShortenedMonths, month_TextBox.Text)];
                }
                else if (!GlobalProperties.Months.Contains(month_TextBox.Text) && !GlobalProperties.ShortenedMonths.Contains(month_TextBox.Text))
                {
                    MessageBox.Show("Invalid input. Please enter a valid month.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Create a new sales report
                YearlyReport newYearlyReport = new YearlyReport
                {
                    Month = month_TextBox.Text,
                    Year = parsedYear,
                    TotalSales = parsedTotalSales,
                };

                // Add the new sales report to the list
                YearlyReports.Add(newYearlyReport);

                // Save the updated sales reports list
                SaveYearlyReports();

                // Display a message box with the generated sales report
                MessageBox.Show($"Sales Report for {newYearlyReport.Month} {newYearlyReport.Year}:\n" +
                                $"Total Sales: {newYearlyReport.TotalSales:C}", "Generated Sales Report");
            }
            else
            {
                MessageBox.Show("Please enter a valid month, year, and total sales amount.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Update the chart
            UpdateSales_Chart();
        }

        private void UpdateSales_Chart()
        {
            // Clear previous data
            sales_Chart.Series["TotalSales"].Points.Clear();

            // Sort the reports by year and month
            var monthDict = new Dictionary<string, int>
            {
                { "January", 1 }, { "February", 2 }, { "March", 3 }, { "April", 4 },
                { "May", 5 }, { "June", 6 }, { "July", 7 }, { "August", 8 },
                { "September", 9 }, { "October", 10 }, { "November", 11 }, { "December", 12 }
            };

            var sortedReports = YearlyReports
            .OrderByDescending(report => report.Year)
            .ThenByDescending(report => monthDict[report.Month])
            .ToList();

            // Add new data
            int previousYear = 0;
            foreach (var report in sortedReports)
            {
                string label = report.Year != previousYear ? $"{report.Month} {report.Year}" : report.Month;
                sales_Chart.Series["TotalSales"].Points.AddXY(label, report.TotalSales);
                previousYear = report.Year;
            }

            // Set the interval for the Y-axis labels
            sales_Chart.ChartAreas[0].AxisX.Interval = 1;

            // Remove gridlines
            sales_Chart.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
            sales_Chart.ChartAreas[0].AxisY.MajorGrid.Enabled = false;

            // Set the chart title
            sales_Chart.Titles.Clear();
            sales_Chart.Titles.Add("Total Sales by Month");
        }

        private void SaveYearlyReports()
        {
            string json = JsonSerializer.Serialize(YearlyReports);
            File.WriteAllText(YearlyReportsFilePath, json);
        }

        private void LoadYearlyReports()
        {
            if (File.Exists(YearlyReportsFilePath))
            {
                string json = File.ReadAllText(YearlyReportsFilePath);
                YearlyReports = JsonSerializer.Deserialize<List<YearlyReport>>(json);
            }
        }

        private void ViewYearlyReports_Button_Click(object sender, EventArgs e)
        {
            // Create a new form to display sales reports
            using (var YearlyReportForm = new YearlyReportViewerForm(YearlyReports))
            {
                YearlyReportForm.ShowDialog();
            }
        }

        private void DeleteYearlyReports_Button_Click(object sender, EventArgs e)
        {
            // Create a new form to prompt user for deletion
            var deleteYearlyReportForm = new DeleteYearlyReportForm(YearlyReports)
            {
                BackColor = GlobalProperties.BackgroundColor,
                ForeColor = Color.Black,
            };

            DialogResult result = deleteYearlyReportForm.ShowDialog();

            // Check if the user clicked OK on the dialog
            if (result == DialogResult.OK)
            {
                // Get the month and year to delete from the form
                string monthToDelete = deleteYearlyReportForm.GetMonthToDelete();
                int yearToDelete = deleteYearlyReportForm.GetYearToDelete();

                // Check if there are records to delete
                var reportsToDelete = YearlyReports
                    .Where(report => report.Month.Equals(monthToDelete, StringComparison.OrdinalIgnoreCase) && report.Year == yearToDelete)
                    .ToList();

                if (reportsToDelete.Any())
                {
                    // Remove the matching records
                    foreach (var reportToDelete in reportsToDelete)
                    {
                        YearlyReports.Remove(reportToDelete);
                    }

                    // Update the chart
                    UpdateSales_Chart();

                    // Save the updated sales reports list
                    SaveYearlyReports();
                }
                else
                {
                    MessageBox.Show($"No matching records found for {monthToDelete} {yearToDelete}.", "No Records Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
    }

    class YearlyReportViewerForm : Form
    {
        private List<YearlyReport> YearlyReports;

        public YearlyReportViewerForm(List<YearlyReport> YearlyReports)
        {
            this.YearlyReports = YearlyReports;

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            Text = "Sales Reports Viewer";
            Size = new Size(380, 600);
            BackColor = GlobalProperties.BackgroundColor;

            // Create a DataGridView to display sales reports
            var salesDataGridView = new DataGridView
            {
                Parent = this,
                Dock = DockStyle.Fill,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleVertical,

                RowHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.Blue,
                },
                // Set the default cell style for column headers
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.Blue,
                }
            };

            // Create a new cell style
            var cellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.LightGray,
                Font = new Font(salesDataGridView.Font, FontStyle.Bold)
            };

            // Add columns to DataGridView
            salesDataGridView.Columns.Add("Month", "Month");
            salesDataGridView.Columns.Add("Year", "Year");
            salesDataGridView.Columns.Add("TotalSales", "Total Sales");

            // Apply the cell style to the columns
            salesDataGridView.Columns["Month"].DefaultCellStyle = cellStyle;
            salesDataGridView.Columns["Year"].DefaultCellStyle = cellStyle;
            salesDataGridView.Columns["TotalSales"].DefaultCellStyle = cellStyle;

            // Populate DataGridView with sales report data
            foreach (var report in YearlyReports)
            {
                salesDataGridView.Rows.Add(report.Month, report.Year, report.TotalSales.ToString("C"));
            }
        }
    }


    class DeleteYearlyReportForm : Form
    {
        private List<YearlyReport> YearlyReports;

        private ComboBox month_ComboBox;
        private ComboBox year_ComboBox;
        private Button delete_Button;
        private Button cancel_Button;

        public DeleteYearlyReportForm(List<YearlyReport> YearlyReports)
        {
            this.YearlyReports = YearlyReports;

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            Text = "Delete Sales Report";
            Size = new Size(300, 150);

            // Create and configure month combo box
            month_ComboBox = new ComboBox
            {
                Location = new Point(30, 20),
                Size = new Size(100, 20),
                DropDownStyle = ComboBoxStyle.DropDownList,
                DataSource = YearlyReports.Select(report => report.Month).Distinct().ToList(),
                MaxDropDownItems = 7
            };

            // Create and configure year combo box
            year_ComboBox = new ComboBox
            {
                Location = new Point(155, 20),
                Size = new Size(100, 20),
                DropDownStyle = ComboBoxStyle.DropDownList,
                DataSource = YearlyReports.Select(report => report.Year).Distinct().ToList(),
                MaxDropDownItems = 7
            };

            // Create and configure delete button
            delete_Button = new Button
            {
                BackColor = GlobalProperties.PrimaryColor,
                ForeColor = Color.Black,
                Location = new Point(52, 60),
                Size = new Size(80, 25),
                Text = "Delete",
                DialogResult = DialogResult.OK,
                FlatStyle = FlatStyle.Flat
            };
            delete_Button.FlatAppearance.BorderSize = 0;

            // Wire up event handler for delete button click
            delete_Button.Click += delete_Button_Click;

            // Create and configure cancel button
            cancel_Button = new Button
            {
                BackColor = GlobalProperties.SecondaryColor,
                ForeColor = Color.Black,
                Location = new Point(152, 60),
                Size = new Size(80, 25),
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                FlatStyle = FlatStyle.Flat
            };
            cancel_Button.FlatAppearance.BorderSize = 0;

            // Wire up event handler for cancel button click
            cancel_Button.Click += cancel_Button_Click;

            // Add controls to form
            Controls.Add(month_ComboBox);
            Controls.Add(year_ComboBox);
            Controls.Add(cancel_Button);
            Controls.Add(delete_Button);
        }

        private void delete_Button_Click(object sender, EventArgs e)
        {
            // Perform any additional validation checks before closing the form
            if (string.IsNullOrEmpty(GetMonthToDelete()) || GetYearToDelete() == 0)
            {
                MessageBox.Show("Please select a valid month and year.", "Invalid Selection", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.None;
            }
        }

        private void cancel_Button_Click(object sender, EventArgs e)
        {
            // Close the form
            Close();
        }

        public string GetMonthToDelete()
        {
            return month_ComboBox.SelectedItem?.ToString();
        }

        public int GetYearToDelete()
        {
            return year_ComboBox.SelectedItem != null ? (int)year_ComboBox.SelectedItem : 0;
        }
    }
}