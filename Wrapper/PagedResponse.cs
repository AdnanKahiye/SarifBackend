namespace Backend.Wrapper
{
    public class PagedResponse<T>
    {
        public IReadOnlyList<T> Data { get; private set; }

        public int Page { get; private set; }
        public int PageSize { get; private set; }

        public int TotalRecords { get; private set; }

        public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);

        public PagedResponse(List<T> data, int page, int pageSize, int totalRecords)
        {
            Data = data;
            Page = page;
            PageSize = pageSize;
            TotalRecords = totalRecords;
        }
    }
}