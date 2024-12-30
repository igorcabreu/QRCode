using Npgsql;
using QRCoder;
using System.Collections.ObjectModel;

namespace QRCode
{
    public partial class MainPage : ContentPage
    {
        public ObservableCollection<QRCodeItem> QRCodeItems { get; set; }
        private readonly NpgsqlConnection _connection;

        public MainPage()
        {
            InitializeComponent();
            QRCodeItems = new ObservableCollection<QRCodeItem>();
            qrCodeList.ItemsSource = QRCodeItems;

            string connectionString = "host=localhost; port=5432; Database=QRCode;Username=postgres; password=teste123;";
            _connection = new NpgsqlConnection(connectionString);

            _connection.Open();

            CreateTable();
            LoadQRCodeItems();
        }

        private void CreateTable()
        {
            string createTaleQuery =
                @"
                    CREATE TABLE IF NOT EXISTS QRCodeItem (
                        Id SERIAL PRIMARY KEY
                        QrCodeBase64 TEXT NOT NULL
                );
                ";
            using var cmd = new NpgsqlCommand(createTaleQuery, _connection);
            cmd.ExecuteNonQuery();
        }
        private void LoadQRCodeItems()
        {
            string selectQuery = "SELECT Id, QrCodeBase64 FROM QRCodeItem";
            using var cmd = new NpgsqlCommand(selectQuery, _connection);
            using var reader = cmd.ExecuteReader();

            QRCodeItems.Clear();
            while (reader.Read())
            {
                var qrCodeBase64 = reader.GetString(1);
                var qrCodeItem = new QRCodeItem
                {
                    Id = reader.GetInt32(0),
                    QrCodeImg = ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(qrCodeBase64))),
                    QrCodeBase64 = qrCodeBase64
                };
                QRCodeItems.Add(qrCodeItem);
            }

        }
        
        private void GenerateQR_Clicked(object sender, EventArgs e)
        {
            string name = nameEntry.Text;
            string local = localEntry.Text;
            string qr = $"{name} + {local}";

            QRCodeGenerator qrCodeGenerator = new QRCodeGenerator();
            QRCodeData qRCodeData = qrCodeGenerator.CreateQrCode(qr, QRCodeGenerator.ECCLevel.L);
            PngByteQRCode qRCode = new PngByteQRCode(qRCodeData);
            byte[] qrCodeBbytes = qRCode.GetGraphic(20);

            if (qrCodeBbytes == null || qrCodeBbytes.Length == 0)
            {
                DisplayAlert("Erro", "Adicione Valores", "Ok");
                return;
            }

            string qrCodeBase64 = Convert.ToBase64String(qrCodeBbytes);

            string insertQuery = "INSERT INTO QRCodeItem (QrCodeBase64) VALUES (@QrCodeBase64) RETURNING Id";
            using var cmd = new NpgsqlCommand(insertQuery, _connection);
            cmd.Parameters.AddWithValue("@QrCodeBase64", qrCodeBase64);
            int id = (int)cmd.ExecuteScalar();

            var qrCodeItem = new QRCodeItem
            {
                Id = id,
                QrCodeImg = ImageSource.FromStream(() => new MemoryStream(qrCodeBbytes)),
                QrCodeBase64 = qrCodeBase64
            };

            QRCodeItems.Add(qrCodeItem);
        }

        private async void ViewQR_Clicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var qrCodeItem = button.BindingContext as QRCodeItem;

            if(qrCodeItem != null)
            {
                await Navigation.PushAsync(new NewPage1(qrCodeItem.QrCodeImg));
            }

        }

        private void DeleteQR_Cliked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var qrCodeItem = button.BindingContext as QRCodeItem;

            if (qrCodeItem != null)
            {
                string deleteQuery = "DELETE FROM QRCodeItem WHERE Id = @Id;";
                using var cmd = new NpgsqlCommand(deleteQuery, _connection);
                cmd.Parameters.AddWithValue("@Id", qrCodeItem.Id);
                cmd.ExecuteNonQuery();

                QRCodeItems.Remove(qrCodeItem);
            }
        }
    }

}
