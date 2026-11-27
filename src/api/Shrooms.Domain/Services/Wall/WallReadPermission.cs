using Shrooms.Contracts.Constants;
using Shrooms.Contracts.Enums;

namespace Shrooms.Domain.Services.Wall
{
    /// <summary>
    /// The permission a user needs to read a wall's posts, and so to be told about them.
    /// Event walls are read on the event page, which roles such as External open with <see cref="BasicPermissions.Event"/> alone.
    /// </summary>
    public static class WallReadPermission
    {
        public static string For(WallType wallType)
        {
            return wallType == WallType.Events ? BasicPermissions.Event : BasicPermissions.Wall;
        }
    }
}
