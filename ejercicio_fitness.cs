namespace Microsoft.Samples.Kinect.ColorBasics
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Microsoft.Kinect;
   
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
   public partial class MainWindow : Window
   {
      bool fijo = false;
      Joint pecho;
      public void prueba(Skeleton skel, DrawingContext dc)
      {
         if (!fijo)
         {
            pecho = skel.Joints[JointType.ShoulderCenter];
            fijo = true;
         }
         dc.DrawEllipse(Brushes.Aqua, null, this.SkeletonPointToScreen(pecho.Position), 5, 5);
         // prevent drawing outside of our render area
         this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, RenderWidth, RenderHeight));
      }
   }
}
