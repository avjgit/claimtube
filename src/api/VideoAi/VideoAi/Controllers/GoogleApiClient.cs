using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace VideoAi.Controllers
{
    public class GoogleApiClient
    {
        public async Task<string> GetVehiclePlateAsync(string imageContent)
        {

            if (String.IsNullOrEmpty(imageContent))
            {
                throw new NullReferenceException("Image is not provided");
            }

            var requestContent = new StringContent(JsonConvert.SerializeObject(new RequestRootObject()
            {
                requests = new List<Request>()
                {
                    new Request()
                    {
                        image = new Image
                        {
                            content = imageContent
                        },
                        features = new List<Feature>
                        {
                            new Feature()
                            {
                                type = "TYPE_UNSPECIFIED",
                                maxResults = 50
                            },
                            new Feature()
                            {
                                type = "LANDMARK_DETECTION",
                                maxResults = 50
                            },
                            new Feature()
                            {
                                type = "FACE_DETECTION",
                                maxResults = 50
                            },
                            new Feature()
                            {
                                type = "LOGO_DETECTION",
                                maxResults = 50
                            },
                            new Feature()
                            {
                                type = "LABEL_DETECTION",
                                maxResults = 50
                            },
                            new Feature()
                            {
                                type = "DOCUMENT_TEXT_DETECTION",
                                maxResults = 50
                            },
                            new Feature()
                            {
                                type = "SAFE_SEARCH_DETECTION",
                                maxResults = 50
                            },
                            new Feature()
                            {
                                type = "IMAGE_PROPERTIES",
                                maxResults = 50
                            },
                            new Feature()
                            {
                                type = "CROP_HINTS",
                                maxResults = 50
                            },
                            new Feature()
                            {
                                type = "WEB_DETECTION",
                                maxResults = 50
                            }
                        },
                        imageContext = new ImageContext()
                        {
                            cropHintsParams = new CropHintsParams()
                            {
                                aspectRatios = new List<double>()
                                {
                                    0.8,
                                    1,
                                    1.2
                                }
                            }
                        }
                    }
                }
            }), Encoding.UTF8, "application/json");

            var client = new HttpClient()
            {
                BaseAddress = new Uri("https://cxl-services.appspot.com")
            };

            var response = await client.PostAsync("/proxy?url=https%3A%2F%2Fvision.googleapis.com%2Fv1%2Fimages%3Aannotate", requestContent);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"{response.ReasonPhrase}");
            }
            var data = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ResponseRootObject>(data)
                .responses
                .Select(r => r.textAnnotations.Select(a => a.description))
                .First().OrderByDescending(p => p.Length).First();



        }
    }

    public class Image
    {
        public string content { get; set; }
    }

    public class Feature
    {
        public string type { get; set; }
        public int maxResults { get; set; }
    }

    public class CropHintsParams
    {
        public List<double> aspectRatios { get; set; }
    }

    public class ImageContext
    {
        public CropHintsParams cropHintsParams { get; set; }
    }

    public class Request
    {
        public Image image { get; set; }
        public List<Feature> features { get; set; }
        public ImageContext imageContext { get; set; }
    }

    public class RequestRootObject
    {
        public List<Request> requests { get; set; }
    }

    public class LabelAnnotation
    {
        public string mid { get; set; }
        public string description { get; set; }
        public double score { get; set; }
    }

    public class Vertex
    {
        public int x { get; set; }
        public int y { get; set; }
    }

    public class BoundingPoly
    {
        public List<Vertex> vertices { get; set; }
    }

    public class TextAnnotation
    {
        public string locale { get; set; }
        public string description { get; set; }
        public BoundingPoly boundingPoly { get; set; }
    }

    public class SafeSearchAnnotation
    {
        public string adult { get; set; }
        public string spoof { get; set; }
        public string medical { get; set; }
        public string violence { get; set; }
    }

    public class Color2
    {
        public int red { get; set; }
        public int green { get; set; }
        public int blue { get; set; }
    }

    public class Color
    {
        public Color2 color { get; set; }
        public double score { get; set; }
        public double pixelFraction { get; set; }
    }

    public class DominantColors
    {
        public List<Color> colors { get; set; }
    }

    public class ImagePropertiesAnnotation
    {
        public DominantColors dominantColors { get; set; }
    }

    public class Vertex2
    {
        public int x { get; set; }
        public int? y { get; set; }
    }

    public class BoundingPoly2
    {
        public List<Vertex2> vertices { get; set; }
    }

    public class CropHint
    {
        public BoundingPoly2 boundingPoly { get; set; }
        public double confidence { get; set; }
        public double importanceFraction { get; set; }
    }

    public class CropHintsAnnotation
    {
        public List<CropHint> cropHints { get; set; }
    }

    public class DetectedLanguage
    {
        public string languageCode { get; set; }
        public int confidence { get; set; }
    }

    public class Property
    {
        public List<DetectedLanguage> detectedLanguages { get; set; }
    }

    public class Vertex3
    {
        public int x { get; set; }
        public int y { get; set; }
    }

    public class BoundingBox
    {
        public List<Vertex3> vertices { get; set; }
    }

    public class Vertex4
    {
        public int x { get; set; }
        public int y { get; set; }
    }

    public class BoundingBox2
    {
        public List<Vertex4> vertices { get; set; }
    }

    public class DetectedLanguage2
    {
        public string languageCode { get; set; }
    }

    public class Property2
    {
        public List<DetectedLanguage2> detectedLanguages { get; set; }
    }

    public class Vertex5
    {
        public int x { get; set; }
        public int y { get; set; }
    }

    public class BoundingBox3
    {
        public List<Vertex5> vertices { get; set; }
    }

    public class DetectedLanguage3
    {
        public string languageCode { get; set; }
    }

    public class DetectedBreak
    {
        public string type { get; set; }
    }

    public class Property3
    {
        public List<DetectedLanguage3> detectedLanguages { get; set; }
        public DetectedBreak detectedBreak { get; set; }
    }

    public class Vertex6
    {
        public int x { get; set; }
        public int y { get; set; }
    }

    public class BoundingBox4
    {
        public List<Vertex6> vertices { get; set; }
    }

    public class Symbol
    {
        public Property3 property { get; set; }
        public BoundingBox4 boundingBox { get; set; }
        public string text { get; set; }
    }

    public class Word
    {
        public Property2 property { get; set; }
        public BoundingBox3 boundingBox { get; set; }
        public List<Symbol> symbols { get; set; }
    }

    public class Paragraph
    {
        public BoundingBox2 boundingBox { get; set; }
        public List<Word> words { get; set; }
    }

    public class Block
    {
        public BoundingBox boundingBox { get; set; }
        public List<Paragraph> paragraphs { get; set; }
        public string blockType { get; set; }
    }

    public class Page
    {
        public Property property { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public List<Block> blocks { get; set; }
    }

    public class FullTextAnnotation
    {
        public List<Page> pages { get; set; }
        public string text { get; set; }
    }

    public class WebEntity
    {
        public string entityId { get; set; }
        public double score { get; set; }
        public string description { get; set; }
    }

    public class FullMatchingImage
    {
        public string url { get; set; }
    }

    public class PartialMatchingImage
    {
        public string url { get; set; }
    }

    public class PagesWithMatchingImage
    {
        public string url { get; set; }
    }

    public class VisuallySimilarImage
    {
        public string url { get; set; }
    }

    public class WebDetection
    {
        public List<WebEntity> webEntities { get; set; }
        public List<FullMatchingImage> fullMatchingImages { get; set; }
        public List<PartialMatchingImage> partialMatchingImages { get; set; }
        public List<PagesWithMatchingImage> pagesWithMatchingImages { get; set; }
        public List<VisuallySimilarImage> visuallySimilarImages { get; set; }
    }

    public class Respons
    {
        public List<LabelAnnotation> labelAnnotations { get; set; }
        public List<TextAnnotation> textAnnotations { get; set; }
        public SafeSearchAnnotation safeSearchAnnotation { get; set; }
        public ImagePropertiesAnnotation imagePropertiesAnnotation { get; set; }
        public CropHintsAnnotation cropHintsAnnotation { get; set; }
        public FullTextAnnotation fullTextAnnotation { get; set; }
        public WebDetection webDetection { get; set; }
    }

    public class ResponseRootObject
    {
        public List<Respons> responses { get; set; }
    }
}
