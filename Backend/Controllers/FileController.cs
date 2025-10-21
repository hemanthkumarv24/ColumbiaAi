using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ColumbiaAi.Backend.Services;

namespace ColumbiaAi.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FileController : ControllerBase
{
    private readonly IBlobStorageService _blobStorage;
    private readonly ILogger<FileController> _logger;

    public FileController(IBlobStorageService blobStorage, ILogger<FileController> logger)
    {
        _blobStorage = blobStorage;
        _logger = logger;
    }

    [HttpPost("upload")]
    public async Task<ActionResult<string>> UploadFile(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file provided" });
            }

            using var stream = file.OpenReadStream();
            var url = await _blobStorage.UploadFileAsync(stream, file.FileName, file.ContentType);

            return Ok(new { url });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file");
            return StatusCode(500, new { message = "An error occurred while uploading the file" });
        }
    }

    [HttpDelete("{fileName}")]
    public async Task<ActionResult> DeleteFile(string fileName)
    {
        try
        {
            var success = await _blobStorage.DeleteFileAsync(fileName);
            if (!success)
            {
                return NotFound(new { message = "File not found" });
            }

            return Ok(new { message = "File deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file");
            return StatusCode(500, new { message = "An error occurred while deleting the file" });
        }
    }
}
