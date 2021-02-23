// -----------------------------------------------------------------------
// <copyright file="DiagramShape.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.Core.Models
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;

    using Serilog;

    using VisioCleanup.Core.Models.Config;

    internal class DiagramShape
    {
        private readonly ILogger logger;

        private DiagramShape? above;

        private DiagramShape? below;

        private DiagramShape? left;

        private DiagramShape? right;

        /// <summary>
        ///     Initialises a new instance of the <see cref="DiagramShape" /> class.
        /// </summary>
        /// <param name="visioId">Visio shape ID.</param>
        public DiagramShape(int visioId)
        {
            this.logger = Log.ForContext<DiagramShape>();
            this.VisioId = visioId;
            this.Children = new Collection<DiagramShape>();
            this.Corners = new Corners
                               {
                                   Top = AppConfig!.Height, Left = 0, Right = AppConfig!.Width, Base = 0,
                               };
        }

        public static AppConfig? AppConfig { get; set; }

        /// <summary>
        ///     Gets the shape above.
        /// </summary>
        public DiagramShape? Above
        {
            get => this.above;
            private set
            {
                this.above = value;
                if (value == null)
                {
                    return;
                }

                if (value.Below == null)
                {
                    value.Below = this;
                }
                else if (!value.Below.Equals(this))
                {
                    value.Below = this;
                }
            }
        }

        public Collection<DiagramShape> Children { get; set; }

        /// <summary>
        ///     Gets or sets the corner structure.
        /// </summary>
        public Corners Corners { get; set; }

        /// <summary>
        ///     Gets the shape to the left.
        /// </summary>
        public DiagramShape? Left
        {
            get => this.left;
            private set
            {
                this.left = value;
                if (value == null)
                {
                    return;
                }

                if (value.Right == null)
                {
                    value.Right = this;
                }
                else if (!value.Right.Equals(this))
                {
                    value.Right = this;
                }
            }
        }

        public DiagramShape? ParentShape { get; set; }

        /// <summary>
        ///     Gets the shape text.
        /// </summary>
        public string? ShapeText { get; init; }

        /// <summary>
        ///     Gets or sets the shape type.
        /// </summary>
        public ShapeType ShapeType { get; set; }

        public int VisioId { get; set; }

        /// <summary>
        ///     Gets or sets the shape to the right.
        /// </summary>
        internal DiagramShape? Right
        {
            get => this.right;
            set
            {
                this.right = value;
                if (value == null)
                {
                    return;
                }

                if (value.Left == null)
                {
                    value.Left = this;
                }
                else if (!value.Left.Equals(this))
                {
                    value.Left = this;
                }
            }
        }

        /// <summary>
        ///     Gets or sets the shape below.
        /// </summary>
        private DiagramShape? Below
        {
            get => this.below;
            set
            {
                this.below = value;
                if (value == null)
                {
                    return;
                }

                if (value.Above == null)
                {
                    value.Above = this;
                }
                else if (!value.Above.Equals(this))
                {
                    value.Above = this;
                }
            }
        }

        /// <summary>
        ///     Add child shape to parent.
        /// </summary>
        /// <param name="childShape">New child shape of this shape.</param>
        public void AddChildShape(DiagramShape childShape)
        {
            if (!this.Children.Contains(childShape))
            {
                this.Children.Add(childShape);
            }

            // add to array
            childShape.ParentShape = this;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{this.VisioId}: {this.ShapeText}";
        }

        /// <summary>
        ///     Map all neighbour shapes within tolerance of 10.
        /// </summary>
        /// <exception cref="NotImplementedException">No idea what to do yet with this.</exception>
        internal void FindNeighbours()
        {
            if (!this.HasChildren())
            {
                return;
            }

            // reset all shapes.
            var children = this.Children;
            this.ClearNeighbours();

            const double Tolerance = 5000;

            var lines = children.OrderBy(shape => shape.Corners.Left).Select(shape => shape.Corners.Left);
            foreach (var line in lines.Distinct())
            {
                bool AbsoluteShapeSize(DiagramShape shape)
                {
                    var side = shape.Corners.Left - line;
                    return Math.Abs(side) < Tolerance;
                }

                var diagramShapes = children.Where(AbsoluteShapeSize);
                var bottomOrdered = diagramShapes.OrderBy(shape => shape.Corners.Base).ToList();
                DiagramShape? currentShape = null;

                foreach (var shape in bottomOrdered)
                {
                    switch (currentShape)
                    {
                        case not null when currentShape.Corners.Base.Equals(shape.Corners.Base):
                            // overlap!
                            throw new NotImplementedException("No idea what to do yet with this!");
                        case not null when currentShape.Corners.Base >= shape.Corners.Base:
                            continue;
                        case not null when (shape.Corners.Base - currentShape.Corners.Top) < (Tolerance * 2):
                            shape.Below = currentShape;

                            currentShape = shape;
                            break;
                        default:
                            currentShape = shape;
                            break;
                    }
                }
            }

            lines = children.OrderBy(shape => shape.Corners.Top).Select(shape => shape.Corners.Top);
            foreach (var line in lines.Distinct())
            {
                bool AbsoluteShapeSize(DiagramShape shape)
                {
                    var side = shape.Corners.Top - line;
                    return Math.Abs(side) < Tolerance;
                }

                var diagramShapes = children.Where(AbsoluteShapeSize);
                var bottomOrdered = diagramShapes.OrderBy(shape => shape.Corners.Left);
                DiagramShape? currentShape = null;

                foreach (var shape in bottomOrdered)
                {
                    switch (currentShape)
                    {
                        case not null when currentShape.Corners.Left.Equals(shape.Corners.Left):
                            // overlap!
                            throw new NotImplementedException("No idea what to do yet with this!");
                        case not null when currentShape.Corners.Left >= shape.Corners.Left:
                            continue;
                        case not null when (shape.Corners.Left - currentShape.Corners.Right) < (Tolerance * 2):
                            shape.Left = currentShape;

                            currentShape = shape;
                            break;
                        default:
                            currentShape = shape;
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Does this shape have children.
        /// </summary>
        /// <returns>true if at least one.</returns>
        internal bool HasChildren()
        {
            return this.Children.Count > 0;
        }

        /// <summary>
        ///     Remove all records of shape neighbours.
        /// </summary>
        private void ClearNeighbours()
        {
            // reset all shapes.
            foreach (var shape in this.Children)
            {
                shape.below = null;
                shape.above = null;
                shape.right = null;
                shape.left = null;
            }
        }
    }
}