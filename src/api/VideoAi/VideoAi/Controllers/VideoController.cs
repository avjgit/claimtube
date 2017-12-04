using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace VideoAi.Controllers
{
    [Produces("application/json")]
    [Route("api/video")]
    public class VideoController : Controller
    {
        private static HttpClient CreateClient()
        {
            var client = new
                HttpClient
                {
                    BaseAddress = new Uri("https://videobreakdown.azure-api.net/Breakdowns/Api/Partner/Breakdowns/")
                };

            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "a8eb463602114ea2ade37fc4ad0a73eb");

            return client;
        }

        // GET: api/video/5
        [HttpGet("{id}", Name = "Get")]
        public IActionResult Get(int id)
        {
            var stream = System.IO.File.OpenRead($"{id}");
            var response = File(stream, "application/octet-stream");
            return response;
        }

        // GET: api/video/5/state
        [HttpGet("{id}/state")]
        public async Task<IActionResult> State(string id)
        {
            var response = await CreateClient()
                .GetStringAsync(id);

            var videoBreakdown = (VideoBreakdown)JsonConvert.DeserializeObject(response, typeof(VideoBreakdown));
            var breakdown = videoBreakdown.Breakdowns.First();
            return Ok(new
            {
                breakdown.State,
                breakdown.ProcessingProgress,
                videoBreakdown.DurationInSeconds,
                videoBreakdown.SummarizedInsights.ThumbnailUrl,
                tags = breakdown.Insights.TranscriptBlocks
                    .SelectMany(b => b.Annotations)
                    .Select(a => a.Name)
            });
        }

        // POST: api/video
        [HttpPost]
        public async Task Post()
        {
            if (!IsMultipartContentType(HttpContext.Request.ContentType))
            {
                return;
            }

            var boundary = GetBoundary(HttpContext.Request.ContentType);
            var reader = new MultipartReader(boundary, HttpContext.Request.Body);
            var section = await reader.ReadNextSectionAsync();

            try
            {
                while (section != null)
                {
                    // process each image
                    const int chunkSize = 1024;
                    var buffer = new byte[chunkSize];
                    var fileName = "1";//GetFileName(section.ContentDisposition);

                    using (var stream = new FileStream($"{fileName}", FileMode.Create))
                    {
                        int bytesRead;
                        do
                        {
                            bytesRead = await section.Body.ReadAsync(buffer, 0, buffer.Length);
                            stream.Write(buffer, 0, bytesRead);

                        } while (bytesRead > 0);
                    }

                    section = await reader.ReadNextSectionAsync();

                    var response = await CreateClient().PostAsync("?name=test1&privacy=Private&videoUrl=http://hackif2017agapi.azurewebsites.net/api/video/1", null);
                    var contents = await response.Content.ReadAsStringAsync();

                    await HttpContext.Response.WriteAsync(contents);
                }
            }
            catch (Exception ex)
            {
                await HttpContext.Response.WriteAsync(ex.Message);
            }
        }

        private static bool IsMultipartContentType(string contentType)
        {
            return
                !string.IsNullOrEmpty(contentType) &&
                contentType.IndexOf("multipart/", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string GetBoundary(string contentType)
        {
            var elements = contentType.Split(' ');
            var element = elements.First(entry => entry.StartsWith("boundary="));
            var boundary = element.Substring("boundary=".Length);
            // Remove quotes
            if (boundary.Length >= 2 && boundary[0] == '"' &&
                boundary[boundary.Length - 1] == '"')
            {
                boundary = boundary.Substring(1, boundary.Length - 2);
            }
            return boundary;
        }

        private static string GetFileName(string contentDisposition)
        {
            return contentDisposition
                .Split(';')
                .SingleOrDefault(part => part.Contains("filename"))?
                .Split('=')
                .Last()
                .Trim('"');
        }
    }
}
