using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Gw2BuildHelper {
    /// <summary>
    /// Interaction logic for OverlayWindow.xaml
    /// </summary>
    public partial class OverlayWindow : Window {


        public InterfaceSize hp;

        public OverlayWindow () {
            InitializeComponent();
            hp = InterfaceSize.LoadInterfaceSize();
            
            Left = System.Windows.Forms.SystemInformation.VirtualScreen.Left;
            Top = System.Windows.Forms.SystemInformation.VirtualScreen.Top;
            Width = System.Windows.Forms.SystemInformation.VirtualScreen.Width;
            Height = System.Windows.Forms.SystemInformation.VirtualScreen.Height;
        }

        public void ShowScreenHelpers (Bitmap bmpScreenCapture, System.Drawing.Point p) {

            int rectCount = 0;
            int circleCount = 0;

            //rectCount += DebugSaveSourceImages(bmpScreenCapture, p);

            for (int specIndex = 0; specIndex < 3; ++specIndex) {

                int correctSpecIndex = MainWindow.instance.m_CurrentBuild.Specializations[specIndex].specIndex;

                if (correctSpecIndex == -1)
                    continue;

                if (IsUsingCorrectSpec(bmpScreenCapture, p, specIndex))
                {

                    for (int traitIndex = 0; traitIndex < 3; ++traitIndex) {

                        int traitValue = MainWindow.instance.m_CurrentBuild.Specializations[specIndex].traitValues[traitIndex] - 1;

                        if (traitValue == -1)
                            continue;
                        
                        if (rectangles.Children.Count <= rectCount)
                            rectangles.Children.Add(CreateRectangle());

                        var rectangle = rectangles.Children[rectCount++] as System.Windows.Shapes.Rectangle;

                        rectangle.Width = hp.TraitIconSize;
                        rectangle.Height = hp.TraitIconSize;

                        bool correctTrait = IsUsingCorrectTrait(bmpScreenCapture, p, specIndex, traitIndex, traitValue);
                        rectangle.Stroke = correctTrait ? System.Windows.Media.Brushes.Green : System.Windows.Media.Brushes.Red;

                        double x = hp.TraitOffsetX + traitIndex * hp.TraitSpacingX;
                        double y = hp.TraitOffsetY + specIndex * hp.TraitSpacingY;

                        y += traitValue * hp.TraitChoiceSpacing;

                        rectangle.Margin = new Thickness(p.X + x, p.Y + y, 0.0f, 0.0f);
                    }
                }
                else
                {
                    if (rectangles.Children.Count <= rectCount)
                        rectangles.Children.Add(CreateRectangle());

                    var rectangle = rectangles.Children[rectCount++] as System.Windows.Shapes.Rectangle;

                    rectangle.Width = hp.SpecDropDownWidth;
                    rectangle.Height = hp.SpecDropDownHeight;
                    rectangle.Stroke = System.Windows.Media.Brushes.Red;

                    double x = hp.SpecDropDownOffsetX;
                    double y = hp.SpecDropDownOffsetY + specIndex * hp.SpecDropDownSpacingY;

                    rectangle.Margin = new Thickness(p.X + x, p.Y + y, 0.0f, 0.0f);

                    ////

                    var r = new System.Drawing.Rectangle((int)rectangle.Margin.Left, (int)rectangle.Margin.Top, hp.SpecDropDownMouseOverSizeX, (int)rectangle.Height);
                    var mouse = System.Windows.Forms.Control.MousePosition;
                    if (r.Contains(mouse)) {
                        if (circles.Children.Count <= circleCount)
                            circles.Children.Add(CreateCircle());

                        var circle = circles.Children[circleCount++] as Ellipse;

                        circle.Width = circle.Height = hp.SpecCorrectChoiceIconSize;

                        x = hp.SpecCorrectChoiceOffsetX + (correctSpecIndex / 3) * hp.SpecCorrectChoiceSpacingX;
                        y = hp.SpecCorrectChoiceOffsetY + (correctSpecIndex % 3) * hp.SpecCorrectChoiceSpacingY;
                        y += specIndex * hp.SpecSourceImageSpacingY;

                        circle.Margin = new Thickness(p.X + x, p.Y + y, 0.0f, 0.0f);
                    }
                }
            }

            while (rectangles.Children.Count > rectCount)
                rectangles.Children.RemoveAt(rectangles.Children.Count - 1);

            while (circles.Children.Count > circleCount)
                circles.Children.RemoveAt(circles.Children.Count - 1);
        }

        private bool IsUsingCorrectSpec (Bitmap bmpScreenCapture, System.Drawing.Point p, int specIndex) {
            float x = hp.SpecSourceImageOffsetX;
            float y = hp.SpecSourceImageOffsetY + specIndex * hp.SpecSourceImageSpacingY;

            var rect = new RectangleF((float)p.X + x - 5.0f, (float)p.Y + y - 5.0f, hp.SpecSourceImageWidth + 10.0f, hp.SpecSourceImageHeight + 10.0f);

            bool ret = false;

            if (MainWindow.instance.m_bmpSpecializations[specIndex] != null) {
                var screenCaptureRegion = bmpScreenCapture.Clone(rect, bmpScreenCapture.PixelFormat);
                ret = ImageUtils.SearchBitmap(MainWindow.instance.m_bmpSpecializations[specIndex], screenCaptureRegion, 0.35).Width != 0;
				//screenCaptureRegion.Save("dbg" + specIndex + ".bmp", System.Drawing.Imaging.ImageFormat.Bmp);
                screenCaptureRegion.Dispose();
            }

            return ret;
        }

        private bool IsUsingCorrectTrait (Bitmap bmpScreenCapture, System.Drawing.Point p, int specIndex, int traitIndex, int traitValue) {

            int brightestIndex = -1;
            ulong brightestColor = 0;

            for (int ii = 0; ii < 3; ++ii) {

                double x = hp.TraitOffsetX + traitIndex * hp.TraitSpacingX;
                double y = hp.TraitOffsetY + specIndex * hp.TraitSpacingY;

                y += ii * hp.TraitChoiceSpacing;

                int border = 7;
                var rect = new System.Drawing.Rectangle(p.X + (int)x + border, p.Y + (int)y + border, (int)hp.TraitIconSize - border*2, (int)hp.TraitIconSize-border*2);

                var screenCaptureRegion = bmpScreenCapture.Clone(rect, bmpScreenCapture.PixelFormat);

                ulong brightness = ImageUtils.AverageBrightness(screenCaptureRegion);
                if (brightness > brightestColor) {
                    brightestColor = brightness;
                    brightestIndex = ii;
                }

                //for debugging:
                //screenCaptureRegion.Save(string.Format("TraitIcons/spec{0}-trait{1}-{2}.bmp", specIndex, traitIndex, ii), System.Drawing.Imaging.ImageFormat.Bmp);

                screenCaptureRegion.Dispose();
            }

            return traitValue == brightestIndex;
        }

        private Ellipse CreateCircle () {
            var circle = new Ellipse() {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                StrokeThickness = 2,
                Stroke = System.Windows.Media.Brushes.Red
            };
            return circle;
        }

        private System.Windows.Shapes.Rectangle CreateRectangle () {
            var rect = new System.Windows.Shapes.Rectangle() {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                StrokeThickness = 2,
            };
            return rect;
        }


        private int DebugSaveSourceImages (Bitmap bmpScreenCapture, System.Drawing.Point p) {
            int drawnRectangles = 0;

            for (int ii = 0; ii < 3; ++ii) {
                
                
                float x = hp.SpecSourceImageOffsetX;
                float y = hp.SpecSourceImageOffsetY + ii * hp.SpecSourceImageSpacingY;

                var rect = new RectangleF((float)p.X + x, (float)p.Y + y, hp.SpecSourceImageWidth, hp.SpecSourceImageHeight);

                ///

                /*if (rectangles.Children.Count <= ii)
                    rectangles.Children.Add(CreateRectangle());

                var rectangle = rectangles.Children[ii] as System.Windows.Shapes.Rectangle;
                rectangle.Stroke = System.Windows.Media.Brushes.Blue;
                drawnRectangles++;

                rectangle.Width = hp.SpecSourceImageWidth;
                rectangle.Height = hp.SpecSourceImageHeight;
                
                rectangle.Margin = new Thickness(p.X + x, p.Y + y, 0.0f, 0.0f);
                */
                ///

                var bmp = bmpScreenCapture.Clone(rect, bmpScreenCapture.PixelFormat);
                bmp.Save("out" + ii + ".bmp", System.Drawing.Imaging.ImageFormat.Bmp);
                bmp.Dispose();
            }

            return drawnRectangles;
        }


    }
}
