using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Globalization;
using System.Text.Json;

namespace Practice.Controllers.UnauthorizedControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlacesController : ControllerBase
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly IConfiguration _config;

        public PlacesController(IHttpClientFactory httpFactory, IConfiguration config)
        {
            _httpFactory = httpFactory;
            _config = config;
        }

        [HttpGet("nearby")]
        [ProducesResponseType(typeof(NearbyBusinessDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "поиск ближайшей точки",
            Description = "Возвращает название, категорию и адрес ближайшей точки в радиусе 50 метров на основе предоставленных широты и долготы. Если точка не найдена или произошла ошибка, возвращает null."
        )]
        public async Task<ActionResult<NearbyBusinessDTO?>> Nearby(
            [FromQuery] decimal lat,
            [FromQuery] decimal lng,
            CancellationToken ct)
        {
            var apiKey = _config["Dgis:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return Ok(null);
            }

            var url =
                "https://catalog.api.2gis.com/3.0/items" +
                $"?point={lng.ToString(CultureInfo.InvariantCulture)}," +
                $"{lat.ToString(CultureInfo.InvariantCulture)}" +
                "&radius=50&type=branch&fields=items.address,items.rubrics" +
                "&page_size=1&locale=ru_RU" +
                $"&key={apiKey}";

            try
            {
                var client = _httpFactory.CreateClient();
                var res = await client.GetAsync(url, ct);
                if (!res.IsSuccessStatusCode)
                {
                    return Ok(null);
                }

                using var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync(ct));
                var root = doc.RootElement;

                if (root.TryGetProperty("meta", out var meta) &&
                    meta.TryGetProperty("code", out var code) &&
                    code.GetInt32() >= 400)
                {
                    return Ok(null);
                }

                if (!root.TryGetProperty("result", out var result) ||
                    !result.TryGetProperty("items", out var items) ||
                    items.GetArrayLength() == 0)
                {
                    return Ok(null);
                }

                var item = items[0];

                string? name = item.TryGetProperty("name", out var n)
                    ? n.GetString()
                    : null;

                string? address = item.TryGetProperty("address_name", out var a)
                    ? a.GetString()
                    : null;

                string? category = null;
                if (item.TryGetProperty("rubrics", out var rubrics) && rubrics.GetArrayLength() > 0)
                {
                    category = rubrics[0].TryGetProperty("name", out var rubricName)
                        ? rubricName.GetString()
                        : null;
                }

                return Ok(new NearbyBusinessDTO
                {
                    Name = name,
                    Category = category,
                    Address = address
                });
            }
            catch
            {
                return Ok(null);
            }
        }
    }

    public class NearbyBusinessDTO
    {
        public string? Name { get; set; }
        public string? Category { get; set; }
        public string? Address { get; set; }
    }
}