namespace Microsoft.Samples.Kinect.ColorBasics
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Microsoft.Kinect;
    using System.Collections.Generic;
   
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
   public partial class MainWindow : Window
   {
      bool fijo = false;
      Joint pecho;


      Joint cadera, rodilla;

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



      // Devuelve en vector[0] el ángulo de la pierna y en vector[1] el valor de la coordenada x de la rodilla
      public List<double> movimientoPierna(Skeleton skel, int cad, DrawingContext dc)
      {
         List<double> vector = new List<double>();

         Joint rodilla_actual, cadera_actual;

         if (cad == 0)   // Si cadera derecha
         {
            cadera_actual = skel.Joints[JointType.HipRight];
            rodilla_actual = skel.Joints[JointType.KneeRight];
         }
         else   // Si cadera izquierda
         {
            cadera_actual = skel.Joints[JointType.HipLeft];
            rodilla_actual = skel.Joints[JointType.KneeLeft];
         }

         if (!fijo)
         {
            cadera = cadera_actual;
            rodilla = rodilla_actual;
            fijo = true;
         }

         // QUITAR EN VERSIÓN FINAL
         dc.DrawEllipse(Brushes.Aqua, null, this.SkeletonPointToScreen(cadera_actual.Position), 5, 5);
         dc.DrawEllipse(Brushes.Aqua, null, this.SkeletonPointToScreen(rodilla_actual.Position), 5, 5);

         double angulo, a, b;
         this.valores_base(cadera.Position, rodilla.Position, rodilla_actual.Position, out angulo, out a, out b);
         vector.Add(angulo);
         vector.Add(rodilla_actual.Position.X);

         ang_pierna.Clear();
         ang_pierna.AppendText(angulo.ToString());

         return vector;
      }
   }
}
