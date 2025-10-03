using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace MyLibrary
{
    public interface IParcel
    {
        int InsertOrder(string json);
        int RetrieveExpiredOrder(string json);
        string GetFreeCells();
    }

    /// <summary>
    /// Клиент для взаимодействия с API постамата.
    /// Реализует IParcel и IDisposable.
    /// Вход и выход – JSON-строки, внутри они парсятся и «нормализуются».
    /// </summary>
    public class PostamatClient : IParcel, IDisposable
    {
        private readonly HttpClient _http;  // HTTP-клиент для запросов
        private readonly string _baseUri;  // Базовый URL API


        // Опции сериализации и десериализации JSON:
        // camelCase для имен свойств, игнорируем null, не форматируем отступами.
        private readonly JsonSerializerOptions _jsonOpts = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };

        /// <summary>
        /// Конструктор.
        /// Устанавливает базовый URI, авторизационный заголовок и (опционально) кастомный HttpMessageHandler.
        /// </summary>
        public PostamatClient(string baseUri, string apiKey, HttpMessageHandler? handler = null)
        {
            _baseUri = baseUri.TrimEnd('/'); // Убираем завершающий '/'
            _http = handler is null ? new HttpClient() : new HttpClient(handler);
            // Добавляем заголовок Authorization: Bearer {apiKey}
            _http.DefaultRequestHeaders.Authorization =
              new AuthenticationHeaderValue("Bearer", apiKey);
        }

        /// <summary>
        /// Положить заказ в ячейку.
        /// </summary>
        /// <param name="json">Готовая JSON-строка, возможно с числовыми и булевыми значениями в кавычках.</param>
        /// <returns>0 – успех, иначе код HTTP-статуса ошибки.</returns>
        public int InsertOrder(string json)
        {
            // Парсинг и нормализация JSON-запроса: строки-числа → реальные числа, строки-"true"/"false" → bool
            string payload = NormalizeJson(json);

            // Формирование URI и HttpContent
            var uri = $"{_baseUri}/auth/postamat/insert";
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            // Отправка POST и синхронное ожидание результата
            using var resp = _http.PostAsync(uri, content).GetAwaiter().GetResult();

            // Возвращаем 0 при успехе, иначе числовой код статуса
            return resp.IsSuccessStatusCode
                   ? 0
                   : (int)resp.StatusCode;
        }

        /// <summary>
        /// Извлечь просроченный заказ из ячейки.
        /// </summary>
        /// <param name="json">Готовая JSON-строка с идентификаторами заказа и пр.</param>
        /// <returns>0 – успех, иначе код HTTP-статуса ошибки.</returns>
        public int RetrieveExpiredOrder(string json)
        {
            // Нормализация входного JSON
            string payload = NormalizeJson(json);

            var uri = $"{_baseUri}/auth/postamat/retrieve";
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            using var resp = _http.PostAsync(uri, content).GetAwaiter().GetResult();

            return resp.IsSuccessStatusCode
                   ? 0
                   : (int)resp.StatusCode;
        }

        /// <summary>
        /// Получить список свободных ячеек постамата.
        /// </summary>
        /// <returns>Нормализованная JSON-строка вида {"cells":["a100","a101",...]}</returns>
        public string GetFreeCells()
        {
            var uri = $"{_baseUri}/auth/postamat/available";
            using var resp = _http.GetAsync(uri).GetAwaiter().GetResult();
            resp.EnsureSuccessStatusCode();

            // читаем «сырую» строку
            string raw = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            // Нормализуем JSON-ответ перед возвратом
            return NormalizeJson(raw);
        }

        /// <summary>
        /// Освобождение ресурсов HttpClient.
        /// </summary>
        public void Dispose() => _http.Dispose();

        // ------------------------------------------------------------------
        // Вспомогательный метод: приводит любую JSON-строку к «правильному» виду,
        // превращая строковые числа и булевы в реальные JSON-типизированные узлы.
        // ------------------------------------------------------------------
        private string NormalizeJson(string json)
        {
            // Парсим строку в дерево JsonNode
            JsonNode? root = JsonNode.Parse(json);
            if (root is not null)
                NormalizeNode(root); // Рекурсивное преобразование узлов

            // Сериализуем обратно в JSON-строку по нашим опциям
            return root?.ToJsonString(_jsonOpts) ?? json;
        }

        // Рекурсивная обработка узла JsonNode
        private void NormalizeNode(JsonNode node)
        {
            switch (node)
            {
                case JsonObject obj:
                    // ToList() нужен, чтобы безопасно менять свойства во время обхода
                    foreach (var kv in obj.ToList())
                    {
                        var child = kv.Value!;
                        // Если это JsonValue и внутри – строка
                        if (child is JsonValue val && val.TryGetValue<string>(out var s))
                        {
                            // Пробуем привести к int
                            if (Int32.TryParse(s, out var i))
                                obj[kv.Key] = JsonValue.Create(i);
                            // Или к double
                            else if (Double.TryParse(s, out var d))
                                obj[kv.Key] = JsonValue.Create(d);
                            // Или к bool
                            else if (Boolean.TryParse(s, out var b))
                                obj[kv.Key] = JsonValue.Create(b);
                            // Иначе оставляем в виде строки
                        }
                        else
                        {
                            // Рекурсивно обрабатываем вложенные объекты/массивы
                            NormalizeNode(child);
                        }
                    }
                    break;

                case JsonArray arr:
                    for (int i = 0; i < arr.Count; i++)
                    {
                        var child = arr[i]!;
                        if (child is JsonValue val && val.TryGetValue<string>(out var s))
                        {
                            if (Int32.TryParse(s, out var ii))
                                arr[i] = JsonValue.Create(ii);
                            else if (Double.TryParse(s, out var dd))
                                arr[i] = JsonValue.Create(dd);
                            else if (Boolean.TryParse(s, out var bb))
                                arr[i] = JsonValue.Create(bb);
                        }
                        else
                        {
                            NormalizeNode(child);
                        }
                    }
                    break;

                // JsonValue, где уже число или булево, оставляем «как есть»
                case JsonValue:
                    break;
            }
        }
    }
}