using ALM.Entities;

using System.Collections.Generic;

namespace ALM
{
    public interface IRepository
    {
        Primitive GetByGuid(string guid, string horizon);
        string GetLongGeometry(Primitive primitive, string horizon);
        IEnumerable<Primitive> GetAllFullPolylines(int limit = int.MaxValue);
    }
}
