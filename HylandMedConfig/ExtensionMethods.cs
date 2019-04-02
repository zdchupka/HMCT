using HylandMedConfig.Common;
using HylandMedConfig.Properties;
using HylandMedConfig.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Deployment.Application;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HylandMedConfig
{
	public static class ExtensionMethods
	{
		private static Random _random = new Random();

		public static void Restart( this Application application )
		{
			Settings.Default.Save();
			if( ApplicationDeployment.IsNetworkDeployed )
			{
				String entryPoint = ApplicationDeployment.CurrentDeployment.UpdateLocation.AbsoluteUri;
				Process.Start( entryPoint );
				Application.Current.Shutdown();
			}
			else
			{
				Process.Start( Application.ResourceAssembly.Location );
				Application.Current.Shutdown();
			}
		}

		public static void Shuffle<T>( this IList<T> list )
		{
			int n = list.Count;
			while( n > 1 )
			{
				n--;
				int k = _random.Next( n + 1 );
				T value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
		}

		public static void InsertText( this TextBox textBox, string text )
		{
			textBox.SelectedText = text;
			textBox.CaretIndex += textBox.SelectedText.Length;
			textBox.SelectionLength = 0;
		}

		/// <summary>
		/// Reset property to it's default value
		/// </summary>
		public static void Reset( this ApplicationSettingsBase settings, string propertyName )
		{
			SettingsProperty property = settings.Properties[propertyName];
			TypeConverter converter = TypeDescriptor.GetConverter( property.PropertyType );
			settings[propertyName] = converter.ConvertFromString( property.DefaultValue.ToString() );
		}

		public static T FindParent<T>( this DependencyObject child ) where T : DependencyObject
		{
			//get parent item
			DependencyObject parentObject = VisualTreeHelper.GetParent( child );

			//we've reached the end of the tree
			if( parentObject == null ) return null;

			//check if the parent matches the type we're looking for
			T parent = parentObject as T;
			if( parent != null )
				return parent;
			else
				return FindParent<T>( parentObject );
		}

        public static bool SequenceEqualAnyOrder<TSource>(this IList<TSource> first, IList<TSource> second)
        {
            return first.Count == second.Count && !first.Except(second).Any();
        }

		/// <summary>
		/// Searches the Control's template for a child with the specified name, and returns it if found.
		/// If not found, this method will throw a missing member exception.
		/// </summary>
		/// <typeparam name="T">The type of the template child.</typeparam>
		/// <returns>The template child with the specified name.</returns>
		public static T EnsureTemplateChild<T>( this Control control, string templatePartName ) where T : class
		{
			if( control.Template == null )
			{
				throw new ArgumentException( "Control does not have a template." );
			}

			T child = control.Template.FindName( templatePartName, control ) as T;
			if( child == null )
			{
				throw new MissingMemberException( string.Format( "{0} requires a {1} named {2}.", control.GetType().Name, typeof( T ).Name, templatePartName ) );
			}

			return child;
		}

		public static ImageSource ToImageSource( this Icon icon )
		{
			Bitmap bitmap = icon.ToBitmap();
			IntPtr hBitmap = bitmap.GetHbitmap();

			ImageSource wpfBitmap = Imaging.CreateBitmapSourceFromHBitmap(
				hBitmap,
				IntPtr.Zero,
				Int32Rect.Empty,
				BitmapSizeOptions.FromEmptyOptions() );

			if( !NativeMethods.DeleteObject( hBitmap ) )
			{
				throw new Win32Exception();
			}

			return wpfBitmap;
		}

		public static void UpdateTaskbarItemInfo(this Window window, long count )
		{
			if( window.TaskbarItemInfo == null )
			{
				return;
			}

			int iconWidth = 20;
			int iconHeight = 20;

			if( count > 0 )
			{
				string countText = Math.Min( count, 999 ).ToString();

				RenderTargetBitmap bmp = new RenderTargetBitmap( iconWidth, iconHeight, 96, 96, PixelFormats.Default );

				ContentControl root = new ContentControl();

				root.ContentTemplate = ( Application.Current.TryFindResource( "TaskbarItemInfoTemplate" ) as DataTemplate );
				root.Content = countText;

				root.Arrange( new Rect( 0, 0, iconWidth, iconHeight ) );

				bmp.Render( root );

				window.TaskbarItemInfo.Overlay = (ImageSource)bmp;
			}
			else
			{
				window.TaskbarItemInfo.Overlay = null;
			}
		}
	}
}
