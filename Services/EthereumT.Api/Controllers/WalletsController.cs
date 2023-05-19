using EthereumT.Api.Models.Dto;
using EthereumT.DAL.Repositories.Base;
using EthereumT.Domain.Base.Entities;
using EthereumT.Domain.Base.Repositories.Base;
using EthereumT.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace EthereumT.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WalletsController : ControllerBase
    {
        private readonly IMemoryCache _cache;

        public WalletsController(IMemoryCache cache)
        {
            _cache = cache;
        }

        [HttpGet(nameof(GetPageWallersSortBalance))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IPage<WalletDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(IPage<WalletDto>))]
        public IActionResult GetPageWallersSortBalance([FromQuery] int pageIndex, [FromQuery] int pageSize)
        {
            if (pageSize <= 0 || pageIndex < 0) return NotFound(new Page<Wallet>(Enumerable.Empty<Wallet>(), pageSize, pageIndex, pageSize));

            if (_cache.TryGetValue("IsActualWalletsSortBalance", out bool isActualSortBalance))
            {
                if(!isActualSortBalance is false)
                {
                    if (_cache.TryGetValue("Wallets", out Dictionary<string, WalletDto> wallets))
                    {
                        IEnumerable<WalletDto> walletsNewSortBalance = wallets.Select(x => x.Value).OrderByDescending(x => x.Balance);
                        _cache.Set("WalletsSortBalance", walletsNewSortBalance);
                        _cache.Set("IsActualWalletsSortBalance", true);
                    }
                }

                if (_cache.TryGetValue("WalletsSortBalance", out IEnumerable<WalletDto> walletsSortBalance))
                {
                    IEnumerable<WalletDto> result = walletsSortBalance.Skip(pageIndex * pageSize).Take(pageSize);
                    return Ok(new Page<WalletDto>(result, walletsSortBalance.Count(), pageIndex, pageSize));
                }
            }

            return NotFound(new Page<Wallet>(Enumerable.Empty<Wallet>(), pageSize, pageIndex, pageSize));
        }
    }
}