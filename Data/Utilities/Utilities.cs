using System;
using System.Collections.Generic;
using System.Linq;
using AForge;

namespace Data
{
	public static class Utilities
	{
        public static IEnumerable<IntPoint> rearrangeCorners(IEnumerable<IntPoint> corners)
        {
            float[] pointDistances = new float[4];
			
			var ret = corners.ToList();
			
            for (int x = 0; x < ret.Count(); x++)
            {
                IntPoint point = ret[x];												
                pointDistances[x] = point.DistanceTo((x == (ret.Count() - 1) ? ret[0] : ret[x + 1]));
            }

            float shortestDist = float.MaxValue;
            Int32 shortestSide = Int32.MaxValue;
			
            for (int x = 0; x < ret.Count(); x++)
            {
                if (pointDistances[x] < shortestDist)
                {
                    shortestSide = x;
                    shortestDist = pointDistances[x];
                }
            }
			
            if (shortestSide != 0 && shortestSide != 2)
            {				
                IntPoint endPoint = ret[0];
                ret.RemoveAt(0);
                ret.Add(endPoint);				
            }
			
			return ret;
        }
		
		public static double GetDeterminant(double x1, double y1, double x2, double y2)
        {
            return x1 * y2 - x2 * y1;
        }

        public static double GetArea(IList<IntPoint> vertices)
        {
            if (vertices.Count < 3)
            {
                return 0;
            }
            double area = GetDeterminant(vertices[vertices.Count - 1].X, vertices[vertices.Count - 1].Y, vertices[0].X, vertices[0].Y);
            for (int i = 1; i < vertices.Count; i++)
            {
                area += GetDeterminant(vertices[i - 1].X, vertices[i - 1].Y, vertices[i].X, vertices[i].Y);
            }
            return area / 2;
        }
	}
}

