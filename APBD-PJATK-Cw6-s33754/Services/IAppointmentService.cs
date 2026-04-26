using APBD_PJATK_Cw6_s33754.DTOs;

namespace APBD_PJATK_Cw6_s33754.Services;

public class IAppointmentService
{
    Task<List<AppointmentListDto>> GetAppointmentsAsync(string? status, string? patientLastName)
    {
        throw new NotImplementedException();
    }

    Task<AppointmentDetailsDto> GetAppointmentAsync(int id)
    {
        throw new NotImplementedException();
    }

    Task CreateAppointmentAsync(CreateAppointmentRequestDto dto)
    {
        throw new NotImplementedException();
    }

    Task UpdateAppointmentAsync(int id, UpdateAppointmentRequestDto dto)
    {
        throw new NotImplementedException();
    }

    Task DeleteAppointmentAsync(int id)
    {
        throw new NotImplementedException();
    }
}