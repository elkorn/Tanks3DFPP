using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Tanks3DFPP.Terrain.Optimization.QuadTree
{
    public class QuadNode
    {
        /// <summary>
        /// The parent node - ???
        /// </summary>
        private QuadNode parentNode;

        /// <summary>
        /// The parent tree - lower LOD.
        /// </summary>
        private QuadTreeTerrain parentTree;

        private int depth,
                    size,
                    positionIndex;

        private bool hasChildren;
        private bool _isRendered;
        private bool isSplit;


        public int Depth
        {
            get
            {
                return this.depth;
            }
        }

        private bool isRendered
        {
            get { return _isRendered; }
            set
            {
                Game1.SetNodeAsRendered(this, value);
                _isRendered = value;
            }
        }

        /*
         * Each quad looks like this:
         * (V - QuadNodeVertex)
         *
         *      V------V------V
         *      |      |      |
         *      |      |      |
         *      V------V------V
         *      |      |      |
         *      |      |      |
         *      V------V------V
         * 
         *  Since we are operating on triangles.
         *  Thus the need for 9 vertices per node.
         *  The order of activating vertices is as follows:
         *      1) Top-left
         *      2) Top-right
         *      3) Center
         *      4) Bottom-left
         *      5) Bottom-right
         *  Only after splitting are the Top, Left, Right and Bottom (in that order) vertices activated.
         *  
         * Now let's say that quad has a neighbor to the right.  
         * Since we've activated the "Right" vertex on this node by splitting it 
         * we have to also activate the "Left" node on the neighbor to the right 
         * otherwise we'll have a split node up against an unsplit node which 
         * will create a visible seam. 
         * Having the extra vertices at the quad level allows us to activate 
         * only the necessary vertices without completely splitting 
         * the neighboring quad.
         */
        #region VERTICES
        public QuadNodeVertex VertexTopLeft;
        public QuadNodeVertex VertexTop;
        public QuadNodeVertex VertexTopRight;
        public QuadNodeVertex VertexLeft;
        public QuadNodeVertex VertexCenter;
        public QuadNodeVertex VertexRight;
        public QuadNodeVertex VertexBottomLeft;
        public QuadNodeVertex VertexBottom;
        public QuadNodeVertex VertexBottomRight;
        #endregion

        #region CHILDREN
        public QuadNode ChildTopLeft;
        public QuadNode ChildTopRight;
        public QuadNode ChildBottomLeft;
        public QuadNode ChildBottomRight;
        #endregion

        #region NEIGHBORS
        public QuadNode NeighborTop;
        public QuadNode NeighborRight;
        public QuadNode NeighborBottom;
        public QuadNode NeighborLeft;
        #endregion

        public readonly BoundingBox Bounds;

        public NodeType Type;


        /// <summary>
        /// The absolute value of height constituting the bounding volume of the current node.
        /// </summary>
        private const float limY = 950f;

        /// <summary>
        /// The minimum size a quad node has to be to contain children.
        /// A node smaller than that is actually on the highest LOD.
        /// Setting this to a higher number will result in a terrain with a lower maximum level of detail.
        /// </summary>
        private const int minSizeToContainChildren = 4;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuadNode" /> class.
        /// </summary>
        /// <param name="type">Type of node.</param>
        /// <param name="size">Width/Height of node (# of vertices across - 1).</param>
        /// <param name="depth">Depth of current node</param>
        /// <param name="parent">Parent QuadNode</param>
        /// <param name="parentTree">Top level Tree.</param>
        /// <param name="positionIndex">Index of top left Vertex in the parent tree Vertices array</param>
        public QuadNode(
            NodeType type,
            int size,
            int depth,
            QuadNode parent,
            QuadTreeTerrain parentTree,
            int positionIndex)
        {
            this.Type = type;
            this.size = size;
            this.depth = depth;
            
            this.parentNode = parent;
            this.parentTree = parentTree;

            this.positionIndex = positionIndex;


            this.AddVertices();

            /*
             * The bounding box is used to determine where the tree
             * should be split and where to look for nodes that are within view.
             * 
             * In a production system it would make more sense to use the maximum height value of the tree as a guide to define the bounds of the bounding box.
             * Even the maximum height within each individual quad may be used to define the bounds which would make culling away invisible quads much more efficient when standing on a mountain.
             * In fact, a more efficient method would be to use bounding shapes instead of volumes, using a rectangle as the bounding object.
             * The view frustrum could then be projected onto the terrain as a 2D shape.
             * Intersection checks for LOD would be much more efficient.
             */
            this.Bounds = new BoundingBox(
                this.parentTree.Vertices[this.VertexTopLeft.Index].Position,
                this.parentTree.Vertices[this.VertexBottomRight.Index].Position);
            this.Bounds.Min.Y = -limY;
            this.Bounds.Max.Y = limY;


            //If node does not belong to the highest LOD
            if (this.size >= minSizeToContainChildren)
            {
                this.AddChildren();
            }

            // If this is the root node
            if (this.depth == 1)
            {
                /*
                 * Create the neighbors.
                 * This is essentially the bootstrap point for the whole tree.
                 */

                this.AddNeighbors();

                this.VertexTopLeft.ShouldBeRendered = true;
                this.VertexTopRight.ShouldBeRendered = true;
                this.VertexCenter.ShouldBeRendered = true;
                this.VertexBottomLeft.ShouldBeRendered = true;
                this.VertexBottomRight.ShouldBeRendered = true;

                this.VertexTop.ShouldBeRendered = true;
                this.VertexLeft.ShouldBeRendered = true;
                this.VertexRight.ShouldBeRendered = true;
                this.VertexBottom.ShouldBeRendered = true;
            }
        }

        public void EnforceMinimumDepth()
        {
            if (this.depth < this.parentTree.MinimumDepth)
            {
                if (this.hasChildren)
                {
                    this.isRendered = false;    // These are probably complimentary, as of isSplit == !isRendered. May change with introduction of frustum culling.
                    this.isSplit = true;

                    this.ChildTopLeft.EnforceMinimumDepth();
                    this.ChildTopRight.EnforceMinimumDepth();
                    this.ChildBottomLeft.EnforceMinimumDepth();
                    this.ChildBottomRight.EnforceMinimumDepth();
                }
                else
                {
                  
                    this.StartRendering();
                    this.isSplit = false;
                }
            }
            else
            {
                if (this.depth == this.parentTree.MinimumDepth)
                {

                    this.StartRendering();
                    this.isSplit = false;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("depth", string.Format("Node depth cannot exceed {0}. Culprit value: {1}", this.parentTree.MinimumDepth, this.depth));
                }

            }
        }

        internal void ActivateVertices()
        {
            if (this.isSplit && this.hasChildren)
            {
                this.ChildTopLeft.ActivateVertices();
                this.ChildTopRight.ActivateVertices();
                this.ChildBottomLeft.ActivateVertices();
                this.ChildBottomRight.ActivateVertices();
            }

            #region The top triangle (triangles)
            this.parentTree.UpdateBuffer(this.VertexCenter.Index);
            this.parentTree.UpdateBuffer(this.VertexTopLeft.Index);

            if (this.VertexTop.ShouldBeRendered)
            {
                // split and render an additional triangle
                this.parentTree.UpdateBuffer(this.VertexTop.Index);
                this.parentTree.UpdateBuffer(this.VertexCenter.Index);
                this.parentTree.UpdateBuffer(this.VertexTop.Index);
            }

            this.parentTree.UpdateBuffer(this.VertexTopRight.Index);
            #endregion

            #region The right triangle (triangles)
            this.parentTree.UpdateBuffer(this.VertexCenter.Index);
            this.parentTree.UpdateBuffer(this.VertexTopRight.Index);

            if (this.VertexRight.ShouldBeRendered)
            {
                // split and render an additional triangle
                this.parentTree.UpdateBuffer(this.VertexRight.Index);
                this.parentTree.UpdateBuffer(this.VertexCenter.Index);
                this.parentTree.UpdateBuffer(this.VertexRight.Index);
            }

            this.parentTree.UpdateBuffer(this.VertexBottomRight.Index);
            #endregion

            #region The bottom triangle (triangles)
            this.parentTree.UpdateBuffer(this.VertexCenter.Index);
            this.parentTree.UpdateBuffer(this.VertexBottomRight.Index);

            if (this.VertexBottom.ShouldBeRendered)
            {
                // split and render an additional triangle
                this.parentTree.UpdateBuffer(this.VertexBottom.Index);
                this.parentTree.UpdateBuffer(this.VertexCenter.Index);
                this.parentTree.UpdateBuffer(this.VertexBottom.Index);
            }

            this.parentTree.UpdateBuffer(this.VertexBottomLeft.Index);
            #endregion

            #region The left triangle (triangles)
            this.parentTree.UpdateBuffer(this.VertexCenter.Index);
            this.parentTree.UpdateBuffer(this.VertexBottomLeft.Index);

            if (this.VertexLeft.ShouldBeRendered)
            {
                // split and render an additional triangle
                this.parentTree.UpdateBuffer(this.VertexLeft.Index);
                this.parentTree.UpdateBuffer(this.VertexCenter.Index);
                this.parentTree.UpdateBuffer(this.VertexLeft.Index);
            }

            this.parentTree.UpdateBuffer(this.VertexTopLeft.Index);
            #endregion
        }

        // TODO: better method naming
        internal void StartRendering()
        {
            this.VertexTopLeft.ShouldBeRendered = true;
            this.VertexTopRight.ShouldBeRendered = true;
            this.VertexCenter.ShouldBeRendered = true;
            this.VertexBottomLeft.ShouldBeRendered = true;
            this.VertexBottomRight.ShouldBeRendered = true;
            this.isRendered = true;
        }

        /// <summary>
        /// Adds the relevant vertices to the quad node.
        /// </summary>
        private void AddVertices()
        {
            // The specific case vertices
            switch (this.Type)
            {
                case NodeType.TopLeft:
                    this.VertexTopLeft = parentNode.VertexTopLeft;
                    this.VertexTopRight = parentNode.VertexTop;
                    this.VertexBottomLeft = parentNode.VertexLeft;
                    this.VertexBottomRight = parentNode.VertexCenter;
                    break;
                case NodeType.TopRight:
                    this.VertexTopLeft = parentNode.VertexTop;
                    this.VertexTopRight = parentNode.VertexTopRight;
                    this.VertexBottomLeft = parentNode.VertexCenter;
                    this.VertexBottomRight = parentNode.VertexRight;
                    break;
                case NodeType.BottomLeft:
                    this.VertexTopLeft = parentNode.VertexLeft;
                    this.VertexTopRight = parentNode.VertexCenter;
                    this.VertexBottomLeft = parentNode.VertexBottomLeft;
                    this.VertexBottomRight = parentNode.VertexBottom;
                    break;
                case NodeType.BottomRight:
                    this.VertexTopLeft = parentNode.VertexCenter;
                    this.VertexTopRight = parentNode.VertexRight;
                    this.VertexBottomLeft = parentNode.VertexBottom;
                    this.VertexBottomRight = parentNode.VertexBottomRight;
                    break;
                case NodeType.FullNode:    // Really, if it's a full node.
                    this.VertexTopLeft = new QuadNodeVertex
                        {
                            ShouldBeRendered = true,
                            Index = 0
                        };
                    this.VertexTopRight = new QuadNodeVertex
                    {
                        ShouldBeRendered = true,
                        Index = this.VertexTopLeft.Index + this.size
                    };
                    this.VertexBottomLeft = new QuadNodeVertex
                    {
                        ShouldBeRendered = true,
                        Index = (this.size + 1) * parentTree.TopNodeSize
                    };
                    this.VertexBottomRight = new QuadNodeVertex
                    {
                        ShouldBeRendered = true,
                        Index = this.VertexBottomLeft.Index + this.size
                    };
                    break;
                default:
                    throw new InvalidOperationException("Unknown node type");
            }

            int halfSize = this.size / 2;
            // The vertices common for all node types
            this.VertexTop = new QuadNodeVertex
            {
                ShouldBeRendered = false,
                Index = this.VertexTopLeft.Index + halfSize
            };

            this.VertexLeft = new QuadNodeVertex
            {
                ShouldBeRendered = false,
                Index = this.VertexTopLeft.Index + (this.parentTree.TopNodeSize + 1) * (halfSize)
            };

            this.VertexCenter = new QuadNodeVertex
            {
                ShouldBeRendered = false,
                Index = this.VertexLeft.Index + halfSize
            };

            this.VertexRight = new QuadNodeVertex
            {
                ShouldBeRendered = false,
                Index = this.VertexLeft.Index + this.size
            };

            this.VertexBottom = new QuadNodeVertex
            {
                ShouldBeRendered = false,
                Index = this.VertexBottomLeft.Index + halfSize
            };


        }

        /// <summary>
        /// Adds the children quad nodes.
        /// </summary>
        private void AddChildren()
        {
            int halfSize = this.size / 2,
                nextDepth = this.depth + 1;
            this.ChildTopLeft = new QuadNode(NodeType.TopLeft, halfSize, nextDepth, this, this.parentTree, this.VertexTopLeft.Index);
            this.ChildTopRight = new QuadNode(NodeType.TopRight, halfSize, nextDepth, this, this.parentTree, this.VertexTop.Index);
            this.ChildBottomLeft = new QuadNode(NodeType.BottomLeft, halfSize, nextDepth, this, this.parentTree, this.VertexBottomLeft.Index);
            this.ChildBottomRight = new QuadNode(NodeType.BottomRight, halfSize, nextDepth, this, this.parentTree, this.VertexCenter.Index);
            this.hasChildren = true;
        }

        /// <summary>
        /// Adds the neighboring quad nodes.
        /// </summary>
        private void AddNeighbors()
        {
            switch (this.Type)
            {
                case NodeType.TopLeft:
                    #region The direct neighbors that DO NOT belong within the current node's parent:
                    // In case this is not the topmost quad node of the whole tree
                    if (this.parentNode.NeighborTop != null)
                    {
                        // The parent's top neighbor's next-level bottom-left quad node.
                        // This is effectively the adjacent quad node of the same size as this one.
                        this.NeighborTop = this.parentNode.NeighborTop.ChildBottomLeft;
                    }

                    // In case this is not the leftmost quad node of the whole tree
                    if (this.parentNode.NeighborLeft != null)
                    {
                        // The parent's left neighbor's next-level top-right quad node.
                        // This is effectively the adjacent quad node of the same size as this one.
                        this.NeighborLeft = this.parentNode.NeighborLeft.ChildTopRight;
                    }

                    #endregion
                    #region The direct neighbors within the same parent:
                    
                    this.NeighborRight = this.parentNode.ChildTopRight;
                    this.NeighborBottom = this.parentNode.ChildBottomLeft;

                    #endregion
                    break;
                case NodeType.TopRight:
                    #region The direct neighbors that DO NOT belong within the current node's parent:

                    if (this.parentNode.NeighborTop != null)
                    {
                        this.NeighborTop = this.parentNode.NeighborTop.ChildBottomRight;
                    }

                    if (this.parentNode.NeighborRight != null)
                    {
                        this.NeighborRight = this.parentNode.NeighborRight.ChildTopLeft;
                    }

                    #endregion
                    #region The direct neighbors within the same parent:
                    
                    this.NeighborLeft = this.parentNode.ChildTopLeft;
                    this.NeighborBottom = this.parentNode.ChildBottomRight;

                    #endregion
                    break;
                case NodeType.BottomLeft:
                    #region The direct neighbors that DO NOT belong within the current node's parent:

                    if (this.parentNode.NeighborBottom != null)
                    {
                        this.NeighborBottom = this.parentNode.NeighborBottom.ChildTopLeft;
                    }

                    if (this.parentNode.NeighborLeft != null)
                    {
                        this.NeighborLeft = this.parentNode.NeighborLeft.ChildBottomRight;
                    }

                    #endregion
                    #region The direct neighbors within the same parent:

                    this.NeighborRight = this.parentNode.ChildBottomRight;
                    this.NeighborTop = this.parentNode.ChildTopLeft;

                    #endregion
                    break;
                case NodeType.BottomRight:
                    #region The direct neighbors that DO NOT belong within the current node's parent:

                    if (this.parentNode.NeighborBottom != null)
                    {
                        this.NeighborBottom = this.parentNode.NeighborBottom.ChildTopRight;
                    }

                    if (this.parentNode.NeighborRight != null)
                    {
                        this.NeighborRight = this.parentNode.NeighborRight.ChildBottomLeft;
                    }

                    #endregion
                    #region The direct neighbors within the same parent:

                    this.NeighborLeft = this.parentNode.ChildBottomLeft;
                    this.NeighborTop = this.parentNode.ChildTopRight;

                    #endregion
                    break;

            }

            if (this.hasChildren)
            {
                // recursively add neighbors for all children.
                this.ChildTopLeft.AddNeighbors();
                this.ChildTopRight.AddNeighbors();
                this.ChildBottomLeft.AddNeighbors();
                this.ChildBottomRight.AddNeighbors();

            }

        }
    }
}
