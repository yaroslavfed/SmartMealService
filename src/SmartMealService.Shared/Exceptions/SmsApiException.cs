namespace SmartMealService.Shared.Exceptions;

/// <summary>
/// Исключение для бизнес-ошибок SMS API, когда транспортный запрос выполнен, но сервер вернул Success = false.
/// </summary>
public class SmsApiException(string message) : Exception(message);
