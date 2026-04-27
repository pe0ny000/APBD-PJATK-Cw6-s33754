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
        return Ok(await service.GetAppointmentAsync(id));
    }
}