using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace NaviAuth.Controllers;

[Route("/swagger/custom")]
[ApiController]
public class SwaggerController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetSwaggerDefinition()
    {
        var filePath = "Definition/NaviAuthDefinition.yaml";
        if (!System.IO.File.Exists(filePath))
            throw new FileNotFoundException("Yaml file is not found!");

        await using var file = System.IO.File.OpenRead(filePath);
        using var fileReader = new StreamReader(file);
        var str = await fileReader.ReadToEndAsync();
        return Ok(str);
    }
}