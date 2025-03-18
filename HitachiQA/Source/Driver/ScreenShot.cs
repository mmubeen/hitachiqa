using Microsoft.Playwright;
using OpenQA.Selenium;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;

namespace HitachiQA
{
    public class ScreenShot
    {
        private readonly IPage _page;
        private readonly IWebDriver _driver;
        private readonly TestContext _testContext;
        private readonly FeatureContext _featureCtx;
        private readonly ScenarioContext _scenarioCtx;
        public ScreenShot(FeatureContext fctx, ScenarioContext sctx)
        {
            _featureCtx = fctx;
            _scenarioCtx = sctx;
        }

        public ScreenShot(FeatureContext fctx, ScenarioContext sctx, IWebDriver driver, TestContext testContext):this(fctx, sctx)
        {
            _driver = driver;
            _testContext = testContext;
        }
        public ScreenShot(FeatureContext fctx, ScenarioContext sctx, IPage page, TestContext testContext) : this(fctx, sctx)
        {
            _page = page;
            _testContext = testContext;
        }
        public void Info(string filename = null) => Take(Severity.INFO, filename!);
        public void Debug(string filename = null) => Take(Severity.DEBUG, filename!);
        public void Warn(string filename = null) => Take(Severity.WARN, filename!);
        public void Error(string filename = null) => Take(Severity.ERROR, filename!);
        public void Critical(string filename = null) => Take(Severity.CRITICAL, filename!);


        /// <summary>
        /// Take screenshot, by defualt the filename will be Severity_CurrentScenario_currentDateTime unless otherwise specified
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
        public void Take(Severity severity, string fileName = null)
        {
            var currentSev = Severity.parseLevel(Main.Configuration.GetSection("Logging").GetSection("LogLevel")["Default"]).Level;

            if (currentSev == 0)
            {
                return;
            }
            else if (severity.Level <= currentSev)
            {


                fileName ??= $"{severity.Name}_{FileNameBase}";

                if (!Directory.Exists(ArtifactDirectory)) { Directory.CreateDirectory(ArtifactDirectory); }

                string pageSource = _driver == null ? _page.ContentAsync().Result : _driver.PageSource;
                string sourceFilePath = Path.Combine(ArtifactDirectory, fileName + "_source.html");
                File.WriteAllText(sourceFilePath, pageSource, Encoding.UTF8);
                _testContext.AddResultFile(sourceFilePath);
                Console.WriteLine($"\nPage Source: {new Uri(sourceFilePath)}\n");



                string screenshotFilePath = Path.Combine(ArtifactDirectory, fileName + "_screenshot.png");

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
                    this._testContext.AddResultFile(screenshotFilePath);
                }

            }
        }
        private static string ArtifactDirectory => Path.Combine(Directory.GetCurrentDirectory(), "Screenshots");
        private string FileNameBase => string.Format($"{_featureCtx.FeatureInfo.Title}_{_scenarioCtx.ScenarioInfo.Title}_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}").Replace(" ", "_");

        private string GetCurrentURL()
        {
            return _driver == null ? _page.Url : _driver.Url;
        }
        private bool SaveScreenshot(string filePath)
        {
            ITakesScreenshot takesScreenshot = (_driver as ITakesScreenshot);
            if (takesScreenshot != null)
            {
                var screenshot = takesScreenshot.GetScreenshot();

                string screenshotFilePath = Path.Combine(ArtifactDirectory, filePath);

                screenshot.SaveAsFile(screenshotFilePath);
                return true;
            }
            else
            {
                _page.ScreenshotAsync(new() { Path = filePath, FullPage = true }).Wait();
                return true;
            }
        }
    }
}
