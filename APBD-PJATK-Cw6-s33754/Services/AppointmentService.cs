using APBD_PJATK_Cw6_s33754.DTOs;
using Microsoft.Data.SqlClient;
using APBD_PJATK_Cw6_s33754.Exceptions;


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


    // POST /api/appointments Dodaje nową wizytę.
    public async Task<CreateAppointmentRequestDto> CreateAppointmentAsync(CreateAppointmentRequestDto dto)
    {
        if (dto.AppointmentDate < DateTime.Now)
        {
            throw new BadRequestException("Appointment date cannot be in the past");
        }
        var appointment = new CreateAppointmentRequestDto();
        await using var connection = new SqlConnection(configuration.GetConnectionString("Default"));
        await using var command = new SqlCommand();

        await connection.OpenAsync();

        
        command.Connection = connection;
        //istnienie pacjenta
        command.CommandText = """
                              SELECT 1 from Patients where IdPatient = @idPatient and IsActive = 1
                              """;
        command.Parameters.AddWithValue("@IdPatient", dto.IdPatient);
        var patientExists = await command.ExecuteScalarAsync();
        if (patientExists is null)
            throw new NotFoundException($"Patient with id {dto.IdPatient} not found");
        command.Parameters.Clear();
        
        //istnienie lekarza
        command.CommandText = """
                              SELECT 1 from Doctors where IdDoctor = @idDoctor and IsActive = 1
                              """;
        command.Parameters.AddWithValue("@idDoctor", dto.IdDoctor);
        var doctorExists = await command.ExecuteScalarAsync();
        if (doctorExists is null)
            throw new NotFoundException($"Doctor with id {dto.IdDoctor} not found");
        command.Parameters.Clear();
        
        //termin
        command.CommandText = """
                              SELECT 1 from Appointments where IdDoctor = @IdDoctor 
                                                           and AppointmentDate = @AppointmentDate 
                                                           and Status = 'Scheduled'
                              """;
        command.Parameters.AddWithValue("@IdDoctor", dto.IdDoctor);
        command.Parameters.AddWithValue("@AppointmentDate", dto.AppointmentDate);
        var conflict = await command.ExecuteScalarAsync();
        if (conflict is not null)
            throw new ConflictException("Doctor already has an appointment at this time");
        command.Parameters.Clear();

        command.CommandText = """
                              INSERT INTO Appointments (IdPatient, IdDoctor, AppointmentDate, Status, Reason, CreatedAt)
                              VALUES (@IdPatient, @IdDoctor, @AppointmentDate, 'Scheduled', @Reason, GETDATE())
                              """;
        command.Parameters.AddWithValue("@IdDoctor", dto.IdDoctor);
        command.Parameters.AddWithValue("@AppointmentDate", dto.AppointmentDate);
        command.Parameters.AddWithValue("@Reason", dto.Reason);
        command.Parameters.AddWithValue("@IdPatient", dto.IdPatient);
        await command.ExecuteNonQueryAsync();
        return appointment;
    }

    
   // PUT /api/appointments/{idAppointment}
    public async Task UpdateAppointmentAsync(int id, UpdateAppointmentRequestDto dto)
    {
        
        await using var connection = new SqlConnection(configuration.GetConnectionString("Default"));
        await using var command = new SqlCommand();

        await connection.OpenAsync();
        
        command.Connection = connection;
        
        //sprawdzenie rezerwacji
        command.CommandText = """
                              SELECT 1 from Appointments where IdAppointment = @idAppointment
                              """;
        command.Parameters.AddWithValue("@idAppointment", id);
        var appointmentExists = await command.ExecuteScalarAsync();
        if (appointmentExists is null)
            throw new NotFoundException($"Appointment with id {id} not found");
        command.Parameters.Clear();

        //sprawdzenie lekarza
        command.CommandText = """Select 1 from Doctors where IdDoctor = @IdDoctor and IsActive = 1""";
        command.Parameters.AddWithValue("@IdDoctor", dto.IdDoctor);
        var doctorExists = await command.ExecuteScalarAsync();
        if (doctorExists is null)
            throw new NotFoundException($"Doctor with id {dto.IdDoctor} not found");
        command.Parameters.Clear();
        
        //sprawdzenie pacjenta
        command.CommandText = """ Select 1 from Patients where IdPatient = @IdPatient and IsActive = 1""";
        command.Parameters.AddWithValue("@IdPatient", dto.IdPatient);
        var patientExists = await command.ExecuteScalarAsync();
        if (patientExists is null)
            throw new NotFoundException($"Patient with id {dto.IdPatient} not found");
        command.Parameters.Clear();
        
        //Sprawdzenie statusu
        command.CommandText = "SELECT Status, AppointmentDate FROM dbo.Appointments WHERE IdAppointment = @idAppointment";
        command.Parameters.AddWithValue("@idAppointment", id);
        await using var reader = await command.ExecuteReaderAsync();
        await reader.ReadAsync();
        var currentStatus = reader.GetString(0);
        var currentDate = reader.GetDateTime(1);
        await reader.CloseAsync();
        command.Parameters.Clear();

        if (currentStatus == "Completed" && dto.AppointmentDate != currentDate)
            throw new BadHttpRequestException("Cannot change date of a completed appointment");
        
        var validStatus = new[]{"Scheduled", "Completed", "Cancelled"};
        if (!validStatus.Contains(dto.Status))
            throw new BadHttpRequestException("Invalid status");
        
        //czy lekarz jest zajety
        command.CommandText = """
                              SELECT 1 from Appointments where IdDoctor = @IdDoctor 
                                                           and AppointmentDate = @AppointmentDate 
                                                           and Status = 'Scheduled'
                              """;
        command.Parameters.AddWithValue("@IdDoctor", dto.IdDoctor);
        command.Parameters.AddWithValue("@AppointmentDate", dto.AppointmentDate);
        var conflict = await command.ExecuteScalarAsync();
        if (conflict is not null)
            throw new ConflictException($"Doctor already has an appointment at this time");
        command.Parameters.Clear();
        
        command.CommandText = """
                              UPDATE Appointments SET IdPatient = @idPatient, IdDoctor = @idDoctor, AppointmentDate = @AppointmentDate, Status = @Status, Reason = @Reason, InternalNotes = @InternalNotes WHERE IdAppointment = @idAppointment
                              """;
        command.Parameters.AddWithValue("@idDoctor", dto.IdDoctor);
        command.Parameters.AddWithValue("@AppointmentDate", dto.AppointmentDate);
        command.Parameters.AddWithValue("@Status", dto.Status);
        command.Parameters.AddWithValue("@Reason", dto.Reason);
        command.Parameters.AddWithValue("@IdPatient", dto.IdPatient);
        command.Parameters.AddWithValue("@InternalNotes", (object?)dto.InternalNotes ?? DBNull.Value);
        command.Parameters.AddWithValue("@idAppointment", id);
        await command.ExecuteNonQueryAsync();
        
    }

    public async Task DeleteAppointmentAsync(int id)
    {
        await using var connection = new SqlConnection(configuration.GetConnectionString("Default"));
        await using var command = new SqlCommand();

        await connection.OpenAsync();
        
        command.Connection = connection;
        command.CommandText = """select 1 from appointments where IdAppointment = @idAppointment""";
        command.Parameters.AddWithValue("@idAppointment", id);
        var appointmentExists = await command.ExecuteScalarAsync();
        if (appointmentExists is null)
            throw new NotFoundException($"Appointment with id {id} not found");
        
        command.CommandText = """SELECT Status FROM dbo.Appointments WHERE IdAppointment = @idAppointment""";
        await using var reader = await command.ExecuteReaderAsync();
        await reader.ReadAsync();
        var currentStatus = reader.GetString(0);
        await reader.CloseAsync();
        command.Parameters.Clear();

        if (currentStatus == "Completed" )
            throw new ConflictException("Cannot delate completed appointment");

        command.CommandText = """
                              Delete From dbo.Appointments Where IdAppointment = @idAppointment
                              """;
        command.Parameters.AddWithValue("@idAppointment", id);
        await command.ExecuteNonQueryAsync();
        

    }
}