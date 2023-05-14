using EthereumT.DAL.Context;
using EthereumT.Domain.Base.Repositories.Base;
using EthereumT.Domain.Entities.Base;
using Microsoft.EntityFrameworkCore;

namespace EthereumT.DAL.Repositories.Base
{
    public class RepositoryAsync<T> : IRepositoryAsync<T> where T : Entity, new()
    {
        private readonly AppDbContext _context;

        protected DbSet<T> Set { get; }

        protected virtual IQueryable<T> Items => Set;

        public RepositoryAsync(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));

            Set = _context.Set<T>();
        }

        public async Task<IPage<T>> GetPageAsync(int pageIndex, int pageSize, CancellationToken cancel = default)
        {
            if (pageSize <= 0) return new Page(Enumerable.Empty<T>(), pageSize, pageIndex, pageSize);

            var totalCount = await Items.CountAsync(cancel).ConfigureAwait(false);

            if (totalCount == 0) return new Page(Enumerable.Empty<T>(), 0, pageIndex, pageSize);

            IQueryable<T> query = Items;

            if (query is not IOrderedQueryable<T>)
                query = query.OrderBy(x => x.Id);

            if (pageIndex > 0) query = query.Skip(pageIndex * pageSize);

            var items = await query.Take(pageSize).ToArrayAsync(cancel).ConfigureAwait(false);

            return new Page(items, totalCount, pageIndex, pageSize);
        }

        protected record Page(IEnumerable<T> Items, int TotalCount, int PageIndex, int PageSize) : IPage<T>;
    }
}