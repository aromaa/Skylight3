using Skylight.API.DependencyInjection;

namespace Skylight.API.Game.Figure;

public interface IFigureConfigurationManager : IFigureConfiguration, ILoadableService<IFigureConfigurationSnapshot>;
