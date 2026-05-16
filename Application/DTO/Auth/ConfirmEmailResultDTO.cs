namespace Application.Data.DTO.Auth
{
    public class ConfirmEmailResultDTO
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
