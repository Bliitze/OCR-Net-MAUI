using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace OCRVisionTest
{
    public partial class MainPage : ContentPage
    {
        int count = 0;
        public string strlines = "";
        public Stream sourceStream = null;
        // Add your Computer Vision subscription key and endpoint
        static string subscriptionKey = "PASTE_YOUR_COMPUTER_VISION_SUBSCRIPTION_KEY_HERE";
        static string endpoint = "PASTE_YOUR_COMPUTER_VISION_ENDPOINT_HERE";
        

        public string READ_TEXT_URL_IMAGE = "https://raw.githubusercontent.com/Bliitze/VIN-Pate-Image/main/1200x0.jpg";
        public MainPage()
        {
            InitializeComponent();
            TxtUrl.Text = READ_TEXT_URL_IMAGE;
        }

        private async void OnCounterClicked(object sender, EventArgs e)
        {
            try
            {
                //TakePhoto();
                
                READ_TEXT_URL_IMAGE = TxtUrl.Text;
                image1.Source = READ_TEXT_URL_IMAGE;
                resultlb.Text = "Analyzing...";
                
                // Create a client
                ComputerVisionClient client = Authenticate(endpoint, subscriptionKey);

                // Extract text (OCR) from a URL image using the Read API
                await ReadFileUrl(client, READ_TEXT_URL_IMAGE);//READ_TEXT_URL_IMAGE
            }
            catch(Exception ex)
            {
               await DisplayAlert("Error", ex.Message, "ok");
            }
        }
        public async void TakePhoto()
        {
            if (MediaPicker.Default.IsCaptureSupported)
            {
                FileResult photo = await MediaPicker.Default.CapturePhotoAsync();

                if (photo != null)
                {
                    // save the file into local storage
                    string localFilePath = Path.Combine(FileSystem.CacheDirectory, photo.FileName);
                    READ_TEXT_URL_IMAGE = localFilePath;
                    sourceStream = await photo.OpenReadAsync();
                    using FileStream localFileStream = File.OpenWrite(localFilePath);

                    await sourceStream.CopyToAsync(localFileStream);
                    image1.Source = localFilePath;
                   
                }

            }
        }

        public  ComputerVisionClient Authenticate(string endpoint, string key)
        {
            ComputerVisionClient client =
              new ComputerVisionClient(new ApiKeyServiceClientCredentials(key))
              { Endpoint = endpoint };
            return client;
        }

        public  async Task ReadFileUrl(ComputerVisionClient client,string urlFile )//string urlFile
        {
           

            // Read text from URL
            var textHeaders = await client.ReadAsync(urlFile);

            // After the request, get the operation location (operation ID)
            string operationLocation = textHeaders.OperationLocation;
            //Thread.Sleep(2000);

            // Retrieve the URI where the extracted text will be stored from the Operation-Location header.
            // We only need the ID and not the full URL
            const int numberOfCharsInOperationId = 36;
            string operationId = operationLocation.Substring(operationLocation.Length - numberOfCharsInOperationId);

            // Extract the text
            ReadOperationResult results;
            
            do
            {
                results = await client.GetReadResultAsync(Guid.Parse(operationId));
            }
            while ((results.Status == OperationStatusCodes.Running ||
                results.Status == OperationStatusCodes.NotStarted));

            // Display the found text.
           // Console.WriteLine();
            var textUrlFileResults = results.AnalyzeResult.ReadResults;
            
            foreach (ReadResult page in textUrlFileResults)
            {
                foreach (Line line in page.Lines)
                {
                    //Console.WriteLine(line.Text);
                    strlines = strlines + line.Text;
                }
            }
            resultlb.Text = "Result: " + strlines;
           // Console.WriteLine();
        }

    }
}