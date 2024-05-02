using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino.Geometry;
using System;
using System.Drawing;
using ABxM.AddOn.Core.Dijkstra;
using Grasshopper.Kernel.Special;
using System.Xml.Schema;
using System.Collections.Generic;
using System.Web;
using ABxM.AddOn.Grasshopper;
using System.Runtime.Versioning;
using ABxM.AddOn.Grasshopper.Properties;

namespace ABxM.AddOn.Grasshopper.AbmFrameworkAddOn.Grasshopper.GhcDijkstra
{
    public class GhcDijkstra_Grid : GH_Component
    {
        DijkstraMesh GridMesh = new DijkstraMesh();
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public GhcDijkstra_Grid()
          : base("Dijkstra_GridFromSurface", "SrfToGrid",
            "Convert a trimmed surface into a triangular or quadrectangular mesh grid",
            "ABxM", "ABxM Core Map")
        {
        }
        public override void AddedToDocument(GH_Document document)
        {
            base.AddedToDocument(document);

            //Add Value List
            int[] stringID = new int[] { 3 };
            for (int i = 0; i < stringID.Length; i++)
            {
                Param_String in0str = Params.Input[stringID[i]] as Param_String;
                if (in0str == null || in0str.SourceCount > 0 || in0str.PersistentDataCount > 0) return;
                Attributes.PerformLayout();
                int x = (int)in0str.Attributes.Pivot.X - 250;
                int y = (int)in0str.Attributes.Pivot.Y - 10;
                GH_ValueList valueList = new GH_ValueList();
                valueList.CreateAttributes();
                valueList.Attributes.Pivot = new System.Drawing.PointF(x, y);
                valueList.ListItems.Clear();

                List<GH_ValueListItem> Type = new List<GH_ValueListItem>() { 
                    new GH_ValueListItem("TriangulateGrid", "0"), 
                    new GH_ValueListItem("QuadrectangulateGrid", "1")
                };
                valueList.ListItems.AddRange(Type);
                document.AddObject(valueList, false);
                in0str.AddSource(valueList);
            }
        }
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("SurfaceMap", "Srf", "The trimmed surface map for converting into grid", GH_ParamAccess.item);
            pManager.AddNumberParameter("GridSize", "Sg", "This size of Grid, an optional setting, is to resize the grid instead of setting from default system unit", GH_ParamAccess.item);
            pManager[1].Optional = true;
            pManager.AddNumberParameter("Tolerance", "Tor", "This parameter is to set the tolerance for converting mesh according to a void or voids on the trimmed surface", GH_ParamAccess.item);
            pManager[2].Optional = true;
            pManager.AddTextParameter("Type", "t", "To set the grid type for the grid, which is a value list", GH_ParamAccess.item);
            pManager[3].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("GridMesh", "M", "The Grid", GH_ParamAccess.item);
            pManager.AddCurveParameter("Periphery", "P", "The boundary of the grid", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Brep iSrf = new Brep();
            DA.GetData(0, ref iSrf);
            double Gridsize = 0;
            DA.GetData(1, ref Gridsize);
            double Tolerance = 0;
            DA.GetData(2, ref Tolerance);
            string TypeName = "";
            DA.GetData(3, ref TypeName);
            
            if (Gridsize > 0)
                this.GridMesh.gridsize = Gridsize;
            else
                this.GridMesh.UnitSize_Adjust();
            
            if (Tolerance > 0)
                this.GridMesh.Tolerance = Tolerance;
            
            else
                this.GridMesh.DocTolerance();
            this.GridMesh.type = TypeName == "0" ? Core.Dijkstra.Type.Triangulate : Core.Dijkstra.Type.Quadrangulate;
            this.GridMesh.SetGrid(iSrf);

            DA.SetData(0, GridMesh.GetGrid());
            DA.SetDataList(1, GridMesh.GetGridBoundary());
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// You can add image files to your project resources and access them like this:
        /// return Resources.IconForThisComponent;
        /// </summary>
        protected override System.Drawing.Bitmap Icon => ((Bitmap) new ImageConverter().ConvertFrom(Properties.Resources.MeshGrid_2));

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("9AE6531D-8919-4449-A14A-8FDCA1BA4C9B");
    }
}