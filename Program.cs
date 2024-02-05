using Dashboard;
using System.Configuration;

namespace Properties
{
    public static class GlobalProperties
    {
        private static readonly string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        public static Color PrimaryColor { get; } = Color.FromArgb(181, 134, 159);
        public static Color SecondaryColor { get; } = Color.FromArgb(142, 151, 193);
        public static Color BackgroundColor { get; } = Color.FromArgb(220, 214, 247);
        public static string[] ShortenedMonths { get; } = new string[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sept", "Nov", "Dec"};

        public static string[] Months { get; } = new string[] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "November", "December"};

        public static string LogoFilePath 
        { 
            get 
            {
                var logoFilePath = ConfigurationManager.AppSettings["LogoFilePath"];
                return Path.Combine(baseDirectory, logoFilePath ?? "assets\\logo.png");
            }
        }

        public static string UsersJSONFilePath 
        { 
            get 
            {
                var usersJSONFilePath = ConfigurationManager.AppSettings["UsersJSONFilePath"];
                return Path.GetFullPath(Path.Combine(baseDirectory, "..\\..\\..\\", usersJSONFilePath ?? "assets\\users.json"));
            }
        }

        public static string MarketJSONFilePath
        {
            get
            {
                var marketJSONFilePath = ConfigurationManager.AppSettings["marketJSONFilePath"];
                return Path.GetFullPath(Path.Combine(baseDirectory, "..\\..\\..\\", marketJSONFilePath ?? "assets\\market.json"));
            }
        }

        public static string ReportsJSONFilePath
        {
            get
            {
                var reportsJSONFilePath = ConfigurationManager.AppSettings["reportsJSONFilePath"];
                return Path.GetFullPath(Path.Combine(baseDirectory, "..\\..\\..\\", reportsJSONFilePath ?? "assets\\sales.json"));
            }
        }

        public static string ItemsJSONFilePath
        {
            get
            {
                var itemsJSONFilePath = ConfigurationManager.AppSettings["itemsJSONFilePath"];
                return Path.GetFullPath(Path.Combine(baseDirectory, "..\\..\\..\\", itemsJSONFilePath ?? "assets\\items.json"));
            }
        }

        public static string CustomersJSONFilePath
        {
            get
            {
                var customersJSONFilePath = ConfigurationManager.AppSettings["customersJSONFilePath"];
                return Path.GetFullPath(Path.Combine(baseDirectory, "..\\..\\..\\", customersJSONFilePath ?? "assets\\customers.json"));
            }
        }

        public static string TransactionsJSONFilePath
        {
            get
            {
                var TransactionsJSONFilePath = ConfigurationManager.AppSettings["TransactionsJSONFilePath"];
                return Path.GetFullPath(Path.Combine(baseDirectory, "..\\..\\..\\", TransactionsJSONFilePath ?? "assets\\transactions.json"));
            }
        }

        public static string FeedbackJSONFilePath
        {
            get
            {
                var feedbackJSONFilePath = ConfigurationManager.AppSettings["feedbackJSONFilePath"];
                return Path.GetFullPath(Path.Combine(baseDirectory, "..\\..\\..\\", feedbackJSONFilePath ?? "assets\\feedback.json"));
            }
        
        }
    }
}

static class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        LoginForm.RunLoginForm();
    }

}