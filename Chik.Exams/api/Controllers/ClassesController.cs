using Microsoft.AspNetCore.Mvc;

namespace Chik.Exams.Api;

[ApiController]
[Route("api/classes")]
public class ClassesController : ControllerBase
{
    private readonly IClassService _classService;
    private readonly ILogger<ClassesController> _logger;

    public ClassesController(IClassService classService, ILogger<ClassesController> logger)
    {
        _classService = classService;
        _logger = logger;
    }

    /// <summary>
    /// Lists classes. Admin sees all; Teacher sees classes they are assigned to.
    /// </summary>
    [HttpGet]
    [AdminOrTeacher]
    public async Task<ActionResult<List<Class>>> List([FromServices] Auth auth)
    {
        var classes = await _classService.List(auth);
        return Ok(classes);
    }

    /// <summary>
    /// Gets a class by id. Admin or Teacher assigned to that class.
    /// </summary>
    [HttpGet("{id:int}")]
    [AdminOrTeacher]
    public async Task<ActionResult<Class>> Get(int id, [FromServices] Auth auth)
    {
        var c = await _classService.Get(auth, id);
        return Ok(c);
    }

    /// <summary>
    /// Creates a class. Admin only.
    /// </summary>
    [HttpPost]
    [AdminOnly]
    public async Task<ActionResult<Class>> Create(
        [FromBody] CreateClassRequest request,
        [FromServices] Auth auth)
    {
        var created = await _classService.Create(auth, new Class.Create(request.Name));
        _logger.LogInformation("Class {ClassId} {Name} created by {User}", created.Id, created.Name, auth.Username);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }
}

public record CreateClassRequest(
    [Required] string Name
);
