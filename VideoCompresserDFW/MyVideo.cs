﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.Drawing;

namespace VideoCompresserDFW
{
    class MyVideo
    {
        //只读
        private readonly String sourceVideoFileName;
        private readonly int smallRectWidthInPix;

        //可以get
        private int frameWidth;
        private int frameHeight;
        private int frameCount;
        private double fps;
        private int currentFrameIndex;
        private int rectRowCount;
        private int rectColCount;
        private Rectangle[,] smallRects;

        //纯私有
        private VideoCapture cap;
        private bool isLastColUncompleted;
        private bool isLastRowUncompleted;
        private Mat mat;

        public MyVideo(String sourceVideoFileName, int smallRectWidthInPix)
        {
            this.sourceVideoFileName = sourceVideoFileName;
            this.smallRectWidthInPix = smallRectWidthInPix;
            mat = new Mat();
            this.CaptureVideo();
            this.InitVideoInfo();
            this.InitRectsInfo();
        }

        public void InitVideoInfo()
        {
            this.frameCount = (int)cap.GetCaptureProperty(CapProp.FrameCount);
            this.frameHeight = (int)cap.GetCaptureProperty(CapProp.FrameHeight);
            this.frameWidth = (int)cap.GetCaptureProperty(CapProp.FrameWidth);
            this.fps = cap.GetCaptureProperty(CapProp.Fps);
        }

        public void CaptureVideo()
        {
            this.cap = new VideoCapture(sourceVideoFileName);
            this.currentFrameIndex = 0;
        }

        public Size GetVideoSize()
        {
            return new Size(this.frameWidth, this.frameHeight);
        }
        public void InitRectsInfo()
        {
            isLastColUncompleted = this.frameWidth % this.smallRectWidthInPix > 0;
            isLastRowUncompleted = this.frameHeight % this.smallRectWidthInPix > 0;

            this.rectColCount = this.frameWidth / this.smallRectWidthInPix + (isLastColUncompleted ? 1 : 0);
            this.rectRowCount = this.frameHeight / this.smallRectWidthInPix + (isLastRowUncompleted ? 1 : 0);

            smallRects = new Rectangle[this.rectRowCount, this.rectColCount];
            Parallel.For(0, this.rectRowCount, i =>
            {
                for (int j = 0; j < this.rectColCount; j++)
                {
                    smallRects[i, j].X = this.smallRectWidthInPix * j;
                    smallRects[i, j].Y = this.smallRectWidthInPix * j;

                    smallRects[i, j].Width = Math.Min(this.frameWidth, this.smallRectWidthInPix * (j + 1)) - smallRects[i, j].X;
                    smallRects[i, j].Height = Math.Min(this.frameHeight, this.smallRectWidthInPix * (i + 1)) - smallRects[i, j].Y;
                }
            });
            //for(int i = 0; i < this.rectRowCount; i++)
            //{
            //    for(int j = 0; j < this.rectColCount; j++)
            //    {
            //        smallRects[i, j].X = this.smallRectWidthInPix * j;
            //        smallRects[i, j].Y = this.smallRectWidthInPix * j;

            //        smallRects[i, j].Width = Math.Min(this.frameWidth, this.smallRectWidthInPix * (j + 1)) - smallRects[i, j].X;
            //        smallRects[i, j].Height = Math.Min(this.frameHeight, this.smallRectWidthInPix * (i + 1)) - smallRects[i, j].Y;
            //    }
            //}
        }

        public Rectangle getRectsPosition(int rowIndex, int colIndex)
        {
            try
            {
                return SmallRects[rowIndex, colIndex];
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return SmallRects[0, 0];
        }

        public void ReCapVideo()
        {
            this.cap.Dispose();
            this.CaptureVideo();
            this.InitVideoInfo();
            //this.smallRectWidthInPix = Math.Min(this.smallRectWidthInPix, Math.Min(this.frameHeight, this.FrameWidth));
            this.InitRectsInfo();
        }

        public Image<Bgr, Byte> ReadVideoImage()
        {
            this.currentFrameIndex++;
            cap.Read(this.mat);
            return this.mat.ToImage<Bgr, Byte>();
        }
        public Mat ReadVideoImageMat()
        {
            this.currentFrameIndex++;
            cap.Read(this.mat);
            return mat;
        }

        public void Release()
        {
            this.cap.Dispose();
            currentFrameIndex = 0;
        }

        public Image<Bgr, Byte> GetAverageFrame(double calcRate, bool offOrOnPrintInfo = false)
        {
            if(this.currentFrameIndex != 0)
            {
                this.ReCapVideo();
            }

            calcRate = Math.Min(1.0, calcRate);
            int skippedEach = (int)(1.0 / calcRate);

            Image<Bgr, int> averageFrame = this.ReadVideoImageMat().ToImage<Bgr, int>();
            Image<Bgr, int> tempFrame;
            int tempImageCount = 1;
            double finishRate = 1.0 / this.frameCount * 100;

            if (offOrOnPrintInfo)
            {
                Console.Write(String.Format("Getting Average Frame... {0,4:F2} %\t\t", finishRate) + 
                    new String('▋', (int)finishRate / 5));
            }

            for(int i = 1; i < this.frameCount; i++)
            {
                tempFrame = this.ReadVideoImageMat().ToImage<Bgr, int>();
                if(i % skippedEach == 0)
                {
                    averageFrame += tempFrame;
                    tempImageCount++;
                }
                if (offOrOnPrintInfo)
                {
                    finishRate = (double)i / this.frameCount * 100;
                    Console.Write("\r" + new String(' ', 70) + "\r" + String.Format("Getting Average Frame... {0,4:F2} %\t\t", finishRate) +
                    new String('▋', (int)finishRate / 5));
                }
            }


            return (averageFrame / tempImageCount).Convert<Bgr, Byte>();
        }

        public ArrayList GetFrameList()
        {
            ArrayList frameList = new ArrayList();
            if(this.currentFrameIndex != 0)
            {
                this.ReCapVideo();
            }

            for(int i = 0; i < this.frameCount; i++)
            {
                frameList.Add(this.ReadVideoImage());
            }

            return frameList;
        }

        public string SourceVideoFileName => sourceVideoFileName;

        public int SmallRectWidthInPix => smallRectWidthInPix;

        public int FrameWidth { get => frameWidth; }
        public int FrameHeight { get => frameHeight;}
        public int FrameCount { get => frameCount; }
        public double Fps { get => fps; }
        public int CurrentFrameIndex { get => currentFrameIndex; }
        public int RectRowCount { get => rectRowCount;}
        public int RectColCount { get => rectColCount;}
        public Rectangle[,] SmallRects { get => smallRects;}
    }
}
