namespace MyLibrary
{
        public interface IParcel
        {
            /// <summary>
            /// Положить заказ в ячейку
            /// </summary>
            /// <param name="json">Данные, необходимые для передачи в постомат</param>
            /// <returns>Статус выполнения, 0 - ошибок нет, иначе код ошибки</returns>
            public int InsertOrder(string json);
            /// <summary>
            /// Извлечь просроченный заказ из ячейки
            /// </summary>
            /// <param name="json">Данные, необходимые для передачи в постомат</param>
            /// <returns>Статус выполнения, 0 - ошибок нет, иначе код ошибки</returns>
            public int RetrieveExpiredOrder(string json);
            /// <summary>
            /// Получить список свободных ячеек
            /// </summary>
            /// <returns>json вида {"cells":["a100", "a101", ...]}</returns>
            public string GetFreeCells();
        }
    
}
