using Newtonsoft.Json.Linq;

namespace Aas.Api.Repository.Mapper
{
    public interface ObjectMapperStrategy<T>
    {
        public T map(JObject obj);
    }
}
