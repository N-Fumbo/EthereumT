using EthereumT.Domain.Base.Entities.Base;

namespace EthereumT.Domain.Base.Repositories.Base
{
    public interface IRepositoryAsync<T> where T : IEntity
    {
        Task<IPage<T>> GetPageAsync(int pageIndex, int pageSize, CancellationToken cancel = default);
    }
}