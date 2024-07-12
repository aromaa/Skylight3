using Microsoft.EntityFrameworkCore;

namespace Skylight.Infrastructure;

public sealed class SkylightContext(DbContextOptions<SkylightContext> options) : BaseSkylightContext(options);
