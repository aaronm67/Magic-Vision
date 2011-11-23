using System;
using System.Collections.Generic;

namespace Data
{
	public interface CardStore
	{
        IEnumerable<ReferenceCard> GetCards();
        void UpdateHash(string id, ulong phash);
	}
}

