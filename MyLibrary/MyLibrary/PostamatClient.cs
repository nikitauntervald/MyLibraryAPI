using System;                     // Базовые типы и интерфейсы (IDisposable)
using System.Net;                 // HTTP-статусы (HttpStatusCode и т.п.)
using System.Net.Http;            // HttpClient, HttpMessageHandler и связанная функциональность
using System.Net.Http.Headers;    // Класс AuthenticationHeaderValue для установки заголовков авторизации
using System.Text;                // Кодировки (Encoding.UTF8)
using System.Text.Json;           // Для сериализации/десериализации

namespace MyLibrary;
/// <summary>
/// Клиент для взаимодействия с API постаматов.
/// Реализует интерфейс IParcel (предполагается, что там описаны InsertOrder и RetrieveExpiredOrder)
/// и IDisposable (для корректного освобождения HttpClient).
/// </summary>
public class PostamatClient : IParcel, IDisposable
{
    private readonly HttpClient _http;  // HttpClient для выполнения HTTP-запросов
    private readonly string _baseUri;  // HttpClient для выполнения HTTP-запросов

    /// <summary>
    /// Конструктор. Настраивает HttpClient и заголовок авторизации.
    /// </summary>
    /// <param name="baseUri">Базовый URL вашего API, например "https://api.your-postamat.com"</param>
    /// <param name="apiKey">API-ключ или токен для авторизации на сервере</param>
    /// <param name="handler">Опциональный обработчик сообщений (для тестов или настройки прокси)</param>
    public PostamatClient(string baseUri, string apiKey, HttpMessageHandler? handler = null)
    {
        // Убираем завершающий '/' на случай, если его передали
        _baseUri = baseUri.TrimEnd('/');
        // Если передан кастомный handler (например для mock в юнит-тестах), используем его
        _http = handler is null ? new HttpClient() : new HttpClient(handler);
        // Устанавливаем заголовок Authorization: Bearer {apiKey}
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        // Вариант, если нужен собственный заголовок:
        // _http.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
    }

    /// <summary>
    /// Помещает новый заказ в постамат.
    /// </summary>
    /// <param name="json">Тело запроса в виде JSON-строки</param>
    /// <returns>0 при успехе, иначе числовой код HTTP-статуса</returns>
    public int InsertOrder(string json)
    {
        // Собираем полный URI для вставки заказа
        var uri = $"{_baseUri}/auth/postamat/insert";
        // Упаковываем тело в StringContent с кодировкой UTF-8 и media-type "application/json"
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        // Делаем синхронный POST-запрос и сразу получаем ответ
        using var resp = _http.PostAsync(uri, content).GetAwaiter().GetResult();
        // Если код 2xx => считаем успехом (возвращаем 0), иначе возвращаем числовой статус
        return resp.IsSuccessStatusCode ? 0 : (int)resp.StatusCode;
    }

    /// <summary>
    /// Извлекает просроченный заказ из постамата.
    /// </summary>
    /// <param name="json">Тело запроса в виде JSON-строки</param>
    /// <returns>0 при успехе, иначе числовой код HTTP-статуса</returns>
    public int RetrieveExpiredOrder(string json)
    {
        // Аналогично InsertOrder, но другой endpoint
        var uri = $"{_baseUri}/auth/postamat/retrieve";
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var resp = _http.PostAsync(uri, content).GetAwaiter().GetResult();
        return resp.IsSuccessStatusCode ? 0 : (int)resp.StatusCode;
    }

    /// <summary>
    /// Запрашивает список свободных ячеек постамата.
    /// </summary>
    /// <returns>JSON-строка с массивом или объектом свободных ячеек</returns>
    public string GetFreeCells()
    {
        // URI для запроса доступных ячеек
        var uri = $"{_baseUri}/auth/postamat/available";
        // GET-запрос
        using var resp = _http.GetAsync(uri).GetAwaiter().GetResult();
        // Бросаем исключение, если статус не 2xx
        resp.EnsureSuccessStatusCode();
        // Читаем тело ответа как строку
        return resp.Content.ReadAsStringAsync().GetAwaiter().GetResult()!;
    }

    /// <summary>
    /// Освобождаем ресурсы HttpClient.
    /// </summary>
    public void Dispose() => _http.Dispose();
}