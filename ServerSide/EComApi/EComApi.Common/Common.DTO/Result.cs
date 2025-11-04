

namespace EComApi.Common.Common.DTO
{
    public abstract class Result
    {
        public List<Error> Errors { get; set; } = new();
        public bool isError => Errors != null && Errors.Any();

    }
    public class Result<T> : Result
    {
        public T Response { get; set; }
        public string WarningMessage { get; set; }
    }
}
