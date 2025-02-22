using Microsoft.AspNetCore.Mvc;
using server.Services;

namespace server.Controllers;

/// <inheritdoc />
[Route("file")]
public class FileController : Controller
{
    private const int MaximumFileSize = 1024 * 1024 * 16;
    private readonly FileStorageService _fileStorageService;

    /// <inheritdoc />
    public FileController(FileStorageService fileStorageService)
    {
        _fileStorageService = fileStorageService;
    }

    private class ErrorResponse(string message)
    {
        public string Message { get; set; } = message;
    }

    private class FileResponse(string fileId)
    {
        public string FileId { get; set; } = fileId;
    }

    /// <response code="201">File saved successfully</response>
    /// <response code="400">File is empty</response>
    /// <response code="413">File is too large (over 16 MB)</response>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(FileResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status413RequestEntityTooLarge, Type = typeof(ErrorResponse))]
    public async Task<IActionResult> SaveFile(IFormFile? file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new ErrorResponse("File is empty!"));

        if (file.Length > MaximumFileSize)
            return StatusCode(StatusCodes.Status413RequestEntityTooLarge, new ErrorResponse("File is too large!"));

        string fileId = await _fileStorageService.StoreFile(file);

        return Created($"/file/{fileId}", new FileResponse(fileId));
    }

    /// <remarks>
    /// Sample request:
    ///
    ///     GET /file/1
    /// 
    /// </remarks>
    /// <response code="404">File not found</response>
    [HttpGet("{id}")]
    [Produces("application/octet-stream")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFile(string id)
    {
        var content = await _fileStorageService.GetFileContent(id);
        if (content == null) return NotFound();

        string fileName = await _fileStorageService.GetFileName(id);
        return File(content, "application/octet-stream", fileName);
    }

    /// <response code="200">File exists</response>
    /// <response code="404">File not found</response>
    [HttpHead("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CheckFileExistence(string id)
    {
        string filePath = await _fileStorageService.GetFilePath(id);
        return string.IsNullOrEmpty(filePath) ? NotFound() : Ok();
    }
}