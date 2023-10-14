namespace CityInfo.API.Services
{
    public class PaginationMetadata
    {
        public int TotalItemsCount { get; set; }
        public int TotalPageCount { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public PaginationMetadata(int totalItemsCount, int pageSize, int currentPage)
        {
            TotalItemsCount = totalItemsCount;
            PageSize = pageSize;
            CurrentPage = currentPage;
            TotalPageCount = (int)Math.Ceiling(totalItemsCount/(double)PageSize);
        }
    }
}
