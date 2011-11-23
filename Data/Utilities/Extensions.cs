using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Linq;
using AForge;


namespace Data
{
	public static class Extensions
	{
        // Convert list of AForge.NET's points to array of .NET points
        public static IEnumerable<System.Drawing.Point> ToPointsArray(this IEnumerable<IntPoint> points)
        {		
			return points.Select(p => new System.Drawing.Point(p.X, p.Y));
        }		
	}
}

