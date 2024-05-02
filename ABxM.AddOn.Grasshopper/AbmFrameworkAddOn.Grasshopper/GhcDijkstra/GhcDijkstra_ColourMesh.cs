using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using ABxM.AddOn.Core.Dijkstra;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Special;
using ABxM.AddOn.Grasshopper.Properties;
using System.Drawing;

namespace ABxM.AddOn.Grasshopper.AbmFrameworkAddOn.Grasshopper.GhcDijkstra
{
    public class GhcDijkstra_ColourMesh : GH_Component
    {
        public GhcDijkstra_ColourMesh() : base(
            "Dijkstra_ColourMesh", "Dijkstra_Mesh",
            "Show the Coloured mesh from Dijkstra alorighm",
            "ABxM", "ABxM Core Map"
            ) { }
        public override void AddedToDocument(GH_Document document)
        {
            base.AddedToDocument(document);

            //Add Value List
            int[] stringID = new int[] { 0 };
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
                    new GH_ValueListItem("Black-White", "0"),
                    new GH_ValueListItem("White-Black", "3"),
                    new GH_ValueListItem("Red-Blue", "1"),
                    new GH_ValueListItem("Green-Red", "2"),
                    new GH_ValueListItem("Blue-Green", "4")
                };
                valueList.ListItems.AddRange(Type);
                document.AddObject(valueList, false);
                in0str.AddSource(valueList);
            }
        }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("ColourChannel", "C", "Set the colour gradient based on dijkstra alorighm onto mesh", GH_ParamAccess.item);
            pManager.AddGenericParameter("Dijkstra", "D", "Dijkstra", GH_ParamAccess.item);
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("ColouredMesh", "M", "The coloured mesh based on the Dijkstra", GH_ParamAccess.list);
            
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Dijkstra dijkstra = null;
            string ColourChannel = "";
            DA.GetData(0, ref ColourChannel);
            DA.GetData(1, ref dijkstra);
            dijkstra = dijkstra.Clone();
            switch (ColourChannel)
            {
                case ("0"):
                    dijkstra.ColourChannel = ColourMesh.BlackToWhite; break;
                case ("1"):
                    dijkstra.ColourChannel = ColourMesh.RedToBlue; break;
                case ("2"):
                    dijkstra.ColourChannel = ColourMesh.GreenToRed; break;
                case ("3"):
                    dijkstra.ColourChannel = ColourMesh.WhiteToBlack; break;
                case ("4"):
                    dijkstra.ColourChannel = ColourMesh.BlueToGreen; break;
                default:
                    dijkstra.ColourChannel = ColourMesh.WhiteToBlack;
                    break;
            }
            var mesh = dijkstra.Get_ColourMesh;
            var Obs = dijkstra.Get_SelectedPt;
            DA.SetDataList(0, mesh);
        }
        protected override System.Drawing.Bitmap Icon => (Bitmap)new ImageConverter().ConvertFrom(Resources.Dijkstra_colour);
        public override Guid ComponentGuid => new Guid("{27B2B63A-C4E4-4DFA-AEB6-46503C2C3DDA}");
    }
}
