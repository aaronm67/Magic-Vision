using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AForge;
using AForge.Imaging.Filters;

namespace Data
{
	public class MagicCard
	{
        private static IntPoint[] artCorners = new IntPoint[]{
            new IntPoint(14, 35),
            new IntPoint(193, 35), 
            new IntPoint(193, 168),
            new IntPoint(14, 168)
        };

        public ReferenceCard referenceCard { get; set; }
        public List<IntPoint> Corners { get; set; }
        public Bitmap CameraBitmap { get; private set; }
        public Bitmap CardBitmap { get; private set; }
        public Bitmap CardArtBitmap { get; private set; }

        public Bitmap GetDrawnCorners()
        {
            var ret = new Bitmap(this.CameraBitmap);
            Graphics g = Graphics.FromImage(ret);
            Pen p = Pens.Red;
            g.DrawPolygon(Pens.Red, this.Corners.Select(c => new System.Drawing.Point(c.X, c.Y)).ToArray());
            g.DrawPolygon(Pens.Blue, artCorners.Select(c => new System.Drawing.Point(c.X, c.Y)).ToArray());
            return ret;
        }

        public MagicCard(Bitmap cameraBitmap, IEnumerable<IntPoint> corners)
        {
            this.CameraBitmap = cameraBitmap;

            QuadrilateralTransformation transformFilter = new QuadrilateralTransformation(corners.ToList(), 211, 298);
            this.CardBitmap = transformFilter.Apply(cameraBitmap);

            QuadrilateralTransformation cardArtFilter = new QuadrilateralTransformation(artCorners.ToList(), 183, 133);
            this.CardArtBitmap = cardArtFilter.Apply(this.CardBitmap);

            this.Corners = corners.ToList();
        }
	}
}

