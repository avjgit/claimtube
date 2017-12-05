using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using VideoAi.Model;

namespace VideoAi.Controllers
{
    public class StringTable
    {
        public string[] ColumnNames { get; set; }
        public string[,] Values { get; set; }
    }

    public class ImageResult
    {
        public string[] Categories { get; set; }
        public string[] Captions { get; set; }
        public string[] Tags { get; set; }
        public string PolicyType { get; set; }
        public string PlateNumber { get; set; }
    }

    [Produces("application/json")]
    [Route("api/image")]
    public class ImageController : Controller
    {
        // GET: api/image/process_image?url{url}
        [HttpGet("process_image")]
        public async Task<IActionResult> ProcessImage([FromQuery] string url)
        {
            var imageData = await MakeAnalysisRequest(url);
            imageData.PolicyType = await IdentifyPolicyType(imageData.Tags);
            try
            {
                imageData.PlateNumber = await GetPlateNumber(url);
            }
            catch (Exception)
            {
                
            }
            return Ok(imageData);
        }

        public async Task<string> GetPlateNumber(string url)
        {
            HttpClient client = new HttpClient();
            var bytes = await client.GetByteArrayAsync(url);
            var content = Convert.ToBase64String(bytes);
            var googleApi = new GoogleApiClient();
            var response = googleApi.GetVehiclePlateAsync(content).Result;

            return response.Replace("\n", "");
        }

        public async Task<string> IdentifyPolicyType(string[] tags)
        {
            //if (tags.Contains("car"))
            //{
            //    return "kasko";
            //}
            //if (tags.Contains("room"))
            //{
            //    return "home";
            //}

            var tagsCount = tags.Count();
            var values = new string[tagsCount, 2];

            for (int i = 0; i < tagsCount; i++)
            {
                values[i, 0] = tags[i];
                values[i, 1] = "";
            }

            using (var client = new HttpClient())
            {
                var scoreRequest = new
                {

                    Inputs = new Dictionary<string, StringTable>
                    {
                        {
                            "input1",
                            new StringTable
                            {
                                ColumnNames = new[]
                                {
                                    "Description", "PolicyType"
                                },
                                Values = values
                                //Values = new[,]
                                //{
                                //    // service documentation:
                                //    // https://studio.azureml.net/apihelp/workspaces/5137d13885304a649c232737fcde3a4e/webservices/dad1feca0be0497a971bcabf4c9bfd1a/endpoints/d5ffb6dc1a21464c8495c47c6d10ea43/score

                                //    // input structure: first is list of tags, second is empty element ("PolicyType")

                                //    {
                                //        String.Join(",", tags), String.Empty
                                //    }

                                //    // Option 1: one image - one list
                                //    // example for Kasko:
                                //    //{
                                //    //   "road, tree, outdoor", "" 
                                //    //}
                                //    // example for Household:
                                //    //{
                                //    //    "home, floor, bathroom", ""
                                //    //}

                                //    // can be comma or just string separated, does not matter

                                //    // Option 2: pass each keyword in separate element, 
                                //    // but then logic to process response (calculate average probability from each element?) required
                                //    //{ "road", "" },
                                //    //{ "tree", "" },
                                //    //{ "outdoor", "" }
                                //}
                            }
                        }
                    },
                    GlobalParameters = new Dictionary<string, string>()
                };

                const string apiKey = "ZCuzusdlqZbL3LUmTPP+9sVL/VQXIFlZ4XlJYeU0LjUJfGhPvsz7vmsHltHKA5DO7Ud743EgvsHXKUdjScw66g==";

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                client.BaseAddress = new Uri("https://ussouthcentral.services.azureml.net/workspaces/5137d13885304a649c232737fcde3a4e/services/d5ffb6dc1a21464c8495c47c6d10ea43/execute?api-version=2.0&details=true");

                var content = new StringContent(JsonConvert.SerializeObject(scoreRequest), Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync("", content);

                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();

                    var o = JObject.Parse(result);
                    try
                    {
                        var tmp = o["Results"]["output1"]["value"]["Values"];
                        var r1 = 0M;
                        var r = "";

                        foreach (var t in tmp)
                        {
                            var tmp1 = (decimal)t[2];
                            var tmp2 = (decimal)t[3];

                            if (tmp1 > tmp2
                                && tmp1 > r1)
                            {
                                r1 = tmp1;
                                r = (string) t[4];
                            }
                            else if(tmp2 > tmp1
                                   && tmp2 > r1)
                            {
                                r1 = tmp2;
                                r = (string)t[4];
                            }
                        }
                        
                        //var res = (string) o["Results"]["output1"]["value"]["Values"]
                        //    ?.LastOrDefault()
                        //    ?.LastOrDefault();
                        return r;
                    }
                    catch (Exception)
                    {
                        return result;
                    }

                }

                    string responseContent = await response.Content.ReadAsStringAsync();
                    return responseContent;
            }
       }

        /// <summary>
        /// Gets the analysis of the specified image file by using the Computer Vision REST API.
        /// </summary>
        /// <param name="url">The image url.</param>
        static async Task<ImageResult> MakeAnalysisRequest(string url)
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

                return new ImageResult
                {
                    Categories = imageBreakdown.Categories.Select(c => c.Name).ToArray(),
                    Captions = imageBreakdown.Description.Captions.Select(c => c.Text).ToArray(),
                    Tags = imageBreakdown.Description.Tags
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
