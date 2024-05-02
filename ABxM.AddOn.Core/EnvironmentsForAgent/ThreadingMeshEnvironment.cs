using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Rhino;
using System.Drawing;
using Rhino.Geometry;
using ABxM.Core.Environments;
using System.Collections.Concurrent;
using ABxM.Core.Utilities;

namespace ABxM.AddOn.Core.EnvironmentsForAgent
{
    /// <summary>
    /// This Class specifies for threading process of mesh.
    /// </summary>
    public class ThreadingMeshEnvironment : SurfaceEnvironment
    {
        public ConcurrentBag<Point3d> MeshVertices = new ConcurrentBag<Point3d>();
        public ConcurrentBag<Point3d> StartMeshVertices = new ConcurrentBag<Point3d>();
        public ConcurrentBag<MeshFace> Faces = new ConcurrentBag<MeshFace>();
        public ConcurrentBag<System.Drawing.Color> MeshColour = new ConcurrentBag<Color>();
        private ThreadingMeshEnvironment() { }
        public ThreadingMeshEnvironment(Mesh mesh)
        {
            this.SetMesh(mesh);
        }
        public override void Reset()
        {
            this.MeshVertices = new ConcurrentBag<Point3d>(StartMeshVertices);
        }
        public void SetMesh(Mesh mesh)
        {
            var Vertices = MeshConvertor.MeshVertices(mesh);
            foreach (var Vertex in Vertices)
            {
                this.MeshVertices.Add(Vertex);
                this.StartMeshVertices.Add(Vertex);
            }
            var faces = mesh.Faces;
            foreach (var Face in faces)
            {
                this.Faces.Add(Face);
            }
            var colours = mesh.VertexColors;
            foreach (var Vertex in colours)
                this.MeshColour.Add(Vertex);
        }
        public Mesh GetMesh()
        {
            var mesh = new Mesh();

            //Vertices Setting
            foreach (Point3d Pt in this.MeshVertices)
                mesh.Vertices.Add(Pt);

            //Faces Setting
            foreach (MeshFace meshFace in this.Faces)
                mesh.Faces.AddFace(meshFace);

            //Colours Setting
            foreach (var Vertex in this.MeshColour)
                mesh.VertexColors.Add(Vertex);
            return mesh;
        }

        public override List<object> GetDisplayGeometry()
        {
            List<object> objectlist = new List<object>();
            objectlist.Add(this.GetMesh());
            return objectlist;
        }
        public override Point3d GetClosestPoint(Point3d position)
        {
            return this.GetMesh().ClosestPoint(position);
        }
        public override Plane GetTangentPlane(Point3d position)
        {
            Point3d ptOnMesh;
            Vector3d normalOnMesh;
            if (this.GetMesh().ClosestPoint(position, out ptOnMesh, out normalOnMesh, 0.0) != -1)
            {
                return new Plane(ptOnMesh, normalOnMesh);
            }
            return Plane.Unset;
        }
        public override Vector3d GetNormal(Point3d position)
        {
            return GetTangentPlane(position).Normal;
        }
        public override Point3d IntersectWithLine(Line line)
        {
            Point3d[] intPts = Rhino.Geometry.Intersect.Intersection.RayShoot(
                new Ray3d(line.From, line.Direction),
                new List<GeometryBase>() { this.GetMesh() }, 1
                );
            //output
            if (intPts == null || intPts.Length == 0) return Point3d.Unset;
            return intPts[0];
        }
        public override List<Curve> GetBoundaryCurves3D()
        {
            List<Curve> boundaryCurves = new List<Curve>();
            Polyline[] contours = this.GetMesh().GetNakedEdges();
            boundaryCurves.AddRange(contours.Select((Polyline p) => p.ToNurbsCurve()));
            return boundaryCurves;
        }
    }
}
