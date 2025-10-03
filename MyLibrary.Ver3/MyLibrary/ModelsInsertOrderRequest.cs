using MyLibrary.Models.Dtos;
using System.Collections.Generic;

namespace MyLibrary.Models.Requests
{
    public class InsertOrderRequest
    {
        public int PostamatId { get; set; }
        public string PostamatCode { get; set; } = null!;
        public OpenByCellCodesDto OpenByCellCodes { get; set; } = null!;
        public OpenByLockerDto OpenByLocker { get; set; } = null!;
    }
}