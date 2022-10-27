using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
 
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PalletOrganizerV3
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        IndexedStack<Rectangle> currentCluster = new IndexedStack<Rectangle>(1000);
        IndexedStack<IndexedStack<Rectangle>> clusters = new IndexedStack<IndexedStack<Rectangle>>(1000*1000*100);
        private static readonly object locker = new object();
        int boxLongEdge = 36;
        int boxShortEdge = 20;
        int boxDepth = 10;//equal to the z height
        int maxWidth = 120;
        int maxHeight = 100;
        int targetLevel = 99999;
        static Dictionary<UInt64, int>clusterPositions=new Dictionary<UInt64, int>(1000*1000*20);
 
        Random random = new Random();
        RectangleOperations rectOps=new RectangleOperations();
        int index = 0;
        IndexedStack<FINALRESULT>finalResult=new IndexedStack<FINALRESULT>(1);
        List<FINALRESULT> finalResultList = new List<FINALRESULT>(1);
        static bool forceStop = false;

         struct FINALRESULT
        {
           public double score;
           public  int index;
        }
          double GetScore(IndexedStack<Rectangle>boxes)
        {
            double totalScore = 0;
            double connectedNumberScore = 0;
            double overlappedPerimeterScore = 0;
            double holeScore = 0;
            double totalArea = 0;

            connectedNumberScore=rectOps.FindConnectedBoxesCount(boxes);
            overlappedPerimeterScore = rectOps.GetConnectedBoxesOverlapScore(boxes);
            holeScore=rectOps.FindHolesScore(boxes,maxWidth,maxHeight);
            for  (int i=0;i<boxes.Count;i++)
            {
               
               
                totalArea += boxes[i].Width * boxes[i].Height;
            }

            totalScore = connectedNumberScore + overlappedPerimeterScore+ holeScore;

            //possible area over bounding box area,1=covered totally
            Rectangle boundingRect=rectOps.GetBoundingBox(boxes.CopyExactNumber());
            totalScore += (totalArea / (boundingRect.Width * boundingRect.Height));



            return totalScore;
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Rectangle rect = new Rectangle();
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                currentCluster.Clear();
                using (StreamReader sr=new StreamReader(ofd.FileName))
                {
                    string content=sr.ReadToEnd();
                    JObject jo = JsonConvert.DeserializeObject<JObject>(content);
                    numericUpDown2.Value = (int)jo["maxWidth"];
                    numericUpDown3.Value = (int)jo["maxHeight"];
                    numericUpDown4.Value = (int)jo["boxLongEdge"];
                    numericUpDown5.Value = (int)jo["boxShortEdge"];
                    targetLevel = (int)jo["targetLevel"];
                    JArray ja = (JArray)jo["boxes"];
                    for (int i=0; i<ja.Count; i++)
                    {
                        JObject box=(JObject)ja[i];
                        JArray values = (JArray)box["box"];
                        rect = new Rectangle((int)values[0], (int)values[1], (int)values[2], (int)values[3]);
                        currentCluster.Push(rect);
                    }

                }
            }
            else
            {
                return;
            }
            
            maxWidth = (int)numericUpDown2.Value;
            maxHeight = (int)numericUpDown3.Value;
            boxLongEdge = (int)numericUpDown4.Value;
            boxShortEdge = (int)numericUpDown5.Value;

            /*
            //make sure even if use middle point
            maxWidth += maxWidth % 2;
            maxHeight+= maxHeight % 2;
            boxLongEdge+= boxLongEdge % 2;
            boxShortEdge+= boxShortEdge % 2;
            */

            button5.Enabled = false;

            //add certain boxes in the pallet manually
            //currentCluster.Clear();
           // rect=new Rectangle(0, 0, boxLongEdge, boxShortEdge);
            //currentCluster.Push(rect);

            rect = new Rectangle(boxLongEdge, 0, boxLongEdge, boxShortEdge);
            // currentCluster.Push(rect);

            rect = new Rectangle(boxLongEdge*2, 0, boxLongEdge, boxShortEdge);
            //currentCluster.Push(rect);



            //

            rect = new Rectangle(0, boxShortEdge, boxLongEdge, boxShortEdge);
           // currentCluster.Push(rect);

            rect = new Rectangle(0, boxShortEdge*2, boxLongEdge, boxShortEdge);
           //  currentCluster.Push(rect);


            //6

            rect = new Rectangle(0, boxShortEdge * 3, boxShortEdge, boxLongEdge);
         // currentCluster.Push(rect);

        

            Bitmap bmp = rectOps.DrawAllBoxes(currentCluster,maxWidth,maxHeight);
    
            pictureBox1.Image = bmp;
            return;

             

        }

        static Stopwatch sw = new Stopwatch();
        static UInt64 counter = 0;
        int maxLevel = 0;
       // static Point[] points = new Point[1000* 4];

        void MoveNext(IndexedStack<Rectangle> cluster)
        {


            //for every possible move, generate 8 boxes
            //topleft,topright,bottomleft,bottomright,rotate 90 and then repeat

            if (forceStop) return;
           

            ++counter;
            if (counter %50000==0)
            {
                //update the status regularly
                richTextBox1.Invoke(new Action(() =>
                {
                    richTextBox1.AppendText("Total nodes(M):" + (counter / 1000000.0).ToString("0.00") + "\r\n" +
                           "Recorded end leaves:" + clusters.Count.ToString() + "\r\n" +
                            "Elapsed(mins):" + sw.Elapsed.TotalMinutes.ToString("0.00") + "\r\n" +
                            "Max level:" + maxLevel.ToString() +
                            "\r\n");

                }
                   ));
            }

            
                var hashvalue = rectOps.GenerateHashCodeLong(cluster);


         
                if (clusterPositions.ContainsKey(hashvalue))
                {
                    //considered previously,do nothing
                    return;
                }
                else
                {

                    clusterPositions.Add(hashvalue, 1);//the value is not important

                }
           
            if (cluster.Count > targetLevel)
            {

                return;
            }
            else if (cluster.Count == targetLevel)
            {
                //copy the current status to the final result
                IndexedStack<Rectangle> leaf = new IndexedStack<Rectangle>(cluster.Count);
                for (int j = 0; j < cluster.Count; j++)
                {
                    leaf.Push(cluster[j]);
                }
                clusters.Push(leaf);
                maxLevel = targetLevel;
                return;
            }


            //generate 4 corner points for each box.
            //can add more such as half of the edge, or even 1/10,1/4,1/3

            int pointIndex = 0;
            Point[] points = new Point[cluster.Count * 4];//max level should not exceed 1000
            /*
            for (int i = 0; i < cluster.Count; i++)
            {
                 
                points[pointIndex].X = cluster[i].X; 
              points[pointIndex].Y = cluster[i].Y;
              points[pointIndex+1].X = cluster[i].X + cluster[i].Width; 
              points[pointIndex+1].Y = cluster[i].Y;
              points[pointIndex+2].X = cluster[i].X; 
              points[pointIndex+2].Y = cluster[i].Y + cluster[i].Height;
              points[pointIndex+3].X = cluster[i].X +cluster[i].Width; 
              points[pointIndex+3].Y = cluster[i].Y + cluster[i].Height;
          
              

                pointIndex += 4;
            }
            */
            Parallel.For (0,cluster.Count,i=>
            {
                if (forceStop) return;
                points[i*4].X = cluster[i].X;
                points[i * 4].Y = cluster[i].Y;
                points[i * 4 + 1].X = cluster[i].X + cluster[i].Width;
                points[i * 4 + 1].Y = cluster[i].Y;
                points[i * 4 + 2].X = cluster[i].X;
                points[i * 4 + 2].Y = cluster[i].Y + cluster[i].Height;
                points[i * 4 + 3].X = cluster[i].X + cluster[i].Width;
                points[i * 4 + 3].Y = cluster[i].Y + cluster[i].Height;



               
            });

             var nonRepeadPoints = points.Distinct().ToArray();
            //var nonRepeadPoints = points;
            //nonRepeadPoints=points.ToArray();
            //for each point ,generate 8 boxes


            int width = boxLongEdge;
            int height = boxShortEdge;
          
            Rectangle[] potentialMoves = new Rectangle[nonRepeadPoints.Length * 8];
            int count = nonRepeadPoints.Length;
            for (int i = 0; i < count; i++)
            {
                int index = i * 8;
                width = boxLongEdge;
                height = boxShortEdge;
                //topleft
              // potentialMoves[index] = new Rectangle(points[i].X, points[i].Y,width, height);
                 potentialMoves[index].X = nonRepeadPoints[i].X;
               potentialMoves[index].Y = nonRepeadPoints[i].Y;
               potentialMoves[index].Width = width;
                potentialMoves[index].Height = height;
                //topright
                index++;
                //potentialMoves[index] = new Rectangle(points[i].X - width, points[i].Y, width, height);
                potentialMoves[index].X = nonRepeadPoints[i].X - width;
                 potentialMoves[index].Y = nonRepeadPoints[i].Y;
              potentialMoves[index].Width = width;
                potentialMoves[index].Height = height;

                //bottomleft
                index++;
               // potentialMoves[index] = new Rectangle(points[i].X , points[i].Y - height, width, height);
                potentialMoves[index].X = nonRepeadPoints[i].X;
                 potentialMoves[index].Y = nonRepeadPoints[i].Y-height;
                potentialMoves[index].Width = width;
                potentialMoves[index].Height = height;

                //bottomright
                index++;
                // potentialMoves[index] = new Rectangle(points[i].X - width, points[i].Y - height, width, height);
                potentialMoves[index].X = nonRepeadPoints[i].X-width;
                potentialMoves[index].Y = nonRepeadPoints[i].Y-height;
                 potentialMoves[index].Width = width;
                 potentialMoves[index].Height = height;

                //do again after rotate 90
                width = boxShortEdge;
                height = boxLongEdge;
                //topleft
                index++;
               //potentialMoves[index] = new Rectangle(points[i].X, points[i].Y, width, height);
                potentialMoves[index].X = nonRepeadPoints[i].X;
                potentialMoves[index].Y = nonRepeadPoints[i].Y;
               potentialMoves[index].Width = width;
                potentialMoves[index].Height = height;

                //topright
                index++;
                //potentialMoves[index] = new Rectangle(points[i].X - width, points[i].Y, width, height);
                potentialMoves[index].X = nonRepeadPoints[i].X-width;
                 potentialMoves[index].Y = nonRepeadPoints[i].Y;
               potentialMoves[index].Width = width;
                potentialMoves[index].Height = height;

                //bottomleft
                index++;
               // potentialMoves[index] = new Rectangle(points[i].X, points[i].Y - height, width, height);
                 potentialMoves[index].X = nonRepeadPoints[i].X;
                 potentialMoves[index].Y = nonRepeadPoints[i].Y-height;
                potentialMoves[index].Width = width;
                potentialMoves[index].Height = height;

                //bottomright
                index++;
               // potentialMoves[index] = new Rectangle(points[i].X - width, points[i].Y - height, width, height);
                 potentialMoves[index].X = nonRepeadPoints[i].X-width;
                 potentialMoves[index].Y = nonRepeadPoints[i].Y-height;
                 potentialMoves[index].Width = width;
                potentialMoves[index].Height = height;
            }

            var nonRepeadBoxes=potentialMoves.Distinct().ToArray();
            //var nonRepeadBoxes = potentialMoves;
            //remove those outside boundary
            var validMoves = (from c in nonRepeadBoxes
                                    where c.X >= 0 && c.Y >= 0 &&
                                    c.X + c.Width <= maxWidth && 
                                    c.Y + c.Height <= maxHeight &&
                                    c.Width>0 && c.Height>0
                                    select c).ToArray();


            //remove those overlapped to the existing cluster
            IndexedStack<Rectangle> validMoves2 = new IndexedStack<Rectangle>(validMoves.Length);
            Parallel.ForEach(validMoves, validMove =>
            //foreach (var validMove in validMoves)
            
            {
                if (forceStop) return;
                if (!rectOps.IsOverlapped(cluster, validMove))
                {
                    lock (locker)
                    {
                        validMoves2.Push(validMove);
                    }

                }
               
            }
            );


            bool canMove = false;
    
           
            for (int i = 0; i < validMoves2.Count; i++)
            {
                //for every allowed move, do recursively
                if (forceStop) return;
                canMove = true;
                cluster.Push(validMoves2[i]);


                MoveNext(cluster);

                cluster.Pop();//remove the last added


            }
          
            if (!canMove)
            {
                if (forceStop) return;
                //end game,save the status

                 
                if (cluster.Count >= maxLevel)
                {
                    
                        maxLevel = cluster.Count;

                        //copy the current status to the final result
                        IndexedStack<Rectangle> leaf = new IndexedStack<Rectangle>(cluster.Count);
                        for (int j = 0; j < cluster.Count; j++)
                        {
                            leaf.Push(cluster[j]);
                        }
                        clusters.Push(leaf);
                  
                      
                }
                 
            }

        }

        void MoveNextParallel(IndexedStack<Rectangle> cluster)
        {

            /*
            //for every possible move, generate 8 boxes
            //topleft,topright,bottomleft,bottomright,rotate 90 and then repeat



            int hashvalue = rectOps.GenerateHashCode(cluster);

            if (clusterPositions.ContainsKey(hashvalue))
            {
                //considered previously,do nothing
                return;
            }
            else
            {

                clusterPositions.Add(hashvalue, 1);//the value is not important

            }





            Point[] points = new Point[cluster.Count * 4];
            int pointIndex = 0;
            for (int i = 0; i < cluster.Count; i++)
            {

                points[pointIndex] = new Point(cluster[i].X, cluster[i].Y);
                points[pointIndex + 1] = new Point(cluster[i].X + cluster[i].Width, cluster[i].Y);
                points[pointIndex + 2] = new Point(cluster[i].X, cluster[i].Y + cluster[i].Height);
                points[pointIndex + 3] = new Point(cluster[i].X + cluster[i].Width, cluster[i].Y + cluster[i].Height);
                pointIndex += 4;
            }



            Rectangle[] potentialMoves = new Rectangle[8 * points.Length];
            bool skipdrawedge = false;
            int width = boxLongEdge;
            int height = boxShortEdge;
            for (int i = 0; i < points.Length; i++)
            {
                int index = i * 8;
                width = boxLongEdge;
                height = boxShortEdge;
                //topleft
                potentialMoves[index] = new Rectangle(points[i].X, points[i].Y, width, height);

                //topright
                index++;
                potentialMoves[index] = new Rectangle(points[i].X - width, points[i].Y, width, height);


                //bottomleft
                index++;
                potentialMoves[index] = new Rectangle(points[i].X, points[i].Y - height, width, height);


                //bottomright
                index++;
                potentialMoves[index] = new Rectangle(points[i].X - width, points[i].Y - height, width, height);


                //do again after rotate 90
                width = boxShortEdge;
                height = boxLongEdge;
                //topleft
                index++;
                potentialMoves[index] = new Rectangle(points[i].X, points[i].Y, width, height);

                //topright
                index++;
                potentialMoves[index] = new Rectangle(points[i].X - width, points[i].Y, width, height);


                //bottomleft
                index++;
                potentialMoves[index] = new Rectangle(points[i].X, points[i].Y - height, width, height);


                //bottomright
                index++;
                potentialMoves[index] = new Rectangle(points[i].X - width, points[i].Y - height, width, height);
            }
            var nonRepeadBoxes = potentialMoves.Distinct().ToArray();





            var validMoves = (from c in nonRepeadBoxes
                              where c.X >= 0 && c.Y >= 0 &&
                              c.X + c.Width <= maxWidth &&
                              c.Y + c.Height <= maxHeight
                              select c).ToArray();

            IndexedStack<Rectangle> validMoves2 = new IndexedStack<Rectangle>(validMoves.Length);
            Parallel.ForEach(validMoves, validMove =>
            {
                if (!rectOps.IsOverlapped(cluster, validMove))
                {
                    lock (locker)
                    {
                        validMoves2.Push(validMove);
                    }

                }
            });


            bool canMove = false;
            Parallel.For(0,validMoves2.Count,i =>
            {

                canMove = true;
                IndexedStack<Rectangle> newNode = new IndexedStack<Rectangle>(cluster.Count);
                for (int j = 0; j < cluster.Count; j++)
                {
                    newNode.Push(cluster[j]);
                }
                newNode.Push(validMoves2[i]);


                MoveNext(newNode);

                //cluster.Pop();//remove it and check other posibble moves.


            }
            );
            if (!canMove)
            {
                //end game,save the status
                if (cluster.Count >= maxLevel)
                {
                    maxLevel = cluster.Count;
                    IndexedStack<Rectangle> endLeaf = new IndexedStack<Rectangle>(cluster.Count);
                    for (int j = 0; j < cluster.Count; j++)
                    {
                        endLeaf.Push(cluster[j]);
                    }
                    clusters.Push(endLeaf);


                }

            }
            */
        }
        private void button2_Click(object sender, EventArgs e)
        {
        
             
        }

       
         
       

     
         
        
        private void button3_Click(object sender, EventArgs e)
        {
            
             
        }

        
        
         
        private void button4_Click(object sender, EventArgs e)
        {
            forceStop = false;
            button1.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = false;
            clusters.Clear();
            clusterPositions = new Dictionary<UInt64, int>();
            finalResult.Clear();
            finalResultList.Clear();
            numericUpDown1.Value = 0;
            index = 0;


            Task t = new Task(() => {
                //sw.Start();
                counter = 0;
                sw.Restart();
                maxLevel = currentCluster.Count;
                //MoveNextParallel(currentCluster);
                MoveNext(currentCluster);
                sw.Stop();
                finalResult = new IndexedStack<FINALRESULT>(clusters.Count);
                //for (int i = 0;i<clusters.Count;i++)
                Parallel.For(0, clusters.Count, (i) =>
                {
                    //if (forceStop) return;
                    FINALRESULT fs = new FINALRESULT();
                    fs.index = i;
                    fs.score = GetScore(clusters[i]);
                    lock (locker)
                    {
                        finalResult.Push(fs);
                        if (i % 10000 == 0)
                        {
                            richTextBox1.Invoke(new Action(() =>
                            {
                                richTextBox1.AppendText("Calculating Score:" + i.ToString() + "/" + clusters.Count.ToString() + "\r\n");

                            }
                            ));
                        }
                    }
                });
              
                finalResultList=finalResult.CopyExactNumber().ToList();
                finalResultList.Sort((a,b)=>b.score.CompareTo(a.score));

                 
                richTextBox1.Invoke(new Action(() =>
                    {
                        richTextBox1.AppendText("Total nodes(M):" + (counter/1000000.0).ToString("0.00") + "\r\n" +
                           "Recorded end leaves:" + clusters.Count.ToString() + "\r\n" +
                            "Elapsed(mins):" + sw.Elapsed.TotalMinutes.ToString("0.00")+ "\r\n" +
                            "Max level:"+maxLevel.ToString()+
                            "\r\n");
                        button1.Enabled = true;
                        button4.Enabled = true;
                        button5.Enabled = true;
                    }
                    ));
               
                MessageBox.Show("DONE");
            });
            t.Start();
           
            

          
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            richTextBox1.ScrollToCaret();
            if (richTextBox1.Lines.Length > 1000)
            {
                richTextBox1.Clear();
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            
            
            
            
            index = (int)numericUpDown1.Value;

          


          
            Bitmap bitmap = rectOps.DrawAllBoxes(clusters[finalResultList[index].index],maxWidth,maxHeight);
         
            pictureBox1.Image = bitmap;
       
           
            textBox1.Text = clusters[finalResultList[index].index].Count.ToString();
            richTextBox1.AppendText("Score:" + finalResultList[index].score + "\r\n");
 
        

            index++;
           
            if (index == clusters.Count)
            {
                index = 0;

                MessageBox.Show("DONE");
            }
            numericUpDown1.Value = index;
            return;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            forceStop = true;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            var score = GetScore(currentCluster);
            richTextBox1.AppendText("CurrentScore:" + score.ToString()+"\r\n");
        }
    }

 
    
    

}