namespace DiscogsInsight.ViewModels.Results
{
    public class ViewResult<T>
    {
        public bool Success { get; set; }
        public required string ErrorMessage { get; set; }
        public T? Data { get; set; }
    }
}
