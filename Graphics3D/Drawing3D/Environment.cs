﻿using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;
using System.Runtime;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using System.Text;
using Graphics3D.Math3D;
using System.Drawing;
using Graphics3D.DepthTest;

namespace Graphics3D.Drawing3D
{
    [Serializable]
    public class Environment
    {
        Dictionary<String,Figure> figures = new Dictionary<String, Figure>();

        public Dictionary<String, Figure> Figures
        {
            get { return figures; }
            set { figures = value; }
        }

        public void Transform(Matrix matrix4x4)
        {
            foreach (Figure f in Figures.Values)
            {
                for (int i = 0; i < f.Vertexes.Count; i++)
                {
                    f.Vertexes[i].Transform(matrix4x4);
                }
            }
        }

        void transform(Matrix Rotate, Matrix Scale, Matrix Translate)
        {
            Matrix transformation = new Matrix(4);
            transformation *= Rotate * Scale * Translate;
            foreach(Figure f in Figures.Values)
            {
                for (int i = 0; i < f.Vertexes.Count; i++)
                {
                    f.Vertexes[i].Transform(transformation);
                }
            }
        }

        Transformation angle = new Transformation(),
                       scale = new Transformation(1,1,1),
                       translate = new Transformation();

        public Transformation Translate
        {
            get { return translate; }
            set { translate = value;
            transform(Matrix.GetRotationMatrix(Angle), Matrix.GetScaleMatrix(Scale), Matrix.GetTranslateMatrix(Translate));
            }
        }

        public Transformation Scale
        {
            get { return scale; }
            set { scale = value;
            transform(Matrix.GetRotationMatrix(Angle), Matrix.GetScaleMatrix(Scale), Matrix.GetTranslateMatrix(Translate));
            }
        }

        public Transformation Angle
        {
            get { return angle; }
            set { angle = value;
            //transform(Matrix.GetRotationMatrix(Angle), Matrix.GetScaleMatrix(Scale), Matrix.GetTranslateMatrix(Translate));
            transform(Matrix.GetRotationOZMatrix(Angle.OZ) * Matrix.GetRotationOYMatrix(Angle.OY) * Matrix.GetRotationOXMatrix(Angle.OX), Matrix.GetScaleMatrix(Scale), Matrix.GetTranslateMatrix(Translate));
            }
        }

        public void TransformationRefresh()
        {
            transform(Matrix.GetRotationOZMatrix(Angle.OZ) * Matrix.GetRotationOYMatrix(Angle.OY) * Matrix.GetRotationOXMatrix(Angle.OX), Matrix.GetScaleMatrix(Scale), Matrix.GetTranslateMatrix(Translate));
        }
        Size lastImage;
        double z = -500;
        public Bitmap GetImage(int width, int height)
        {
           /* TransformationRefresh();
            lastImage = new Size(width, height);
            Bitmap b = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(b);
            g.Clear(backgroundColor);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            
            Random a = new Random();
            try
            {
                foreach (Figure f in Enumerable.Where(Figures.Values, f => !f.Hidden))
                {
                    float w = 1;
                    if (f.Selected)
                    {
                        w = 2;
                    }
                    foreach (Line l in f.Lines)
                    {
                        
                        Pen p = new Pen(l.BorderColor, w);
                        p.DashStyle = l.Type;
                        g.DrawLine(p,
                            new Point3D(l.P1.TPoint.X * (z / (z - l.P1.TPoint.Z)) + width / 2, -l.P1.TPoint.Y * (z / (z - l.P1.TPoint.Z)) + height / 2, 1),
                            new Point3D(l.P2.TPoint.X * (z / (z - l.P2.TPoint.Z)) + width / 2, -l.P2.TPoint.Y * (z / (z - l.P2.TPoint.Z)) + height / 2, 1));
                        
                        p.Dispose();
                    }
                }
            }catch (Exception){}
            return b;*/
            TransformationRefresh();
            lastImage = new Size(width, height);
            ZBitmap b = new ZBitmap(width, height, backgroundColor);
            Random a = new Random();
                foreach (Figure f in Enumerable.Where(Figures.Values, f => !f.Hidden))
                {
                    foreach (Line l in f.Lines)
                        try{
                        b.DrawLine(l);
                        }catch (Exception) { }
            }
           
            return b.Bitmap;
        }

        public Figure CheckFigure(Point2D mouse)
        {
            try
            {
                foreach (Figure f in Figures.Values)
                {
                    foreach (Line l in f.Lines)
                    {
                        Point2D p1 = new Point2D(l.P1.TPoint.X * (z / (z - l.P1.TPoint.Z)) + lastImage.Width / 2, -l.P1.TPoint.Y * (z / (z - l.P1.TPoint.Z)) + lastImage.Height / 2);
                        Point2D p2 = new Point2D(l.P2.TPoint.X * (z / (z - l.P2.TPoint.Z)) + lastImage.Width / 2, -l.P2.TPoint.Y * (z / (z - l.P2.TPoint.Z)) + lastImage.Height / 2);
                        if (dist(mouse, p1) + dist(mouse, p2)- 0.3 <= dist(p1, p2) && f.Selectable)
                            return f;
                    }
                }
            }
            catch (Exception) {}
            return null;
        }

        private double dist(Point2D p1, Point2D p2)
        {
            return Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y));
        }

        public void AddFigure(Figure figure)
        {
            Figures.Add(figure.Name, figure);
        }

        public static void Save(String filename, Environment environment)
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = File.Open(filename, FileMode.Create);
            formatter.Serialize(stream, environment);
            stream.Close(); 
        }

        public static Environment Load(String filename)
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = File.Open(filename, FileMode.Open);
            Environment env = (Environment)formatter.Deserialize(stream);
            stream.Close();
            return env;
        }

        Color backgroundColor = Color.Black;

        public Color BackgroundColor
        {
            get { return backgroundColor; }
            set { backgroundColor = value; }
        }

    }
}
