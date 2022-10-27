using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Emgu.CV;
using Emgu.CV.Structure;

namespace PalletOrganizerV3
{
    internal class RectangleOperations
    {
        static readonly object locker=new object();
        public int FindConnectedBoxesCount(IndexedStack<Rectangle>boxes)
        {
        
            int count = 0;
           
            //corner to corner is not connected.
            Parallel.For(0,boxes.Count, i =>
            {
                Parallel.For(0,boxes.Count, j =>
                {
                    if (i == j)
                    {

                    }
                    else if (IsConnected(boxes[i], boxes[j]))
                    {
                        Interlocked.Increment(ref count);

                    }
                });
            });
            return count;

        }
        public Bitmap DrawBoundingBox(Rectangle boundingRect ,Bitmap bitmap,int maxWidth,int maxHeight)
        {
            if (bitmap == null)
            {
                bitmap = new Bitmap(maxWidth, maxHeight);
            }
            Brush brush = new SolidBrush(Color.FromArgb(255, 255, 0, 255));
            Pen pen = new Pen(Color.FromArgb(255, 255, 0, 255));
            pen.Width = 1;

            using (Graphics g = Graphics.FromImage(bitmap))
            {

                g.DrawRectangle(pen, boundingRect.X, boundingRect.Y, boundingRect.Width - 1, boundingRect.Height - 1);//-1 means inside the rect area

            }
            return bitmap;
        }

    
            public int GenerateHashCode(IndexedStack<Rectangle> boxes)
        {


            //return random.Next();
            int hash = 0;

            int seed1 = 13;
            int seed5 = 487;



            int count = (int)boxes.Count;
            for (int i = 0; i < count; i++)
            {

                hash +=
                    (boxes[i].X * 53 +
                     boxes[i].Y * 13 )*
                     boxes[i].Width * 4549 *
                     boxes[i].Height * 7547;

            }
            return hash;

            for (int i = 0; i < count; i++)
            {

                hash +=
                    boxes[i].X * 53 +
                     boxes[i].Y * 13 +
                     boxes[i].Width * 4549 +
                     boxes[i].Height * 7547;

            }
            return hash;
            for (int i = 0; i < count; i++)
            {
                if (boxes[i].Width < boxes[i].Height)
                {
                    seed5 = 10859;
                }
                else
                {
                    seed5 = 487;
                }
                hash += ShiftAndWrap(boxes[i].X * seed1, 7)*
                     ShiftAndWrap(boxes[i].Y, 5) *
                     boxes[i].Width*149+boxes[i].Height*487;

            }
            return hash;


            for (int i = 0; i < count; i++)
            {
                if (boxes[i].Width < boxes[i].Height)
                {
                    seed5 = 10859;
                }
                else
                {
                    seed5 = 487;
                }
                hash += ShiftAndWrap(boxes[i].X.GetHashCode()* seed1, 7) ^
                     ShiftAndWrap(boxes[i].Y.GetHashCode(), 5) * ShiftAndWrap(seed5, 2);

            }
            return hash;

        }
        public UInt64 GenerateHashCodeLong(IndexedStack<Rectangle> boxes)
        {


            
            UInt64 hash = 0;
            int count = boxes.Count;

            /*
            int[]values=new int[count];
            Parallel.For(0, count, i =>
            {

                values[i] = boxes[i].X * 43781 + boxes[i].Y * 195737 + boxes[i].Width * 1022387 + boxes[i].Height * 166847;

            });
            for (int i=0; i < count; i++)
            {
                hash +=(UInt64) values[i];
            }
            return hash;
            */
            for (int i = 0; i < count; i++)
            {

                hash +=
                     ((UInt64)(boxes[i].X*43781) ^ (UInt64)(boxes[i].Y*195737)) ^
                      (((UInt64)boxes[i].Width) *1022387)^
                     (((UInt64)boxes[i].Height*166847)) ;
                /*
                hash +=
                   ((UInt64)(boxes[i].X * 43781) +(UInt64)(boxes[i].Y * 195737)) +
                    (((UInt64)boxes[i].Width) * 1022387) +
                   (((UInt64)boxes[i].Height * 166847));
                */
            }
            return hash;

             

        }
        private int ShiftAndWrap(int value, int positions)
        {
            positions = positions & 0x1F;

            // Save the existing bit pattern, but interpret it as an unsigned integer.
            uint number = BitConverter.ToUInt32(BitConverter.GetBytes(value), 0);
            // Preserve the bits to be discarded.
            uint wrapped = number >> (32 - positions);
            // Shift and wrap the discarded bits.
            return BitConverter.ToInt32(BitConverter.GetBytes((number << positions) | wrapped), 0);
        }
        public Bitmap DrawAllBoxes(IndexedStack<Rectangle> rects,int maxWidth,int maxHeight,bool drawOutline=true)
        {
            Bitmap bitmap = new Bitmap(maxWidth, maxHeight);

            Brush brush = new SolidBrush(Color.FromArgb(255, 127, 127, 127));
            Brush brushOutline = new SolidBrush(Color.FromArgb(255, 255, 255, 1));


            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.FromArgb(255, 0, 0, 0));
                for (int i = 0; i < rects.Count; i++)
                {
                    //brush = new SolidBrush(Color.FromArgb(255, random.Next(127,255), random.Next(0,127), random.Next(20,127)));
                    //fill the whole area
                    g.FillRectangle(brush, rects[i].X, rects[i].Y, rects[i].Width, rects[i].Height);//exactly the same area as mathematics

                    if (drawOutline)
                    {
                        //draw the outline
                        g.DrawRectangle(Pens.Black, rects[i].X, rects[i].Y, rects[i].Width, rects[i].Height);
                    }
                }

            }
            return bitmap;
        }

        public bool IsOverlapped(IndexedStack<Rectangle> boxes, Rectangle b2)
        {
            bool overlapped = false;
            Parallel.For(0, boxes.Count, (i,state) =>
            {
                if (IsOverlapped(boxes[i], b2))
                {
                    overlapped = true;//either one will be true
                    state.Break();
                }
            });

            return overlapped;


        }
          bool IsOverlapped(Rectangle b1, Rectangle b2)
        {
            if ((b1.X + b1.Width) < b2.X) return false;
            if ((b1.Y + b1.Height) < b2.Y) return false;
            if (b1.X > (b2.X + b2.Width)) return false;
            if (b1.Y > (b2.Y + b2.Height)) return false;
            return !IsConnected(b1, b2, true);


        }
          bool IsConnected(Rectangle b1, Rectangle b2, bool includeCorner = false)
        {
            if (b1 == b2) return false; //same object
            if (b1.X == b2.X && b1.Y == b2.Y &&
                b1.Width == b2.Width && b1.Height == b2.Height)
            {
                //exactly the same
                return false;
            }

            ////same side touch
            //left side
            if (b1.X == b2.X)
            {
                if (b1.Y + b1.Height == b2.Y)//b2 below
                {
                    return true;
                }
                if (b2.Y + b2.Height == b1.Y)//b1 below
                {
                    return true;
                }
            }
            //right side
            if (b1.X + b1.Width == b2.X + b2.Width)
            {
                if (b1.Y + b1.Height == b2.Y)//b2 below
                {
                    return true;
                }
                if (b2.Y + b2.Height == b1.Y)//b1 below
                {
                    return true;
                }
            }

            //top side
            if (b1.Y == b2.Y)
            {
                if (b1.X + b1.Width == b2.X)//b2 right
                {
                    return true;
                }
                if (b2.X + b2.Width == b1.X)//b1 right
                {
                    return true;
                }
            }
            //bottom side
            if (b1.Y + b1.Height == b2.Y + b2.Height)
            {
                if (b1.X + b1.Width == b2.X)//b2 right
                {
                    return true;
                }
                if (b2.X + b2.Width == b1.X)//b1 right
                {
                    return true;
                }
            }

            ///opposite side touch
            //left/right
            if (b1.X + b1.Width == b2.X) //b2 at right
            {
                if (includeCorner)
                {
                    if (b2.Y >= b1.Y && b2.Y <= (b1.Y + b1.Height))// 
                    {
                        return true;
                    }
                    if ((b2.Y + b2.Height) >= b1.Y && (b2.Y + b2.Height) <=
                        (b1.Y + b1.Height))// 
                    {
                        return true;
                    }
                    if (b2.Y <= b1.Y && b2.Y + b2.Height >= b1.Y + b1.Height) return true;
                    if (b2.Y >= b1.Y && b2.Y + b2.Height <= b1.Y + b1.Height) return true;
                }
                else
                {
                    if (b2.Y > b1.Y && b2.Y < (b1.Y + b1.Height))//do not use equal sign such that corner is ignored
                    {
                        return true;
                    }
                    if ((b2.Y + b2.Height) > b1.Y && (b2.Y + b2.Height) <
                        (b1.Y + b1.Height))//do not use equal sign such that corner is ignored
                    {
                        return true;
                    }
                    if (b2.Y <= b1.Y && b2.Y + b2.Height >= b1.Y + b1.Height) return true;
                    if (b2.Y >= b1.Y && b2.Y + b2.Height <= b1.Y + b1.Height) return true;
                }

            }
            if (b1.X == b2.X + b2.Width) //b2 at left
            {
                if (includeCorner)
                {
                    if (b2.Y >= b1.Y && b2.Y <= (b1.Y + b1.Height))//do not use equal sign such that corner is ignored
                    {
                        return true;
                    }
                    if ((b2.Y + b2.Height) >= b1.Y && (b2.Y + b2.Height) <=
                        (b1.Y + b1.Height))//do not use equal sign such that corner is ignored
                    {
                        return true;
                    }

                    if (b2.Y <= b1.Y && b2.Y + b2.Height >= b1.Y + b1.Height) return true;
                    if (b2.Y >= b1.Y && b2.Y + b2.Height <= b1.Y + b1.Height) return true;
                }
                else
                {
                    if (b2.Y > b1.Y && b2.Y < (b1.Y + b1.Height))//do not use equal sign such that corner is ignored
                    {
                        return true;
                    }
                    if ((b2.Y + b2.Height) > b1.Y && (b2.Y + b2.Height) <
                        (b1.Y + b1.Height))//do not use equal sign such that corner is ignored
                    {
                        return true;
                    }
                    if (b2.Y <= b1.Y && b2.Y + b2.Height >= b1.Y + b1.Height) return true;
                    if (b2.Y >= b1.Y && b2.Y + b2.Height <= b1.Y + b1.Height) return true;
                }

            }

            //top/bottom
            if (b1.Y + b1.Height == b2.Y) //b2 at bottom
            {
                if (includeCorner)
                {
                    if (b2.X >= b1.X && b2.X <= (b1.X + b1.Width))//do not use equal sign such that corner is ignored
                    {
                        return true;
                    }
                    if ((b2.X + b2.Width) >= b1.X && (b2.X + b2.Width) <=
                        (b1.X + b1.Width))//do not use equal sign such that corner is ignored
                    {
                        return true;
                    }
                    if (b2.X <= b1.X && b2.X + b2.Width >= b1.X + b1.Width) return true;
                    if (b2.X >= b1.X && b2.X + b2.Width <= b1.X + b1.Width) return true;
                }
                else
                {
                    if (b2.X > b1.X && b2.X < (b1.X + b1.Width))//do not use equal sign such that corner is ignored
                    {
                        return true;
                    }
                    if ((b2.X + b2.Width) > b1.X && (b2.X + b2.Width) <
                        (b1.X + b1.Width))//do not use equal sign such that corner is ignored
                    {
                        return true;
                    }
                    if (b2.X <= b1.X && b2.X + b2.Width >= b1.X + b1.Width) return true;
                    if (b2.X >= b1.X && b2.X + b2.Width <= b1.X + b1.Width) return true;
                }

            }
            if (b1.Y == b2.Y + b2.Height) //b2 at top
            {
                if (includeCorner)
                {
                    if (b2.X >= b1.X && b2.X <= (b1.X + b1.Width))//do not use equal sign such that corner is ignored
                    {
                        return true;
                    }
                    if ((b2.X + b2.Width) >= b1.X && (b2.X + b2.Width) <=
                        (b1.X + b1.Width))//do not use equal sign such that corner is ignored
                    {
                        return true;
                    }
                    if (b2.X <= b1.X && b2.X + b2.Width >= b1.X + b1.Width) return true;
                    if (b2.X >= b1.X && b2.X + b2.Width <= b1.X + b1.Width) return true;
                }
                else
                {
                    if (b2.X > b1.X && b2.X < (b1.X + b1.Width))//do not use equal sign such that corner is ignored
                    {
                        return true;
                    }
                    if ((b2.X + b2.Width) > b1.X && (b2.X + b2.Width) <
                        (b1.X + b1.Width))//do not use equal sign such that corner is ignored
                    {
                        return true;
                    }
                    if (b2.X <= b1.X && b2.X + b2.Width >= b1.X + b1.Width) return true;
                    if (b2.X >= b1.X && b2.X + b2.Width <= b1.X + b1.Width) return true;
                }

            }
            return false;
        }
        double Get2ConnectedBoxOverlapScore(Rectangle mainBox,Rectangle connectedBox)
        {
            double score = 0;
            if (mainBox.X == connectedBox.X)
            {
                //left
                score = Math.Min(mainBox.Width, connectedBox.Width);

            }
            else if (mainBox.X + mainBox.Width == connectedBox.X + connectedBox.Width)
            {
                //right
                score = Math.Min(mainBox.Width, connectedBox.Width);
              
            }
            else if (mainBox.Y == connectedBox.Y)
            {
                //top
                score = Math.Min(mainBox.Height, connectedBox.Height);
               
            }
            else if (mainBox.Y + mainBox.Height == connectedBox.Y + connectedBox.Height)
            {
                //bottom
                score = Math.Min(mainBox.Height, connectedBox.Height);
              
            }

            //opposite side
            else if (mainBox.X + mainBox.Width == connectedBox.X) //at right
            {
                if (mainBox.Y >= connectedBox.Y)
                {
                    score = Math.Abs(mainBox.Y - (connectedBox.Y + connectedBox.Height));
                    if (score > mainBox.Height) score = mainBox.Height;
                }
                else
                {
                    score = Math.Abs(connectedBox.Y - (mainBox.Y + mainBox.Height));
                    if (score > connectedBox.Height) score = connectedBox.Height;
                }
            
            }
            else if (mainBox.X == connectedBox.X + connectedBox.Width) //at left
            {
                if (mainBox.Y >= connectedBox.Y)
                {
                    score = Math.Abs(mainBox.Y - (connectedBox.Y + connectedBox.Height));
                    if (score > mainBox.Height) score = mainBox.Height;
                }
                else
                {
                    score = Math.Abs(connectedBox.Y - (mainBox.Y + mainBox.Height));
                    if (score > connectedBox.Height) score = connectedBox.Height;
                }
                
            }
            else if (mainBox.Y == connectedBox.Y + connectedBox.Height) //at top
            {
                if (mainBox.X >= connectedBox.X)
                {
                    score = Math.Abs(mainBox.X - (connectedBox.X + connectedBox.Width));
                    if (score > mainBox.Width) score = mainBox.Width;
                }
                else
                {
                    score = Math.Abs(connectedBox.X - (mainBox.X + mainBox.Width));
                    if (score > connectedBox.Width) score = connectedBox.Width;
                }
              
            }
            else if (mainBox.Y + mainBox.Height == connectedBox.Y) //at bottom
            {
                if (mainBox.X >= connectedBox.X)
                {
                    score = Math.Abs(mainBox.X - (connectedBox.X + connectedBox.Width));
                    if (score > mainBox.Width) score = mainBox.Width;
                }
                else
                {
                    score = Math.Abs(connectedBox.X - (mainBox.X + mainBox.Width));
                    if (score > connectedBox.Width) score = connectedBox.Width;
                }
                
            }
            score = score / (double)(mainBox.Width * 2 + mainBox.Height * 2);//over the total area
         

            return score;
        }
        public double GetConnectedBoxesOverlapScore(IndexedStack<Rectangle>rects)
        {
            //count the number of points in the overlap boxes, and avg.
            //0=no box connected, 1=all the edges are connected to other boxes
            double overlapScore = 0;

            for (int i = 0; i < rects.Count; i++)
            {
                for (int j = 0; j < rects.Count; j++)
                {
                    if (i == j) continue;
                    overlapScore += Get2ConnectedBoxOverlapScore(rects[i], rects[j]);
                }
                
            }
            



            return overlapScore;
        }

        public Rectangle GetBoundingBox(Rectangle[] boxes)
        {
            //draw a bounding box of the outline,then count the number of pixels

            //
            Array.Sort(boxes, (a, b) => a.X.CompareTo(b.X));

            int minx = boxes[0].X;

            //
            Array.Sort(boxes, (a, b) => a.Y.CompareTo(b.Y));

            int miny = boxes[0].Y;

            //
            Array.Sort(boxes, (a, b) => (a.X + a.Width).CompareTo(b.X + b.Width));

            int maxx = boxes[boxes.Length - 1].X + boxes[boxes.Length - 1].Width;

            //
            Array.Sort(boxes, (a, b) => (a.Y + a.Height).CompareTo(b.Y + b.Height));

            int maxy = boxes[boxes.Length - 1].Y + boxes[boxes.Length - 1].Height;

            return new Rectangle(minx, miny, maxx - minx, maxy - miny);
        }

        public double FindHolesScore(IndexedStack<Rectangle>boxes,int maxWidth,int maxHeight)
        {
            Image<Gray, byte> bmp = new Image<Gray, byte>(maxWidth,maxHeight,new Gray(0));
            for (int i = 0;i<boxes.Count;i++)
            {
                bmp.Draw(boxes[i], new Gray(255), -1);
            }

            int minLength=Math.Min(boxes[0].Width,boxes[0].Height);

            Emgu.CV.Util.VectorOfVectorOfPoint contours = new Emgu.CV.Util.VectorOfVectorOfPoint();

 
            CvInvoke.FindContours(bmp, contours, null, Emgu.CV.CvEnum.RetrType.Ccomp, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxNone);
            var outlineAndInline=contours.ToArrayOfArray();
            double score = 0;
            double adjust = 5.0;
            for (int i=1;i< outlineAndInline.Length; i++)//the first one is outline. we need inline only
            {
                Rectangle rect = CvInvoke.BoundingRectangle(outlineAndInline[i]);
                if (rect.Width<  minLength && rect.Height<  minLength) //should less than certain level
                {
                    score += adjust*minLength * minLength/(double)(rect.Width*rect.Height);
                }
                else
                {
                    score -=10* adjust * minLength * minLength / (double)(rect.Width * rect.Height);
                }
            }

            return score;

        }


    }
}
