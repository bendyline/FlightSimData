/* Copyright (c) Bendyline LLC. All rights reserved. Licensed under the Apache License, Version 2.0.
    You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0. */

using System;
using System.Collections.Generic;

namespace Bendyline.FlightSimulator.Data
{
    public class Vertex
    {
        private double x;
        private double y;

        public const double PolygonClosenessThreshold = 0.01;

        public Vertex()
        {

        }

        public Vertex(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public double X
        {
            get
            {
                return this.x;
            }

            set
            {
                this.x = value;
            }
        }

        public double Y
        {
            get
            {
                return this.y;
            }

            set
            {
                this.y = value;
            }
        }

        public bool IsClose(object obj, double tolerance)
        {
            if (obj is Vertex)
            {
                return (  Math.Abs(this.X - ((Vertex)obj).X) < tolerance &&  Math.Abs( this.Y - ((Vertex)obj).Y) < tolerance);
            }

            return base.Equals(obj);
        }

        public bool IsCloseAndToNorth(object obj, double toleranceX, double toleranceY)
        {
            if (obj is Vertex)
            {
                double yDistance = this.Y - ((Vertex)obj).Y;

                return (Math.Abs(this.X - ((Vertex)obj).X) < toleranceX && yDistance >= 0 && yDistance <= toleranceY);
            }

            return base.Equals(obj);
        }

        public bool IsCloseAndToSouth(object obj, double toleranceX, double toleranceY)
        {
            if (obj is Vertex)
            {
                double yDistance = ((Vertex)obj).Y - this.Y;

                return (Math.Abs(this.X - ((Vertex)obj).X) < toleranceX && yDistance >= 0 && yDistance <= toleranceY);
            }

            return base.Equals(obj);
        }

        public bool IsClose(object obj, double toleranceX, double toleranceY)
        {
            if (obj is Vertex)
            {
                return (Math.Abs(this.X - ((Vertex)obj).X) < toleranceX && Math.Abs(this.Y - ((Vertex)obj).Y) < toleranceY);
            }

            return base.Equals(obj);
        }

        public static List<Vertex> GetAllWesternVertices(List<Vertex> vertices)
        {
            List<Vertex> westernVertices = new List<Vertex>();

            double x = 1;

            foreach (Vertex v in vertices)
            {
                if (v.X < x)
                {
                    x = v.X;
                }
            }

            for (int i = 0; i < vertices.Count; i++)
            {
                Vertex v = vertices[i];

                if (Math.Abs(v.X - x) < PolygonClosenessThreshold)
                {
                    westernVertices.Add(v);
                }
            }

            return westernVertices;
        }


        public static List<Vertex> GetWesternVertices(List<Vertex> vertices)
        {
            List<Vertex> westernVertices = new List<Vertex>();

            double x = 1;

            int maxStreak = 0;
            int curStreak = 0;

            foreach (Vertex v in vertices)
            {
                if (v.X < x)
                {
                    x = v.X;
                }
            }

            for (int i = 1; i < vertices.Count; i++)
            {
                Vertex v = vertices[i];

                if (v.X == x)
                {
                    westernVertices.Add(v);

                    curStreak++;

                    if (curStreak > maxStreak)
                    {
                        maxStreak = curStreak;
                    }
                }
                else
                {
                    curStreak = 0;
                }
            }

            if (maxStreak != westernVertices.Count)
            {
                westernVertices.Clear();
            }


            return westernVertices;
        }


        public static List<Vertex> GetEasternVertices(List<Vertex> vertices)
        {
            List<Vertex> easternVertices = new List<Vertex>();

            double x = 0;

            int maxStreak = 0;
            int curStreak = 0;

            foreach (Vertex v in vertices)
            {
                if (v.X > x)
                {
                    x = v.X;
                }
            }

            for (int i = 1; i < vertices.Count; i++)
            {
                Vertex v = vertices[i];

                if (v.X == x)
                {
                    easternVertices.Add(v);

                    curStreak++;

                    if (curStreak > maxStreak)
                    {
                        maxStreak = curStreak;
                    }
                }
                else
                {
                    curStreak = 0;
                }
            }

            if (maxStreak != easternVertices.Count)
            {
                easternVertices.Clear();
            }

            return easternVertices;
        }

        public static List<Vertex> GetSouthernVertices(List<Vertex> westVertices, List<Vertex> eastVertices)
        {
            List<Vertex> southernVertices = new List<Vertex>();

            if (eastVertices.Count == 2 && westVertices.Count == 2)
            {
                if (westVertices[1].Y < westVertices[0].Y)
                {
                    southernVertices.Add(westVertices[1]);
                }
                else
                {
                    southernVertices.Add(westVertices[0]);
                }

                if (eastVertices[1].Y < eastVertices[0].Y)
                {
                    southernVertices.Add(eastVertices[1]);
                }
                else
                {
                    southernVertices.Add(eastVertices[0]);
                }
            }

            return southernVertices;
        }

        public static List<Vertex> GetNorthernVertices(List<Vertex> westVertices, List<Vertex> eastVertices)
        {
            List<Vertex> northernVertices = new List<Vertex>();

            if (eastVertices.Count == 2 && westVertices.Count == 2)
            {
                if (westVertices[1].Y > westVertices[0].Y)
                {
                    northernVertices.Add(westVertices[1]);
                }
                else
                {
                    northernVertices.Add(westVertices[0]);
                }

                if (eastVertices[1].Y > eastVertices[0].Y)
                {
                    northernVertices.Add(eastVertices[1]);
                }
                else
                {
                    northernVertices.Add(eastVertices[0]);
                }
            }

            return northernVertices;
        }

    }
}
