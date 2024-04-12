﻿using System.Diagnostics.CodeAnalysis;

namespace Skylight.API.Net.EndPoint;

public interface INetworkEndPointParser
{
	public bool TryParse(string value, [NotNullWhen(true)] out INetworkEndPoint? endPoint);
}
