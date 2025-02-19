using Microsoft.AspNetCore.Mvc;

namespace HitachiQA.Backend.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class VideoController : ControllerBase
{
    private readonly string _videoDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Videos");

    // GET api/<RunController>/5
    [HttpGet("{fileName}")]
    public IActionResult Get(string fileName)
    {
        var videoFilePath = Path.Combine(_videoDirectory, fileName);

        if (!System.IO.File.Exists(videoFilePath))
        {
            return NotFound(new { message = "Video not found" });
        }

        var fileStream = new FileStream(videoFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return File(fileStream, "video/webm", enableRangeProcessing: true);
    }

}
