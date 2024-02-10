using ParseBackend.Models.Other.Database.Athena;
using static ParseBackend.Global;

namespace ParseBackend.Services
{
    public interface ICahceService
    {
        public Task Loop();
    }

    public class CahceService : ICahceService
    {
        private readonly IMongoService _mongoService;

        public CahceService(IMongoService mongoService)
        {
            _mongoService = mongoService;
        }

        public async Task Loop()
        {
            while (true)
            {
                var datetime = DateTime.Now;
                await Task.Delay(15000); // wait 10m

                foreach (var profile in GlobalCacheProfiles)
                    if (profile.Value.LastChanges < datetime)
                    {
                        _mongoService.SaveAllProfileData(profile.Key, profile.Value);
                        GlobalCacheProfiles.Remove(profile.Key);
                    }
            }
        }
    }
}
