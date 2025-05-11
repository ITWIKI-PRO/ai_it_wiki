using Swashbuckle.AspNetCore.Filters;

namespace ai_it_wiki.Models.Altron
{
  /// <summary>
  /// Стандартный ответ об успешном действии без дополнительных данных.</summary>
  public class GenericSuccessResponse
  {
    /// <summary>Признак успеха.</summary>
    /// <remarks>Всегда true.</remarks>
    public bool IsSuccess { get; }

    /// <summary>Сообщение или ошибка</summary>
    /// <remarks>Если IsSuccess = false, то это сообщение об ошибке.</remarks>
    public string Message { get; }

    /// <summary>Следующие рекомендуемые действия перед ответом пользователю.</summary>
    /// <remarks>Если null, то не будет рекомендованных действий. Если не null, то это строка с ID действия.</remarks>
    public string NextRecommendedLLMActions { get; internal set; }

    /// <summary>
    /// Конструктор для создания ответа-результата работы с сообщением и указанием к следующим действиям.
    /// </summary>
    /// <param name="message">Сообщение результата.</param>
    /// <param name="nextRecommendedLLMActions">Следующие рекомендуемые действия перед ответом пользователю.</param>
    /// <remarks> 
    /// Если next = null, то не будет рекомендованных действий.
    /// Если не null, то это строка с ID действия.
    /// </remarks>
    /// <param name="isSuccess">Признак успеха.</param>
    public GenericSuccessResponse(string message, bool isSuccess = true, string nextRecommendedLLMActions = null)
    {
      Message = message;
      NextRecommendedLLMActions = nextRecommendedLLMActions;
    }
  }

  // реализация интерфейса IExamplesProvider для примера ответа
  public class GenericSuccessResponseExample : IExamplesProvider<GenericSuccessResponse>
  {
    public GenericSuccessResponse GetExamples()
    {
      return new GenericSuccessResponse("Файл успешно изменен", nextRecommendedLLMActions: "ReadFile");
    }
  }
}
