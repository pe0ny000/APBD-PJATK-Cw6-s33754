using System.ComponentModel.DataAnnotations;

namespace APBD_PJATK_Cw6_s33754.DTOs;

public class AppointmentDetailsDto
//GET /api/appointments/{idAppointment}
//Endpoint zwraca szczegóły jednej wizyty.
//Jeżeli wizyta nie istnieje, zwróć 404 Not Found.
//DTO szczegółów może zawierać e-mail pacjenta, telefon, numer licencji lekarza,
//notatki wewnętrzne i datę utworzenia rekordu.
{
    [Required]
    public int IdAppointment  { get; set; }
    public int IdDoctor { get; set; }
    public int IdPatient { get; set; }
    [MaxLength(120)]
    public string Email { get; set; } = string.Empty;
    [MaxLength(30)]
    public string PhoneNumber { get; set; } = string.Empty;
    [MaxLength(40)]
    public string LicenseNumber { get; set; } = string.Empty;
    [MaxLength(500)]
    public string? InternalNotes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}