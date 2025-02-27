using System.Collections.Concurrent;
using Final.Models.LiveData;

namespace Final.Services
{
    public static class LiveDataStore
    {
        // Live data for tools, indexed by a numeric key.
        public static ConcurrentDictionary<int, GenericToolModel> ToolData
            = new ConcurrentDictionary<int, GenericToolModel>();

        // Live data for cards/rooms, indexed by a numeric key.
        public static ConcurrentDictionary<int, GenericCardModel> LiveRoomData
            = new ConcurrentDictionary<int, GenericCardModel>();
    }
}
