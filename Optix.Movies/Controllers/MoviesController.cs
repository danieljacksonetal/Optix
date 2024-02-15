using MediatR;
using Microsoft.AspNetCore.Mvc;
using Optix.Movies.Infrastructure.Models;

namespace Optix.Movies.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MoviesController : ControllerBase
    {
        private readonly IMediator _mediator;
        public MoviesController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [HttpPost]
        public async Task<IActionResult> GetMovies(MoviesQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

    }
}
