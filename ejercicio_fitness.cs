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

      // Calcula el modulo del vector
      public double modulo(SkeletonPoint vector)
      {
          return Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);
      }
      
      // Calcula el producto escalar de los vectores a y b
      public double producto_escalar(SkeletonPoint a, SkeletonPoint b)
      {
          return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
      }

      // Calcula y devuelve por referencia unos valores esenciales para la deteccion de movimientos:
      // angulo entre el vector base_inicial (vector desde punto_base a punto_inicial) y el vector
      // base_actual (vector desde punto_base a punto_actual); diferencia_X como valor absoluto de la
      // diferencia entre la componente X de punto_actual y punto_inicial; diferencia_Z como la
      // diferencia entre la componente Z de punto_actual y punto_inicial.
      public void valores_base(SkeletonPoint punto_base, SkeletonPoint punto_inicial, SkeletonPoint punto_actual,
          out double angulo, out double diferencia_X, out double diferencia_Z)
      {
          SkeletonPoint vector_base_inicial = new SkeletonPoint();
          SkeletonPoint vector_base_actual = new SkeletonPoint();

          vector_base_inicial.X = punto_inicial.X - punto_base.X;
          vector_base_inicial.Y = punto_inicial.Y - punto_base.Y;
          vector_base_inicial.Z = punto_inicial.Z - punto_base.Z;
          vector_base_actual.X = punto_actual.X - punto_base.X;
          vector_base_actual.Y = punto_actual.Y - punto_base.Y;
          vector_base_actual.Z = punto_actual.Z - punto_base.Z;

          angulo = Math.Acos(producto_escalar(vector_base_inicial, vector_base_actual) / 
              (modulo(vector_base_inicial) * modulo(vector_base_actual))) / Math.PI * 180.0;
          diferencia_X = Math.Abs(punto_actual.X - punto_inicial.X);
          diferencia_Z = punto_actual.Z - punto_inicial.Z;
      }
   }
}
