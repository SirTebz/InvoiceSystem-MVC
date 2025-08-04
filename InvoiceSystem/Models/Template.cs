using System.ComponentModel.DataAnnotations;

namespace InvoiceSystem.Models
{
    public class Template
    {
        [Key]
        public int id { get; set; }
        [Required, MaxLength(200)]
        public string TemplateName { get; set; }
        [Required]
        public string HtmlContent { get; set; }
        public TemplateType TemplateType { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string? UpdateBy { get; set; }
    }
}
