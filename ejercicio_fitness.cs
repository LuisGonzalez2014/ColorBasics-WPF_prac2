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

      // Devuelve en vector[0] el ángulo de la pierna y en vector[1] el valor de la coordenada x de la rodilla
      public List<double> movimientoPierna(Skeleton skel, int cad) {
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

         double cad_X = cadera_actual.Position.X;
         double rod_X = rodilla_actual.Position.X;
         double cad_Y = cadera_actual.Position.Y;
         double rod_Y = rodilla_actual.Position.Y;
         double cad_Z = cadera_actual.Position.Z;
         double rod_Z = rodilla_actual.Position.Z;
         
         double aX, aY, aZ, bX, bY, bZ;

         // Vector de la cadera a la rodilla en las posiciones iniciales
         // SÓLO SE MODIFICA LA PRIMERA VEZ.          ********* CAMBIAR **********
         aX = rodilla.Position.X - cadera.Position.X;
         aY = rodilla.Position.Y - cadera.Position.Y;
         aZ = rodilla.Position.Z - cadera.Position.Z;

         // Vector de la cadera a la rodilla en las posiciones actuales
         bX = rod_X - cad_X;
         bY = rod_Y - cad_Y;
         bZ = rod_Z - cad_Z;

         double a_mod = Math.Sqrt(Math.Abs(Math.Pow(aX,2)+Math.Pow(aY,2)+Math.Pow(aZ,2)));
         double b_mod = Math.Sqrt(Math.Abs(Math.Pow(bX, 2) + Math.Pow(bY, 2) + Math.Pow(bZ, 2)));

         double angulo = Math.Acos(((aX*bX)+(aY*bY)+(aZ*bZ))/(a_mod+b_mod));

         vector[0] = angulo;
         vector[1] = rod_X;

         ang_pierna.Clear();
         ang_pierna.AppendText(angulo.ToString());

         return vector;
      }
   }
}
