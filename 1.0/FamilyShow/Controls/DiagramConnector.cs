/*
 * A connector consists of two nodes and a connection type. A connection has a
 * filtered state. The opacity is reduced when drawing a connection that is 
 * filtered. An animation is applied to the brush when the filtered state changes.
*/

using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.FamilyShowLib;
using System.Globalization;

namespace Microsoft.FamilyShow
{
    /// <summary>
    /// Type of connection.
    /// </summary>
    public enum ConnectorType
    {
        Child,
        Married,
        PreviousMarried
    }

    /// <summary>
    /// One of the nodes in a connection.
    /// </summary>
    public class DiagramConnectorNode
    {
        #region fields

        // Node location in the diagram.
        private DiagramRow row;
        private DiagramGroup group;
        private DiagramNode node;

        #endregion

        #region properties

        /// <summary>
        /// Node for this connection point.
        /// </summary>
        public DiagramNode Node
        {
            get { return node; }
        }

        /// <summary>
        /// Center of the node relative to the diagram.
        /// </summary>
        public Point Center
        {
            get { return GetPoint(this.node.Center); }
        }

        /// <summary>
        /// LeftCenter of the node relative to the diagram.
        /// </summary>
        public Point LeftCenter
        {
            get { return GetPoint(this.node.LeftCenter); }
        }

        /// <summary>
        /// RightCenter of the node relative to the diagram.
        /// </summary>
        public Point RightCenter
        {
            get { return GetPoint(this.node.RightCenter); }
        }

        /// <summary>
        /// TopCenter of the node relative to the diagram.
        /// </summary>
        public Point TopCenter
        {
            get { return GetPoint(this.node.TopCenter); }
        }

        /// <summary>
        /// TopRight of the node relative to the diagram.
        /// </summary>
        public Point TopRight
        {
            get { return GetPoint(this.node.TopRight); }
        }

        /// <summary>
        /// TopLeft of the node relative to the diagram.
        /// </summary>
        public Point TopLeft
        {
            get { return GetPoint(this.node.TopLeft); }
        }

        #endregion

        public DiagramConnectorNode(DiagramNode node, DiagramGroup group, DiagramRow row)
        {
            this.node = node;
            this.group = group;
            this.row = row;
        }

        /// <summary>
        /// Return the point shifted by the row and group location.
        /// </summary>
        private Point GetPoint(Point point)
        {
            point.Offset(
                this.row.Location.X + this.group.Location.X,
                this.row.Location.Y + this.group.Location.Y);

            return point;
        }
    }

    /// <summary>
    /// Connection that connects two diagram nodes.
    /// </summary>
    public class DiagramConnector
    {
        private static class Const
        {
            // Filtered settings.
            public static double OpacityFiltered = 0.15;
            public static double OpacityNormal = 1.0;
            public static double AnimationDuration = 300;
            
            // Pen for children connections.
            public static Color ChildPenColor = Color.FromArgb(0x80, 0x80, 0x80, 0x80);
            public static double ChildPenSize = 1;

            // Pen for married connections.
            public static Color MarriedPenColor = Color.FromRgb(0x90, 0xc0, 0x90);
            public static double MarriedPenSize = 2;
            
            // Connection text.
            public static Color TextColor = Colors.White;
            public static string TextFont = "Calibri";
            public static double TextSize = 12;
        }
    
        #region fields

        // The two nodes.
        private DiagramConnectorNode start;
        private DiagramConnectorNode end;

        // Type of connection.
        private ConnectorType type;

        // Flag, if the connection is currently filtered. The
        // connection is drawn in a dim-state when filtered.
        private bool isFiltered;

        // Animation if the filtered state has changed.
        private DoubleAnimation animation;

        #endregion

        /// <summary>
        /// Return true if this is a child connector.
        /// </summary>
        public bool IsChildConnector
        {
            get { return (type == ConnectorType.Child) ? true : false; }
        }

        /// <summary>
        /// Return true if the connection is currently filtered.
        /// </summary>
        private bool IsFiltered
        {
            set { isFiltered = value; }
            get { return isFiltered; }
        }
        
        /// <summary>
        /// Gets the married date for the connector. Can be null.
        /// </summary>
        public DateTime? MarriedDate
        {
            get
            {
                if (type == ConnectorType.Married)
                {
                    SpouseRelationship rel = start.Node.Person.GetSpouseRelationship(end.Node.Person);
                    if (rel != null)
                        return rel.MarriageDate;
                }
                return null;                    
            }
        }

        /// <summary>
        /// Get the previous married date for the connector. Can be null.
        /// </summary>
        public DateTime? PreviousMarriedDate
        {
            get
            {
                if (type == ConnectorType.PreviousMarried)
                {
                    SpouseRelationship rel = start.Node.Person.GetSpouseRelationship(end.Node.Person);
                    if (rel != null)
                        return rel.DivorceDate;
                }
                return null;
            }
        }

        /// <summary>
        /// Gets the pen for child connections.
        /// </summary>
        private Pen ChildPen
        {
            get { return GetPen(Const.ChildPenColor, Const.ChildPenSize, true); }
        }

        /// <summary>
        /// Gets the pen for married connections.
        /// </summary>
        private Pen MarriedPen
        {
            get { return GetPen(Const.MarriedPenColor, Const.MarriedPenSize, false); }
        }

        /// <summary>
        /// Gets the pen for previous married connections.
        /// </summary>
        private Pen PreviousMarriedPen
        {
            get { return GetPen(Const.MarriedPenColor, Const.MarriedPenSize, true); }
        }

        /// <summary>
        /// Gets the brush for connection text.
        /// </summary>
        private SolidColorBrush TextBrush
        {
            get { return GetBrush(Const.TextColor); }
        }

        public DiagramConnector(ConnectorType type,
            DiagramConnectorNode startConnector, DiagramConnectorNode endConnector)
        {
            this.type = type;
            this.start = startConnector;
            this.end = endConnector;
        }

        public DiagramConnector(ConnectorType type,
            DiagramNode startNode, DiagramGroup startGroup, DiagramRow startRow,
            DiagramNode endNode, DiagramGroup endGroup, DiagramRow endRow)
        {
            this.type = type;
            this.start = new DiagramConnectorNode(startNode, startGroup, startRow);
            this.end = new DiagramConnectorNode(endNode, endGroup, endRow);
        }

        #region filtered state / animation

        /// <summary>
        /// Return the current filtered state of the connection. This depends
        /// on the connection nodes, marriage date and previous marriage date.
        /// </summary>
        private bool GetNewFilteredState()
        {
            // Connection is filtered if any of the nodes are filtered.
            if (start.Node.IsFiltered || end.Node.IsFiltered)
                return true;

            // Check the married date.
            if (type == ConnectorType.Married)
            {
                SpouseRelationship rel = start.Node.Person.GetSpouseRelationship(end.Node.Person);
                if (rel != null && rel.MarriageDate != null &&
                    (start.Node.DisplayYear < rel.MarriageDate.Value.Year))
                {
                    return true;
                }
            }

            // Check the previous married date.
            if (type == ConnectorType.PreviousMarried)
            {
                SpouseRelationship rel = start.Node.Person.GetSpouseRelationship(end.Node.Person);
                if (rel != null && rel.DivorceDate != null &&
                    (start.Node.DisplayYear < rel.DivorceDate.Value.Year))
                {
                    return true;
                }
            }

            // Connection is not filtered.
            return false;
        }

        /// <summary>
        /// Determine if the filtered state has changed, and create
        /// the animation that is used to draw the connection.
        /// </summary>
        private void CheckIfFilteredChanged()
        {
            // See if the filtered state has changed.
            bool newFiltered = GetNewFilteredState();
            if (newFiltered != this.IsFiltered)
            {
                // Filtered state did change, create the animation.
                this.IsFiltered = newFiltered;
                animation = new DoubleAnimation();
                animation.From = isFiltered ? Const.OpacityNormal : Const.OpacityFiltered;
                animation.To = isFiltered ? Const.OpacityFiltered : Const.OpacityNormal;
                animation.Duration = App.GetAnimationDuration(Const.AnimationDuration);
            }
            else
            {
                // Filtered state did not change, clear the animation.
                animation = null;
            }
        }

        /// <summary>
        /// Create the specified pen. The opacity is set based on the 
        /// current filtered state. The pen contains an animation if
        /// the filtered state has changed.
        /// </summary>
        private Pen GetPen(Color color, double thickness, bool dash)
        {
            // Create the pen.
            Pen pen = new Pen(new SolidColorBrush(color), thickness);
            pen.DashStyle = dash ? DashStyles.Dash : DashStyles.Solid;

            // Set opacity based on the filtered state.
            pen.Brush.Opacity = (this.isFiltered) ? Const.OpacityFiltered : Const.OpacityNormal;

            // Create animation if the filtered state has changed.
            if (animation != null)
                pen.Brush.BeginAnimation(Brush.OpacityProperty, animation);

            return pen;
        }

        /// <summary>
        /// Create the specified brush. The opacity is set based on the 
        /// current filtered state. The brush contains an animation if 
        /// the filtered state has changed.
        /// </summary>
        private SolidColorBrush GetBrush(Color color)
        {
            // Create the brush.
            SolidColorBrush brush = new SolidColorBrush(color);

            // Set the opacity based on the filtered state.
            brush.Opacity = (this.isFiltered) ? Const.OpacityFiltered : Const.OpacityNormal;

            // Create animation if the filtered state has changed.
            if (animation != null)
                brush.BeginAnimation(Brush.OpacityProperty, animation);

            return brush;
        }

        #endregion

        #region drawing

        /// <summary>
        /// Draw the connection between the two nodes.
        /// </summary>
        public void Draw(DrawingContext drawingContext)
        {
            // Don't draw if either of the nodes are filtered.
            if (start.Node.Visibility != Visibility.Visible || end.Node.Visibility != Visibility.Visible)
                return;

            // First check if the filtered state has changed, an animation
            // if created if the state has changed which is used for all 
            // connection drawing.
            CheckIfFilteredChanged();

            // Next draw the connection.            
            switch (this.type)
            {
                case ConnectorType.Child:
                    DrawChild(drawingContext);
                    break;

                case ConnectorType.Married:
                    DrawMarried(drawingContext, true);
                    break;

                case ConnectorType.PreviousMarried:
                    DrawMarried(drawingContext, false);
                    break;
            }
        }

        /// <summary>
        /// Draw child connector between nodes.
        /// </summary>
        private void DrawChild(DrawingContext drawingContext)
        {
            drawingContext.DrawLine(this.ChildPen, start.Center, end.Center);
        }

        /// <summary>
        /// Draw married or previous married connector between nodes.
        /// </summary>
        private void DrawMarried(DrawingContext drawingContext, bool married)
        {
            const double TextSpace = 3;

            // Determine the start and ending points based on what node is on the left / right.
            Point startPoint = (start.TopCenter.X < end.TopCenter.X) ? start.TopCenter : end.TopCenter;
            Point endPoint = (start.TopCenter.X < end.TopCenter.X) ? end.TopCenter : start.TopCenter;

            // Use a higher arc when the nodes are further apart.
            double arcHeight = (endPoint.X - startPoint.X) / 4;
            Point middlePoint = new Point(startPoint.X + ((endPoint.X - startPoint.X) / 2), startPoint.Y - arcHeight);

            // Draw the arc, get the bounds so can draw connection text.
            Rect bounds = DrawArc(drawingContext, married ? this.MarriedPen : this.PreviousMarriedPen,
                startPoint, middlePoint, endPoint);

            // Get the relationship info so the dates can be displayed.
            SpouseRelationship rel = start.Node.Person.GetSpouseRelationship(end.Node.Person);
            if (rel != null)
            {
                // Marriage date.
                if (rel.MarriageDate != null)
                {
                    string text = rel.MarriageDate.Value.Year.ToString(CultureInfo.CurrentCulture);

                    FormattedText format = new FormattedText(text,
                        System.Globalization.CultureInfo.CurrentUICulture,
                        FlowDirection.LeftToRight, new Typeface(Const.TextFont), 
                        Const.TextSize, this.TextBrush);

                    drawingContext.DrawText(format, new Point(
                        bounds.Left + ((bounds.Width / 2) - (format.Width / 2)),
                        bounds.Top - format.Height - TextSpace));
                }
                
                // Previous marriage date.
                if (!married && rel.DivorceDate != null)
                {
                    string text = rel.DivorceDate.Value.Year.ToString(CultureInfo.CurrentCulture);

                    FormattedText format = new FormattedText(text,
                        System.Globalization.CultureInfo.CurrentUICulture,
                        FlowDirection.LeftToRight, new Typeface(Const.TextFont), 
                        Const.TextSize, this.TextBrush);

                    drawingContext.DrawText(format, new Point(
                        bounds.Left + ((bounds.Width / 2) - (format.Width / 2)),
                        bounds.Top + TextSpace));
                }
            }
        }

        /// <summary>
        /// Draw an arc connecting the two nodes.
        /// </summary>
        private static Rect DrawArc(DrawingContext drawingContext, Pen pen,
            Point startPoint, Point middlePoint, Point endPoint)
        {
            PathGeometry geometry = new PathGeometry();
            PathFigure figure = new PathFigure();
            figure.StartPoint = startPoint;
            figure.Segments.Add(new QuadraticBezierSegment(middlePoint, endPoint, true));
            geometry.Figures.Add(figure);
            drawingContext.DrawGeometry(null, pen, geometry);
            return geometry.Bounds;
        }

        #endregion

    }
}
