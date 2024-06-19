using Microsoft.EntityFrameworkCore;
using Skylight.Domain.Achievements;
using Skylight.Domain.Badges;
using Skylight.Domain.Catalog;
using Skylight.Domain.Furniture;
using Skylight.Domain.Items;
using Skylight.Domain.Navigator;
using Skylight.Domain.Recycler.FurniMatic;
using Skylight.Domain.Rooms;
using Skylight.Domain.Rooms.Layout;
using Skylight.Domain.Rooms.Sound;
using Skylight.Domain.Server;
using Skylight.Domain.Users;
using Skylight.Infrastructure.EntityConfigurations.Achievements;
using Skylight.Infrastructure.EntityConfigurations.Badges;
using Skylight.Infrastructure.EntityConfigurations.Catalog;
using Skylight.Infrastructure.EntityConfigurations.Furniture;
using Skylight.Infrastructure.EntityConfigurations.Items;
using Skylight.Infrastructure.EntityConfigurations.Navigator;
using Skylight.Infrastructure.EntityConfigurations.Recycler.FurniMatic;
using Skylight.Infrastructure.EntityConfigurations.Room;
using Skylight.Infrastructure.EntityConfigurations.Room.Layout;
using Skylight.Infrastructure.EntityConfigurations.Room.Sound;
using Skylight.Infrastructure.EntityConfigurations.Server;
using Skylight.Infrastructure.EntityConfigurations.Users;

namespace Skylight.Infrastructure;

public sealed class SkylightContext(DbContextOptions<SkylightContext> options) : BaseSkylightContext(options)
{
}
