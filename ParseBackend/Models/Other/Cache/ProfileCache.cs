using ParseBackend.Models.Other.Database;
using ParseBackend.Models.Other.Database.Athena;
using ParseBackend.Models.Other.Database.CommonCore;
using ParseBackend.Models.Other.Database.Other;

namespace ParseBackend.Models.Other.Cache
{
    public class ProfileCache
    {
        public DateTime LastChanges {  get; set; }
        public UserData UserData { get; set; }
        public AthenaData AthenaData { get; set; }
        public CommonCoreData CommonData { get; set; }
        public FriendsData FriendsData { get; set; }
    }
}
