using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using HylandMedConfig.Attached;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;
using HylandMedConfig.Windows;
using System.Xml.Serialization;

namespace HylandMedConfig.Properties
{
	internal sealed partial class Settings
	{
		internal readonly Color DefaultColor;

		private Dictionary<string, string> _defaultBrushMapping;

		public Brush MessageBackgroundResolvedBrush
		{
			get
			{
				return GetResolvedBrush( "MessageBackgroundColor" );
			}
		}

		public Brush MessageForegroundResolvedBrush
		{
			get
			{
				return GetResolvedBrush( "MessageForegroundColor" );
			}
		}

		public Brush SystemForegroundResolvedBrush
		{
			get
			{
				return GetResolvedBrush( "SystemForegroundColor" );
			}
		}

		public Brush GroupMessageBackgroundResolvedBrush
		{
			get
			{
				return GetResolvedBrush( "GroupMessageBackgroundColor" );
			}
		}

		public Brush WhisperMessageBackgroundResolvedBrush
		{
			get
			{
				return GetResolvedBrush( "WhisperMessageBackgroundColor" );
			}
		}

		public Brush WhisperMessageForegroundResolvedBrush
		{
			get
			{
				return GetResolvedBrush( "WhisperMessageForegroundColor" );
			}
		}

		public Brush GroupMessageForegroundResolvedBrush
		{
			get
			{
				return GetResolvedBrush( "GroupMessageForegroundColor" );
			}
		}

		public Brush TagMessageBackgroundResolvedBrush
		{
			get
			{
				return GetResolvedBrush( "TagMessageBackgroundColor" );
			}
		}

		public Brush TagMessageForegroundResolvedBrush
		{
			get
			{
				return GetResolvedBrush( "TagMessageForegroundColor" );
			}
		}

		public Brush MessageBorderResolvedBrush
		{
			get
			{
				return GetResolvedBrush( "MessageBorderColor" );
			}
		}


		public Settings()
		{
			_defaultBrushMapping = new Dictionary<string, string>
			{
				{ "MessageBackgroundColor", "ChatMessageCalloutNormalBackgroundBrush"},
				{ "MessageForegroundColor", "ForegroundBrush"},
				{ "GroupMessageBackgroundColor", "ChatMessageCalloutGroupWhisperBackgroundBrush"},
				{ "WhisperMessageBackgroundColor", "ChatMessageCalloutWhisperBackgroundBrush"},
				{ "WhisperMessageForegroundColor", "ForegroundBrush"},
				{ "TagMessageBackgroundColor", "ChatMessageCalloutTagBackgroundBrush"},
				{ "TagMessageForegroundColor", "ForegroundBrush"},
				{ "GroupMessageForegroundColor", "ForegroundBrush"},
				{ "MessageBorderColor", "ChatMessageCalloutBorderBrush"},
				{ "SystemForegroundColor", "SystemForegroundBrush"},
			};

			this.PropertyChanged += Settings_PropertyChanged;

			FrameworkElementBehavior.ThemeChanged += FrameworkElementBehavior_ThemeChanged;

			SettingsProperty property = Properties["MessageBackgroundColor"];
			TypeConverter converter = TypeDescriptor.GetConverter( property.PropertyType );
			DefaultColor = (Color)converter.ConvertFromString( property.DefaultValue.ToString() );
		}

		private void Settings_PropertyChanged( object sender, PropertyChangedEventArgs e )
		{
			if( _defaultBrushMapping.ContainsKey( e.PropertyName ) )
			{
				string resolvedName = e.PropertyName.Substring( 0, e.PropertyName.Length - 5 ) + "ResolvedBrush";
				OnPropertyChanged( this, new PropertyChangedEventArgs( resolvedName ) );
			}

			if( e.PropertyName == "EnableAnimations" )
			{
				if( EnableAnimations )
				{
					Application.Current.Resources[SystemParameters.MenuPopupAnimationKey] = PopupAnimation.Slide;
					Application.Current.Resources[SystemParameters.ComboBoxPopupAnimationKey] = PopupAnimation.Slide;
				}
				else
				{
					Application.Current.Resources[SystemParameters.MenuPopupAnimationKey] = PopupAnimation.None;
					Application.Current.Resources[SystemParameters.ComboBoxPopupAnimationKey] = PopupAnimation.None;
				}
			}
		}

		private Brush GetResolvedBrush( string settingName )
		{
			Color color = (Color)this[settingName];
			if( color == DefaultColor )
			{
				return Application.Current.MainWindow.TryFindResource( _defaultBrushMapping[settingName] ) as Brush;
			}
			return new SolidColorBrush( color );
		}

		void FrameworkElementBehavior_ThemeChanged( object sender, EventArgs e )
		{
			foreach( string key in _defaultBrushMapping.Keys )
			{
				string resolvedName = key.Substring( 0, key.Length - 5 ) + "ResolvedBrush";
				OnPropertyChanged( this, new PropertyChangedEventArgs( resolvedName ) );
			}
		}

		private void SettingChangingEventHandler( object sender, System.Configuration.SettingChangingEventArgs e )
		{
			// Add code to handle the SettingChangingEvent event here.
		}

		private void SettingsSavingEventHandler( object sender, System.ComponentModel.CancelEventArgs e )
		{
			// Add code to handle the SettingsSaving event here.
		}

		public bool IsTagIgnored( string tag )
		{
			foreach( string ignoredTag in IgnoredTags )
			{
				Match m = Regex.Match( tag, WildcardToRegex( ignoredTag ) );
				if( m.Success )
				{
					return true;
				}
			}
			return false;
		}

		internal void Export( string settingsFilePath )
		{
			Save();
			var config = ConfigurationManager.OpenExeConfiguration( ConfigurationUserLevel.PerUserRoamingAndLocal );
			config.SaveAs( settingsFilePath );
		}

		internal void Import( string settingsFilePath )
		{
			if( !File.Exists( settingsFilePath ) )
			{
				throw new FileNotFoundException();
			}

			try
			{
				// Open settings file as XML
				var import = XDocument.Load( settingsFilePath );
				// Get the <setting> elements
				var settings = import.XPathSelectElements( "//setting" );
				foreach( var setting in settings )
				{
					string name = setting.Attribute( "name" ).Value;
					string value = setting.XPathSelectElement( "value" ).FirstNode?.ToString();

					if( value == null ) { value = string.Empty; };

					try
					{
						Type t = this[name].GetType();
						TypeConverter typeConverter = TypeDescriptor.GetConverter( this[name].GetType() );

						if( typeConverter.CanConvertFrom( typeof( string ) ) )
						{
							this[name] = typeConverter.ConvertFrom( value );
						}
						else
						{
							XmlSerializer s = new XmlSerializer( t );
							using( TextReader reader = new StringReader( value ) )
							{
								object xmlValue = s.Deserialize( reader );
								this[name] = xmlValue;
							}
						}
					}
					catch( SettingsPropertyNotFoundException )
					{

					}
				}
				OnPropertyChanged( this, new PropertyChangedEventArgs( string.Empty ) );
			}
			catch( Exception )
			{
				MedConfigMessageBox.ShowError( "Invalid Configuration File" );
				Reload();
			}
		}

		public static string WildcardToRegex( string pattern )
		{
			return "^" + Regex.Escape( pattern )
							  .Replace( @"\*", ".*" )
							  .Replace( @"\?", "." )
					   + "$";
		}
	}
}
