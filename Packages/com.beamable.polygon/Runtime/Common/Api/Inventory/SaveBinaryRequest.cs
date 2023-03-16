using System;
using System.Collections.Generic;

namespace Beamable.Polygon.Common.Api.Inventory
{
	[Serializable]
	public class SaveBinaryRequest
	{
		public List<BinaryDefinition> binary;
	}
}