//---------------------------------------------------------------------------------
// <copyright file="ejercicio_fitness.cs"
//      autor="Luis Alejandro González Borrás y José Manuel Gómez González">
// </copyright>
//---------------------------------------------------------------------------------

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
      /// <summary>
      /// Calcula el modulo del vector
      /// </summary>
      /// <param name="vector">vector</param>
      /// <returns>módulo del vector</returns>
      public double modulo(SkeletonPoint vector)
      {
         return Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);
      }

      /// <summary>
      /// Calcula el producto escalar de los vectores a y b
      /// </summary>
      /// <param name="a">punto en el espacio</param>
      /// <param name="b">punto en el espacio</param>
      /// <returns>producto escalar</returns>
      public double producto_escalar(SkeletonPoint a, SkeletonPoint b)
      {
         return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
      }

      /// <summary>
      /// Calcula y devuelve por referencia unos valores esenciales para la deteccion de movimientos
      /// </summary>
      /// <param name="punto_base">punto base</param>
      /// <param name="punto_inicial">punto inicial</param>
      /// <param name="punto_actual">punto actual</param>
      /// <param name="angulo">angulo entre el vector base_inicial (vector desde punto_base a punto_inicial) y el vector 
      /// base_actual (vector desde punto_base a punto_actual)</param>
      /// <param name="diferencia_X">diferencia_X como valor absoluto de la diferencia entre la componente X de punto_actual
      /// y punto_inicial</param>
      /// <param name="diferencia_Z">diferencia_Z como la diferencia entre la componente Z de punto_actual y punto_inicial</param>
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


      bool capturada = false;
      Joint cadera, rodilla;

      /// <summary>
      /// Devuelve el ángulo que forma la pierna al elevar la rodilla y el desplazamiento de ésta
      /// en el eje X
      /// </summary>
      /// <param name="cadera_actual">punto de la cadera en el frame actual</param>
      /// <param name="rodilla_actual">punto de la rodilla en el frame actual</param>
      /// <param name="dc">drawing context</param>
      /// <returns>vector con dos elementos: [0]: ángulo, [1]: desplazamiento en X de la rodilla</returns>
      public List<double> movimientoPierna(Joint cadera_actual, Joint rodilla_actual, DrawingContext dc)
      {
         List<double> vector = new List<double>();

         if (!capturada)
         {
            cadera = cadera_actual;
            rodilla = rodilla_actual;
            capturada = true;
         }

         double angulo, a, b;
         this.valores_base(cadera.Position, rodilla.Position, rodilla_actual.Position, out angulo, out a, out b);
         vector.Add(angulo);
         vector.Add(rodilla_actual.Position.X);

         this.dibujarPuntos(cadera_actual, rodilla_actual, dc);

         return vector;
      }

      /// <summary>
      /// Dibuja en pantalla cinco puntos para la realización del movimiento
      /// </summary>
      /// <param name="punto_A">punto 1 detectado</param>
      /// <param name="punto_B">punto 2 detectado</param>
      /// <param name="dc">drawing context</param>
      public void dibujarPuntos(Joint punto_A, Joint punto_B, DrawingContext dc)
      {
         // Paleta de colores para la retroalimentación del usuario
         Brush color_1 = Brushes.LightGray;
         Brush color_2 = Brushes.GreenYellow;
         Brush color_3 = Brushes.Red;

         SkeletonPoint punto_1 = punto_A.Position;
         if (punto_A.JointType == JointType.HipRight || punto_A.JointType == JointType.ShoulderRight)
            punto_1.X += 0.2f;  // Desplazamiento hacia la derecha de la cadera
         else
            punto_1.X -= 0.2f;  // Desplazamiento hacia la izquierda de la cadera

         SkeletonPoint punto_5 = punto_1, punto_2 = punto_1, punto_3 = punto_1, punto_4 = punto_1;

         if (punto_A.JointType == JointType.HipRight || punto_A.JointType == JointType.HipLeft)
         {
            punto_5.Y = punto_1.Y + (rodilla.Position.Y - cadera.Position.Y);
         }
/*         else if (punto_A.JointType == JointType.ShoulderRight || punto_A.JointType == JointType.ShoulderLeft)
         {
            punto_5.Y = punto_1.Y + (muñeca_inicial.Position.Y - hombro_inicial.Position.Y);
         }
*/
         punto_3.Y = (punto_1.Y + punto_5.Y) / 2;
         punto_2.Y = (punto_1.Y + punto_3.Y) / 2;
         punto_4.Y = (punto_3.Y + punto_5.Y) / 2;

         if (punto_B.Position.Y <= punto_4.Y)
         {
            dc.DrawEllipse(color_1, null, this.SkeletonPointToScreen(punto_1), 10, 5);
            dc.DrawEllipse(color_1, null, this.SkeletonPointToScreen(punto_2), 10, 5);
            dc.DrawEllipse(color_1, null, this.SkeletonPointToScreen(punto_3), 10, 5);
            dc.DrawEllipse(color_1, null, this.SkeletonPointToScreen(punto_4), 10, 5);
            dc.DrawEllipse(color_3, null, this.SkeletonPointToScreen(punto_5), 10, 5);
         }
         else if (punto_B.Position.Y < punto_3.Y)
         {
            dc.DrawEllipse(color_1, null, this.SkeletonPointToScreen(punto_1), 10, 5);
            dc.DrawEllipse(color_1, null, this.SkeletonPointToScreen(punto_2), 10, 5);
            dc.DrawEllipse(color_1, null, this.SkeletonPointToScreen(punto_3), 10, 5);
            dc.DrawEllipse(color_3, null, this.SkeletonPointToScreen(punto_4), 10, 5);
            dc.DrawEllipse(color_2, null, this.SkeletonPointToScreen(punto_5), 10, 5);
         }
         else if (punto_B.Position.Y < punto_2.Y)
         {
            dc.DrawEllipse(color_1, null, this.SkeletonPointToScreen(punto_1), 10, 5);
            dc.DrawEllipse(color_1, null, this.SkeletonPointToScreen(punto_2), 10, 5);
            dc.DrawEllipse(color_3, null, this.SkeletonPointToScreen(punto_3), 10, 5);
            dc.DrawEllipse(color_2, null, this.SkeletonPointToScreen(punto_4), 10, 5);
            dc.DrawEllipse(color_2, null, this.SkeletonPointToScreen(punto_5), 10, 5);
         }
         else if (punto_B.Position.Y < punto_1.Y)
         {
            dc.DrawEllipse(color_1, null, this.SkeletonPointToScreen(punto_1), 10, 5);
            dc.DrawEllipse(color_3, null, this.SkeletonPointToScreen(punto_2), 10, 5);
            dc.DrawEllipse(color_2, null, this.SkeletonPointToScreen(punto_3), 10, 5);
            dc.DrawEllipse(color_2, null, this.SkeletonPointToScreen(punto_4), 10, 5);
            dc.DrawEllipse(color_2, null, this.SkeletonPointToScreen(punto_5), 10, 5);
         }
         else if (punto_B.Position.Y >= punto_1.Y)
         {
            dc.DrawEllipse(color_3, null, this.SkeletonPointToScreen(punto_1), 10, 5);
            dc.DrawEllipse(color_2, null, this.SkeletonPointToScreen(punto_2), 10, 5);
            dc.DrawEllipse(color_2, null, this.SkeletonPointToScreen(punto_3), 10, 5);
            dc.DrawEllipse(color_2, null, this.SkeletonPointToScreen(punto_4), 10, 5);
            dc.DrawEllipse(color_2, null, this.SkeletonPointToScreen(punto_5), 10, 5);
         }
      }
   }
}
