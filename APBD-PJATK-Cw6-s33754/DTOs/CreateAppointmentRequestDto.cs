using System.ComponentModel.DataAnnotations;

namespace APBD_PJATK_Cw6_s33754.DTOs;

public class CreateAppointmentRequestDto
{
    public int IdPatient { get; set; }
    public int IdDoctor { get; set; }
    public DateTime AppointmentDate { get; set; }
    [Required]
    [MaxLength(250)]
    public string Reason { get; set; } = string.Empty;
    
}