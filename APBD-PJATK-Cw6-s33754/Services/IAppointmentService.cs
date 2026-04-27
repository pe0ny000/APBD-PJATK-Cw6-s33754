using APBD_PJATK_Cw6_s33754.DTOs;

namespace APBD_PJATK_Cw6_s33754.Services;

public interface IAppointmentService
{
    Task<List<AppointmentListDto>> GetAppointmentsAsync(string? status, string? patientLastName);
    Task<AppointmentDetailsDto> GetAppointmentAsync(int id);
    Task<CreateAppointmentRequestDto> CreateAppointmentAsync(CreateAppointmentRequestDto dto);
    Task UpdateAppointmentAsync(int id, UpdateAppointmentRequestDto dto);
    Task DeleteAppointmentAsync(int id);
}