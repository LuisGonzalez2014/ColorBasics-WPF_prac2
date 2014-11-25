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
      public class WriteableJoint
      {
         public SkeletonPoint Position;
         public JointType JointType;

         public WriteableJoint(Joint j)
         {
            this.Position = j.Position;
            this.JointType = j.JointType;
         }

         public WriteableJoint(SkeletonPoint sp, JointType jt)
         {
            this.Position = sp;
            this.JointType = jt;
         }
      }

      public class Indicador
      {
         private DrawingContext dc;
         private WriteableJoint A_initial, B_initial;
         private Joint A_actual, B_actual;
         private int num_puntos;
         private double angulo_max;
         private Brush color_1;
         private Brush color_2;
         private Brush color_3;
         MainWindow main_window;

         public Indicador(int num, double angulo_max, DrawingContext drco, WriteableJoint A_ini, WriteableJoint B_ini, Joint A_act, Joint B_act, MainWindow mw)
         {
            this.num_puntos = ((num<3) ? 3 : num);
            this.angulo_max = angulo_max;
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
         public WriteableJoint getAinitial()
         {
            return this.A_initial;
         }

         public WriteableJoint getBinitial()
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
         public void setAinitial(WriteableJoint A_ini)
         {
            this.A_initial = A_ini;
         }

         public void setBinitial(WriteableJoint B_ini)
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
         /// Dibuja en pantalla una barra de indicadores para retroalimentación del movimiento
         /// </summary>
         public void dibujarPuntos()
         {
            float dist = (this.getBinitial().Position.Y - this.getAinitial().Position.Y) * (1 - (float) Math.Cos(this.angulo_max*Math.PI/180)) / this.getNumPuntos();

            SkeletonPoint punto_1 = this.getAactual().Position;
            if (this.getAactual().JointType == JointType.HipRight || this.getAactual().JointType == JointType.ShoulderRight)
               punto_1.X += 0.2f;  // Desplazamiento hacia la derecha
            else
               punto_1.X -= 0.2f;  // Desplazamiento hacia la izquierda

            for (int i = 0; i < this.getNumPuntos(); i++)
            {
               SkeletonPoint punto = punto_1, panterior = punto_1, psiguiente = punto_1;
               punto.Y += dist * i + (this.getBinitial().Position.Y - this.getAinitial().Position.Y) * (float) Math.Cos(this.angulo_max * Math.PI / 180);

               float pos_Y = this.getBactual().Position.Y;

               if (pos_Y < punto.Y)
               {
                  dc.DrawEllipse(color_1, null, this.main_window.SkeletonPointToScreen(punto), 10, 5);
               }
               else
               {
                  panterior.Y = punto.Y - dist;
                  if ((i > 0 && pos_Y >= punto.Y && pos_Y < panterior.Y) || (i == 0 && pos_Y >= punto.Y))
                  {
                     dc.DrawEllipse(color_3, null, this.main_window.SkeletonPointToScreen(punto), 10, 5);
                  }

                  psiguiente.Y = punto.Y + dist;
                  if ((i < this.getNumPuntos() - 1) && (pos_Y >= punto.Y && pos_Y >= psiguiente.Y))
                  {
                     dc.DrawEllipse(color_2, null, this.main_window.SkeletonPointToScreen(psiguiente), 10, 5);
                  }
               }
            }
         }
      }

      public class Posicion
      {
         // boolean method that return true if body is completely aligned and arms are in a relaxed position
         public static bool IsAlignedBodyAndArms(Skeleton received)
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
         public static bool AreFeetTogether(Skeleton received)
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
         public static bool AreFeetSeparate(Skeleton received)
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
      }

      public class MovimientoBrazo : Movimiento
      {
         public enum ESTADO { CALIBRAR, PREPARADO, HACIA_ARRIBA, HACIA_ABAJO, COMPLETADO, ERROR_MARGEN_X, ERROR_MARGEN_Z }

         private JointType wrist_type;
         private JointType shoulder_type;
         private double angulo_objetivo;
         private double angulo;
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
         private double offset_perc;
         private double offset_dim;
         private double offset_angulo;

         public MovimientoBrazo(JointType wrist, JointType shoulder, double angulo = 70.0, double offset_perc = 0.2, int puntos_calibracion = 60)
         {
            this.wrist_type = wrist;
            this.shoulder_type = shoulder;
            this.angulo_objetivo = angulo;
            this.angulo = 0;
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
            this.offset_dim = 0;
            this.offset_angulo = 0;
            this.offset_perc = offset_perc;
         }

         public double getAngulo()
         {
            return this.angulo;
         }

         public WriteableJoint getShoulderPoint()
         {
            WriteableJoint j = new WriteableJoint(this.initial_shoulder, this.shoulder_type);
            return j;
         }

         public WriteableJoint getWristPoint()
         {
            WriteableJoint j = new WriteableJoint(this.initial_wrist, this.wrist_type);
            return j;
         }

         public void reset()
         {
            this.estado = ESTADO.CALIBRAR;
         }

         public void actualizar(Skeleton skel)
         {
            SkeletonPoint wrist = skel.Joints[wrist_type].Position;
            SkeletonPoint shoulder = skel.Joints[shoulder_type].Position;
            double diferencia_X, diferencia_Z;

            if (estado == ESTADO.CALIBRAR)
            // Establece de manera precisa la posicion de muñeca y hombro.
            // Ademas, calcula automaticamente el grado de error en algunas medidas.
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
                  SkeletonPoint wrist_with_error = new SkeletonPoint();
                  SkeletonPoint vector_brazo = new SkeletonPoint();
                  SkeletonPoint wrist_with_Z_offset = new SkeletonPoint();

                  foreach (SkeletonPoint punto in l_puntos_calibracion)
                  {
                     error_medio.X += Math.Abs(punto.X - initial_shoulder.X) / (float)puntos_calibracion;
                     error_medio.Y += Math.Abs(punto.Y - initial_shoulder.Y) / (float)puntos_calibracion;
                     error_medio.Z += Math.Abs(punto.Z - initial_shoulder.Z) / (float)puntos_calibracion;
                  }
                  wrist_with_error.X = initial_wrist.X + error_medio.X;
                  wrist_with_error.Y = initial_wrist.Y + error_medio.Y;
                  wrist_with_error.Z = initial_wrist.Z + error_medio.Z;
                  valores_base(initial_shoulder, initial_wrist, wrist_with_error, out error_medio_angulo,
                      out error_medio_X, out error_medio_Z);
                  vector_brazo.X = initial_wrist.X - initial_shoulder.X;
                  vector_brazo.Y = initial_wrist.Y - initial_shoulder.Y;
                  vector_brazo.Z = initial_wrist.Z - initial_shoulder.Z;
                  offset_dim = offset_perc * modulo(vector_brazo);
                  wrist_with_Z_offset = initial_wrist;
                  wrist_with_Z_offset.Z += (float)offset_dim;
                  valores_base(initial_shoulder, initial_wrist, wrist_with_Z_offset, out offset_angulo,
                      out diferencia_X, out diferencia_Z);

                  estado = ESTADO.PREPARADO;
               }
            }
            else if (estado == ESTADO.HACIA_ARRIBA) // Detecta el movimiento hacia arriba del brazo
            {
               valores_base(initial_shoulder, initial_wrist, wrist, out this.angulo, out diferencia_X, out diferencia_Z);

               if (diferencia_X > (2 * error_medio_X + offset_dim)) // margen entorno a eje X
               {
                  estado = ESTADO.ERROR_MARGEN_X;
               }
               else if (diferencia_Z > (2 * error_medio_Z + offset_dim)) // no retroceder el brazo hacia atras
               {
                  estado = ESTADO.ERROR_MARGEN_Z;
               }
               else if ((angulo_objetivo - error_medio_angulo - offset_angulo / 2) < this.angulo &&
                   this.angulo < (angulo_objetivo + error_medio_angulo + offset_angulo / 2)) // movimiento completado 
               {
                  estado = ESTADO.COMPLETADO;
               }
            }/*
            else if (estado == ESTADO.HACIA_ABAJO)
            {
               valores_base(initial_shoulder, initial_wrist, wrist, out this.angulo, out diferencia_X, out diferencia_Z);

               if (diferencia_X > (2 * error_medio_X + offset_dim))
               {
                  estado = ESTADO.ERROR_MARGEN_X;
               }
               else if (this.angulo >= (angulo_objetivo + error_medio_angulo + offset_angulo / 2))
               {
                  estado = ESTADO.ERROR_MARGEN_Z;
               }
               else if (this.angulo < (error_medio_angulo + offset_angulo / 2))
               {
                  estado = ESTADO.COMPLETADO;
               }
            }*/
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
         // Posibles estados de ejecución del movimiento
         public enum ESTADO { INITIAL, REPOSE, MOVING, COMPLETE, FAIL };

         private const double MIN_ANGULO = 3;        // Ángulo a partir del cual se considera que la pierna entra en movimiento
         private const double MAX_ANGULO = 50;       // Ángulo establecido como tope del movimiento a realizar
         private const double ERROR = 0.05;          // Se admite un error del 5%
         private const double DESPL_PERMITED = 0.04; // Se admite un desplazamiento lateral de la rodilla de hasta 4 cm
         private Joint hip_initial, knee_initial;    // Cadera y rodilla capturadas inicialmente
         private double angle;                       // Ángulo de la pierna en movimiento
         private ESTADO state;                       // Estado de ejecución del movimiento

         /// <summary>
         /// Constructor de la clase
         /// </summary>
         public MovimientoPierna()
         {
            this.state = ESTADO.INITIAL;
         }

         /// <summary>
         /// Devuelve la cadera detectada inicialmente
         /// </summary>
         /// <returns>cadera inicial</returns>
         public Joint getInitialHip()
         {
            return this.hip_initial;
         }

         /// <summary>
         /// Devuelve la rodilla detectada inicialmente
         /// </summary>
         /// <returns>rodilla inicial</returns>
         public Joint getInitialKnee()
         {
            return this.knee_initial;
         }

         /// <summary>
         /// Devuelve el estado de ejecución del movimiento
         /// </summary>
         /// <returns>estado del movimiento</returns>
         public ESTADO getState()
         {
            return this.state;
         }

         /// <summary>
         /// Devuelve el ángulo de la pierna en movimiento
         /// </summary>
         /// <returns>ángulo de  la pierna</returns>
         public double getAngle()
         {
            return this.angle;
         }

         /// <summary>
         /// Devuelve si la ejecución se encuentra en estado INITIAL
         /// </summary>
         /// <returns>initial state</returns>
         public bool getINITIAL()
         {
            return this.getState() == ESTADO.INITIAL;
         }

         /// <summary>
         /// Devuelve si la ejecución se encuentra en estado REPOSE
         /// </summary>
         /// <returns>repose state</returns>
         public bool getREPOSE()
         {
            return this.getState() == ESTADO.REPOSE;
         }

         /// <summary>
         /// Devuelve si la ejecución se encuentra en estado MOVING
         /// </summary>
         /// <returns>moving state</returns>
         public bool getMOVING()
         {
            return this.getState() == ESTADO.MOVING;
         }

         /// <summary>
         /// Devuelve si la ejecución se encuentra en estado COMPLETE
         /// </summary>
         /// <returns>complete state</returns>
         public bool getCOMPLETE()
         {
            return this.getState() == ESTADO.COMPLETE;
         }

         /// <summary>
         /// Devuelve si la ejecución se encuentra en estado FAIL
         /// </summary>
         /// <returns>fail state</returns>
         public bool getFAIL()
         {
            return this.getState() == ESTADO.FAIL;
         }

         /// <summary>
         /// Establece la cadera inicialmente
         /// </summary>
         /// <param name="ini_hip">cadera inicial</param>
         public void setInitialHip(Joint ini_hip)
         {
            this.hip_initial = ini_hip;
         }

         /// <summary>
         /// Establece la rodilla inicialmente
         /// </summary>
         /// <param name="ini_knee">rodilla inicial</param>
         public void setInitialKnee(Joint ini_knee)
         {
            this.knee_initial = ini_knee;
         }

         /// <summary>
         /// Establece el estado de ejecución del movimiento
         /// </summary>
         /// <param name="st">estado de ejecución</param>
         public void setState(ESTADO st)
         {
            this.state = st;
         }

         /// <summary>
         /// Establece el ángulo que forma la pierna en movimiento
         /// </summary>
         /// <param name="alpha">ángulo de la pierna</param>
         public void setAngle(double alpha)
         {
            this.angle = alpha;
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
               this.setAngle(angulo);

               // Si levantando la pierna, la rodilla se desplaza hacia algún lado más de lo
               // permitido, el movimiento es INCORRECTO.
               if (dif_x > (DESPL_PERMITED * ERROR) && this.getMOVING())
               {
                  this.setState(ESTADO.FAIL);
               }
               // Si el ángulo de la pierna supera el ángulo mínimo, se está elevando la rodilla
               else if (MIN_ANGULO < angulo && angulo < MAX_ANGULO && this.getREPOSE())
               {
                  this.setState(ESTADO.MOVING);
               }
               // Si el ángulo de la pierna supera el ángulo MÁXIMO, se ha completado el movimiento
               else if (MAX_ANGULO <= angulo && this.getMOVING())
               {
                  this.setState(ESTADO.COMPLETE);
               }
               // Si el ángulo de la pierna decrementa por debajo del ángulo máximo, se está bajando la rodilla
               else if (MIN_ANGULO < angulo && angulo < MAX_ANGULO && this.getCOMPLETE())
               {
                  this.setState(ESTADO.MOVING);
               }
               // Si el ángulo de la pierna decrementa por debajo del ángulo mínimo, se ha llegado a la posición de reposo
               else if (angulo < MIN_ANGULO && this.getMOVING())
               {
                  this.setState(ESTADO.REPOSE);
               }
               // Si el estado es INITIAL, se capturan la cadera y la rodilla y se pasa al estado REPOSE
               else if (this.getINITIAL())
               {
                  this.setInitialHip(hip);
                  this.setInitialKnee(knee);
                  this.setState(ESTADO.REPOSE);
               }
            }
            else // Si no se trata de la cadera y rodilla de la misma pierna no se ejecuta nada
               return;
         }
      }
   }
}
