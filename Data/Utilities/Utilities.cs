using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using AForge;
using AForge.Imaging.Filters;
using System.Drawing.Imaging;
using AForge.Imaging;
using AForge.Math.Geometry;
using System.Diagnostics;

namespace Data
{
	public static class Utilities
	{
        public static ReferenceCard MatchCard(MagicCard card, IEnumerable<ReferenceCard> referenceCards)
        {
            int cardTempId = 0;
            cardTempId++;
            // Write the image to disk to be read by the pHash library.. should really find
            // a way to pass a pointer to image data directly
            card.CardArtBitmap.Save("tempCard" + cardTempId + ".jpg", ImageFormat.Jpeg);


            // Calculate art bitmap hash
            UInt64 cardHash = 0;
            Phash.ph_dct_imagehash("tempCard" + cardTempId + ".jpg", ref cardHash);

            int lowestHamming = int.MaxValue;
            ReferenceCard bestMatch = null;

            foreach (ReferenceCard referenceCard in referenceCards)
            {
                int hamming = Phash.HammingDistance(referenceCard.pHash, cardHash);
                if (hamming < lowestHamming)
                {
                    lowestHamming = hamming;
                    bestMatch = referenceCard;
                }
            }

            Debug.WriteLine("Highest Similarity: " + bestMatch.name + " ID: " + bestMatch.cardId.ToString());                
            return bestMatch;
        }

        public static IEnumerable<IntPoint> RotateCorners(IEnumerable<IntPoint> corners)
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
                return 0;
            double area = GetDeterminant(vertices[vertices.Count - 1].X, vertices[vertices.Count - 1].Y, vertices[0].X, vertices[0].Y);
            
            for (int i = 1; i < vertices.Count; i++)
            {
                area += GetDeterminant(vertices[i - 1].X, vertices[i - 1].Y, vertices[i].X, vertices[i].Y);
            }
            return area / 2;
        }

        public static IEnumerable<MagicCard> DetectCardArt(Bitmap cameraBitmap)
        {
            var ret = new List<MagicCard>();
            
            var filteredBitmap = Grayscale.CommonAlgorithms.BT709.Apply(cameraBitmap);

            // edge filter
            var edgeFilter = new SobelEdgeDetector();
            edgeFilter.ApplyInPlace(filteredBitmap);

            // Threshhold filter
            var threshholdFilter = new Threshold(190);
            threshholdFilter.ApplyInPlace(filteredBitmap);


            var bitmapData = filteredBitmap.LockBits(
                new Rectangle(0, 0, filteredBitmap.Width, filteredBitmap.Height),
                ImageLockMode.ReadWrite, filteredBitmap.PixelFormat);

            var blobCounter = new BlobCounter();

            blobCounter.FilterBlobs = true;
            blobCounter.MinHeight = 125;
            blobCounter.MinWidth = 125;

            blobCounter.ProcessImage(bitmapData);
            var blobs = blobCounter.GetObjectsInformation();
            filteredBitmap.UnlockBits(bitmapData);

            var shapeChecker = new SimpleShapeChecker();
            var bm = new Bitmap(filteredBitmap.Width, filteredBitmap.Height, PixelFormat.Format24bppRgb);

            var cardPositions = new List<IntPoint>();
            foreach (var blob in blobs)
            {
                var edgePoints = blobCounter.GetBlobsEdgePoints(blob);

                List<IntPoint> corners;

                // only operate on 4 sided polygons
                if (shapeChecker.IsConvexPolygon(edgePoints, out corners))
                {
                    var subtype = shapeChecker.CheckPolygonSubType(corners);

                    if (corners.Count() != 4)
                        continue;

                    if (subtype != PolygonSubType.Parallelogram && subtype != PolygonSubType.Rectangle)
                        continue;

                    // if the image is sideways, rotate it so it'll match the DB card art
                    corners = Utilities.RotateCorners(corners).ToList();

                    if (Utilities.GetArea(corners) < 20000)
                        continue;

                    ret.Add( new MagicCard(cameraBitmap, corners));
                }
            }

            return ret;
        }
	}
}

