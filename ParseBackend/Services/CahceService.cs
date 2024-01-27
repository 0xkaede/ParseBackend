using ParseBackend.Models.Database.Athena;
using ParseBackend.Models.Database.CommonCore;
using ParseBackend.Utils;

namespace ParseBackend.Services
{
    public interface ICahceService
    {

    }

    public class CahceService : ICahceService //cba rn do later on
    {
        public Dictionary<string, AthenaData> AthenaDataGlobal { get; set; } = new Dictionary<string, AthenaData>();
        public Dictionary<string, CommonCoreData> CommonCoreDataGlobal { get; set; } = new Dictionary<string, CommonCoreData>();

        public AthenaData GetAthenaUser(string accountId)
        {
            var athena = AthenaDataGlobal[accountId];

            Logger.Log(athena is null ? "No user found in cache" : "User found in cache");

            return athena is null ? null : athena;
        }

        public void SaveAthenaUser(string accountId, AthenaData athenaData)
        {
            AthenaDataGlobal[accountId] = athenaData;
        }
    }
}
