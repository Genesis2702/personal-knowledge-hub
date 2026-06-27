namespace PersonalKnowledgeHub.Common
{
    public class PageResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int PageCount { get; set; }
        public bool HasNextPage => PageIndex < PageCount;
        public bool HasPreviousPage => PageIndex > 1;
    }
}
