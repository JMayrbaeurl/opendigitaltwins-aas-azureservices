using Newtonsoft.Json.Linq;

namespace AAS.API.Repository
{
    public interface ObjectMapperStrategy<T>
    {
        public T map(JObject obj);
    }
}
