using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseNormalizer.DatabaseHandlers
{
    public interface IDatabaseHandler
    {
        List<string[]> GetDataFromDatabase(string databaseLocation, string[] columns, string queryConditions);
    }
}