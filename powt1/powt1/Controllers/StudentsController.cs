using Microsoft.AspNetCore.Mvc;
using powt1.Exeptions;
using powt1.Models;
using powt1.Models.DTOs;
using powt1.Services;

namespace powt1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentsController(IDbService dbService) : ControllerBase
{  
    [HttpGet]
    public async Task<IActionResult> GetStudentsDetails()
    {
        return Ok(await dbService.GetStudentsDetailsAsync());
    }

    public async Task<IActionResult> CreateStudent([FromBody] StudentWithGroupsPostDTO student)
    {
        try
        {
            return Ok(await dbService.CreateStudentWithGroups(student));

        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }

    }
}