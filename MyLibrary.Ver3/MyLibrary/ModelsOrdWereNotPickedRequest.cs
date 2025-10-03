using System.Collections.Generic;

namespace MyLibrary.Models.Requests
{
    public class OrdWereNotPickedRequest
    {
        public int PostamatId { get; set; }
        public string PostamatCode { get; set; } = null!;
        public List<string> CellCodes { get; set; } = new();
    }
}