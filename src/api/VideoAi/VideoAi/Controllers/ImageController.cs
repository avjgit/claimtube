using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using VideoAi.Model;

namespace VideoAi.Controllers
{
    [Produces("application/json")]
    [Route("api/image")]
    public class ImageController : Controller
    {
        // GET: api/image/process_image?url{url}
        [HttpGet("process_image")]
        public async Task<IActionResult> ProcessImage([FromQuery] string url)
        {
            var response = await MakeAnalysisRequest(url);

            return Ok(response);
        }

        /// <summary>
        /// Gets the analysis of the specified image file by using the Computer Vision REST API.
        /// </summary>
        /// <param name="url">The image url.</param>
        static async Task<object> MakeAnalysisRequest(string url)
        {
            HttpClient client = new HttpClient();

            // Request headers.
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "33bf53fcbfd74c9a878519518d97f574");

            // Request parameters. A third optional parameter is "details".
            string requestParameters = "visualFeatures=Categories,Description,Color&language=en";

            // Assemble the URI for the REST API Call.
            string uri = "https://westcentralus.api.cognitive.microsoft.com/vision/v1.0/analyze" + "?" + requestParameters;

            // Request body. Posts a locally stored JPEG image.
            byte[] byteData = GetImageAsByteArray(url)
                .Result;

            using (ByteArrayContent content = new ByteArrayContent(byteData))
            {
                // This example uses content type "application/octet-stream".
                // The other content types you can use are "application/json" and "multipart/form-data".
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                // Execute the REST API call.
                var response = await client.PostAsync(uri, content);

                // Get the JSON response.
                string contentString = await response.Content.ReadAsStringAsync();

                var imageBreakdown = (ImageBreakdown)JsonConvert.DeserializeObject(contentString, typeof(ImageBreakdown));

                return new
                {
                    categories = imageBreakdown.Categories.Select(c => c.Name),
                    captions = imageBreakdown.Description.Captions.Select(c => c.Text),
                    tags = imageBreakdown.Description.Tags
                };
            }
        }

        /// <summary>
        /// Returns the contents of the specified file as a byte array.
        /// </summary>
        /// <param name="url">The image file to read.</param>
        /// <returns>The byte array of the image data.</returns>
        static async Task<byte[]> GetImageAsByteArray(string url)
        {
            var client = new HttpClient();
            return await client.GetByteArrayAsync(url);
        }
    }
}
