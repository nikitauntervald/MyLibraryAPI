using MyLibrary.Models.Requests;
using MyLibrary.Models.Responses;

namespace MyLibrary.Interfaces
{
    public interface IParcel
    {
        int InsertOrder(InsertOrderRequest request);
        int RetrieveExpiredOrder(RetrieveExpiredOrderRequest request);
        FreeCellsResponse GetFreeCells();
        int OrdWereNotPicked(OrdWereNotPickedRequest request);
    }
}