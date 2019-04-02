using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interactivity;
using System.Windows.Media;
using HylandMedConfig.Attached;
using HylandMedConfig.Common;
using HylandMedConfig.Controls;
using HylandMedConfig.Properties;

namespace HylandMedConfig.Converters
{
	public class ChatMessageBubbleStatusTextConverter : ConverterMarkupExtension<ChatMessageBubbleStatusTextConverter>
	{
		public ChatMessageBubbleStatusTextConverter()
		{

		}

		public override object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
		{
			ChatUserMessage message = value as ChatUserMessage;
			if( message == null )
			{
				return string.Empty;
			}
			bool isFromCurrentUser = ApplicationViewModel.Current.ChatProxy.CurrentUser == message.FromUser;


			// •
			StackPanel sp = new StackPanel
			{
				Orientation = Orientation.Horizontal,
			};

			if( !isFromCurrentUser )
			{
				TextBlock fromUser = new TextBlock
				{
					ToolTip = Application.Current.Resources["UserTooltip"],
					ContextMenu = Application.Current.Resources["UserContextMenu"] as ContextMenu,
					DataContext = message.FromUser,
					VerticalAlignment = VerticalAlignment.Center,
				};

				fromUser.SetBinding( TextBlock.TextProperty, new Binding( nameof( ChatUser.DisplayNameResolved ) ) );

				Interaction.GetBehaviors( fromUser ).Add( new FrameworkElementWhisperBehavior() { User = message.FromUser } );
				sp.Children.Add( fromUser );
				sp.Children.Add( new TextBlock { Text = "  •  " } );
			}

			sp.Children.Add( new TextBlock { Text = message.Date.ToString( "t" ), VerticalAlignment = VerticalAlignment.Center } );

			if( message.ToUsers.Count == 1 )
			{
				sp.Children.Add( new TextBlock
				{
					Text = "  •  To ",
					VerticalAlignment = VerticalAlignment.Center,
				} );
				TextBlock toUser = new TextBlock
				{
					ToolTip = Application.Current.Resources["UserTooltip"],
					ContextMenu = Application.Current.Resources["UserContextMenu"] as ContextMenu,
					DataContext = message.ToUsers[0],
					VerticalAlignment = VerticalAlignment.Center,
				};
				toUser.SetBinding( TextBlock.TextProperty, new Binding( nameof( ChatUser.DisplayNameResolved ) ) );
				Interaction.GetBehaviors( toUser ).Add( new FrameworkElementWhisperBehavior() { User = message.ToUsers[0] } );
				sp.Children.Add( toUser );
			}

			if( message.ToUsers.Count > 1 )
			{
				TextBlock toUser = new TextBlock
				{
					Text = "  •  Group Message",
					VerticalAlignment = VerticalAlignment.Center,
                    ContextMenu = Application.Current.Resources["GroupMessageContextMenu"] as ContextMenu,
				};

				List<ChatUser> replyUsers = new List<ChatUser>();

				if( message.FromUser != ApplicationViewModel.Current.ChatProxy.CurrentUser )
				{
					replyUsers.Add( message.FromUser );
				}

				foreach( ChatUser user in message.ToUsers )
				{
					if( user != ApplicationViewModel.Current.ChatProxy.CurrentUser )
					{
						replyUsers.Add( user );
					}
				}

				toUser.ToolTip = string.Join( Environment.NewLine, message.ToUsers.Union( new List<ChatUser> { message.FromUser } ).Select( u => u.DisplayNameResolved ).OrderBy( u => u ) );
				Interaction.GetBehaviors( toUser ).Add( new FrameworkElementWhisperUsers() { Users = replyUsers } );
				sp.Children.Add( toUser );
			}

			Binding fontSizeBinding = new Binding( "SystemFontSize" ) { Source = Settings.Default };

			if( message.Tags.Count > 0 )
			{
				TextBlock separator = new TextBlock { Text = "  •  ", VerticalAlignment = VerticalAlignment.Center };
				sp.Children.Add( separator );

				if( message.Tags.Count > 1 )
				{
					TagListButton tagListButton = new TagListButton()
					{
						Height = Settings.Default.SystemFontSize,
						Width = Settings.Default.SystemFontSize,
						Margin = new Thickness( 2 ),
						Command = ApplicationViewModel.Current.ReplyAllTagsCommand,
						CommandParameter = message,
					};

					tagListButton.SetBinding( ThumbsUp.HeightProperty, fontSizeBinding );
					tagListButton.SetBinding( ThumbsUp.WidthProperty, fontSizeBinding );

					sp.Children.Add( tagListButton );
				}

				foreach( string tag in message.Tags )
				{
                    
					EmoticonRichTextBox toUser = new EmoticonRichTextBox
					{
						ImageSize = Size.Parse( "20,20" ),
						RawText = "#" + tag,
						ContextMenu = Application.Current.Resources["TagContextMenu"] as ContextMenu,
						Margin = new Thickness( 2 ),
						DataContext = ApplicationViewModel.Current.GetTag(tag),
						VerticalAlignment = VerticalAlignment.Center,
					};
					Interaction.GetBehaviors( toUser ).Add( new EmoticonRichTextBoxSendTaggedMessage() { Tag = tag } );
					sp.Children.Add( toUser );
				}
			}

			sp.Children.Add( new TextBlock { Text = "  •  ", VerticalAlignment = VerticalAlignment.Center } );

			// If this is a combined message, the rating system will apply to the first message
			CombinedNormalChatUserMessages combinedMessage = message as CombinedNormalChatUserMessages;
			ChatUserMessage firstMessage = message;
			if( combinedMessage != null )
			{
				firstMessage = combinedMessage.Messages.First();
			}

			ThumbsUp thumbsUp = new ThumbsUp { Height = Settings.Default.SystemFontSize, Width = Settings.Default.SystemFontSize };
			thumbsUp.Command = ApplicationViewModel.Current.ToggleThumbsUpCommand;
			thumbsUp.CommandParameter = firstMessage;
			MultiBinding thumbsUpCheckedBinding = new MultiBinding { Converter = new IsMessageThumbedUpByCurrentUserConverter() };
			thumbsUpCheckedBinding.Bindings.Add( new Binding( "VotesFor" ) { Source = firstMessage } );
			thumbsUpCheckedBinding.Bindings.Add( new Binding( "VotesFor.Count" ) { Source = firstMessage } );
			thumbsUp.SetBinding( ThumbsUp.IsCheckedProperty, thumbsUpCheckedBinding );
			thumbsUp.SetBinding( ThumbsUp.HeightProperty, fontSizeBinding );
			thumbsUp.SetBinding( ThumbsUp.WidthProperty, fontSizeBinding );
			sp.Children.Add( thumbsUp );

			ItemsControl thumbsUpVotes = new ItemsControl();
			thumbsUpVotes.SetBinding( ItemsControl.ItemsSourceProperty, new Binding( "VotesFor" ) { Source = firstMessage } );
			ToolTip thumbsUpTooltip = new ToolTip() { Content = thumbsUpVotes };

			TextBlock txtThumbsUp = new TextBlock { Margin = new Thickness( 5, 0, 10, 0 ), VerticalAlignment = VerticalAlignment.Center };
			Binding b = new Binding( "VotesFor.Count" ) { Source = firstMessage };
			txtThumbsUp.SetBinding( TextBlock.TextProperty, b );
			txtThumbsUp.ToolTip = thumbsUpTooltip;
			sp.Children.Add( txtThumbsUp );

			ThumbsDown thumbsDown = new ThumbsDown { Height = Settings.Default.SystemFontSize, Width = Settings.Default.SystemFontSize };
			thumbsDown.Command = ApplicationViewModel.Current.ToggleThumbsDownCommand;
			thumbsDown.CommandParameter = firstMessage;
			MultiBinding thumbsDownCheckedBinding = new MultiBinding { Converter = new IsMessageThumbedUpByCurrentUserConverter() };
			thumbsDownCheckedBinding.Bindings.Add( new Binding( "VotesAgainst" ) { Source = firstMessage } );
			thumbsDownCheckedBinding.Bindings.Add( new Binding( "VotesAgainst.Count" ) { Source = firstMessage } );
			thumbsDown.SetBinding( ThumbsDown.IsCheckedProperty, thumbsDownCheckedBinding );
			thumbsDown.SetBinding( ThumbsDown.HeightProperty, fontSizeBinding );
			thumbsDown.SetBinding( ThumbsDown.WidthProperty, fontSizeBinding );
			sp.Children.Add( thumbsDown );

			ItemsControl thumbsDownVotes = new ItemsControl();
			thumbsDownVotes.SetBinding( ItemsControl.ItemsSourceProperty, new Binding( "VotesAgainst" ) { Source = firstMessage } );
			ToolTip thumbsDownTooltip = new ToolTip() { Content = thumbsDownVotes };

			TextBlock txtThumbsDown = new TextBlock { Margin = new Thickness( 5, 0, 0, 0 ), VerticalAlignment = VerticalAlignment.Center };
			Binding b2 = new Binding( "VotesAgainst.Count" ) { Source = firstMessage };
			txtThumbsDown.SetBinding( TextBlock.TextProperty, b2 );
			txtThumbsDown.ToolTip = thumbsDownTooltip;
			sp.Children.Add( txtThumbsDown );

			return sp;
		}
	}

	public class ChatStatusMessageBubbleStatusTextConverter : ConverterMarkupExtension<ChatStatusMessageBubbleStatusTextConverter>
	{
		public ChatStatusMessageBubbleStatusTextConverter()
		{

		}

		public override object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
		{
			ChatUserMessage message = value as ChatUserMessage;
			if( message == null )
			{
				return string.Empty;
			}
			bool isFromCurrentUser = ApplicationViewModel.Current.ChatProxy.CurrentUser == message.FromUser;


			// •
			StackPanel sp = new StackPanel
			{
				Orientation = Orientation.Horizontal,
			};


			sp.Children.Add( new TextBlock { Text = message.Date.ToString( "t" ), VerticalAlignment = VerticalAlignment.Center } );

			int toUserCount = message.ToUsers == null ? 0 : message.ToUsers.Count;

			if( toUserCount == 1 )
			{
				sp.Children.Add( new TextBlock
				{
					Text = "  •  To ",
				} );
				TextBlock toUser = new TextBlock();
				toUser.SetBinding( TextBlock.TextProperty, new Binding( nameof( ChatUser.DisplayNameResolved ) ) );
				Interaction.GetBehaviors( toUser ).Add( new FrameworkElementWhisperBehavior() { User = message.ToUsers[0] } );
				sp.Children.Add( toUser );
			}

			if( toUserCount > 1 )
			{
				TextBlock toUser = new TextBlock
				{
					Text = "  •  Group Message",
                    ContextMenu = Application.Current.Resources["GroupMessageContextMenu"] as ContextMenu,
				};

				List<ChatUser> replyUsers = new List<ChatUser>();

				if( message.FromUser != ApplicationViewModel.Current.ChatProxy.CurrentUser )
				{
					replyUsers.Add( message.FromUser );
				}

				foreach( ChatUser user in message.ToUsers )
				{
					if( user != ApplicationViewModel.Current.ChatProxy.CurrentUser )
					{
						replyUsers.Add( user );
					}
				}

				toUser.ToolTip = string.Join( Environment.NewLine, message.ToUsers.Union( new List<ChatUser> { message.FromUser } ).Select( u => u.DisplayNameResolved ).OrderBy( u => u ) );
				Interaction.GetBehaviors( toUser ).Add( new FrameworkElementWhisperUsers() { Users = replyUsers } );
				sp.Children.Add( toUser );
			}

			sp.Children.Add( new TextBlock { Text = "  •  " } );

			ThumbsUp thumbsUp = new ThumbsUp();
			Binding fontSizeBinding = new Binding( "SystemFontSize" ) { Source = Settings.Default };
			thumbsUp.Command = ApplicationViewModel.Current.ToggleThumbsUpCommand;
			thumbsUp.CommandParameter = message;
			MultiBinding thumbsUpCheckedBinding = new MultiBinding { Converter = new IsMessageThumbedUpByCurrentUserConverter() };
			thumbsUpCheckedBinding.Bindings.Add( new Binding( "VotesFor" ) { Source = message } );
			thumbsUpCheckedBinding.Bindings.Add( new Binding( "VotesFor.Count" ) { Source = message } );
			thumbsUp.SetBinding( ThumbsUp.IsCheckedProperty, thumbsUpCheckedBinding );
			thumbsUp.SetBinding( ThumbsUp.HeightProperty, fontSizeBinding );
			thumbsUp.SetBinding( ThumbsUp.WidthProperty, fontSizeBinding );
			sp.Children.Add( thumbsUp );

			ItemsControl thumbsUpVotes = new ItemsControl();
			thumbsUpVotes.SetBinding( ItemsControl.ItemsSourceProperty, new Binding( "VotesFor" ) { Source = message } );
			ToolTip thumbsUpTooltip = new ToolTip() { Content = thumbsUpVotes };

			TextBlock txtThumbsUp = new TextBlock { Margin = new Thickness( 5, 0, 10, 0 ) };
			Binding b = new Binding( "VotesFor.Count" ) { Source = message };
			txtThumbsUp.SetBinding( TextBlock.TextProperty, b );
			txtThumbsUp.ToolTip = thumbsUpTooltip;
			sp.Children.Add( txtThumbsUp );

			ThumbsDown thumbsDown = new ThumbsDown();
			thumbsDown.Command = ApplicationViewModel.Current.ToggleThumbsDownCommand;
			thumbsDown.CommandParameter = message;
			MultiBinding thumbsDownCheckedBinding = new MultiBinding { Converter = new IsMessageThumbedUpByCurrentUserConverter() };
			thumbsDownCheckedBinding.Bindings.Add( new Binding( "VotesAgainst" ) { Source = message } );
			thumbsDownCheckedBinding.Bindings.Add( new Binding( "VotesAgainst.Count" ) { Source = message } );
			thumbsDown.SetBinding( ThumbsDown.IsCheckedProperty, thumbsDownCheckedBinding );
			thumbsDown.SetBinding( ThumbsDown.HeightProperty, fontSizeBinding );
			thumbsDown.SetBinding( ThumbsDown.WidthProperty, fontSizeBinding );
			sp.Children.Add( thumbsDown );

			ItemsControl thumbsDownVotes = new ItemsControl();
			thumbsDownVotes.SetBinding( ItemsControl.ItemsSourceProperty, new Binding( "VotesAgainst" ) { Source = message } );
			ToolTip thumbsDownTooltip = new ToolTip() { Content = thumbsDownVotes };

			TextBlock txtThumbsDown = new TextBlock { Margin = new Thickness( 5, 0, 0, 0 ) };
			Binding b2 = new Binding( "VotesAgainst.Count" ) { Source = message };
			txtThumbsDown.SetBinding( TextBlock.TextProperty, b2 );
			txtThumbsDown.ToolTip = thumbsDownTooltip;
			sp.Children.Add( txtThumbsDown );

			return sp;
		}
	}
}
