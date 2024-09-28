using FakeUsers.Server.Models;
using FakeUsers.Server.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

[ApiController]
[Route("api/fakedata")]
public class FakeDataController : ControllerBase
{

    [HttpGet]
    public IActionResult GetFakeData([FromQuery] Region region, [FromQuery] double errorCount, [FromQuery] int page, [FromQuery] string seed = "")
    {
        var data = FakeDataGenerator.GenerateData(region, errorCount, seed, page);
        return Ok(data);
    }

    [HttpGet("export")]
    public IActionResult ExportToCsv([FromQuery] Region region, [FromQuery] double errorCount, [FromQuery] int toPage, [FromQuery] int fromPage = 0, [FromQuery] string seed = "")
    {
        var data = Enumerable.Range(fromPage, toPage + 1).SelectMany(page => FakeDataGenerator.GenerateData(region, errorCount, seed, page));
        var csv = CsvConverter.ConvertToCsv(data);
        return File(csv, "text/csv", "fake_user_data.csv");
    }
}