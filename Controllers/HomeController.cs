using Microsoft.AspNetCore.Mvc;
using System.Runtime.Versioning;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats;
using Image = SixLabors.ImageSharp.Image;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;

namespace ai_it_wiki.Controllers
{
  [ApiExplorerSettings(IgnoreApi = true)]
  public class HomeController : Controller
  {
    public IActionResult Index()
    {
      return View();
    }

    [Route("Privacy")]
    public IActionResult Privacy()
    {
      if (OperatingSystem.IsWindows())
      {
        //ImageProcessor.FixImagesFromAI();
        //ImageProcessor.AddLogoToImages();
      }
      return View();
    }

    [Route("About")]
    public IActionResult About()
    {
      return View();
    }

    [Route("Contact")]
    public IActionResult Contact()
    {
      return View();
    }
  }
}
