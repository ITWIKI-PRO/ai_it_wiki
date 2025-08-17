using System.Threading.Tasks;
using ai_it_wiki.Controllers;
using ai_it_wiki.Services.OpenAI;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace ai_it_wiki.Tests
{
  public class LLMOpenAPIControllerTests
  {
    [Fact]
    public async Task Completion_ReturnsOk()
    {
      var serviceMock = new Mock<IOpenAiService>();
      serviceMock.Setup(s => s.SendMessageAsync("ping")).ReturnsAsync("pong");

      var controller = new LLMOpenAPIController();
      var result = await controller.Completion("ping", serviceMock.Object);

      var okResult = Assert.IsType<OkObjectResult>(result.Result);
      Assert.Equal("pong", okResult.Value);
    }
  }
}
