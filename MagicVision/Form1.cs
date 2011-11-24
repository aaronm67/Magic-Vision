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
        private Filters cameraFilters = new Filters();
        //Data.CardStore sql = new MySqlClient();
        private String refCardDir = @"C:\Users\Pete\Pictures\New Phyrexia\Crops\";
        Capture capture;

        public Form1()
        {
            InitializeComponent();
        }

        private void hashCalcButton_Click(object sender, EventArgs e)
        {
            foreach (ReferenceCard card in sql.GetCards())
            {
                Phash.ph_dct_imagehash(refCardDir + (String)card.dataRow["Set"] + "\\" + card.cardId + ".jpg", ref card.pHash);
                sql.UpdateHash(card.cardId, card.pHash);
            }
        }
		
        private void Form1_Load(object sender, EventArgs e)
        {            
            capture = new Capture(cameraFilters.VideoInputDevices[0], cameraFilters.AudioInputDevices[0]);

            VideoCapabilities vc = capture.VideoCaps;
            capture.FrameSize = new Size(640, 480);
            capture.PreviewWindow = cam;

            var referenceCards = sql.GetCards();

            capture.FrameEvent2 += new Capture.HeFrame((Bitmap bitmap) => {
                var magicCards = Utilities.DetectCardArt(bitmap);                
                foreach (var card in magicCards)
                {
                    camWindow.Image = bitmap;
                    image_output.Image = card.GetDrawnCorners();
                    cardArtImage.Image = card.CardArtBitmap;

                    var bestMatch = Utilities.MatchCard(card, referenceCards);
                    Graphics g = Graphics.FromImage(bitmap);
                    g.DrawString(bestMatch.name, new Font("Tahoma", 25), Brushes.Black, new PointF(card.Corners[0].X - 29, card.Corners[0].Y - 39));
                    g.DrawString(bestMatch.name, new Font("Tahoma", 25), Brushes.Yellow, new PointF(card.Corners[0].X - 30, card.Corners[0].Y - 40));
                    g.Dispose();
                    image_output.Image = bitmap;
                }
            });

			capture.GrapImg();
        }

        private void camWindow_MouseClick(object sender, MouseEventArgs e)
        { 
            
        }

        //{
        //    foreach (MagicCard card in magicCards)
        //    {
        //        Rectangle rect = new Rectangle(card.Corners[0].X, card.Corners[0].Y, (card.Corners[1].X - card.Corners[0].X), (card.Corners[2].Y - card.Corners[1].Y));
        //        if (rect.Contains(e.Location))
        //        {
        //            Debug.WriteLine(card.referenceCard.name);
        //            cardArtImage.Image = card.cardArtBitmap;
        //            cardImage.Image = card.cardBitmap;

        //            cardInfo.Text = "Card Name: " + card.referenceCard.name + Environment.NewLine + 
        //                "Set: " + (String)card.referenceCard.dataRow["Set"] + Environment.NewLine + 
        //                "Type: " + (String)card.referenceCard.dataRow["Type"] + Environment.NewLine + 
        //                "Casting Cost: " + (String)card.referenceCard.dataRow["Cost"] + Environment.NewLine + 
        //                "Rarity: " + (String)card.referenceCard.dataRow["Rarity"] + Environment.NewLine;

        //        }
        //    }
        //}
    }
}
