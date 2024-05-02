
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Drawing;
using ABxM.AddOn.Core.Dijkstra;
using System.Collections.Generic;
using ABxM.AddOn.Grasshopper.Properties;

namespace ABxM.AddOn.Grasshopper.AbmFrameworkAddOn.Grasshopper.GhcDijkstra
{
    public class GhcDijkstra : GH_Component
    {
        private Dijkstra dijkstra = new Dijkstra();
        public GhcDijkstra():base("Dijkstra", "Dijkstra",
            "To Set up a dijkstra alorighm to get the distances among vertices from a mesh map",
            "ABxM", "ABxM Core Map") { }
        
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("SourcePts", "S", "The start point on a dijkstra map", GH_ParamAccess.list);
            pManager.AddMeshParameter("Mesh", "M", "Input mesh as a map", GH_ParamAccess.item);
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Dijstra", "D", "The dijstra class for asking other result", GH_ParamAccess.item);
            pManager.AddNumberParameter("ShortDistance", "SDist", "This distance among vertices in the map", GH_ParamAccess.tree);
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            this.dijkstra = new Dijkstra();
            Mesh mesh = new Mesh();
            var Pts = new List<Point3d>();
            DA.GetDataList(0, Pts);
            DA.GetData(1, ref mesh);
            this.dijkstra.Set_Graph(mesh);
            var source = this.dijkstra.Set_Source(Pts);
            this.dijkstra.DijkstraAlgo(source);
            DA.SetData(0, dijkstra);
            DA.SetDataTree(1, dijkstra.Get_ShortDistance);
        }
        protected override System.Drawing.Bitmap Icon => (Bitmap)new ImageConverter().ConvertFrom(Resources.Dijkstra);
        public override Guid ComponentGuid => new Guid("{03A9303E-3C85-45E1-9AB2-24E8CE817F36}");
    }
}
