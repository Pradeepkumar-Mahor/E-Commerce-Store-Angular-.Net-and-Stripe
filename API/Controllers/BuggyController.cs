﻿using API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers;

public class BuggyController : BaseApiController
{
    [HttpGet("unauthorized")]
    public IActionResult GetUnauthorized()
    {
        return Unauthorized();
    }

    [HttpGet("badrequest")]
    public IActionResult GetBadRequest()
    {
        return BadRequest("Not a good request");
    }

    [HttpGet("notfound")]
    public IActionResult GetNotFound()
    {
        return NotFound();
    }

    [HttpGet("internalerror")]
    public IActionResult GetInternalError()
    {
        throw new Exception("This is a test exception");
    }

    [HttpPost("validationerror")]
    public IActionResult GetValidationError(CreateProductDto product)
    {
        return Ok();
    }

    [Authorize]
    [HttpGet("secret")]
    public IActionResult GetSecret()
    {
        string? name = User.FindFirst(ClaimTypes.Name)?.Value;
        string? id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return Ok("Hello " + name + " with the id of " + id);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin-secret")]
    public IActionResult GetAdminSecret()
    {
        string? name = User.FindFirst(ClaimTypes.Name)?.Value;
        string? id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        bool isAdmin = User.IsInRole("Admin");
        string? roles = User.FindFirstValue(ClaimTypes.Role);

        return Ok(new
        {
            name,
            id,
            isAdmin,
            roles
        });
    }
}
