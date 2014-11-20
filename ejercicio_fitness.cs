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
      public class Indicador
      {
         private DrawingContext dc;
         private Joint A_initial, B_initial, A_actual, B_actual;
         private int num_puntos;
         private Brush color_1;
         private Brush color_2;
         private Brush color_3;
         MainWindow main_window;

         public Indicador(int num, DrawingContext drco, Joint A_ini, Joint B_ini, Joint A_act, Joint B_act, MainWindow mw)
         {
            this.num_puntos = ((num<3) ? 2 : num);
            this.dc = drco;
            this.A_initial = A_ini;
            this.B_initial = B_ini;
            this.A_actual = A_act;
            this.B_actual = B_act;
            this.color_1 = Brushes.LightGray;
            this.color_2 = Brushes.GreenYellow;
            this.color_3 = Brushes.Red;
            this.main_window = mw;
         }

         // Métodos consultores
         public Joint getAinitial()
         {
            return this.A_initial;
         }

         public Joint getBinitial()
         {
            return this.B_initial;
         }

         public Joint getAactual()
         {
            return this.A_actual;
         }

         public Joint getBactual()
         {
            return this.B_actual;
         }

         public int getNumPuntos()
         {
            return this.num_puntos;
         }

         // Métodos modificadores

         public void setAinitial(Joint A_ini)
         {
            this.A_initial = A_ini;
         }

         public void setBinitial(Joint B_ini)
         {
            this.B_initial = B_ini;
         }

         public void setAactual(Joint A_act)
         {
            this.A_actual = A_act;
         }

         public void setBactual(Joint B_act)
         {
            this.B_actual = B_act;
         }

         /// <summary>
         /// Dibuja en pantalla cinco puntos para la realización del movimiento
         /// </summary>
         /// <param name="punto_A">punto 1 detectado</param>
         /// <param name="punto_B">punto 2 detectado</param>
         /// <param name="dc">drawing context</param>
         public void dibujarPuntos()
         {
            SkeletonPoint punto_1 = this.getAactual().Position;
            if (this.getAactual().JointType == JointType.HipRight || this.getAactual().JointType == JointType.ShoulderRight)
               punto_1.X += 0.2f;  // Desplazamiento hacia la derecha
            else
               punto_1.X -= 0.2f;  // Desplazamiento hacia la izquierda

            SkeletonPoint punto_5 = punto_1, punto_2 = punto_1, punto_3 = punto_1, punto_4 = punto_1;

            punto_5.Y = punto_1.Y + (this.getBinitial().Position.Y - this.getAinitial().Position.Y);
            punto_3.Y = (punto_1.Y + punto_5.Y) / 2;
            punto_2.Y = (punto_1.Y + punto_3.Y) / 2;
            punto_4.Y = (punto_3.Y + punto_5.Y) / 2;

            if (this.getBactual().Position.Y <= punto_4.Y)
            {
               dc.DrawEllipse(color_1, null, this.main_window.SkeletonPointToScreen(punto_1), 10, 5);
               dc.DrawEllipse(color_1, null, this.main_window.SkeletonPointToScreen(punto_2), 10, 5);
               dc.DrawEllipse(color_1, null, this.main_window.SkeletonPointToScreen(punto_3), 10, 5);
               dc.DrawEllipse(color_1, null, this.main_window.SkeletonPointToScreen(punto_4), 10, 5);
               dc.DrawEllipse(color_3, null, this.main_window.SkeletonPointToScreen(punto_5), 10, 5);
            }
            else if (this.getBactual().Position.Y < punto_3.Y)
            {
               dc.DrawEllipse(color_1, null, this.main_window.SkeletonPointToScreen(punto_1), 10, 5);
               dc.DrawEllipse(color_1, null, this.main_window.SkeletonPointToScreen(punto_2), 10, 5);
               dc.DrawEllipse(color_1, null, this.main_window.SkeletonPointToScreen(punto_3), 10, 5);
               dc.DrawEllipse(color_3, null, this.main_window.SkeletonPointToScreen(punto_4), 10, 5);
               dc.DrawEllipse(color_2, null, this.main_window.SkeletonPointToScreen(punto_5), 10, 5);
            }
            else if (this.getBactual().Position.Y < punto_2.Y)
            {
               dc.DrawEllipse(color_1, null, this.main_window.SkeletonPointToScreen(punto_1), 10, 5);
               dc.DrawEllipse(color_1, null, this.main_window.SkeletonPointToScreen(punto_2), 10, 5);
               dc.DrawEllipse(color_3, null, this.main_window.SkeletonPointToScreen(punto_3), 10, 5);
               dc.DrawEllipse(color_2, null, this.main_window.SkeletonPointToScreen(punto_4), 10, 5);
               dc.DrawEllipse(color_2, null, this.main_window.SkeletonPointToScreen(punto_5), 10, 5);
            }
            else if (this.getBactual().Position.Y < punto_1.Y)
            {
               dc.DrawEllipse(color_1, null, this.main_window.SkeletonPointToScreen(punto_1), 10, 5);
               dc.DrawEllipse(color_3, null, this.main_window.SkeletonPointToScreen(punto_2), 10, 5);
               dc.DrawEllipse(color_2, null, this.main_window.SkeletonPointToScreen(punto_3), 10, 5);
               dc.DrawEllipse(color_2, null, this.main_window.SkeletonPointToScreen(punto_4), 10, 5);
               dc.DrawEllipse(color_2, null, this.main_window.SkeletonPointToScreen(punto_5), 10, 5);
            }
            else if (this.getBactual().Position.Y >= punto_1.Y)
            {
               dc.DrawEllipse(color_3, null, this.main_window.SkeletonPointToScreen(punto_1), 10, 5);
               dc.DrawEllipse(color_2, null, this.main_window.SkeletonPointToScreen(punto_2), 10, 5);
               dc.DrawEllipse(color_2, null, this.main_window.SkeletonPointToScreen(punto_3), 10, 5);
               dc.DrawEllipse(color_2, null, this.main_window.SkeletonPointToScreen(punto_4), 10, 5);
               dc.DrawEllipse(color_2, null, this.main_window.SkeletonPointToScreen(punto_5), 10, 5);
            }
         }

         public void dibujarPuntos_2()
         {
            float dist = (this.getBinitial().Position.Y - this.getAinitial().Position.Y) / this.getNumPuntos();

            SkeletonPoint punto_1 = this.getAactual().Position;
            if (this.getAactual().JointType == JointType.HipRight || this.getAactual().JointType == JointType.ShoulderRight)
               punto_1.X += 0.2f;  // Desplazamiento hacia la derecha
            else
               punto_1.X -= 0.2f;  // Desplazamiento hacia la izquierda

            List<SkeletonPoint> puntos = new List<SkeletonPoint>();
            for (int i = 0; i < this.getNumPuntos(); i++)
            {
               SkeletonPoint punto = punto_1;
               punto.Y += dist * i;
               puntos.Add(punto);
            }
            
            for (int i = 0; i < this.getNumPuntos(); i++)
            {
               float pos_Y = this.getBactual().Position.Y;

               if (pos_Y < puntos[i].Y)
               {
                  dc.DrawEllipse(color_1, null, this.main_window.SkeletonPointToScreen(puntos[i]), 10, 5);
               }
               else
               {
                  if ((i > 0 && pos_Y >= puntos[i].Y && pos_Y < puntos[i - 1].Y) || (i == 0 && pos_Y >= puntos[i].Y))
                  {
                     dc.DrawEllipse(color_3, null, this.main_window.SkeletonPointToScreen(puntos[i]), 10, 5);
                  }

                  if (i < this.getNumPuntos()-1)
                  {
                     if (pos_Y >= puntos[i].Y && pos_Y >= puntos[i + 1].Y)
                     {
                        dc.DrawEllipse(color_2, null, this.main_window.SkeletonPointToScreen(puntos[i + 1]), 10, 5);
                     }
                  }
               }
            }
         }
      }

      public class Movimiento
      {
         /// <summary>
         /// Calcula el modulo del vector
         /// </summary>
         /// <param name="vector">vector</param>
         /// <returns>módulo del vector</returns>
         public double modulo(SkeletonPoint vector)
         {   return Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);   }

         /// <summary>
         /// Calcula el producto escalar de los vectores a y b
         /// </summary>
         /// <param name="a">punto en el espacio</param>
         /// <param name="b">punto en el espacio</param>
         /// <returns>producto escalar</returns>
         public double producto_escalar(SkeletonPoint a, SkeletonPoint b)
         {   return a.X * b.X + a.Y * b.Y + a.Z * b.Z;   }

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

         // boolean method that return true if body is completely aligned and arms are in a relaxed position
         public bool IsAlignedBodyAndArms(Skeleton received)
         {
            double HipCenterPosX = received.Joints[JointType.HipCenter].Position.X;
            double HipCenterPosY = received.Joints[JointType.HipCenter].Position.Y;
            double HipCenterPosZ = received.Joints[JointType.HipCenter].Position.Z;

            double ShoulCenterPosX = received.Joints[JointType.ShoulderCenter].Position.X;
            double ShoulCenterPosY = received.Joints[JointType.ShoulderCenter].Position.Y;
            double ShoulCenterPosZ = received.Joints[JointType.ShoulderCenter].Position.Z;

            double HeadCenterPosX = received.Joints[JointType.Head].Position.X;
            double HeadCenterPosY = received.Joints[JointType.Head].Position.Y;
            double HeadCenterPosZ = received.Joints[JointType.Head].Position.Z;

            double ElbLPosX = received.Joints[JointType.ElbowLeft].Position.X;
            double ElbLPosY = received.Joints[JointType.ElbowLeft].Position.Y;

            double ElbRPosX = received.Joints[JointType.ElbowRight].Position.X;
            double ElbRPosY = received.Joints[JointType.ElbowRight].Position.Y;

            double WriLPosX = received.Joints[JointType.WristLeft].Position.X;
            double WriLPosY = received.Joints[JointType.WristLeft].Position.Y;
            double WriLPosZ = received.Joints[JointType.WristLeft].Position.Z;

            double WriRPosX = received.Joints[JointType.WristRight].Position.X;
            double WriRPosY = received.Joints[JointType.WristRight].Position.Y;
            double WriRPosZ = received.Joints[JointType.WristRight].Position.Z;

            double ShouLPosX = received.Joints[JointType.ShoulderLeft].Position.X;
            double ShouLPosY = received.Joints[JointType.ShoulderLeft].Position.Y;
            double ShouLPosZ = received.Joints[JointType.ShoulderLeft].Position.Z;

            double ShouRPosX = received.Joints[JointType.ShoulderRight].Position.X;
            double ShouRPosY = received.Joints[JointType.ShoulderRight].Position.Y;
            double ShouRPosZ = received.Joints[JointType.ShoulderRight].Position.Z;

            //have to change to correspond to the 5% error
            //distance from Shoulder to Wrist for the projection in line with shoulder
            double distShouLtoWristL = ShouLPosY - WriLPosY;
            //caldulate admited error 5% that correspond to 9 degrees for each side
            double radian = (9 * Math.PI) / 180;
            double DistErrorL = distShouLtoWristL * Math.Tan(radian);

            double distShouLtoWristR = ShouRPosY - WriRPosY;
            //caldulate admited error 5% that correspond to 9 degrees for each side

            double DistErrorR = distShouLtoWristR * Math.Tan(radian);
            //double ProjectionWristX = ShouLPosX;
            //double ProjectionWristZ = WriLPosZ;

            //determine of projected point from shoulder to wrist LEFT and RIGHT and then assume error
            double ProjectedPointWristLX = ShouLPosX;
            double ProjectedPointWristLY = WriLPosY;
            double ProjectedPointWristLZ = ShouLPosZ;

            double ProjectedPointWristRX = ShouRPosX;
            double ProjectedPointWristRY = WriRPosY;
            double ProjectedPointWristRZ = ShouRPosZ;

            //Create method to verify if the center of the body is completely aligned
            //head with shoulder center and with hip center
            if (Math.Abs(HeadCenterPosX - ShoulCenterPosX) <= 0.05 && Math.Abs(ShoulCenterPosX - HipCenterPosX) <= 0.05)
            {
               //if position of left wrist is between [ProjectedPointWrist-DistError,ProjectedPointWrist+DistError]
               if (Math.Abs(WriLPosX - ProjectedPointWristLX) <= DistErrorL && Math.Abs(WriRPosX - ProjectedPointWristRX) <= DistErrorR)
               {
                  return true;
               }
               else
                  return false;
            }
            else
               return false;
         }
         
         //first position to be Tracked and Accepted
         public bool AreFeetTogether(Skeleton received)
         {
            foreach (Joint joint in received.Joints)
            {
               if (joint.TrackingState == JointTrackingState.Tracked)
               {//first verify if the body is alignet and arms are in a relaxed position

                  //{here verify if the feet are together
                  //use the same strategy that was used in the previous case of the arms in a  relaxed position
                  double HipCenterPosX = received.Joints[JointType.HipCenter].Position.X;
                  double HipCenterPosY = received.Joints[JointType.HipCenter].Position.Y;
                  double HipCenterPosZ = received.Joints[JointType.HipCenter].Position.Z;

                  //if left ankle is very close to right ankle then verify the rest of the skeleton points
                  //if (received.Joints[JointType.AnkleLeft].Equals(received.Joints[JointType.AnkleRight])) 
                  double AnkLPosX = received.Joints[JointType.AnkleLeft].Position.X;
                  double AnkLPosY = received.Joints[JointType.AnkleLeft].Position.Y;
                  double AnkLPosZ = received.Joints[JointType.AnkleLeft].Position.Z;

                  double AnkRPosX = received.Joints[JointType.AnkleRight].Position.X;
                  double AnkRPosY = received.Joints[JointType.AnkleRight].Position.Y;
                  double AnkRPosZ = received.Joints[JointType.AnkleRight].Position.Z;
                  //assume that the distance Y between HipCenter to each foot is the same
                  double distHiptoAnkleL = HipCenterPosY - AnkLPosY;
                  //caldulate admited error 5% that correspond to 9 degrees for each side
                  double radian1 = (4.5 * Math.PI) / 180;
                  double DistErrorL = distHiptoAnkleL * Math.Tan(radian1);
                  //determine of projected point from HIP CENTER to LEFT ANKLE and RIGHT and then assume error
                  double ProjectedPointFootLX = HipCenterPosX;
                  double ProjectedPointFootLY = AnkLPosY;
                  double ProjectedPointFootLZ = HipCenterPosZ;

                  // could variate AnkLposX and AnkLPosY
                  if (Math.Abs(AnkLPosX - ProjectedPointFootLX) <= DistErrorL && Math.Abs(AnkRPosX - ProjectedPointFootLX) <= DistErrorL)
                     return true;
                  else
                     return false;
               }//CLOSE if (joint.TrackingState == JointTrackingState.Tracked)
               else
                  return false;
            }//close foreach
            return false;
         }//close method AreFeetTogether
         
         //method for the second position feet separate between 60 degrees to be accepted
         public bool AreFeetSeparate(Skeleton received)
         {
            foreach (Joint joint in received.Joints)
            {
               if (joint.TrackingState == JointTrackingState.Tracked)
               {//first verify if the body is alignet and arms are in a relaxed position
                  //{//here verify if the feet are together
                  //use the same strategy that was used in the previous case of the arms in a  relaxed position
                  double HipCenterPosX = received.Joints[JointType.HipCenter].Position.X;
                  double HipCenterPosY = received.Joints[JointType.HipCenter].Position.Y;
                  double HipCenterPosZ = received.Joints[JointType.HipCenter].Position.Z;

                  //if left ankle is very close to right ankle then verify the rest of the skeleton points
                  //if (received.Joints[JointType.AnkleLeft].Equals(received.Joints[JointType.AnkleRight])) 
                  double AnkLPosX = received.Joints[JointType.AnkleLeft].Position.X;
                  double AnkLPosY = received.Joints[JointType.AnkleLeft].Position.Y;
                  double AnkLPosZ = received.Joints[JointType.AnkleLeft].Position.Z;

                  double AnkRPosX = received.Joints[JointType.AnkleRight].Position.X;
                  double AnkRPosY = received.Joints[JointType.AnkleRight].Position.Y;
                  double AnkRPosZ = received.Joints[JointType.AnkleRight].Position.Z;
                  //assume that the distance Y between HipCenter to each foot is the same
                  double distHiptoAnkleL = HipCenterPosY - AnkLPosY;
                  //caldulate admited error 5% that correspond to 9 degrees for each side
                  double radian1 = (4.5 * Math.PI) / 180;
                  double DistErrorL = distHiptoAnkleL * Math.Tan(radian1);
                  //determine of projected point from HIP CENTER to LEFT ANKLE and RIGHT and then assume error
                  double ProjectedPointFootLX = HipCenterPosX;
                  double ProjectedPointFootLY = AnkLPosY;
                  double ProjectedPointFootLZ = HipCenterPosZ;

                  double radian2 = (30 * Math.PI) / 180;
                  double DistSeparateFoot = distHiptoAnkleL * Math.Tan(radian2);
                  //DrawingVisual MyDrawingVisual = new DrawingVisual();

                  // could variate AnkLposX and AnkLPosY
                  if (Math.Abs(AnkRPosX - AnkLPosX) <= Math.Abs(DistSeparateFoot + DistErrorL) && Math.Abs(AnkRPosX - AnkLPosX) >= Math.Abs((DistSeparateFoot) - DistErrorL))
                     return true;
                  else
                     return false;
               }//CLOSE if (joint.TrackingState == JointTrackingState.Tracked)
               else
                  return false;
            }//close foreach
            return false;
         }//close method AreFeetseparate
      }

      public class MovimientoBrazo : Movimiento
      {
         public enum ESTADO { CALIBRAR, PREPARADO, HACIA_ARRIBA, HACIA_ABAJO, COMPLETADO, ERROR_MARGEN_X, ERROR_MARGEN_Z }

         private JointType wrist_type;
         private JointType shoulder_type;
         private ESTADO estado;

         private int contador_puntos;
         private int puntos_calibracion;
         private List<SkeletonPoint> l_puntos_calibracion;

         private SkeletonPoint initial_wrist;
         private SkeletonPoint initial_shoulder;

         private SkeletonPoint error_medio;
         private double error_medio_angulo;
         private double error_medio_X;
         private double error_medio_Z;

         public MovimientoBrazo(JointType wrist, JointType shoulder, int puntos_calibracion = 25)
         {
            this.wrist_type = wrist;
            this.shoulder_type = shoulder;
            this.estado = ESTADO.CALIBRAR;
            this.contador_puntos = 0;
            this.puntos_calibracion = puntos_calibracion;
            this.l_puntos_calibracion = new List<SkeletonPoint>();
            this.initial_wrist = new SkeletonPoint();
            this.initial_wrist.X = this.initial_wrist.Y = this.initial_wrist.Z = 0;
            this.initial_shoulder = new SkeletonPoint();
            this.initial_shoulder.X = this.initial_shoulder.Y = this.initial_shoulder.Z = 0;
            this.error_medio = new SkeletonPoint();
            this.error_medio.X = this.error_medio.Y = this.error_medio.Z = 0;
            this.error_medio_angulo = 0;
            this.error_medio_X = 0;
            this.error_medio_Z = 0;
         }

         public void actualizar(Skeleton skel)
         {
            SkeletonPoint wrist = skel.Joints[wrist_type].Position;
            SkeletonPoint shoulder = skel.Joints[shoulder_type].Position;
            double angulo, diferencia_X, diferencia_Z;

            if (estado == ESTADO.CALIBRAR)
            {
               if (contador_puntos < puntos_calibracion)
               {
                  initial_wrist.X += wrist.X / (float)puntos_calibracion;
                  initial_wrist.Y += wrist.Y / (float)puntos_calibracion;
                  initial_wrist.Z += wrist.Z / (float)puntos_calibracion;
                  initial_shoulder.X += shoulder.X / (float)puntos_calibracion;
                  initial_shoulder.Y += shoulder.Y / (float)puntos_calibracion;
                  initial_shoulder.Z += shoulder.Z / (float)puntos_calibracion;
                  l_puntos_calibracion.Add(shoulder);
                  contador_puntos++;
               }
               else
               {
                  SkeletonPoint prediccion_error = new SkeletonPoint();

                  foreach (SkeletonPoint punto in l_puntos_calibracion)
                  {
                     error_medio.X += Math.Abs(punto.X - initial_shoulder.X) / (float)puntos_calibracion;
                     error_medio.Y += Math.Abs(punto.Y - initial_shoulder.Y) / (float)puntos_calibracion;
                     error_medio.Z += Math.Abs(punto.Z - initial_shoulder.Z) / (float)puntos_calibracion;
                  }
                  prediccion_error.X = initial_wrist.X + error_medio.X;
                  prediccion_error.Y = initial_wrist.Y + error_medio.Y;
                  prediccion_error.Z = initial_wrist.Z + error_medio.Z;
                  valores_base(initial_shoulder, initial_wrist, prediccion_error, out error_medio_angulo,
                      out error_medio_X, out error_medio_Z);
                  estado = ESTADO.PREPARADO;
               }
            }
            else if (estado == ESTADO.HACIA_ARRIBA)
            {
               valores_base(initial_shoulder, initial_wrist, wrist, out angulo, out diferencia_X, out diferencia_Z);

               if (diferencia_X > (2 * error_medio_X))
               {
                  estado = ESTADO.ERROR_MARGEN_X;
               }
               else if (diferencia_Z < (2 * error_medio_Z))
               {
                  estado = ESTADO.ERROR_MARGEN_Z;
               }
               else if (angulo > (70.0 - error_medio_angulo) && angulo < (70.0 + error_medio_angulo))
               {
                  estado = ESTADO.HACIA_ABAJO;
               }
            }
            else if (estado == ESTADO.HACIA_ABAJO)
            {
               valores_base(initial_shoulder, initial_wrist, wrist, out angulo, out diferencia_X, out diferencia_Z);

               if (diferencia_X > (2 * error_medio_X))
               {
                  estado = ESTADO.ERROR_MARGEN_X;
               }
               else if (angulo >= (70.0 + error_medio_angulo))
               {
                  estado = ESTADO.ERROR_MARGEN_Z;
               }
               else if (angulo > (0.0 - error_medio_angulo) && angulo < (0.0 + error_medio_angulo))
               {
                  estado = ESTADO.COMPLETADO;
               }
            }
         }

         public ESTADO getEstado()
         {
            return estado;
         }

         public bool preparado()
         {
            return estado == ESTADO.PREPARADO;
         }

         public bool completado()
         {
            return estado == ESTADO.COMPLETADO;
         }

         public void detectar()
         {
            if (estado == ESTADO.PREPARADO || estado == ESTADO.COMPLETADO)
            {
               estado = ESTADO.HACIA_ARRIBA;
            }
         }
      }

      public class MovimientoPierna : Movimiento
      {
         public enum ESTADO { REPOSO, MOVIMIENTO, ALCANZADO, FAIL, INICIAL };

         private Joint hip_initial, knee_initial;
         private const double MIN_ANGULO = 5;
         private const double MAX_ANGULO = 70;
         private const double ERROR = 0.05;          // Se admite un error del 5%
         private const double DESPL_PERMITED = 4;    // Se admite un desplazamiento lateral de la rodilla de hasta 4 cm.
         private int repetitions;
         private String message_error;
         private ESTADO state;

         public MovimientoPierna()
         {
            this.state = ESTADO.INICIAL;
            this.repetitions = 0;
            message_error = "Realice el movimiento como se le ha indicado.";
         }

         // Métodos consultores
         public Joint getInitialHip()
         {
            return this.hip_initial;
         }

         public Joint getInitialKnee()
         {
            return this.knee_initial;
         }

         public int getRepetitions()
         {
            return repetitions;
         }

         public ESTADO getState()
         {
            return this.state;
         }

         public String getMessageError()
         {
            return this.message_error;
         }

         // Métodos modificadores
         public void setInitialHip(Joint ini_hip)
         {
            this.hip_initial = ini_hip;
         }

         public void setInitialKnee(Joint ini_knee)
         {
            this.knee_initial = ini_knee;
         }

         public void setRepetitions(int rep)
         {
            this.repetitions = rep;
         }

         public void setState(ESTADO st)
         {
            this.state = st;
         }

         public void setMessageError(String sms)
         {
            this.message_error = sms;
         }

         /// <summary>
         /// Actualiza las componentes del movimiento de la pierna
         /// </summary>
         /// <param name="hip">punto de la cadera en el frame actual</param>
         /// <param name="knee">punto de la rodilla en el frame actual</param>
         /// <param name="skel">skeleton capturado por el sensor</param>
         public void updateMovement(Joint hip, Joint knee, Skeleton skel)
         {
            // Comprobamos que se trate de la cadera y rodilla de la misma pierna
            if (hip.JointType == JointType.HipRight && knee.JointType == JointType.KneeRight
               || hip.JointType == JointType.HipLeft && knee.JointType == JointType.KneeLeft)
            {
               double angulo, dif_x, b;
               this.valores_base(this.getInitialHip().Position, this.getInitialKnee().Position, knee.Position, out angulo, out dif_x, out b);

               double ang_err_max = angulo + (angulo*ERROR);
               double ang_err_min = angulo - (angulo*ERROR);

               if (this.getState() == ESTADO.MOVIMIENTO && dif_x >= (DESPL_PERMITED*ERROR))
               {
                  this.setState(ESTADO.FAIL);
               }
               else if (MIN_ANGULO < ang_err_min && ang_err_max < MAX_ANGULO && this.getState() == ESTADO.REPOSO)
               {
                  this.setState(ESTADO.MOVIMIENTO);
               }
               else if (MAX_ANGULO <= ang_err_max && this.getState() == ESTADO.MOVIMIENTO)
               {
                  this.setState(ESTADO.ALCANZADO);
                  this.setRepetitions(this.getRepetitions() +1);
               }
               else if (MIN_ANGULO < ang_err_min && ang_err_max < MAX_ANGULO && this.getState() == ESTADO.ALCANZADO)
               {
                  this.setState(ESTADO.MOVIMIENTO);
               }
               else if (ang_err_min <= MIN_ANGULO && this.getState() == ESTADO.MOVIMIENTO)
               {
                  this.setState(ESTADO.REPOSO);
               }
               else if (this.getState() == ESTADO.FAIL)
               {
                  if (this.IsAlignedBodyAndArms(skel) && (this.AreFeetTogether(skel) || this.AreFeetSeparate(skel)))
                  {
                     this.setState(ESTADO.INICIAL);
                  }
                  else
                     this.setMessageError("Coloque el cuerpo en la posición de reposo.");
               }
               else if (this.getState() == ESTADO.INICIAL)
               {
                  this.setInitialHip(hip);
                  this.setInitialKnee(knee);
                  this.setState(ESTADO.REPOSO);
               }
            }
            else // Si no se trata de la cadera y rodilla de la misma pierna no se ejecuta nada
               return;
         }
      }
   }
}
