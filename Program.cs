using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Newtonsoft.Json;

namespace ImageDownloader
{
    class ImageDownloader
    {
        private static string ConfigFileName = "config.json";

        public class Config
        {
            public string SavePath { get; set; }
        }

        static Config LoadConfig()
        {
            if (File.Exists(ConfigFileName))
            {
                string configJson = File.ReadAllText(ConfigFileName);
                return JsonConvert.DeserializeObject<Config>(configJson);
            }
            return new Config();
        }
        static void SaveConfig(Config config)
        {
            string configJson = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(ConfigFileName, configJson);
        }

        // Asynchronous method to download images from a webpage
        private static async Task DownloadImagesAsync(string url, string targetSubstring = null)
        {
            // Initialize a new ChromeDriver instance
            using (var driver = new ChromeDriver())
            {
                // Navigate to the provided URL using the driver
                driver.Navigate().GoToUrl(url);

                // Prompt user to start image download
                Console.WriteLine("Press any key to start image download...");
                Console.ReadKey();
                Console.WriteLine("Downloading images...");

                string folderPath = null;

                // Check if the URL contains specific domain to determine image saving logic
                if (url.Contains("funbe274.com"))
                {
                    // Find the folder name element on the webpage
                    var folderNameElement = driver.FindElement(By.CssSelector("#thema_wrapper > div > div > div > div.view-wrap > h1"));
                    string folderName = CleanFileName(folderNameElement.Text); // Clean folder name

                    Config config = LoadConfig(); // Загрузить конфигурацию

                    if (string.IsNullOrEmpty(config.SavePath))
                    {
                        Console.Write("Enter the default save path: ");
                        config.SavePath = Console.ReadLine();
                        SaveConfig(config); // Сохранить конфигурацию
                    }

                    string baseFolderPath = config.SavePath; // Base folder path
                    folderPath = Path.Combine(baseFolderPath, folderName); // Combine paths
                    Directory.CreateDirectory(folderPath); // Create directory if it doesn't exist
                }
                else
                {
                    string currentDateAndTime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                    string baseFolderPath = "C:\\Users\\trety\\Downloads\\qq"; // Path to the base folder
                    string folderName = $"Other_{currentDateAndTime}"; // Generate folder name with date and time
                    folderPath = Path.Combine(baseFolderPath, folderName); // Combine paths
                    Directory.CreateDirectory(folderPath); // Create directory if it doesn't exist
                }

                // Find all image elements on the webpage
                var imgElements = driver.FindElements(By.TagName("img"));

                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri(url); // Set the base address for relative URLs

                    int imageNumber = 1;
                    foreach (var imgElement in imgElements)
                    {
                        var src = imgElement.GetAttribute("src");
                        if (!string.IsNullOrEmpty(src) && (string.IsNullOrEmpty(targetSubstring) || src.Contains(targetSubstring)))
                        {
                            Console.WriteLine($"Downloading image {imageNumber}...");

                            var fileName = Path.GetFileName(src);
                            var filePath = Path.Combine(folderPath, $"{imageNumber:D3}_{fileName}");

                            var absoluteUri = new Uri(httpClient.BaseAddress, src);
                            var imageBytes = await httpClient.GetByteArrayAsync(absoluteUri);
                            File.WriteAllBytes(filePath, imageBytes);

                            Console.WriteLine($"Image {imageNumber} saved in: {filePath}");
                            imageNumber++;
                        }
                    }
                }
            }
        }

        // Method to clean invalid characters from file names
        static string CleanFileName(string fileName)
        {
            foreach (char invalidChar in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(invalidChar.ToString(), "");
            }
            return fileName;
        }

        // Main entry point of the program
        static async Task Main(string[] args)
        {
            try
            {
                Console.Write("Choose a site for downloading character images\n" +
                              "1) funbe274.com\n" +
                              "2) mangaread.org\n" +
                              "3) Other\n" +
                              "4) Clear saved path\n");  // Добавили вариант для очистки пути

                int choose;
                while (true)
                {
                    Console.Write("Input number: ");
                    if (!int.TryParse(Console.ReadLine(), out choose) || (choose != 1 && choose != 2))
                    {
                        Console.WriteLine("Incorrect input");
                    }
                    else
                    {
                        break;
                    }
                }

                string url;

                if (choose == 1)
                {
                    url = "https://funbe274.com/";
                    string targetSubstring = "data/file/wtoon/";
                    await DownloadImagesAsync(url, targetSubstring);
                }
                else if (choose == 2)
                {
                    url = "https://www.mangaread.org/";
                    string targetSubstring = "wp-content/uploads/WP-manga/data";
                    await DownloadImagesAsync(url, targetSubstring);
                }
                else if (choose == 3)
                {
                    Console.Write("Input your link: ");
                    url = Console.ReadLine();
                    Console.WriteLine("Do you know the part of the link that leads directly to the image " +
                                      "(for example, if you have a link like https://funbe274.com/data/file/wtoon/16584682143624.jpeg then type /data/file/wtoon/)," +
                                      " if you don't know, just press enter");
                    string targetSubstring = Console.ReadLine();

                    await DownloadImagesAsync(url, targetSubstring);
                }
                else if (choose == 4)  // Добавили обработку выбора "Clear saved path"
                {
                    Config config = LoadConfig();
                    config.SavePath = null;  // Очистить сохраненный путь
                    SaveConfig(config);      // Сохранить измененную конфигурацию
                    Console.WriteLine("Saved path cleared!");
                }


                Console.WriteLine("Image download completed!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
}
