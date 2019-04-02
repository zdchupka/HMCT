using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace HylandMedConfig.Controls
{
	[TemplatePart( Name = PART_Presenter, Type = typeof( ContentPresenter ) )]
	public class ZoomControl : ContentControl
	{
		public enum ZoomControlModes
		{
			Fill,
			Custom
		}

		public enum ZoomViewModifierMode
		{
			None,
			Pan,
			ZoomIn,
			ZoomOut,
			ZoomBox
		}

		#region Declarations

		private const string PART_Presenter = "PART_Presenter";
		private Point _mouseDownPos;
		private Vector _startTranslate;

		#endregion

		#region Properties 

		public static readonly DependencyProperty MaxZoomDeltaProperty =
			DependencyProperty.Register( "MaxZoomDelta", typeof( double ), typeof( ZoomControl ), new UIPropertyMetadata( 5.0 ) );

		public static readonly DependencyProperty MaxZoomProperty =
			DependencyProperty.Register( "MaxZoom", typeof( double ), typeof( ZoomControl ), new UIPropertyMetadata( 100.0 ) );

		public static readonly DependencyProperty MinZoomProperty =
			DependencyProperty.Register( "MinZoom", typeof( double ), typeof( ZoomControl ), new UIPropertyMetadata( 0.01 ) );

		public static readonly DependencyProperty ModeProperty =
			DependencyProperty.Register( "Mode", typeof( ZoomControlModes ), typeof( ZoomControl ), new UIPropertyMetadata( ZoomControlModes.Fill, Mode_PropertyChanged ) );

		public static readonly DependencyProperty ClipContentProperty =
			DependencyProperty.Register( "ClipContent", typeof( bool ), typeof( ZoomControl ), new PropertyMetadata( true ) );

		public static readonly DependencyProperty ZoomControlVisibilityProperty =
			DependencyProperty.Register( "ZoomControlVisibility", typeof( Visibility ), typeof( ZoomControl ), new PropertyMetadata( Visibility.Visible ) );

		public static readonly DependencyProperty ModifierModeProperty =
			DependencyProperty.Register( "ModifierMode", typeof( ZoomViewModifierMode ), typeof( ZoomControl ), new UIPropertyMetadata( ZoomViewModifierMode.None ) );

		public static readonly DependencyProperty TranslateXProperty =
			DependencyProperty.Register( "TranslateX", typeof( double ), typeof( ZoomControl ), new UIPropertyMetadata( 0.0 ) );

		public static readonly DependencyProperty TranslateYProperty =
			DependencyProperty.Register( "TranslateY", typeof( double ), typeof( ZoomControl ), new UIPropertyMetadata( 0.0 ) );

		public static readonly DependencyProperty ZoomBoxBackgroundProperty =
			DependencyProperty.Register( "ZoomBoxBackground", typeof( Brush ), typeof( ZoomControl ), new UIPropertyMetadata( null ) );

		public static readonly DependencyProperty ZoomBoxBorderBrushProperty =
			DependencyProperty.Register( "ZoomBoxBorderBrush", typeof( Brush ), typeof( ZoomControl ), new UIPropertyMetadata( null ) );

		public static readonly DependencyProperty ZoomBoxBorderThicknessProperty =
			DependencyProperty.Register( "ZoomBoxBorderThickness", typeof( Thickness ), typeof( ZoomControl ), new UIPropertyMetadata( null ) );

		public static readonly DependencyProperty ZoomBoxOpacityProperty =
			DependencyProperty.Register( "ZoomBoxOpacity", typeof( double ), typeof( ZoomControl ), new UIPropertyMetadata( 0.5 ) );

		public static readonly DependencyProperty ZoomBoxProperty =
			DependencyProperty.Register( "ZoomBox", typeof( Rect ), typeof( ZoomControl ), new UIPropertyMetadata( new Rect() ) );

		public static readonly DependencyProperty ZoomDeltaMultiplierProperty =
			DependencyProperty.Register( "ZoomDeltaMultiplier", typeof( double ), typeof( ZoomControl ), new UIPropertyMetadata( 20.0 ) );

		public static readonly DependencyProperty ZoomProperty =
			DependencyProperty.Register( "Zoom", typeof( double ), typeof( ZoomControl ), new UIPropertyMetadata( 1.0 ) );

		public static readonly DependencyProperty ZoomControlCanvasLeftProperty =
			DependencyProperty.Register( "ZoomControlCanvasLeft", typeof( double ), typeof( ZoomControl ), new PropertyMetadata( 5d ) );

		public Brush ZoomBoxBackground
		{
			get { return (Brush)GetValue( ZoomBoxBackgroundProperty ); }
			set { SetValue( ZoomBoxBackgroundProperty, value ); }
		}

		public Brush ZoomBoxBorderBrush
		{
			get { return (Brush)GetValue( ZoomBoxBorderBrushProperty ); }
			set { SetValue( ZoomBoxBorderBrushProperty, value ); }
		}

		public Thickness ZoomBoxBorderThickness
		{
			get { return (Thickness)GetValue( ZoomBoxBorderThicknessProperty ); }
			set { SetValue( ZoomBoxBorderThicknessProperty, value ); }
		}

		public double ZoomBoxOpacity
		{
			get { return (double)GetValue( ZoomBoxOpacityProperty ); }
			set { SetValue( ZoomBoxOpacityProperty, value ); }
		}

		public double ZoomControlCanvasLeft
		{
			get { return (double)GetValue( ZoomControlCanvasLeftProperty ); }
			set { SetValue( ZoomControlCanvasLeftProperty, value ); }
		}

		public Rect ZoomBox
		{
			get { return (Rect)GetValue( ZoomBoxProperty ); }
			set { SetValue( ZoomBoxProperty, value ); }
		}

		public Point OrigoPosition
		{
			get { return new Point( ActualWidth / 2, ActualHeight / 2 ); }
		}

		public double TranslateX
		{
			get { return (double)GetValue( TranslateXProperty ); }
			set
			{
				BeginAnimation( TranslateXProperty, null );
				SetValue( TranslateXProperty, value );
			}
		}

		public double TranslateY
		{
			get { return (double)GetValue( TranslateYProperty ); }
			set
			{
				BeginAnimation( TranslateYProperty, null );
				SetValue( TranslateYProperty, value );
			}
		}

		public double MinZoom
		{
			get { return (double)GetValue( MinZoomProperty ); }
			set { SetValue( MinZoomProperty, value ); }
		}

		public double MaxZoom
		{
			get { return (double)GetValue( MaxZoomProperty ); }
			set { SetValue( MaxZoomProperty, value ); }
		}

		public double MaxZoomDelta
		{
			get { return (double)GetValue( MaxZoomDeltaProperty ); }
			set { SetValue( MaxZoomDeltaProperty, value ); }
		}

		public double ZoomDeltaMultiplier
		{
			get { return (double)GetValue( ZoomDeltaMultiplierProperty ); }
			set { SetValue( ZoomDeltaMultiplierProperty, value ); }
		}

		public bool ClipContent
		{
			get { return (bool)GetValue( ClipContentProperty ); }
			set { SetValue( ClipContentProperty, value ); }
		}

		public Visibility ZoomControlVisibility
		{
			get { return (Visibility)GetValue( ZoomControlVisibilityProperty ); }
			set { SetValue( ZoomControlVisibilityProperty, value ); }
		}

		public double Zoom
		{
			get { return (double)GetValue( ZoomProperty ); }
			set { SetValue( ZoomProperty, value ); }
		}

		protected ContentPresenter Presenter
		{
			get;
			private set;
		}

		public ZoomViewModifierMode ModifierMode
		{
			get { return (ZoomViewModifierMode)GetValue( ModifierModeProperty ); }
			set { SetValue( ModifierModeProperty, value ); }
		}

		public ZoomControlModes Mode
		{
			get { return (ZoomControlModes)GetValue( ModeProperty ); }
			set { SetValue( ModeProperty, value ); }
		}

		#endregion

		#region Commands

		public static readonly ICommand ResetCommand = new RoutedCommand();

		#endregion

		#region  Construction

		static ZoomControl()
		{
			DefaultStyleKeyProperty.OverrideMetadata( typeof( ZoomControl ), new FrameworkPropertyMetadata( typeof( ZoomControl ) ) );
		}

		public ZoomControl()
		{
			PreviewMouseWheel += ZoomControl_MouseWheel;
			PreviewMouseDown += ZoomControl_PreviewMouseDown;
			MouseDown += ZoomControl_MouseDown;
			MouseUp += ZoomControl_MouseUp;
			CommandBindings.Add( new CommandBinding( ResetCommand, ResetExecuted ) );
		}

		#endregion

		#region Private Methods

		private void ResetExecuted( object sender, ExecutedRoutedEventArgs e )
		{
			Mode = ZoomControlModes.Fill;
		}

		private void ZoomControl_MouseUp( object sender, MouseButtonEventArgs e )
		{
			switch( ModifierMode )
			{
				case ZoomViewModifierMode.None:
					return;
				case ZoomViewModifierMode.Pan:
					break;
				case ZoomViewModifierMode.ZoomIn:
					break;
				case ZoomViewModifierMode.ZoomOut:
					break;
				case ZoomViewModifierMode.ZoomBox:
					ZoomTo( ZoomBox );
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			ModifierMode = ZoomViewModifierMode.None;
			PreviewMouseMove -= ZoomControl_PreviewMouseMove;
			ReleaseMouseCapture();
		}

		private void ZoomTo( Rect rect )
		{
			double deltaZoom = Math.Min(
				ActualWidth / rect.Width,
				ActualHeight / rect.Height );

			Point startHandlePosition = new Point( rect.X + rect.Width / 2, rect.Y + rect.Height / 2 );

			DoZoom( deltaZoom, OrigoPosition, startHandlePosition, OrigoPosition );
			ZoomBox = new Rect();
		}

		private void ZoomControl_PreviewMouseMove( object sender, MouseEventArgs e )
		{
			switch( ModifierMode )
			{
				case ZoomViewModifierMode.None:
					return;
				case ZoomViewModifierMode.Pan:
					var translate = _startTranslate + ( e.GetPosition( this ) - _mouseDownPos );
					TranslateX = translate.X;
					TranslateY = translate.Y;
					Mode = ZoomControlModes.Custom;
					break;
				case ZoomViewModifierMode.ZoomIn:
					break;
				case ZoomViewModifierMode.ZoomOut:
					break;
				case ZoomViewModifierMode.ZoomBox:
					var pos = e.GetPosition( this );
					var x = Math.Min( _mouseDownPos.X, pos.X );
					var y = Math.Min( _mouseDownPos.Y, pos.Y );
					var sizeX = Math.Abs( _mouseDownPos.X - pos.X );
					var sizeY = Math.Abs( _mouseDownPos.Y - pos.Y );
					ZoomBox = new Rect( x, y, sizeX, sizeY );
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void ZoomControl_MouseDown( object sender, MouseButtonEventArgs e )
		{
			OnMouseDown( e, false );
		}

		private void ZoomControl_PreviewMouseDown( object sender, MouseButtonEventArgs e )
		{
			OnMouseDown( e, true );
		}

		private void OnMouseDown( MouseButtonEventArgs e, bool isPreview )
		{
			if( ( Keyboard.Modifiers & ModifierKeys.Control ) > 0 && e.MiddleButton == MouseButtonState.Pressed )
			{
				Mode = ZoomControlModes.Fill;
				e.Handled = true;
				return;
			}

			if( ModifierMode != ZoomViewModifierMode.None )
				return;

			switch( Keyboard.Modifiers )
			{
				case ModifierKeys.None:
					if( !isPreview )
						ModifierMode = ZoomViewModifierMode.Pan;
					break;
				case ModifierKeys.Alt:
					ModifierMode = ZoomViewModifierMode.ZoomBox;
					break;
				case ModifierKeys.Control:
					break;
				case ModifierKeys.Shift:
					ModifierMode = ZoomViewModifierMode.Pan;
					break;
				case ModifierKeys.Windows:
					break;
				default:
					return;
			}

			if( ModifierMode == ZoomViewModifierMode.None )
				return;

			_mouseDownPos = e.GetPosition( this );
			_startTranslate = new Vector( TranslateX, TranslateY );
			Mouse.Capture( this );
			PreviewMouseMove += ZoomControl_PreviewMouseMove;
		}

		private void ZoomControl_MouseWheel( object sender, MouseWheelEventArgs e )
		{
			if( ( Keyboard.Modifiers & ModifierKeys.Control ) > 0 && ModifierMode == ZoomViewModifierMode.None )
			{
				Point origoPosition = new Point( ActualWidth / 2, ActualHeight / 2 );
				Point mousePosition = e.GetPosition( this );

				DoZoom(
					Math.Max( 1 / MaxZoomDelta, Math.Min( MaxZoomDelta, e.Delta / 10000.0 * ZoomDeltaMultiplier + 1 ) ),
					origoPosition,
					mousePosition,
					mousePosition );
				e.Handled = true;
			}
		}

		private void DoZoom( double deltaZoom, Point origoPosition, Point startHandlePosition, Point targetHandlePosition )
		{
			double startZoom = Zoom;
			double currentZoom = startZoom * deltaZoom;
			currentZoom = Math.Max( MinZoom, Math.Min( MaxZoom, currentZoom ) );

			Vector startTranslate = new Vector( TranslateX, TranslateY );

			Vector v = ( startHandlePosition - origoPosition );
			Vector vTarget = ( targetHandlePosition - origoPosition );

			Vector targetPoint = ( v - startTranslate ) / startZoom;
			Vector zoomedTargetPointPos = targetPoint * currentZoom + startTranslate;
			Vector endTranslate = vTarget - zoomedTargetPointPos;

			double transformX = TranslateX + endTranslate.X;
			double transformY = TranslateY + endTranslate.Y;

			TranslateX = transformX;
			TranslateY = transformY;
			Zoom = currentZoom;
			Mode = ZoomControlModes.Custom;
		}

		private static void Mode_PropertyChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			ZoomControl zc = (ZoomControl)d;
			ZoomControlModes mode = (ZoomControlModes)e.NewValue;

			if( mode == ZoomControlModes.Fill )
			{
				zc.TranslateX = 0;
				zc.TranslateY = 0;
				zc.Zoom = 1;
			}
		}

		#endregion

		#region Public Methods

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			Presenter = GetTemplateChild( PART_Presenter ) as ContentPresenter;

			//add the ScaleTransform to the presenter
			TransformGroup transformGroup = new TransformGroup();

			ScaleTransform scaleTransform = new ScaleTransform();
			transformGroup.Children.Add( scaleTransform );
			BindingOperations.SetBinding( scaleTransform, ScaleTransform.ScaleXProperty, new Binding( nameof( Zoom ) ) { Source = this } );
			BindingOperations.SetBinding( scaleTransform, ScaleTransform.ScaleYProperty, new Binding( nameof( Zoom ) ) { Source = this } );

			// Translate
			TranslateTransform translateTransform = new TranslateTransform();
			transformGroup.Children.Add( translateTransform );
			BindingOperations.SetBinding( translateTransform, TranslateTransform.YProperty, new Binding( nameof( TranslateY ) ) { Source = this } );
			BindingOperations.SetBinding( translateTransform, TranslateTransform.XProperty, new Binding( nameof( TranslateX ) ) { Source = this } );

			Presenter.RenderTransform = transformGroup;
			Presenter.RenderTransformOrigin = new Point( 0.5, 0.5 );
		}

		#endregion
	}
}
