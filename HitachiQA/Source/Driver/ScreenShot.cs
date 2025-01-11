using Microsoft.Playwright;
using OpenQA.Selenium;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;

namespace HitachiQA
{
    public class ScreenShot
    {
        IWebDriver? Driver;
        IPage? Page;
        TestContext TestContext;
        public ScreenShot(IWebDriver driver, TestContext testContext)
        {
            Driver = driver;
            TestContext = testContext;
        }
        public ScreenShot(IPage page, TestContext testContext)
        {
            Page = page;
            TestContext = testContext;
        }
        public void Info(String? filename = null) => Take(Severity.INFO, filename!);
        public void Debug(String? filename = null) => Take(Severity.DEBUG, filename!);
        public void Warn(String? filename = null) => Take(Severity.WARN, filename!);
        public void Error(String? filename = null) => Take(Severity.ERROR, filename!);
        public void Critical(String? filename = null) => Take(Severity.CRITICAL, filename!);


        /// <summary>
        /// Take screenshot, by defualt the filename will be Severity_CurrentScenario_currentDateTime unless otherwise specified
        /// </summary>
        public void Take(Severity severity, String? filename = null)
        {
            var currentSev = Severity.parseLevel(Main.Configuration.GetSection("Logging").GetSection("LogLevel")["Default"]).Level;

            if (currentSev == 0)
            {
                return;
            }
            else if (severity.Level <= currentSev)
            {


                FileNameBase = $"{severity.Name}_{FileNameBase}";

                if (!Directory.Exists(ArtifactDirectory)) { Directory.CreateDirectory(ArtifactDirectory); }

                string pageSource = Driver == null ? Page.ContentAsync().Result : Driver.PageSource;
                string sourceFilePath = Path.Combine(ArtifactDirectory, FileNameBase + "_source.html");
                File.WriteAllText(sourceFilePath, pageSource, Encoding.UTF8);
                this.TestContext.AddResultFile(sourceFilePath);
                Console.WriteLine($"\nPage Source: {new Uri(sourceFilePath)}\n");



                string screenshotFilePath = Path.Combine(ArtifactDirectory, FileNameBase + "_screenshot.png");

                if (SaveScreenshot(screenshotFilePath))
                {


                    try
                    {
                        var screenshotBitMap = (Bitmap)Image.FromFile(screenshotFilePath);
                        var resultBitMap = new Bitmap(screenshotBitMap.Width, screenshotBitMap.Height + 30);

                        using (Graphics g = Graphics.FromImage(resultBitMap))
                        {

                            g.DrawString(GetCurrentURL(), new Font("Arial", 15), Brushes.Red, new PointF(0, 0));
                            g.DrawImageUnscaled(screenshotBitMap, 0, 30);

                        }
                        screenshotBitMap.Dispose();
                        File.Delete(screenshotFilePath);

                        resultBitMap.Save(screenshotFilePath, ImageFormat.Png);
                        resultBitMap.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Log.Warn($"error writing url in screenshot\n {ex.Message} \n{ex.StackTrace}");
                    }
                    Console.WriteLine($"\nScreenshot: {new Uri(screenshotFilePath)}\n");
                    this.TestContext.AddResultFile(screenshotFilePath);
                }

            }
        }
        private static string ArtifactDirectory => Path.Combine(Directory.GetCurrentDirectory(), "Screenshots");
        private static String FileNameBase = string.Format($"{FeatureContext.Current.FeatureInfo.Title}_{ScenarioContext.Current.ScenarioInfo.Title}_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}").Replace(" ", "_");

        private string GetCurrentURL()
        {
            return Driver == null ? Page.Url : Driver.Url;
        }
        private bool SaveScreenshot(string filePath)
        {
            ITakesScreenshot? takesScreenshot = (Driver as ITakesScreenshot);
            if (takesScreenshot != null)
            {
                var screenshot = takesScreenshot.GetScreenshot();

                string screenshotFilePath = Path.Combine(ArtifactDirectory, FileNameBase + "_screenshot.png");

                screenshot.SaveAsFile(screenshotFilePath);
                return true;
            }
            else
            {
                Page.ScreenshotAsync(new() { Path = filePath, FullPage = true }).Wait();
                return true;
            }
        }
    }
}
