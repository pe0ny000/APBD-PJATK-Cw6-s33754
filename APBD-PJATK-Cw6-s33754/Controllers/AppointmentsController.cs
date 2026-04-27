using APBD_PJATK_Cw6_s33754.DTOs;
using APBD_PJATK_Cw6_s33754.Exceptions;
using APBD_PJATK_Cw6_s33754.Services;
using Microsoft.AspNetCore.Mvc;

namespace APBD_PJATK_Cw6_s33754.Controllers;

[ApiController]
[Route("api/appointments")]
public class AppointmentsController(IAppointmentService service) : ControllerBase
{
    //GET /api/appointments?status=Scheduled&patientLastName=Kowalska
    [HttpGet]
    public async Task<IActionResult> GetAppointments([FromQuery] string? status,[FromQuery] string? patientLastName)
    {
        return Ok(await service.GetAppointmentsAsync(status, patientLastName));
    }
    
    
    //GET /api/appointments/{idAppointment} Zwraca szczegóły jednej wizyty.

    [HttpGet]
    [Route("{id:int}")]
    public async Task<IActionResult> GetAppointment(int id)
    {
        try
        {
            return Ok(await service.GetAppointmentAsync(id));
        }
        catch (NotFoundException e)
        {
            return NotFound(new ErrorResponseDto { Message = e.Message });
        }
    }
    
    // POST /api/appointments
    [HttpPost]
    public async Task<IActionResult> CreateAppointmentAsync([FromBody] CreateAppointmentRequestDto dto)
    {
        try
        {
            await service.CreateAppointmentAsync(dto);
            return Created();
        }
        catch (ConflictException e)
        {
            return BadRequest(new ErrorResponseDto { Message = e.Message });
        }
        catch (NotFoundException e)
        {
            return NotFound(new ErrorResponseDto { Message = e.Message });
        }
    }

    [HttpPut]
    [Route("{id:int}")]
    public async Task<IActionResult> UpdateAppointmentAsync(int id, [FromBody] UpdateAppointmentRequestDto dto)
    {
        try
        {
            await service.UpdateAppointmentAsync(id, dto);
            return Ok();
        }
        catch (NotFoundException e)
        {
            return NotFound(new ErrorResponseDto { Message = e.Message });
        }
        catch (BadRequestException e)
        {
            return BadRequest(new ErrorResponseDto { Message = e.Message });
        }
        catch (ConflictException e)
        {
            return Conflict(new ErrorResponseDto { Message = e.Message });
        }
    }

    [HttpDelete]
    [Route("{id:int}")]
    public async Task<IActionResult> DeleteAppointmentAsync(int id)
    {
        try
        {
            await service.DeleteAppointmentAsync(id);
            return NoContent();
        }
        catch (ConflictException e)
        {
            return Conflict(new ErrorResponseDto { Message = e.Message });
        }
        catch (NotFoundException e)
        {
            return NotFound(new ErrorResponseDto { Message = e.Message });
        }
        
    }
}