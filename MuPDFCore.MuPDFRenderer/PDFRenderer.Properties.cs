/*
    MuPDFCore.MuPDFRenderer - A control to display documents in Avalonia using MuPDFCore.
    Copyright (C) 2020  Giorgio Bianchini

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as
    published by the Free Software Foundation, version 3.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>
*/

using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using MuPDFCore.StructuredText;
using System;
using System.Collections.Generic;

namespace MuPDFCore.MuPDFRenderer
{
    public partial class PDFRenderer : Control
    {
        /// <summary>
        /// Defines the <see cref="RenderThreadCount"/> property.
        /// </summary>
        public static readonly DirectProperty<PDFRenderer, int> RenderThreadCountProperty = AvaloniaProperty.RegisterDirect<PDFRenderer, int>(nameof(RenderThreadCount), o => o.RenderThreadCount);
        /// <summary>
        /// Backing field for the <see cref="RenderThreadCount"/> property.
        /// </summary>
        private int _RenderThreadCount;
        /// <summary>
        /// Exposes the number of threads that the current instance is using to render the document. Read-only.
        /// </summary>
        public int RenderThreadCount
        {
            get
            {
                return _RenderThreadCount;
            }

            private set
            {
                SetAndRaise(RenderThreadCountProperty, ref _RenderThreadCount, value);
            }
        }

        /// <summary>
        /// Defines the <see cref="PageNumber"/> property.
        /// </summary>
        public static readonly DirectProperty<PDFRenderer, int> PageNumberProperty = AvaloniaProperty.RegisterDirect<PDFRenderer, int>(nameof(PageNumber), o => o.PageNumber);
        /// <summary>
        /// Backing field for the <see cref="PageNumber"/> property.
        /// </summary>
        private int _PageNumber;
        /// <summary>
        /// Exposes the number of the page that the current instance is rendering. Read-only.
        /// </summary>
        public int PageNumber
        {
            get
            {
                return _PageNumber;
            }

            private set
            {
                SetAndRaise(PageNumberProperty, ref _PageNumber, value);
            }
        }

        /// <summary>
        /// Defines the <see cref="IsViewerInitialized"/> property.
        /// </summary>
        public static readonly DirectProperty<PDFRenderer, bool> IsViewerInitializedProperty = AvaloniaProperty.RegisterDirect<PDFRenderer, bool>(nameof(IsViewerInitialized), o => o.IsViewerInitialized);
        /// <summary>
        /// Backing field for the <see cref="IsViewerInitialized"/> property.
        /// </summary>
        private bool _IsViewerInitialized = false;
        /// <summary>
        /// Whether the current instance has been initialised with a document to render or not. Read-only.
        /// </summary>
        public bool IsViewerInitialized
        {
            get
            {
                return _IsViewerInitialized;
            }

            private set
            {
                SetAndRaise(IsViewerInitializedProperty, ref _IsViewerInitialized, value);
            }
        }

        /// <summary>
        /// Defines the <see cref="PageSize"/> property.
        /// </summary>
        public static readonly DirectProperty<PDFRenderer, Rect> PageSizeProperty = AvaloniaProperty.RegisterDirect<PDFRenderer, Rect>(nameof(PageSize), o => o.PageSize);
        /// <summary>
        /// Backing field for the <see cref="PageSize"/> property.
        /// </summary>
        private Rect _PageSize;
        /// <summary>
        /// Exposes the size of the page that is drawn by the current instance (in page units).
        /// </summary>
        public Rect PageSize
        {
            get
            {
                return _PageSize;
            }

            private set
            {
                SetAndRaise(PageSizeProperty, ref _PageSize, value);
            }
        }

        /// <summary>
        /// Defines the <see cref="DisplayArea"/> property.
        /// </summary>
        public static readonly StyledProperty<Rect> DisplayAreaProperty = AvaloniaProperty.Register<PDFRenderer, Rect>(nameof(DisplayArea));
        /// <summary>
        /// The region of the page (in page units) that is currently displayed by the current instance. This always has the same aspect ratio of the bounds of this control.
        /// When this is set, the value is sanitised so that the smallest rectangle with the correct aspect ratio containing the requested value is chosen.
        /// </summary>
        public Rect DisplayArea
        {
            get
            {
                return GetValue(DisplayAreaProperty);
            }

            set
            {
                double widthRatio = value.Width / (this.Bounds.Width);
                double heightRatio = value.Height / (this.Bounds.Height);

                double containingWidth = Math.Max(widthRatio, heightRatio) * this.Bounds.Width;
                double containingHeight = Math.Max(widthRatio, heightRatio) * this.Bounds.Height;

                double deltaW = (containingWidth - value.Width) * 0.5;
                double deltaH = (containingHeight - value.Height) * 0.5;

                Rect newDispArea = new Rect(new Point(value.X - deltaW, value.Y - deltaH), new Point(value.Right + deltaW, value.Bottom + deltaH));

                SetValue(DisplayAreaProperty, newDispArea);
            }
        }

        /// <summary>
        /// Defines the <see cref="ZoomIncrement"/> property.
        /// </summary>
        public static readonly StyledProperty<double> ZoomIncrementProperty = AvaloniaProperty.Register<PDFRenderer, double>(nameof(ZoomIncrement), Math.Pow(2, 1.0 / 3.0), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);
        /// <summary>
        /// Determines by how much the scale will be increased/decreased by the <see cref="ZoomStep(double, Point?)"/> method. Set this to a value smaller than 1 to invert the zoom in/out direction.
        /// </summary>
        public double ZoomIncrement
        {
            get { return GetValue(ZoomIncrementProperty); }

            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(ZoomIncrement), value, "The ZoomIncrement must be greater than 0!");
                }

                SetValue(ZoomIncrementProperty, value);
            }
        }

        /// <summary>
        /// Defines the <see cref="Background"/> property.
        /// </summary>
        public static readonly StyledProperty<IBrush> BackgroundProperty = AvaloniaProperty.Register<PDFRenderer, IBrush>(nameof(Background));
        /// <summary>
        /// The background colour of the control.
        /// </summary>
        public IBrush Background
        {
            get { return GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        /// <summary>
        /// Defines the <see cref="PageBackground"/> property.
        /// </summary>
        public static readonly StyledProperty<IBrush> PageBackgroundProperty = AvaloniaProperty.Register<PDFRenderer, IBrush>(nameof(PageBackground));
        /// <summary>
        /// The background colour to use for the page drawn by the control.
        /// </summary>
        public IBrush PageBackground
        {
            get { return GetValue(PageBackgroundProperty); }
            set { SetValue(PageBackgroundProperty, value); }
        }

        /// <summary>
        /// Defines the <see cref="Zoom"/> property.
        /// </summary>
        public static readonly DirectProperty<PDFRenderer, double> ZoomProperty = AvaloniaProperty.RegisterDirect<PDFRenderer, double>(nameof(Zoom), o => o.Zoom, (o, v) => o.Zoom = v, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);
        /// <summary>
        /// Backing field for the <see cref="Zoom"/> property.
        /// </summary>
        private double _Zoom;
        /// <summary>
        /// The current zoom level. Setting this will change the <see cref="DisplayArea"/> appropriately, zooming around the center of the <see cref="DisplayArea"/>.
        /// </summary>
        public double Zoom
        {
            get
            {
                return _Zoom;
            }

            set
            {                
                double actualZoom = value / _Zoom;

                double currZoomX = FixedArea.Width / DisplayArea.Width * actualZoom;
                double currZoomY = FixedArea.Height / DisplayArea.Height * actualZoom;

                double currWidth = FixedArea.Width / currZoomX;
                double currHeight = FixedArea.Height / currZoomY;


                Point pos = new Point(this.Bounds.Width * 0.5, this.Bounds.Height * 0.5);

                double deltaW = currWidth - DisplayArea.Width;
                double deltaH = currHeight - DisplayArea.Height;

                SetValue(DisplayAreaProperty, new Rect(new Point(DisplayArea.X - deltaW * pos.X / this.Bounds.Width, DisplayArea.Y - deltaH * pos.Y / this.Bounds.Height), new Point(DisplayArea.Right + deltaW * (1 - pos.X / this.Bounds.Width), DisplayArea.Bottom + deltaH * (1 - pos.Y / this.Bounds.Height))));
            }
        }

        /// <summary>
        /// Identifies the action to perform on pointer events.
        /// </summary>
        public enum PointerEventHandlers
        {
            /// <summary>
            /// Pointer events will be used to pan around the page.
            /// </summary>
            Pan,

            /// <summary>
            /// Pointer events will be used to highlight text.
            /// </summary>
            Highlight,

            /// <summary>
            /// Pointer events will be used to pan around the page or to highlight text, depending on where they start.
            /// </summary>
            PanHighlight,

            /// <summary>
            /// Pointer events will be ignored. If you use this value, you will have to implement your own way to pan around the document by changing the <see cref="DisplayArea"/> or to select text.
            /// </summary>
            Custom
        }

        /// <summary>
        /// Defines the <see cref="PointerEventHandlersType"/> property.
        /// </summary>
        public static readonly StyledProperty<PointerEventHandlers> PointerEventHandlerTypeProperty = AvaloniaProperty.Register<PDFRenderer, PointerEventHandlers>(nameof(PointerEventHandlersType), PointerEventHandlers.PanHighlight);
        /// <summary>
        /// Whether the default handlers for pointer events (which are used for panning around the page) should be enabled. If this is false, you will have to implement your own way to pan around the document by changing the <see cref="DisplayArea"/>.
        /// </summary>
        public PointerEventHandlers PointerEventHandlersType
        {
            get { return GetValue(PointerEventHandlerTypeProperty); }
            set { SetValue(PointerEventHandlerTypeProperty, value); }
        }

        /// <summary>
        /// Defines the <see cref="ZoomEnabled"/> property.
        /// </summary>
        public static readonly StyledProperty<bool> ZoomEnabledProperty = AvaloniaProperty.Register<PDFRenderer, bool>(nameof(ZoomEnabled), true);
        /// <summary>
        /// Whether the default handlers for pointer wheel events (which are used for zooming in/out) should be enabled. If this is false, you will have to implement your own way to zoom by changing the <see cref="DisplayArea"/>.
        /// </summary>
        public bool ZoomEnabled
        {
            get { return GetValue(ZoomEnabledProperty); }
            set { SetValue(ZoomEnabledProperty, value); }
        }

        /// <summary>
        /// Defines the <see cref="Selection"/> property.
        /// </summary>
        public static readonly StyledProperty<MuPDFStructuredTextAddressSpan> SelectionProperty = AvaloniaProperty.Register<PDFRenderer, MuPDFStructuredTextAddressSpan>(nameof(Selection), null);
        /// <summary>
        /// The start and end of the currently selected text.
        /// </summary>
        public MuPDFStructuredTextAddressSpan Selection
        {
            get { return GetValue(SelectionProperty); }
            set { SetValue(SelectionProperty, value); }
        }

        /// <summary>
        /// Defines the <see cref="SelectionBrush"/> property.
        /// </summary>
        public static readonly StyledProperty<IBrush> SelectionBrushProperty = AvaloniaProperty.Register<PDFRenderer, IBrush>(nameof(SelectionBrush), new SolidColorBrush(Color.FromArgb(96, 86, 180, 233)));
        /// <summary>
        /// The colour used to highlight the <see cref="Selection"/>.
        /// </summary>
        public IBrush SelectionBrush
        {
            get { return GetValue(SelectionBrushProperty); }
            set { SetValue(SelectionBrushProperty, value); }
        }

        /// <summary>
        /// Defines the <see cref="HighlightedRegions"/> property.
        /// </summary>
        public static readonly StyledProperty<IEnumerable<MuPDFStructuredTextAddressSpan>> HighlightedRegionsProperty = AvaloniaProperty.Register<PDFRenderer, IEnumerable<MuPDFStructuredTextAddressSpan>>(nameof(HighlightedRegions), null);
        /// <summary>
        /// A collection of highlighted regions, e.g. as a result of a text search.
        /// </summary>
        public IEnumerable<MuPDFStructuredTextAddressSpan> HighlightedRegions
        {
            get { return GetValue(HighlightedRegionsProperty); }
            set { SetValue(HighlightedRegionsProperty, value); }
        }

        /// <summary>
        /// Defines the <see cref="HighlightBrush"/> property.
        /// </summary>
        public static readonly StyledProperty<IBrush> HighlightBrushProperty = AvaloniaProperty.Register<PDFRenderer, IBrush>(nameof(HighlightBrush), new SolidColorBrush(Color.FromArgb(96, 230, 159, 0)));
        /// <summary>
        /// The colour used to highlight the <see cref="HighlightedRegions"/>.
        /// </summary>
        public IBrush HighlightBrush
        {
            get { return GetValue(HighlightBrushProperty); }
            set { SetValue(HighlightBrushProperty, value); }
        }
    }
}
