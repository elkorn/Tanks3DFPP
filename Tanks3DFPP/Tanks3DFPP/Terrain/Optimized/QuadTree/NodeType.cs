namespace Tanks3DFPP.Terrain.Optimization.QuadTree
{
    public enum NodeType
    {

        /// <summary>
        /// The full node - at the top level only. 
        /// A full node does not have any neighbors.
        /// </summary>
        FullNode = 0,

        /*
         * Following node typess appear at each level below the top one.
         */
        /// <summary>
        /// The top left node.
        /// </summary>
        TopLeft = 1,

        /// <summary>
        /// The top right node.
        /// </summary>
        TopRight = 2,

        /// <summary>
        /// The bottom left node.
        /// </summary>
        BottomLeft = 3,

        /// <summary>
        /// The bottom right node.
        /// </summary>
        BottomRight = 4
    }
}
