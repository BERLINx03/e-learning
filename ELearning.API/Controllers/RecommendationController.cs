using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ELearning.Services;

namespace ELearning.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecommendationController : ControllerBase
    {
        private readonly RecommendationService _recommendationService;

        public RecommendationController(RecommendationService recommendationService)
        {
            _recommendationService = recommendationService;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> Get(int userId)
        {
            var result = await _recommendationService.GetRecommendationsAsync(userId);
            return Ok(result);
        }
    }
}
