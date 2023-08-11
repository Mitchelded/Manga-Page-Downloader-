using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenQA.Selenium;

namespace ImageDownloader
{
    class ImageDownloader
    {
        // Main entry point of the program
        static async Task Main(string[] args)
        {
            try
            {
                int[] command = {1,2,3,4};
                int choose = 0;
                Downloader downloader = new Downloader();
                Console.Write("Choose a site for downloading character images\n" +
                              "1) funbe274.com\n" +
                              "2) mangaread.org\n" +
                              "3) Other\n" +
                              "4) Clear saved path\n");  // Added an option to clear the path

                while (true)
                {
                    Console.Write("Input number: ");
                    if (!int.TryParse(Console.ReadLine(), out choose))
                    {
                        Console.WriteLine("Incorrect input");
                        continue;
                    }

                    bool inputMatchesCommand = false;

                    for (int i = 0; i < command.Length; i++)
                    {
                        if (choose == command[i])
                        {
                            inputMatchesCommand = true;
                            break;
                        }
                    }

                    if (inputMatchesCommand)
                    {
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Incorrect input");
                    }
                }

                string url;

                if (choose == 1)
                {
                    url = "https://funbe274.com/";
                    string targetSubstring = "data/file/wtoon/";
                    await downloader.DownloadImagesAsync(url, targetSubstring);
                    Console.WriteLine("Image download completed!");
                }
                else if (choose == 2)
                {
                    url = "https://www.mangaread.org/";
                    string targetSubstring = "wp-content/uploads/WP-manga/data";
                    await downloader.DownloadImagesAsync(url, targetSubstring);
                    Console.WriteLine("Image download completed!");
                }

                else if (choose == 3)
                {
                    Config config = Config.LoadConfig();
                    config.PrintSavedSites();

                    Console.WriteLine("Choose an option:");
                    Console.WriteLine("1) Select a saved site");
                    Console.WriteLine("2) Enter a new URL");
                    Console.WriteLine("3) Clear a saved site");

                    int option = int.Parse(Console.ReadLine());

                    string inputUrl = "";
                    string targetSubstring = "";

                    switch (option)
                    {
                        case 1:
                            Console.Write("Select a saved site by entering its number: ");
                            int savedSiteNumber = int.Parse(Console.ReadLine());

                            if (savedSiteNumber >= 1 && savedSiteNumber <= config.UrlsAndSubstrings.Count)
                            {
                                inputUrl = config.UrlsAndSubstrings.Keys.ElementAt(savedSiteNumber - 1);
                                targetSubstring = config.UrlsAndSubstrings[inputUrl];
                            }
                            else
                            {
                                Console.WriteLine("Invalid option.");
                                return;
                            }
                            break;

                        case 2:
                            Console.Write("Input your new URL: ");
                            inputUrl = Console.ReadLine();

                            Console.WriteLine("Do you know the part of the link that leads directly to the image " +
                                              "(for example, if you have a link like https://funbe274.com/data/file/wtoon/16584682143624.jpeg then type /data/file/wtoon/)," +
                                              " if you don't know, just press enter");
                            targetSubstring = Console.ReadLine();

                            if (!string.IsNullOrWhiteSpace(inputUrl) && !string.IsNullOrWhiteSpace(targetSubstring))
                            {
                                config.UrlsAndSubstrings[inputUrl] = targetSubstring;
                                Config.SaveConfig(config);
                            }
                            break;

                        case 3:
                            Console.Write("Enter the URL to clear: ");
                            string urlToClear = Console.ReadLine();
                            if (config.UrlsAndSubstrings.ContainsKey(urlToClear))
                            {
                                config.UrlsAndSubstrings.Remove(urlToClear);
                                Config.SaveConfig(config);
                                Console.WriteLine($"Cleared URL: {urlToClear}");
                            }
                            else
                            {
                                Console.WriteLine($"URL '{urlToClear}' not found in the saved sites.");
                            }
                            return;
                    }

                    await downloader.DownloadImagesAsync(inputUrl, targetSubstring);

                    Console.WriteLine("Image download completed!");
                }


                else if (choose == 4)  // Added "Clear saved path" selection processing
                {
                    Config config = Config.LoadConfig();
                    config.SavePath = null;  // Clear the saved path
                    Config.SaveConfig(config);      // Save the modified configuration
                    Console.WriteLine("Saved path cleared!");
                }


                
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
