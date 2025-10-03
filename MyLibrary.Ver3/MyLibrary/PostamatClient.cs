using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MyLibrary
{
    // 1) Интерфейс, работающий не с "сырыми" JSON-строками, а с DTO
    public interface IParcel
    {
        int InsertOrder(InsertOrderRequest request);
        int RetrieveExpiredOrder(RetrieveExpiredOrderRequest request);
        FreeCellsResponse GetFreeCells();
    }

    // 2) DTO-модели

    // Параметры для InsertOrder
    public class InsertOrderRequest
    {
        public int PostamatId { get; set; }
        public string PostamatCode { get; set; } = null!;
        public OpenByCellCodesDto OpenByCellCodes { get; set; } = null!;
        public OpenByLockerDto OpenByLocker { get; set; } = null!;
    }

    public class OpenByCellCodesDto
    {
        public List<string> CellCodes { get; set; } = new();
    }

    public class OpenByLockerDto
    {
        public int OpenType { get; set; }
        public int LockerNumber { get; set; }
    }

    // Параметры для RetrieveExpiredOrder
    public class RetrieveExpiredOrderRequest
    {
        public int PostamatId { get; set; }
        public string DeliveryDate { get; set; } = null!;  // "yyyy-MM-dd"
        public ParcelSizeDto ParcelSize { get; set; } = null!;
    }

    public class ParcelSizeDto
    {
        public int Length { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    // Модель ответа для GetFreeCells
    public class FreeCellsResponse
    {
        public List<string> Cells { get; set; } = new();
    }

    // 3) Клиент, работающий напрямую с DTO и HttpClient.Json extensions
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

        // Отправляем DTO объект InsertOrderRequest
        public int InsertOrder(InsertOrderRequest request)
        {
            var response = _http
                .PostAsJsonAsync("insertOrder", request, _opts)
                .GetAwaiter().GetResult();

            return response.IsSuccessStatusCode
                ? 0
                : (int)response.StatusCode;
        }

        // Отправляем DTO объект RetrieveExpiredOrderRequest
        public int RetrieveExpiredOrder(RetrieveExpiredOrderRequest request)
        {
            var response = _http
                .PostAsJsonAsync("retrieveExpiredOrder", request, _opts)
                .GetAwaiter().GetResult();

            return response.IsSuccessStatusCode
                ? 0
                : (int)response.StatusCode;
        }

        // Получаем ответ сервера и десериализуем в FreeCellsResponse
        public FreeCellsResponse GetFreeCells()
        {
            var result = _http
                .GetFromJsonAsync<FreeCellsResponse>("getFreeCells", _opts)
                .GetAwaiter().GetResult();

            return result ?? new FreeCellsResponse();
        }

        public void Dispose() => _http.Dispose();
    }
}