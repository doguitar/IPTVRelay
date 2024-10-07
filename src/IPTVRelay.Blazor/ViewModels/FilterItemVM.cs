namespace IPTVRelay.Blazor.ViewModels
{
    public class FilterItemVM<T>
    {
        public Guid Id { get; } = Guid.NewGuid();
        public T? Item { get; set; }
        public bool Filtered { get; set; }
        public override string ToString()
        {
            return Item?.ToString() ?? string.Empty;
        }
    }
}
