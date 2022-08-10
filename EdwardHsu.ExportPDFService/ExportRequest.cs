using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using PuppeteerSharp.Media;

namespace EdwardHsu.ExportPDFService
{
    public class ExportRequest
    {
        [Required]
        public string html { get; set; }

        
        public PaperSize PaperSize { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PaperSize
    {
        Letter,
        Legal,
        Tabloid,
        Ledger,
        A0,
        A1,
        A2,
        A3,
        A4,
        A5,
        A6
    }
}
