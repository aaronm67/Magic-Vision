/* Magic Vision
 * Created by Peter Simard
 * You are free to use this source code any way you wish, all I ask for is an attribution
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using DirectX.Capture;
using AForge;
using AForge.Imaging.Filters;
using AForge.Imaging;
using AForge.Math.Geometry;
using Data;

namespace PoolVision
{
    public partial class Form1 : Form
    {
		/*
        private Bitmap cameraBitmap;
        private Bitmap cameraBitmapLive;
        private Bitmap filteredBitmap;
        private Bitmap cardBitmap;
        private Bitmap cardArtBitmap;
        private String refCardDir = @"C:\Users\Pete\Pictures\New Phyrexia\Crops\";
        private Capture capture = null;
        private Filters cameraFilters = new Filters();
        private List<MagicCard> magicCards = new List<MagicCard>();
        private List<MagicCard> magicCardsLastFrame = new List<MagicCard>();
        private List<ReferenceCard> referenceCards = new List<ReferenceCard>();
        static readonly object _locker = new object();
        */
	
		public Data.CardStore sql;
			
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            foreach (ReferenceCard card in referenceCards)
            {
                Phash.ph_dct_imagehash(refCardDir + (String)card.dataRow["Set"] + "\\" + card.cardId + ".jpg", ref card.pHash);

                sql.UpdateHash(card.cardId, card.pHash);
            }
        }
		
		/*
        */

        private void detectQuads(Bitmap bitmap)
        {
            // Greyscale
            filteredBitmap = Grayscale.CommonAlgorithms.BT709.Apply(bitmap);

            // edge filter
            SobelEdgeDetector edgeFilter = new SobelEdgeDetector();
            edgeFilter.ApplyInPlace(filteredBitmap);

            // Threshhold filter
            Threshold threshholdFilter = new Threshold(190);
            threshholdFilter.ApplyInPlace(filteredBitmap);

            BitmapData bitmapData = filteredBitmap.LockBits(
                new Rectangle(0, 0, filteredBitmap.Width, filteredBitmap.Height),
                ImageLockMode.ReadWrite, filteredBitmap.PixelFormat);

 
            BlobCounter blobCounter = new BlobCounter();

            blobCounter.FilterBlobs = true;
            blobCounter.MinHeight = 125;
            blobCounter.MinWidth = 125;

            blobCounter.ProcessImage(bitmapData);
            Blob[] blobs = blobCounter.GetObjectsInformation();
            filteredBitmap.UnlockBits(bitmapData);

            SimpleShapeChecker shapeChecker = new SimpleShapeChecker();

            Bitmap bm = new Bitmap(filteredBitmap.Width, filteredBitmap.Height, PixelFormat.Format24bppRgb);

            Graphics g = Graphics.FromImage(bm);
            g.DrawImage(filteredBitmap, 0, 0);

            Pen pen = new Pen(Color.Red, 5);
            List<IntPoint> cardPositions = new List<IntPoint>();


            // Loop through detected shapes
            for (int i = 0, n = blobs.Length; i < n; i++)
            {
                List<IntPoint> edgePoints = blobCounter.GetBlobsEdgePoints(blobs[i]);
                List<IntPoint> corners;
                bool sameCard = false;

                // is triangle or quadrilateral
                if (shapeChecker.IsConvexPolygon(edgePoints, out corners))
                {
                    // get sub-type
                    PolygonSubType subType = shapeChecker.CheckPolygonSubType(corners);

                    // Only return 4 corner rectanges
                    if ((subType == PolygonSubType.Parallelogram || subType == PolygonSubType.Rectangle) &&  corners.Count == 4)
                    {
                        // Check if its sideways, if so rearrange the corners so it's veritcal
                        corners = Utilities.rearrangeCorners(corners).ToList();

                        // Prevent it from detecting the same card twice
                        foreach (IntPoint point in cardPositions)
                        {
                            if (corners[0].DistanceTo(point) < 40)
                                sameCard = true;
                        }
                        
                        if (sameCard)
                            continue;

                        // Hack to prevent it from detecting smaller sections of the card instead of the whole card
                        if (Utilities.GetArea(corners) < 20000)
                            continue;
                         
                        cardPositions.Add(corners[0]);
						
						var points = corners.Select(p => new System.Drawing.Point(p.X, p.Y));						
                        g.DrawPolygon(pen, points.ToArray());

                        // Extract the card bitmap
                        QuadrilateralTransformation transformFilter = new QuadrilateralTransformation(corners, 211, 298);
                        cardBitmap = transformFilter.Apply(cameraBitmap);

                        List<IntPoint> artCorners = new List<IntPoint>();
                        artCorners.Add(new IntPoint(14, 35));
                        artCorners.Add(new IntPoint(193, 35));
                        artCorners.Add(new IntPoint(193, 168));
                        artCorners.Add(new IntPoint(14, 168));

                        // Extract the art bitmap
                        QuadrilateralTransformation cartArtFilter = new QuadrilateralTransformation(artCorners, 183, 133);
                        cardArtBitmap = cartArtFilter.Apply(cardBitmap);

                        MagicCard card = new MagicCard();
                        card.corners = corners;
                        card.cardBitmap = cardBitmap;
                        card.cardArtBitmap = cardArtBitmap;

                        magicCards.Add(card);
                    }
                } 
            }

            pen.Dispose();
            g.Dispose();

            filteredBitmap = bm;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cameraBitmap = new Bitmap(640, 480);
            capture = new Capture(cameraFilters.VideoInputDevices[0], cameraFilters.AudioInputDevices[0]);
            VideoCapabilities vc = capture.VideoCaps;
            capture.FrameSize = new Size(640, 480);
            capture.PreviewWindow = cam;
			capture.FrameEvent2 += new Capture.HeFrame(CaptureDone);
			capture.GrapImg();

            referenceCards = sql.GetCards().ToList();
        }

        private void CaptureDone(System.Drawing.Bitmap e)
        {
            lock (_locker)
            {
                magicCardsLastFrame = new List<MagicCard>(magicCards);
                magicCards.Clear();
                cameraBitmap = e;
                cameraBitmapLive = (Bitmap)cameraBitmap.Clone();
                detectQuads(cameraBitmap);
                matchCard();

                image_output.Image = filteredBitmap;
                camWindow.Image = cameraBitmap;                
            }
        }

        private void matchCard()
        {
            int cardTempId = 0;
            foreach(MagicCard card in magicCards)
            {
                cardTempId++;
                // Write the image to disk to be read by the pHash library.. should really find
                // a way to pass a pointer to image data directly
                card.cardArtBitmap.Save("tempCard" + cardTempId + ".jpg", ImageFormat.Jpeg);


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

                if (bestMatch != null)
                {
                    card.referenceCard = bestMatch;
                    //Debug.WriteLine("Highest Similarity: " + bestMatch.name + " ID: " + bestMatch.cardId.ToString());
                    
                    Graphics g = Graphics.FromImage(cameraBitmap);
                    g.DrawString(bestMatch.name, new Font("Tahoma", 25), Brushes.Black, new PointF(card.corners[0].X - 29, card.corners[0].Y - 39));
                    g.DrawString(bestMatch.name, new Font("Tahoma", 25), Brushes.Yellow, new PointF(card.corners[0].X - 30, card.corners[0].Y - 40));
                    g.Dispose();
                }
            }
        }
         
        private void camWindow_MouseClick(object sender, MouseEventArgs e)
        {
            lock (_locker)
            {
                foreach (MagicCard card in magicCards)
                {
                    Rectangle rect = new Rectangle(card.corners[0].X, card.corners[0].Y, (card.corners[1].X - card.corners[0].X), (card.corners[2].Y - card.corners[1].Y));
                    if (rect.Contains(e.Location))
                    {
                        Debug.WriteLine(card.referenceCard.name);
                        cardArtImage.Image = card.cardArtBitmap;
                        cardImage.Image = card.cardBitmap;

                        cardInfo.Text = "Card Name: " + card.referenceCard.name + Environment.NewLine + 
                            "Set: " + (String)card.referenceCard.dataRow["Set"] + Environment.NewLine + 
                            "Type: " + (String)card.referenceCard.dataRow["Type"] + Environment.NewLine + 
                            "Casting Cost: " + (String)card.referenceCard.dataRow["Cost"] + Environment.NewLine + 
                            "Rarity: " + (String)card.referenceCard.dataRow["Rarity"] + Environment.NewLine;

                    }
                }
            }
        }
    }
}
