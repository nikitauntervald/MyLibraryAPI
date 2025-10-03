using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using MyLibrary.Interfaces;
using MyLibrary.Models.Requests;
using MyLibrary.Models.Responses;

namespace MyLibrary.Services
{
    public class ParcelClient : IParcel, IDisposable
    {
        private readonly HttpClient _http;
        private readonly JsonSerializerOptions _opts = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public ParcelClient(string baseAddress)
        {
            _http = new HttpClient { BaseAddress = new Uri(baseAddress) };
        }

        public int InsertOrder(InsertOrderRequest request)
        {
            var response = _http
                .PostAsJsonAsync("insertOrder", request, _opts)
                .GetAwaiter().GetResult();

            return response.IsSuccessStatusCode
                ? 0
                : (int)response.StatusCode;
        }

        public int RetrieveExpiredOrder(RetrieveExpiredOrderRequest request)
        {
            var response = _http
                .PostAsJsonAsync("retrieveExpiredOrder", request, _opts)
                .GetAwaiter().GetResult();

            return response.IsSuccessStatusCode
                ? 0
                : (int)response.StatusCode;
        }

        public FreeCellsResponse GetFreeCells()
        {
            var result = _http
                .GetFromJsonAsync<FreeCellsResponse>("getFreeCells", _opts)
                .GetAwaiter().GetResult();

            return result ?? new FreeCellsResponse();
        }

        // реализация нового метода
        public int OrdWereNotPicked(OrdWereNotPickedRequest request)
        {
            var response = _http
                .PostAsJsonAsync("ordWereNotPicked", request, _opts)
                .GetAwaiter().GetResult();

            return response.IsSuccessStatusCode
                ? 0
                : (int)response.StatusCode;
        }

        public void Dispose() => _http.Dispose();
    }
}