using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HylandMedConfig.Controls
{
    public class HylandAnimatedImage : Control
    {
        private System.Windows.Forms.PictureBox PART_PictureBox;

        public Uri UriSource
        {
            get { return (Uri)GetValue(UriSourceProperty); }
            set { SetValue(UriSourceProperty, value); }
        }

        public static readonly DependencyProperty UriSourceProperty =
            DependencyProperty.Register("UriSource", typeof(Uri), typeof(HylandAnimatedImage), new FrameworkPropertyMetadata((s, a) =>
            {
                HylandAnimatedImage image = s as HylandAnimatedImage;
                Uri uri = a.NewValue as Uri;
                if (image != null && image.PART_PictureBox != null && uri != null)
                {
                    image.PART_PictureBox.LoadAsync(uri.AbsoluteUri);
                }
            }));

        public long DownloadProgress
        {
            get { return (long)GetValue(DownloadProgressProperty); }
            set { SetValue(DownloadProgressProperty, value); }
        }

        public static readonly DependencyProperty DownloadProgressProperty =
            DependencyProperty.Register("DownloadProgress", typeof(long), typeof(HylandAnimatedImage), new PropertyMetadata(0L));

        static HylandAnimatedImage()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HylandAnimatedImage), new FrameworkPropertyMetadata(typeof(HylandAnimatedImage)));
        }

        public bool IsImageLoaded
        {
            get { return (bool)GetValue(IsLoadedProperty); }
            set { SetValue(IsLoadedProperty, value); }
        }

        public static readonly DependencyProperty IsLoadedProperty =
            DependencyProperty.Register("IsImageLoaded", typeof(bool), typeof(HylandAnimatedImage), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure));



        public bool HasError
        {
            get { return (bool)GetValue(HasErrorProperty); }
            set { SetValue(HasErrorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HasError.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HasErrorProperty =
            DependencyProperty.Register("HasError", typeof(bool), typeof(HylandAnimatedImage), new PropertyMetadata(false));



        public string ErrorMessage
        {
            get { return (string)GetValue(ErrorMessageProperty); }
            set { SetValue(ErrorMessageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ErrorMessage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ErrorMessageProperty =
            DependencyProperty.Register("ErrorMessage", typeof(string), typeof(HylandAnimatedImage), new PropertyMetadata(""));




        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            PART_PictureBox = this.EnsureTemplateChild<System.Windows.Forms.PictureBox>("PART_PictureBox");
            PART_PictureBox.LoadProgressChanged += pictureBoxLoading_LoadProgressChanged;
            PART_PictureBox.LoadAsync(UriSource.OriginalString);
            PART_PictureBox.LoadCompleted += PART_PictureBox_LoadCompleted;
        }

        private void PART_PictureBox_LoadCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                IsImageLoaded = true;
                MaxHeight = PART_PictureBox.PreferredSize.Height;
                MaxWidth = PART_PictureBox.PreferredSize.Width;
            }
            else
            {
                HasError = true;
                ErrorMessage = e.Error.Message;
            }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            //if (PART_PictureBox != null)
            //{
            //    if (IsImageLoaded)
            //    {
            //        double nativeWidth = PART_PictureBox.PreferredSize.Width;
            //        double nativeHeight = PART_PictureBox.PreferredSize.Height;
            //        double nativeAspectRatio = nativeWidth / nativeHeight;
            //        double constraintAspectRatio = constraint.Width / constraint.Height;

            //        double width = Math.Min(PART_PictureBox.PreferredSize.Width, constraint.Width);
            //        double height = Math.Min(PART_PictureBox.PreferredSize.Height, constraint.Height);



            //        //if (nativeAspectRatio >= constraintAspectRatio)
            //        //{
            //        //    width = constraint.Width;
            //        //    height = width / nativeAspectRatio;
            //        //}
            //        //else
            //        //{

            //        //}

            //        //if (nativeWidth > constraint.Width)
            //        //{
            //        //    nativeWidth = constraint.Width;
            //        //    nativeHeight = nativeWidth / nativeAspectRatio;
            //        //}

            //        //if (nativeHeight > constraint.Height)
            //        //{
            //        //    nativeHeight = constraint.Height;
            //        //    nativeWidth = nativeHeight * nativeAspectRatio;
            //        //}

            //        return new Size(width, height);

            //        //Size maxSize = new Size
            //        //{
            //        //    Height = Math.Min(PART_PictureBox.PreferredSize.Height, constraint.Height),
            //        //    Width = Math.Min(PART_PictureBox.PreferredSize.Width, constraint.Width),
            //        //};

            //        //return ResizeFit(new Size(PART_PictureBox.PreferredSize.Width, PART_PictureBox.PreferredSize.Height), maxSize);

            //        //double resizeWidth = 0d;
            //        //double resizeHeight = 0d;
            //        //double maxAspect = (double)constraint.Width / (double)constraint.Height;
            //        //double aspect = (double)PART_PictureBox.PreferredSize.Width / (double)PART_PictureBox.PreferredSize.Height;

            //        //if (maxAspect > aspect && PART_PictureBox.PreferredSize.Width > constraint.Width)
            //        //{
            //        //    //Width is the bigger dimension relative to max bounds
            //        //    resizeWidth = constraint.Width;
            //        //    resizeHeight = constraint.Width / aspect;
            //        //}
            //        //else if (maxAspect <= aspect && PART_PictureBox.PreferredSize.Height > constraint.Height)
            //        //{
            //        //    //Height is the bigger dimension
            //        //    resizeHeight = constraint.Height;
            //        //    resizeWidth = constraint.Height * aspect;
            //        //}

            //        //return new Size(resizeWidth, resizeHeight);
            //    }
            //}
            return base.MeasureOverride(constraint);
        }

        private void pictureBoxLoading_LoadProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            DownloadProgress = e.ProgressPercentage;
        }

        private Size ResizeFit(Size originalSize, Size maxSize)
        {
            var widthRatio = (double)maxSize.Width / (double)originalSize.Width;
            var heightRatio = (double)maxSize.Height / (double)originalSize.Height;
            var minAspectRatio = Math.Min(widthRatio, heightRatio);
            if (minAspectRatio > 1)
                return originalSize;
            return new Size((int)(originalSize.Width * minAspectRatio), (int)(originalSize.Height * minAspectRatio));
        }
    }
}


namespace HylandMedConfig
{
    public static class ControlExtensionMethods
    {
        /// <summary>
        /// Searches the Control's template for a child with the specified name, and returns it if found.
        /// If not found, this method will throw a missing member exception.
        /// </summary>
        /// <typeparam name="T">The type of the template child.</typeparam>
        /// <returns>The template child with the specified name.</returns>
        public static T EnsureTemplateChild<T>(this Control control, string templatePartName) where T : class
        {
            if (control.Template == null)
            {
                throw new ArgumentException("Control does not have a template.");
            }

            T child = control.Template.FindName(templatePartName, control) as T;
            if (child == null)
            {
                throw new MissingMemberException(string.Format("{0} requires a {1} named {2}.", control.GetType().Name, typeof(T).Name, templatePartName));
            }

            return child;
        }
    }
}