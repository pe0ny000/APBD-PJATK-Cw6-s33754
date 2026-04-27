using APBD_PJATK_Cw6_s33754.DTOs;
using Microsoft.Data.SqlClient;
using APBD_PJATK_Cw6_s33754.DTOs;
using APBD_PJATK_Cw6_s33754.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace APBD_PJATK_Cw6_s33754.Services;

public class AppointmentService(IConfiguration configuration) : IAppointmentService
{
    //GET /api/appointments?status=Scheduled&patientLastName=Kowalska
    public async Task<List<AppointmentListDto>> GetAppointmentsAsync(string? status, string? patientLastName)
    {
        var appointments = new List<AppointmentListDto>();

        await using var connection = new SqlConnection(configuration.GetConnectionString("Default"));
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

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            appointments.Add(new AppointmentListDto
            {
                IdAppointment = reader.GetInt32(0),
                AppointmentDate = reader.GetDateTime(1),
                Status = reader.GetString(2),
                Reason = reader.GetString(3),
                PatientFullName = reader.GetString(4),
                PatientEmail = reader.GetString(5)
            });
        }

        return appointments;
    }

    //GET /api/appointments/{idAppointment} Zwraca szczegóły jednej wizyty.
    public async Task<AppointmentDetailsDto> GetAppointmentAsync(int id)
    {
        var appointment = new AppointmentDetailsDto();
        await using var connection = new SqlConnection(configuration.GetConnectionString("Default"));
        await using var command = new SqlCommand();

        await connection.OpenAsync();

        command.Connection = connection;
        command.CommandText = """
                              SELECT a.IdAppointment, a.IdPatient, a.IdDoctor, a.CreatedAt, 
                                     a.InternalNotes, p.Email, p.PhoneNumber,d.LicenseNumber
                              FROM dbo.Appointments a 
                              LEFT JOIN Doctors d ON d.IdDoctor = a.IdDoctor
                              Left JOIN Patients p ON p.IdPatient = a.IdPatient
                              Where a.IdAppointment = @id

                              """;
        command.Parameters.AddWithValue("@id", id);
        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            throw new NotFoundException($"Appointment with id {id} not found");
        appointment = new AppointmentDetailsDto()
        {
            IdAppointment = reader.GetInt32(0),
            IdPatient = reader.GetInt32(1),
            IdDoctor = reader.GetInt32(2),
            CreatedAt = reader.GetDateTime(3),
            InternalNotes = reader.IsDBNull(4) ? null : reader.GetString(4),
            Email = reader.GetString(5),
            PhoneNumber = reader.GetString(6),
            LicenseNumber = reader.GetString(7)
        };


        return appointment;
    }


    public Task CreateAppointmentAsync(CreateAppointmentRequestDto dto)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAppointmentAsync(int id, UpdateAppointmentRequestDto dto)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAppointmentAsync(int id)
    {
        throw new NotImplementedException();
    }
}