using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageDownloader
{
    public class Downloader
    {
        // Asynchronous method to download images from a webpage
        public async Task DownloadImagesAsync(string url, string targetSubstring = null)
        {


            // Initialize a new ChromeDriver instance
            using (var driver = new ChromeDriver())
            {

                Config config = Config.LoadConfig();

                // Navigating to the provided URL using the driver
                driver.Navigate().GoToUrl(url);

                bool shouldScroll = true; // Переменная для определения, нужно ли выполнять прокрутку

                if (!config.ShouldAutoLoad(url))
                {
                    Console.WriteLine("Do you want to automatically upload and download images from this site in the future? (Yes/No)");
                    string userResponse = Console.ReadLine().Trim().ToLower();

                    config.AutoLoadSitePreferences[url] = userResponse == "yes";
                    Config.SaveConfig(config);

                    if (config.ShouldAutoLoad(url))
                    {
                        Console.WriteLine("The site has been added to the automatic download list.");
                    }
                    else
                    {
                        Console.WriteLine("The site will not load automatically.");
                        shouldScroll = false; // If the user does not want to download automatically, scrolling is not performed
                    }
                }

                if (shouldScroll)
                {
                    Console.WriteLine("Press any key to start uploading images...");
                    Console.ReadKey();
                    Console.WriteLine("Uploading images...");

                    // Creating a CancellationTokenSource for possible scroll cancellation
                    var cancellationTokenSource = new CancellationTokenSource();

                    // Start scrolling down the page to load images
                    var scrollingTask = SmoothScrollPageWithDelayAsync(driver, config, cancellationTokenSource.Token);

                    // Prompt the user to stop scrolling
                    Console.WriteLine("Нажмите любую клавишу, чтобы остановить загрузку изображений...");
                    Console.ReadKey();

                    // Canceling a scroll task
                    cancellationTokenSource.Cancel();

                    try
                    {
                        // Waiting for the scroll task to complete or cancel
                        await scrollingTask;
                    }
                    catch (OperationCanceledException)
                    {
                        Console.WriteLine("The image upload has been canceled.");
                    }
                }
                else
                {
                    Console.WriteLine("Scrolling and loading images have been canceled.");
                }



                string folderPath = null;

                // Check if the URL contains specific domain to determine image saving logic
                if (url.Contains("funbe274.com"))
                {
                    // Find the folder name element on the webpage
                    var folderNameElement = driver.FindElement(By.CssSelector("#thema_wrapper > div > div > div > div.view-wrap > h1"));
                    string folderName = CleanFileName(folderNameElement.Text); // Clean folder name



                    if (string.IsNullOrEmpty(config.SavePath))
                    {
                        Console.Write("Enter the default save path: ");
                        config.SavePath = Console.ReadLine();
                        Config.SaveConfig(config); // Save Configuration
                    }

                    string baseFolderPath = config.SavePath; // Base folder path
                    folderPath = Path.Combine(baseFolderPath, folderName); // Combine paths
                    Directory.CreateDirectory(folderPath); // Create directory if it doesn't exist
                }

                // Check if the URL contains specific domain to determine image saving logic
                else if (url.Contains("mangaread.org"))
                {
                    // Find the folder name element on the webpage
                    var folderNameElement = driver.FindElement(By.CssSelector("#chapter-heading"));
                    string folderName = CleanFileName(folderNameElement.Text); // Clean folder name



                    if (string.IsNullOrEmpty(config.SavePath))
                    {
                        Console.Write("Enter the default save path: ");
                        config.SavePath = Console.ReadLine();
                        Config.SaveConfig(config); // Save Configuration
                    }

                    string baseFolderPath = config.SavePath; // Base folder path
                    folderPath = Path.Combine(baseFolderPath, folderName); // Combine paths
                    Directory.CreateDirectory(folderPath); // Create directory if it doesn't exist
                }


                else
                {
                    string currentDateAndTime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");




                    if (string.IsNullOrEmpty(config.SavePath))
                    {
                        Console.Write("Enter the default save path: ");
                        config.SavePath = Console.ReadLine();
                        Config.SaveConfig(config); // Save Configuration
                    }

                    string baseFolderPath = config.SavePath; // Base folder path
                    folderPath = Path.Combine(baseFolderPath, currentDateAndTime); // Combine paths
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

        // CleanFileName method
        private static string CleanFileName(string fileName)
        {
            foreach (char invalidChar in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(invalidChar.ToString(), "");
            }
            return fileName;
        }




        private async Task SmoothScrollPageWithDelayAsync(IWebDriver driver, Config config, CancellationToken cancellationToken)
        {
            var jsExecutor = (IJavaScriptExecutor)driver;
            var scrollHeight = (long)jsExecutor.ExecuteScript("return document.body.scrollHeight");
            var windowHeight = (long)jsExecutor.ExecuteScript("return window.innerHeight");
            var scrollStep = windowHeight / 2;

            while (scrollHeight > windowHeight)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                jsExecutor.ExecuteScript($"window.scrollBy(0, {scrollStep});");
                await Task.Delay(config.DelayBetweenScrollsMs, cancellationToken);
                scrollHeight -= scrollStep / config.ScrollSpeedDivisor;
            }
        }
    }
}
