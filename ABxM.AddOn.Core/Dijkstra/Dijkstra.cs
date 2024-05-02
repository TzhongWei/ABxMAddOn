using Grasshopper.Kernel.Data;
using Grasshopper;
using System;
using System.Collections.Generic;
using Rhino.Geometry;
using ABxM.Core.Utilities;
using System.Linq;

namespace ABxM.AddOn.Core.Dijkstra
{
    /// <summary>
    /// 
    /// </summary>
    internal class NamespaceDoc { }

    public enum ColourMesh
    {
        BlackToWhite = 0,
        RedToBlue = 1,
        GreenToRed = 2,
        WhiteToBlack = 3,
        BlueToGreen = 4
    }
    public class Dijkstra
    {
        //Property
        private double[,] graph { get; set; }
        private List<int> source = new List<int>();
        private List<int> V0List = new List<int>();
        private List<int> V1List = new List<int>();
        private List<double> dList = new List<double>();
        public ColourMesh ColourChannel = ColourMesh.BlackToWhite;
        protected readonly DataTree<double> ShortDistTree = new DataTree<double>();
        public Mesh mesh { get; set; }
        private int NumOfVertices = 0;
        //constructor
        public Dijkstra() { }
        public Dijkstra Set_Graph(Mesh mesh)
        {
            MeshConvertor.MeshConvert(mesh, out NumOfVertices, out V0List, out V1List, out dList);
            this.Set_Graph(NumOfVertices, V0List, V1List, dList);
            this.mesh = mesh;
            return this;
        }
        //Method
        public Dijkstra Set_Graph(int NumOfVertices, List<int> V0List, List<int> V1List, List<double> dList)
        {
            this.V0List = V0List;
            this.V1List = V1List;
            this.dList = dList;
            this.NumOfVertices = NumOfVertices;
            graph = new double[NumOfVertices, NumOfVertices];
            for (int i = 0; i < NumOfVertices; i++)
            {
                this.graph[i, i] = 0.0;
                for (int j = 0; j < NumOfVertices; j++)
                    this.graph[i, j] = double.MaxValue;
            }
            for (int i = 0; i < dList.Count; i++)
                this.graph[V0List[i], V1List[i]] = dList[i];
            return this;
        }        
        public List<int> Set_Source(List<Point3d> Pts)
        {
            var Verts = MeshConvertor.MeshVertices(mesh);
            var Index = RTree.Point3dKNeighbors(Verts, Pts, 1);
            foreach (int[] In in Index)
                this.source.AddRange(In);
            return source;
        }
        public void DijkstraAlgo(int source)
        {
            this.source.Add(source);
            var ShortDist = Dijkstra.DijkstraAlgo(this.graph, source, this.NumOfVertices);
            this.ShortDistTree.AddRange(ShortDist, new GH_Path(0));
        }
        public void DijkstraAlgo(List<int> source)
        {
            this.source = source;
            for (int i = 0; i < source.Count; i++)
            {
                GH_Path Path = new GH_Path(i);
                double[] ShortDist = Dijkstra.DijkstraAlgo(this.graph, source[i], this.NumOfVertices);
                ShortDistTree.AddRange(ShortDist, Path);
            }
        }
        public double[,] Get_Graph => this.graph;
        public DataTree<double> Get_ShortDistance => this.ShortDistTree;
        public List<Point3d> Get_SelectedPt => MeshConvertor.MeshVertex_From_Index(mesh, source);
        public List<Mesh> Get_ColourMesh => MeshConvertor.ColourMesh(mesh, this.ShortDistTree, (int)ColourChannel).MeshCols;
        public List<Point3d> Get_MeshVertices => MeshConvertor.MeshVertices(mesh);
        public List<Point3d> Get_FurtherPts => this.FurtherPts();
        public List<int> Get_Source => this.source;
        protected List<Point3d> FurtherPts()
        {
            List<Point3d> FurPts = new List<Point3d>();
            for (int i = 0; i < source.Count; i++)
            {
                double[] DistList = ShortDistTree.Branch(i).ToArray();
                int[] ints = Enumerable.Range(0, DistList.Length).ToArray();
                Array.Sort(DistList, ints);
                FurPts.Add(MeshConvertor.MeshVertex_From_Index(this.mesh, ints[DistList.Length - 1]));
            }
            return FurPts;
        }
        //static
       
        private static int MinimumDistance(double[] distance, bool[] shortestPathTreeSet, int verticesCount)
        {
            double min = double.MaxValue;
            int minIndex = 0;

            for (int v = 0; v < verticesCount; ++v)
            {
                if (shortestPathTreeSet[v] == false && distance[v] <= min)
                {
                    min = distance[v];
                    minIndex = v;
                }
            }

            return minIndex;
        }
        public static double[] DijkstraAlgo(double[,] graph, int source, int verticesCount)
        {
            double[] distance = new double[verticesCount];
            bool[] shortestPathTreeSet = new bool[verticesCount];

            for (int i = 0; i < verticesCount; ++i)
            {
                distance[i] = double.MaxValue;
                shortestPathTreeSet[i] = false;
            }

            distance[source] = 0.0;

            for (int count = 0; count < verticesCount - 1; ++count)
            {
                int u = MinimumDistance(distance, shortestPathTreeSet, verticesCount);
                shortestPathTreeSet[u] = true;

                for (int v = 0; v < verticesCount; ++v)
                    if (!shortestPathTreeSet[v] && Convert.ToBoolean(graph[u, v]) && distance[u] != int.MaxValue && distance[u] + graph[u, v] < distance[v])
                        distance[v] = distance[u] + graph[u, v];
            }

            //PrintResult(distance, verticesCount);
            return (distance);
        }
        public Dijkstra Clone()
        {
            Dijkstra clone = new Dijkstra();
            clone.source = source;
            clone.ColourChannel = this.ColourChannel;
            clone.mesh = mesh;
            clone.Set_Graph(this.mesh);
            clone.DijkstraAlgo(clone.source);
            return clone;
        }
    }
}
