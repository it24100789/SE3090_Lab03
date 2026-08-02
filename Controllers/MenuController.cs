using CampusEats.Api.Dtos;
using CampusEats.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampusEats.Api.Controllers;

[ApiController]                     // automatic 400 on invalid model
[Route("api/[controller]")]          // route = /api/menu
public class MenuController : ControllerBase
{
    private readonly IMenuService _svc;

    // Constructor injection – the service is provided by DI
    public MenuController(IMenuService svc) => _svc = svc;

    // GET /api/menu
    [HttpGet]
    public ActionResult<IEnumerable<MenuItemDto>> GetAll()
    {
        return Ok(_svc.GetAll());
    }

    // GET /api/menu/{id}
    [HttpGet("{id}")]
    public ActionResult<MenuItemDto> GetById(int id)
    {
        var item = _svc.GetById(id);
        return item is null ? NotFound() : Ok(item);
    }

    // POST /api/menu
    [HttpPost]
    public ActionResult<MenuItemDto> Create([FromBody] CreateMenuItemDto dto)
    {
        var created = _svc.Create(dto);
        // Returns 201 Created with Location header pointing to the new resource
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT /api/menu/{id}
    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] CreateMenuItemDto dto)
    {
        var ok = _svc.Update(id, dto);
        return ok ? NoContent() : NotFound();   // 204 on success, 404 if not found
    }

    // DELETE /api/menu/{id}
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var ok = _svc.Delete(id);
        return ok ? NoContent() : NotFound();   // 204 on success, 404 if not found
    }
}