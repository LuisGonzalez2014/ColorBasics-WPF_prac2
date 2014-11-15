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
      class Movimiento
      {
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
              this.l_puntos_calibracion = new List<SkeletonPoint> ();
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
                      initial_wrist.X += wrist.X / (float) puntos_calibracion;
                      initial_wrist.Y += wrist.Y / (float) puntos_calibracion;
                      initial_wrist.Z += wrist.Z / (float) puntos_calibracion;
                      initial_shoulder.X += shoulder.X / (float) puntos_calibracion;
                      initial_shoulder.Y += shoulder.Y / (float) puntos_calibracion;
                      initial_shoulder.Z += shoulder.Z / (float) puntos_calibracion;
                      l_puntos_calibracion.Add(shoulder);
                      contador_puntos++;
                  }
                  else
                  {
                      SkeletonPoint prediccion_error = new SkeletonPoint();

                      foreach (SkeletonPoint punto in l_puntos_calibracion)
                      {
                          error_medio.X += Math.Abs(punto.X - initial_shoulder.X) / (float) puntos_calibracion;
                          error_medio.Y += Math.Abs(punto.Y - initial_shoulder.Y) / (float) puntos_calibracion;
                          error_medio.Z += Math.Abs(punto.Z - initial_shoulder.Z) / (float) puntos_calibracion;
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
   }
}
