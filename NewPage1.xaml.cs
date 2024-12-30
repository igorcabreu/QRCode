namespace QRCode;

public partial class NewPage1 : ContentPage
{
	public NewPage1(ImageSource qrCodeImg)
	{
		InitializeComponent();
		qrCodeImage.Source = qrCodeImg;
	}

    private async void BackBtn_Clicked(object sender, EventArgs e)
    {
		await Navigation.PopAsync();
    }
}