using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace api.Helpers;

public class PagedList<T> : List<T> // Generic (Type Agnostic)
{
    public PagedList() { }
    
    // set props values
    private PagedList(IEnumerable<T> items, int itemsCount, int pageNumber, int pageSize)
    {
        CurrentPage = pageNumber;
        TotalPages = (int)Math.Ceiling(itemsCount / (double)pageSize); // 10 items, 3 pageSize => 4 total pages
        PageSize = pageSize;
        TotalItems = itemsCount;
        AddRange(items);
    }

    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }

    /// <summary>
    /// Call MongoDB collection and get a limited number of items based on the pageSize and pageNumber.
    /// </summary>
    /// <param name="query"></param>: getting a query to use agains MongoDB _collection
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>PageList<T> object with its prop values</returns>
    public static async Task<PagedList<T>> CreatePagedListAsync(IMongoQueryable<T>? query, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        int count = await query.CountAsync<T>(cancellationToken);

        IEnumerable<T> items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedList<T>(items, count, pageNumber, pageSize);
    }
}
