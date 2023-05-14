using EthereumT.DAL.Repositories.Base;
using EthereumT.Domain.Base.Repositories.Base;
using EthereumT.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace EthereumT.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WalletsController : ControllerBase
    {
        private readonly RepositoryAsync<Wallet> _walletRepository;

        public WalletsController(RepositoryAsync<Wallet> walletsRepository)
        {
            _walletRepository = walletsRepository;
        }

        [HttpGet("get_page", Name = nameof(GetPage))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IPage<Wallet>))]
        public async Task<IActionResult> GetPage([FromQuery] int pageIndex, [FromQuery] int pageSize)
        {
            IPage<Wallet> page = await _walletRepository.GetPageAsync(pageIndex, pageSize);
            return Ok(page);
        }
    }
}