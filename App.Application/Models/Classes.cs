

namespace App.Application.Models
{

    public class SelectItemModel
    {
        public string? Title { get; set; }
        public string? Name { get; set; }
        public int Ordered { get; set; }
    }
    public record UserInfo
    {
        public long? Id { get; set; }
        public Guid? KeyId { get; set; }
        public List<long?>? RoleIds { get; set; }
        public List<long>? AccessIds { get; set; }
        public string? Name { get; set; }
        public string? ImagePath { get; set; }
        public bool? IsAdmin { get; set; }
        public DateTime? LoginDateTime { get; set; }
        public int? LoginCount { get; set; }
        public int LanguageId { get; set; }
        public int CompanyId { get; set; }
        public string? CompanyName { get; set; }
    }

    public record YearInfo
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
    public class RequestInfo
    {
        public string? RemoteIp { get; set; }
        public int? RemotePort { get; set; }
        public string? LocalIp { get; set; }
        public int? LocalPort { get; set; }
        public string? UserAgent { get; set; }
        public string? Host { get; set; }
        public string? Headers { get; set; }
        public string? Browser { get; set; }
        public string? Mobile { get; set; }
        public string? Platform { get; set; }
        public string? Scheme { get; set; }
    }
}
