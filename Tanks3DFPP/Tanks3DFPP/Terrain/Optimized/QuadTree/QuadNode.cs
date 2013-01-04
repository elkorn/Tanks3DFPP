using System;
using Microsoft.Xna.Framework;

namespace Tanks3DFPP.Terrain.Optimization.QuadTree
{
    public class QuadNode
    {
        /// <summary>
        ///     The absolute value of height constituting the bounding volume of the current node.
        /// </summary>
        private const float limY = 950f;

        /// <summary>
        ///     The minimum size a quad node has to be to contain children.
        ///     A node smaller than that is actually on the highest LOD.
        ///     Setting this to a higher number will result in a terrain with a lower maximum level of detail.
        /// </summary>
        private const int minSizeToContainChildren = 4;

        public readonly BoundingBox Bounds;
        private readonly int depth;

        /// <summary>
        ///     The parent node - ???
        /// </summary>
        private readonly QuadNode parentNode;

        /// <summary>
        ///     The parent tree - lower LOD.
        /// </summary>
        private readonly Tanks3DFPP.Terrain.QuadTree parentTree;

        private readonly int size;
        public NodeType Type;

        private bool hasChildren;
        private bool isActive;
        private bool isSplit;
        private int positionIndex;

        /// <summary>
        ///     Initializes a new instance of the <see cref="QuadNode" /> class.
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
            Tanks3DFPP.Terrain.QuadTree parentTree,
            int positionIndex)
        {
            Type = type;
            this.size = size;
            this.depth = depth;

            parentNode = parent;
            this.parentTree = parentTree;

            this.positionIndex = positionIndex;


            AddVertices();

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
            Bounds = new BoundingBox(
                this.parentTree.Vertices[VertexTopLeft.Index].Position,
                this.parentTree.Vertices[VertexBottomRight.Index].Position)
                {
                    Min = { Y = -limY },
                    Max = { Y = limY }
                };

            //If node does not belong to the highest LOD
            if (this.size >= minSizeToContainChildren)
            {
                AddChildren();
            }

            // If this is the root node
            if (this.depth == 1)
            {
                /*
                 * Create the neighbors.
                 * This is essentially the bootstrap point for the whole tree.
                 */

                AddNeighbors();

                VertexTopLeft.Activated = true;
                VertexTopRight.Activated = true;
                VertexCenter.Activated = true;
                VertexBottomLeft.Activated = true;
                VertexBottomRight.Activated = true;

                VertexTop.Activated = true;
                VertexLeft.Activated = true;
                VertexRight.Activated = true;
                VertexBottom.Activated = true;
            }
        }

        public int Depth
        {
            get { return depth; }
        }

        public bool IsActive
        {
            get { return isActive; }
            internal set
            {
                Game1.SetNodeAsRendered(this, value);
                isActive = value;
            }
        }

        public bool IsSplit
        {
            get { return isSplit; }
        }

        public bool CanBeSplit
        {
            get { return size >= 2; }
        }

        /// <summary>
        ///     The parent node - ???
        /// </summary>
        public QuadNode Parent
        {
            get { return parentNode; }
        }

        #region VERTICES

        public QuadNodeVertex VertexBottom;
        public QuadNodeVertex VertexBottomLeft;
        public QuadNodeVertex VertexBottomRight;
        public QuadNodeVertex VertexCenter;
        public QuadNodeVertex VertexLeft;
        public QuadNodeVertex VertexRight;
        public QuadNodeVertex VertexTop;
        public QuadNodeVertex VertexTopLeft;
        public QuadNodeVertex VertexTopRight;

        #endregion

        #region CHILDREN

        public QuadNode ChildBottomLeft;
        public QuadNode ChildBottomRight;
        public QuadNode ChildTopLeft;
        public QuadNode ChildTopRight;

        #endregion

        #region NEIGHBORS

        public QuadNode NeighborBottom;
        public QuadNode NeighborLeft;
        public QuadNode NeighborRight;
        public QuadNode NeighborTop;

        #endregion

        internal void Activate()
        {
            VertexTopLeft.Activated = true;
            VertexTopRight.Activated = true;
            VertexCenter.Activated = true;
            VertexBottomLeft.Activated = true;
            VertexBottomRight.Activated = true;
            IsActive = true;
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

        /// <summary>
        ///     Adds the children quad nodes.
        /// </summary>
        private void AddChildren()
        {
            int halfSize = size / 2,
                nextDepth = depth + 1;
            ChildTopLeft = new QuadNode(NodeType.TopLeft, halfSize, nextDepth, this, parentTree, VertexTopLeft.Index);
            ChildTopRight = new QuadNode(NodeType.TopRight, halfSize, nextDepth, this, parentTree, VertexTop.Index);
            ChildBottomLeft = new QuadNode(NodeType.BottomLeft, halfSize, nextDepth, this, parentTree,
                                           VertexBottomLeft.Index);
            ChildBottomRight = new QuadNode(NodeType.BottomRight, halfSize, nextDepth, this, parentTree,
                                            VertexCenter.Index);
            hasChildren = true;
        }

        /// <summary>
        ///     Adds the neighboring quad nodes.
        /// </summary>
        private void AddNeighbors()
        {
            switch (Type)
            {
                case NodeType.TopLeft:

                    #region The direct neighbors that DO NOT belong within the current node's parent:

                    // In case this is not the topmost quad node of the whole tree
                    if (Parent.NeighborTop != null)
                    {
                        // The parent's top neighbor's next-level bottom-left quad node.
                        // This is effectively the adjacent quad node of the same size as this one.
                        NeighborTop = Parent.NeighborTop.ChildBottomLeft;
                    }

                    // In case this is not the leftmost quad node of the whole tree
                    if (Parent.NeighborLeft != null)
                    {
                        // The parent's left neighbor's next-level top-right quad node.
                        // This is effectively the adjacent quad node of the same size as this one.
                        NeighborLeft = Parent.NeighborLeft.ChildTopRight;
                    }

                    #endregion

                    #region The direct neighbors within the same parent:

                    NeighborRight = Parent.ChildTopRight;
                    NeighborBottom = Parent.ChildBottomLeft;

                    #endregion

                    break;
                case NodeType.TopRight:

                    #region The direct neighbors that DO NOT belong within the current node's parent:

                    if (Parent.NeighborTop != null)
                    {
                        NeighborTop = Parent.NeighborTop.ChildBottomRight;
                    }

                    if (Parent.NeighborRight != null)
                    {
                        NeighborRight = Parent.NeighborRight.ChildTopLeft;
                    }

                    #endregion

                    #region The direct neighbors within the same parent:

                    NeighborLeft = Parent.ChildTopLeft;
                    NeighborBottom = Parent.ChildBottomRight;

                    #endregion

                    break;
                case NodeType.BottomLeft:

                    #region The direct neighbors that DO NOT belong within the current node's parent:

                    if (Parent.NeighborBottom != null)
                    {
                        NeighborBottom = Parent.NeighborBottom.ChildTopLeft;
                    }

                    if (Parent.NeighborLeft != null)
                    {
                        NeighborLeft = Parent.NeighborLeft.ChildBottomRight;
                    }

                    #endregion

                    #region The direct neighbors within the same parent:

                    NeighborRight = Parent.ChildBottomRight;
                    NeighborTop = Parent.ChildTopLeft;

                    #endregion

                    break;
                case NodeType.BottomRight:

                    #region The direct neighbors that DO NOT belong within the current node's parent:

                    if (Parent.NeighborBottom != null)
                    {
                        NeighborBottom = Parent.NeighborBottom.ChildTopRight;
                    }

                    if (Parent.NeighborRight != null)
                    {
                        NeighborRight = Parent.NeighborRight.ChildBottomLeft;
                    }

                    #endregion

                    #region The direct neighbors within the same parent:

                    NeighborLeft = Parent.ChildBottomLeft;
                    NeighborTop = Parent.ChildTopRight;

                    #endregion

                    break;
            }

            if (hasChildren)
            {
                // recursively add neighbors for all children.
                ChildTopLeft.AddNeighbors();
                ChildTopRight.AddNeighbors();
                ChildBottomLeft.AddNeighbors();
                ChildBottomRight.AddNeighbors();
            }
        }

        /// <summary>
        ///     Adds the relevant vertices to the quad node.
        /// </summary>
        private void AddVertices()
        {
            // The specific case vertices
            switch (Type)
            {
                case NodeType.TopLeft:
                    VertexTopLeft = Parent.VertexTopLeft;
                    VertexTopRight = Parent.VertexTop;
                    VertexBottomLeft = Parent.VertexLeft;
                    VertexBottomRight = Parent.VertexCenter;
                    break;
                case NodeType.TopRight:
                    VertexTopLeft = Parent.VertexTop;
                    VertexTopRight = Parent.VertexTopRight;
                    VertexBottomLeft = Parent.VertexCenter;
                    VertexBottomRight = Parent.VertexRight;
                    break;
                case NodeType.BottomLeft:
                    VertexTopLeft = Parent.VertexLeft;
                    VertexTopRight = Parent.VertexCenter;
                    VertexBottomLeft = Parent.VertexBottomLeft;
                    VertexBottomRight = Parent.VertexBottom;
                    break;
                case NodeType.BottomRight:
                    VertexTopLeft = Parent.VertexCenter;
                    VertexTopRight = Parent.VertexRight;
                    VertexBottomLeft = Parent.VertexBottom;
                    VertexBottomRight = Parent.VertexBottomRight;
                    break;
                case NodeType.FullNode: // Really, if it's a full node.
                    VertexTopLeft = new QuadNodeVertex
                        {
                            Activated = true,
                            Index = 0
                        };
                    VertexTopRight = new QuadNodeVertex
                        {
                            Activated = true,
                            Index = VertexTopLeft.Index + size
                        };
                    VertexBottomLeft = new QuadNodeVertex
                        {
                            Activated = true,
                            Index = (size + 1) * parentTree.TopNodeSize
                        };
                    VertexBottomRight = new QuadNodeVertex
                        {
                            Activated = true,
                            Index = VertexBottomLeft.Index + size
                        };
                    break;
                default:
                    throw new InvalidOperationException("Unknown node type");
            }

            int halfSize = size / 2;
            // The vertices common for all node types
            VertexTop = new QuadNodeVertex
                {
                    Activated = false,
                    Index = VertexTopLeft.Index + halfSize
                };

            VertexLeft = new QuadNodeVertex
                {
                    Activated = false,
                    Index = VertexTopLeft.Index + (parentTree.TopNodeSize + 1) * (halfSize)
                };

            VertexCenter = new QuadNodeVertex
                {
                    Activated = false,
                    Index = VertexLeft.Index + halfSize
                };

            VertexRight = new QuadNodeVertex
                {
                    Activated = false,
                    Index = VertexLeft.Index + size
                };

            VertexBottom = new QuadNodeVertex
                {
                    Activated = false,
                    Index = VertexBottomLeft.Index + halfSize
                };
        }

        public bool Contains(Vector3 point)
        {
            point.Y = 0;
            return Bounds.Contains(point) == ContainmentType.Contains;
        }

        public QuadNode DeepestNodeContainingPoint(Vector3 point)
        {
            if (!Contains(point))
                return null;

            if (hasChildren)
            {
                if (ChildTopLeft.Contains((point)))
                    return ChildTopLeft.DeepestNodeContainingPoint(point);

                if (ChildTopRight.Contains((point)))
                    return ChildTopRight.DeepestNodeContainingPoint(point);

                if (ChildBottomLeft.Contains((point)))
                    return ChildBottomLeft.DeepestNodeContainingPoint(point);

                return ChildBottomRight.DeepestNodeContainingPoint(point);
            }

            return this;
        }

        public void EnforceMinimumDepth()
        {
            if (depth < parentTree.MinimumDepth)
            {
                if (hasChildren)
                {
                    isActive = false;
                    isSplit = true;

                    ChildTopLeft.EnforceMinimumDepth();
                    ChildTopRight.EnforceMinimumDepth();
                    ChildBottomLeft.EnforceMinimumDepth();
                    ChildBottomRight.EnforceMinimumDepth();
                }
                else
                {
                    Activate();
                    isSplit = false;
                }

                return;
            }

            if (depth == parentTree.MinimumDepth)
            {
                Activate();
                isSplit = false;
            }
        }

        private static void EnsureNeighborParentSplit(QuadNode neighbor)
        {
            if (neighbor != null && neighbor.Parent != null && !neighbor.Parent.IsSplit)
            {
                neighbor.Parent.Split();
            }
        }

        public void Merge()
        {
            VertexTop.Activated = false;
            VertexLeft.Activated = false;
            VertexRight.Activated = false;
            VertexBottom.Activated = false;

            if (Type != NodeType.FullNode)
            {
                VertexTopLeft.Activated = false;
                VertexTopRight.Activated = false;
                VertexBottomLeft.Activated = false;
                VertexBottomRight.Activated = false;
            }
            IsActive = true;
            isSplit = false;

            if (hasChildren)
            {
                if (ChildTopLeft.IsSplit)
                {
                    ChildTopLeft.Merge();
                    ChildTopLeft.IsActive = false;
                }
                else
                {
                    ChildTopLeft.VertexTop.Activated = false;
                    ChildTopLeft.VertexLeft.Activated = false;
                    ChildTopLeft.VertexRight.Activated = false;
                    ChildTopLeft.VertexBottom.Activated = false;
                }

                if (ChildTopRight.IsSplit)
                {
                    ChildTopRight.Merge();
                    ChildTopRight.IsActive = false;
                }
                else
                {
                    ChildTopRight.VertexTop.Activated = false;
                    ChildTopRight.VertexLeft.Activated = false;
                    ChildTopRight.VertexRight.Activated = false;
                    ChildTopRight.VertexBottom.Activated = false;
                }


                if (ChildBottomLeft.IsSplit)
                {
                    ChildBottomLeft.Merge();
                    ChildBottomLeft.IsActive = false;
                }
                else
                {
                    ChildBottomLeft.VertexTop.Activated = false;
                    ChildBottomLeft.VertexLeft.Activated = false;
                    ChildBottomLeft.VertexRight.Activated = false;
                    ChildBottomLeft.VertexBottom.Activated = false;
                }


                if (ChildBottomRight.IsSplit)
                {
                    ChildBottomRight.Merge();
                    ChildBottomRight.IsActive = false;
                }
                else
                {
                    ChildBottomRight.VertexTop.Activated = false;
                    ChildBottomRight.VertexLeft.Activated = false;
                    ChildBottomRight.VertexRight.Activated = false;
                    ChildBottomRight.VertexBottom.Activated = false;
                }
            }
        }

        internal void SetActiveVertices()
        {
            if (IsSplit && hasChildren)
            {
                ChildTopLeft.SetActiveVertices();
                ChildTopRight.SetActiveVertices();
                ChildBottomLeft.SetActiveVertices();
                ChildBottomRight.SetActiveVertices();
                return;
            }

            //Top Triangle(s)
            parentTree.UpdateBuffer(VertexCenter.Index);
            parentTree.UpdateBuffer(VertexTopLeft.Index);

            if (VertexTop.Activated)
            {
                parentTree.UpdateBuffer(VertexTop.Index);

                parentTree.UpdateBuffer(VertexCenter.Index);
                parentTree.UpdateBuffer(VertexTop.Index);
            }
            parentTree.UpdateBuffer(VertexTopRight.Index);

            //Right Triangle(s)
            parentTree.UpdateBuffer(VertexCenter.Index);
            parentTree.UpdateBuffer(VertexTopRight.Index);

            if (VertexRight.Activated)
            {
                parentTree.UpdateBuffer(VertexRight.Index);

                parentTree.UpdateBuffer(VertexCenter.Index);
                parentTree.UpdateBuffer(VertexRight.Index);
            }
            parentTree.UpdateBuffer(VertexBottomRight.Index);

            //Bottom Triangle(s)
            parentTree.UpdateBuffer(VertexCenter.Index);
            parentTree.UpdateBuffer(VertexBottomRight.Index);

            if (VertexBottom.Activated)
            {
                parentTree.UpdateBuffer(VertexBottom.Index);

                parentTree.UpdateBuffer(VertexCenter.Index);
                parentTree.UpdateBuffer(VertexBottom.Index);
            }
            parentTree.UpdateBuffer(VertexBottomLeft.Index);

            //Left Triangle(s)
            parentTree.UpdateBuffer(VertexCenter.Index);
            parentTree.UpdateBuffer(VertexBottomLeft.Index);

            if (VertexLeft.Activated)
            {
                parentTree.UpdateBuffer(VertexLeft.Index);

                parentTree.UpdateBuffer(VertexCenter.Index);
                parentTree.UpdateBuffer(VertexLeft.Index);
            }
            parentTree.UpdateBuffer(VertexTopLeft.Index);
        }

        public void Split()
        {
            if (Parent != null && !Parent.IsSplit)
            {
                Parent.Split();
            }

            if (CanBeSplit)
            {
                if (hasChildren)
                {
                    ChildTopLeft.Activate();
                    ChildTopRight.Activate();
                    ChildBottomLeft.Activate();
                    ChildBottomRight.Activate();
                    IsActive = false;
                }
                else
                {
                    IsActive = true;
                }
            }

            isSplit = true;
            VertexTopLeft.Activated = true;
            VertexTopRight.Activated = true;
            VertexBottomLeft.Activated = true;
            VertexBottomRight.Activated = true;

            EnsureNeighborParentSplit(NeighborTop);
            EnsureNeighborParentSplit(NeighborBottom);
            EnsureNeighborParentSplit(NeighborLeft);
            EnsureNeighborParentSplit(NeighborRight);


            if (NeighborTop != null)
                NeighborTop.VertexBottom.Activated = true;

            if (NeighborRight != null)
                NeighborRight.VertexLeft.Activated = true;

            if (NeighborBottom != null)
                NeighborBottom.VertexTop.Activated = true;

            if (NeighborLeft != null)
                NeighborLeft.VertexRight.Activated = true;
        }
    }
}