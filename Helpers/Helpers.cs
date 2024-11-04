
using Microsoft.AspNetCore.StaticFiles;

namespace MFEServer.Helpers;

public static class Helpers
{

  public static string GetContentType(string filePath)
  {
    var provider = new FileExtensionContentTypeProvider();
    if (!provider.TryGetContentType(filePath, out var contentType))
    {
      contentType = "application/octet-stream";
    }
    return contentType;
  }
}