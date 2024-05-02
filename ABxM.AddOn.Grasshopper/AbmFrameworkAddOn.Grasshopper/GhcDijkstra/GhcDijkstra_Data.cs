using System;
using System.Drawing;
using ABxM.AddOn.Core.Dijkstra;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using ABxM.AddOn.Grasshopper.Properties;
using Grasshopper;
using System.Collections.Generic;
using ABxM.Core.Utilities;
using Rhino.Geometry;

namespace ABxM.AddOn.Grasshopper.AbmFrameworkAddOn.Grasshopper.GhcDijkstra
{
    public class GhcDijkstra_Data : GH_Component
    {
        public GhcDijkstra_Data():base(
            "Dijkstra_GetData", "Dijkstra_Data",
            "Retrieve data from Dijkstra alorighm",
            "ABxM", "ABxM Core Map"
            ) { }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Dijkstra", "D", "Dijkstra", GH_ParamAccess.item);
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("FurthestPoints", "FP", "The furthest points from the source points", GH_ParamAccess.list);
            pManager.AddIntegerParameter("SourcePointId", "ID", "The Indices of the start points", GH_ParamAccess.list);
            pManager.AddPointParameter("SelectedPts", "SP", "The selected points from the source indices", GH_ParamAccess.list);
            //pManager.AddNumberParameter("GraphData", "G", "The two dimension arry of graph data for dijkstra algorithm as a datatree", GH_ParamAccess.tree);
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Dijkstra dijkstra = new Dijkstra();
            DA.GetData(0, ref dijkstra);
            dijkstra = dijkstra.Clone();
            var MeshVertices = dijkstra.Get_MeshVertices;
            var FurtherPts = dijkstra.Get_FurtherPts;
            var sourceIndices = dijkstra.Get_Source;
            double[,] graph = dijkstra.Get_Graph;
            //DataTree<double> graphTree = new DataTree<double>();
            //for (int i = 0; i < graph.GetLength(0); i++)
            //    for (int j = 0; j < graph.GetLength(1); j++)
            //    {
            //        graphTree.Add(graph[i, j], new GH_Path(i));
            //    }
            List<Point3d> SelectedPts = dijkstra.Get_SelectedPt;

            DA.SetDataList(0, FurtherPts);
            DA.SetDataList(1, sourceIndices);
            DA.SetDataList(2, SelectedPts);
            //DA.SetDataTree(3, graphTree);
        }
        protected override System.Drawing.Bitmap Icon => (Bitmap) new ImageConverter().ConvertFrom(Resources.Dijkstra_Data);
        public override Guid ComponentGuid => new Guid("{6200B199-8D38-48EB-977B-713A2B30ED9F}");
    }
}
