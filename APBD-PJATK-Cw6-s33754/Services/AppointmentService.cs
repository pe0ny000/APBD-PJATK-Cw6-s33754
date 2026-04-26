using APBD_PJATK_Cw6_s33754.DTOs;


using Microsoft.Data.SqlClient;
using APBD_PJATK_Cw6_s33754.DTOs;
using APBD_PJATK_Cw6_s33754.Exceptions;

namespace APBD_PJATK_Cw6_s33754.Services;

public class AppointmentService(IConfiguration configuration) : IAppointmentService
{
    
    //GET /api/appointments?status=Scheduled&patientLastName=Kowalska
    public async Task<List<AppointmentListDto>> GetAppointmentsAsync(string? status, string? patientLastName)
    {
        var appointments = new List<AppointmentListDto>();

        await using var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
        await using var command = new SqlCommand();

        await connection.OpenAsync();

        command.Connection = connection;
        command.CommandText = """
                              SELECT
                                  a.IdAppointment,
                                  a.AppointmentDate,
                                  a.Status,
                                  a.Reason,
                                  p.FirstName + N' ' + p.LastName AS PatientFullName,
                                  p.Email AS PatientEmail
                              FROM dbo.Appointments a
                              JOIN dbo.Patients p ON p.IdPatient = a.IdPatient
                              WHERE (@Status IS NULL OR a.Status = @Status)
                                AND (@PatientLastName IS NULL OR p.LastName = @PatientLastName)
                              ORDER BY a.AppointmentDate;

                              """;
        command.Parameters.AddWithValue("@Status", (object?)status ?? DBNull.Value);
        command.Parameters.AddWithValue("@PatientLastName", (object?)patientLastName ?? DBNull.Value);

        
        return appointments;

    }
}

