using System;
using System.Collections.Generic;

using Rhino;
using Rhino.Geometry;
using ABxM.Core.Utilities;

namespace ABxM.AddOn.Core.Dijkstra
{
    public enum Type
    {
        Triangulate = 0,
        Quadrangulate = 1
    }
    public class DijkstraMesh
    {
        public double gridsize = 1.0;
        private Mesh Grid = new Mesh();
        private Brep iSurface;
        private int U = 0, V = 1;
        public Type type = Type.Quadrangulate;
        public double Tolerance = 0.1;
        public DijkstraMesh() { }
        public DijkstraMesh SetGrid(Brep iSurface)
        {
            this.iSurface = iSurface;
            Surface Untrimmed = Untrimmed_Srf(iSurface);
            Interval Dom_U = Untrimmed.Domain(U);
            Interval Dom_V = Untrimmed.Domain(V);
            List<Curve> IsoLines = new List<Curve>();
            double Max = Dom_U.Max, Min = Dom_U.Min;
            double start = Min + (Max - Math.Floor(Max / gridsize) * gridsize);
            double Start = Min + (Max - Math.Floor(Max / gridsize) * gridsize);
            if (Start != Min) IsoLines.Add(Untrimmed.IsoCurve(V, Min));

            for (double i = Start; i < Max; i += gridsize)
                IsoLines.Add(Untrimmed.IsoCurve(V, i));
            if (Math.Floor(Max / gridsize) * gridsize != Max) IsoLines.Add(Untrimmed.IsoCurve(V, Max));

            Max = Dom_V.Max;
            Min = Dom_V.Min;
            Start = Start = Min + (Max - Math.Floor(Max / gridsize) * gridsize);
            List<double> t = new List<double>();
            if (Start != Min) t.Add(Min);
            for (double i = Start; i < Max; i += gridsize) t.Add(i);
            if (Math.Floor(Max / gridsize) * gridsize != Max) t.Add(Max);
            Point3d[,] PtsV = new Point3d[IsoLines.Count, t.Count];

            for (int i = 0; i < t.Count; i++)
            {
                for (int j = 0; j < IsoLines.Count; j++)
                {
                    Point3d Pt = IsoLines[j].PointAt(t[i]);
                    PtsV[j, i] = Pt;
                }
            }

            Grid = new Mesh();
            List<Point3d> MidPts = new List<Point3d>();
            if (type == Type.Triangulate)
            {
                for (int i = 0; i < PtsV.GetLength(0) - 1; i++)
                {
                    for (int j = 0; j < PtsV.GetLength(1) - 1; j++)
                    {
                        Mesh Temp = new Mesh();
                        Point3d V1 = PtsV[i, j], V2 = PtsV[i + 1, j], V3 = PtsV[i + 1, j + 1], V4 = PtsV[i, j + 1];
                        Point3d VMid = (V1 + V2 + V3 + V4) / 4;
                        Temp.Vertices.Add(V1);           //0
                        Temp.Vertices.Add(V2);           //1
                        Temp.Vertices.Add(V3);           //2
                        Temp.Vertices.Add(V4);           //3
                        Temp.Vertices.Add(VMid);         //4
                        Temp.Faces.AddFace(0, 1, 4);
                        Temp.Faces.AddFace(1, 2, 4);
                        Temp.Faces.AddFace(2, 3, 4);
                        Temp.Faces.AddFace(3, 0, 4);
                        Grid.Append(Temp);
                        MidPts.Add(VMid);
                    }
                }
                Grid.Weld(0.5);
            }
            else {
                for (int i = 0; i < PtsV.GetLength(0) - 1; i++)
                {
                    for (int j = 0; j < PtsV.GetLength(1) - 1; j++)
                    {
                        Mesh Temp = new Mesh();
                        Temp.Vertices.Add(PtsV[i, j]);
                        Temp.Vertices.Add(PtsV[i + 1, j]);
                        Temp.Vertices.Add(PtsV[i + 1, j + 1]);
                        Temp.Vertices.Add(PtsV[i, j + 1]);
                        Temp.Faces.AddFace(0, 1, 2, 3);
                        Grid.Append(Temp);
                    }
                }
                Grid.Weld(0.5);
            }

            var Vertices = Grid.Vertices;
            List<double> Dist = new List<double>();
            for (int i = 0; i < Vertices.Count; i++)
            {
                Point3d Pt = new Point(Vertices[i]).Location;
                Dist.Add(BrepDistTo(iSurface, Pt));
            }
            List<int> CullData = new List<int>();
            for (int i = 0; i < Dist.Count; i++)
            {
                if (Dist[i] > this.Tolerance) CullData.Add(i);
            }
            Vertices.Remove(CullData, false);


            return this;
        }
        public Mesh GetGrid() => Grid;
        public Polyline[] GetGridBoundary() => Grid.GetNakedEdges();
        public void DocTolerance()
        {
            RhinoDoc doc = RhinoDoc.ActiveDoc;
            this.Tolerance = doc.ModelAbsoluteTolerance;
        }
        public void UnitSize_Adjust()
        {
            RhinoDoc doc = RhinoDoc.ActiveDoc;
            UnitSystem currentUnit = doc.ModelUnitSystem;
            gridsize = 1;
            switch (currentUnit)
            {
                case UnitSystem.Meters:
                    gridsize = 5;
                    break;
                case UnitSystem.Centimeters:
                    gridsize = 0.5;
                    break;
                case UnitSystem.Feet:
                    gridsize = 3.28084 / 2;
                    break;
                case UnitSystem.Inches:
                    gridsize = 3.28084 * 12 / 2;
                    break;
                default:
                    gridsize = 5;
                    break;
            }
        }
        protected Surface Untrimmed_Srf(Brep TrimmedSrf)
        {
            if (TrimmedSrf.Faces.Count != 1) return null;
            var SingleSrf = TrimmedSrf.Faces[0];
            Surface Srf = SingleSrf.UnderlyingSurface();
            return Srf;
        }
        protected double BrepDistTo(Brep Surf, Point3d Pt)
        {
            Point3d ClosetPt = Surf.ClosestPoint(Pt);
            return ClosetPt.DistanceTo(Pt);
        }
    }
}
