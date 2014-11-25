﻿//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

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
   using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor sensor;

        /// <summary>
        /// Bitmap that will hold color information
        /// </summary>
        private WriteableBitmap colorBitmap;

        /// <summary>
        /// Intermediate storage for the color data received from the camera
        /// </summary>
        private byte[] colorPixels;

        /// <summary>
        /// Width of output drawing
        /// </summary>
        // ****Ancho de dibujo de salida
        private const float RenderWidth = 640.0f;

        /// <summary>
        /// Height of our output drawing
        /// </summary>
        // ****Alto de dibujo de salida
        private const float RenderHeight = 480.0f;

        /// <summary>
        /// Drawing image that we will display
        /// </summary>
        // ****Dibujo de la imagen que se mostrará
        private DrawingImage imageSource;

        /// <summary>
        /// Drawing group for skeleton rendering output
        /// </summary>
        // ****Grupo de dibujo del esqueleto
        private DrawingGroup drawingGroup;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Execute startup tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
//           Image image = new Image();
//           image.Source = (ImageSource)new ImageSourceConverter().ConvertFromString("../../Images/medals.png");
//           medalla.Source = image.Source;

           // Create the drawing group we'll use for drawing
           this.drawingGroup = new DrawingGroup();

           // Create an image source that we can use in our image control
           this.imageSource = new DrawingImage(this.drawingGroup);

            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit (See components in Toolkit Browser).
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (null != this.sensor)
            {
               this.Indicaciones.Source = this.imageSource;

               // Turn on the skeleton stream to receive skeleton frames
               this.sensor.SkeletonStream.Enable();

               // Add an event handler to be called whenever there is new color frame data
               this.sensor.SkeletonFrameReady += this.SensorSkeletonFrameReady;
 
               // Turn on the color stream to receive color frames
                this.sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);

                // Allocate space to put the pixels we'll receive
                this.colorPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];

                // This is the bitmap we'll display on-screen
                this.colorBitmap = new WriteableBitmap(this.sensor.ColorStream.FrameWidth, this.sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);

                // Set the image we display to point to the bitmap where we'll put the image data
                this.Camara.Source = this.colorBitmap;

                // Add an event handler to be called whenever there is new color frame data
                this.sensor.ColorFrameReady += this.SensorColorFrameReady;

                // Start the sensor!
                try
                {
                    this.sensor.Start();
                }
                catch (IOException)
                {
                    this.sensor = null;
                }
            }
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (null != this.sensor)
            {
                this.sensor.Stop();
            }
        }

        /// <summary>
        /// Event handler for Kinect sensor's ColorFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null)
                {
                    // Copy the pixel data from the image to a temporary array
                    colorFrame.CopyPixelDataTo(this.colorPixels);

                    // Write the pixel data into our bitmap
                    this.colorBitmap.WritePixels(
                        new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
                        this.colorPixels,
                        this.colorBitmap.PixelWidth * sizeof(int),
                        0);
                }
            }
        }

        MovimientoPierna mov_pierna = new MovimientoPierna();

        // Tipos de datos necesarios
        enum ESTADO { ESPERA, DETECTADO, MOV_1, MOV_2, COMPLETADO, FAIL, CALIBRAR, INICIO };
        bool movimiento_1 = true;
        const double ANGULO_SINC = 30; // diferencia de angulos en posiciones finales
        MovimientoBrazo mov_brazo_izq = new MovimientoBrazo(JointType.WristLeft, JointType.ShoulderLeft);
        MovimientoBrazo mov_brazo_der = new MovimientoBrazo(JointType.WristRight, JointType.ShoulderRight);
        MovimientoPierna mov_pierna_izq = new MovimientoPierna();
        MovimientoPierna mov_pierna_der = new MovimientoPierna();
        ESTADO state = ESTADO.ESPERA;
        int repeticiones = 10;
        int fallos = 0;
        DateTime t_inicial, t_final;

        /// <summary>
        /// Event handler for Kinect sensor's SkeletonFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        // ****Controlador de eventos del esqueleto
        private void SensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
           Skeleton[] skeletons = new Skeleton[0];

           using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
           {
              if (skeletonFrame != null)
              {
                 skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                 skeletonFrame.CopySkeletonDataTo(skeletons);
              }
           }

           using (DrawingContext dc = this.drawingGroup.Open())
           {
              // Draw a transparent background to set the render size
              dc.DrawRectangle(Brushes.Transparent, null, new Rect(0.0, 0.0, RenderWidth, RenderHeight));

              if (skeletons.Length != 0)
              {
                 foreach (Skeleton skel in skeletons)
                 {
                    if (skel.TrackingState == SkeletonTrackingState.Tracked)
                    {
                       /*
                       mov_pierna_der.updateMovement(skel.Joints[JointType.HipRight], skel.Joints[JointType.KneeRight], skel);
                       sms_block.Text = mov_pierna_der.getMessageError();

                       Indicador barra_pder = new Indicador(15, dc, new WriteableJoint(mov_pierna_der.getInitialHip()), new WriteableJoint(mov_pierna_der.getInitialKnee()),
                                      skel.Joints[JointType.HipRight], skel.Joints[JointType.KneeRight], this);
                       barra_pder.dibujarPuntos();
                       */
                       /*
                        Image image = new Image();
                        image.Source = (ImageSource)new ImageSourceConverter().ConvertFromString("../../Images/Logo.png");
                        medalla.Source = image.Source;
                        */

                       num_rep.Text = repeticiones.ToString();
                       sms_block.Text = mov_pierna_izq.getState().ToString()+ " - ".ToString() + state.ToString() + String.Format("{0:0.0}", mov_brazo_der.getAngulo()) + " - ".ToString()+String.Format("{0:0.0}", mov_pierna_izq.getAngle());

                       if (state == ESTADO.INICIO && Posicion.IsAlignedBodyAndArms(skel) &&
                          (Posicion.AreFeetTogether(skel) || Posicion.AreFeetSeparate(skel)))
                       {
                          state = ESTADO.DETECTADO;
                          mov_brazo_der.reset(); // indica que se va a volver a calibrar porque
                          mov_brazo_izq.reset(); // la posicion inicial puede haber cambiado
                       }
                       else if (state == ESTADO.DETECTADO)
                       {
                          mov_brazo_der.actualizar(skel); // calibrando...
                          mov_brazo_izq.actualizar(skel); // calibrando...

                          if (mov_brazo_der.preparado() && mov_brazo_izq.preparado()) // terminada calibracion?
                          {
                              if (movimiento_1)  // seguimos por el ejercicio que toca
                              {
                                  state = ESTADO.MOV_1;
                                  mov_brazo_der.detectar(); // listo para detectar el movimiento
                              }
                              else
                              {
                                  state = ESTADO.MOV_2;
                                  mov_brazo_izq.detectar(); // listo para detectar el movimiento
                              }
                          }
                       }
                       else if (state == ESTADO.MOV_1)
                       {
                          mov_pierna_izq.updateMovement(skel.Joints[JointType.HipLeft], skel.Joints[JointType.KneeLeft], skel);
                          mov_brazo_der.actualizar(skel);
                          
                          Indicador barra_bder = new Indicador(15, 70, dc, mov_brazo_der.getShoulderPoint(), mov_brazo_der.getWristPoint(),
                                      skel.Joints[JointType.ShoulderRight], skel.Joints[JointType.WristRight], this);
                          barra_bder.dibujarPuntos();

                          Indicador barra_pizq = new Indicador(10, 50, dc, new WriteableJoint(mov_pierna_izq.getInitialHip()), new WriteableJoint(mov_pierna_izq.getInitialKnee()),
                                      skel.Joints[JointType.HipLeft], skel.Joints[JointType.KneeLeft], this);
                          barra_pizq.dibujarPuntos();

                          if ((Math.Abs(mov_brazo_der.getAngulo() - mov_pierna_izq.getAngle()) > ANGULO_SINC) || mov_pierna_izq.getFAIL() || mov_brazo_der.existeError())
                          {
                             state = ESTADO.FAIL;
                          }
                          else if (mov_brazo_der.completado() && mov_pierna_izq.getCOMPLETE()) // cuidado con el angulo de la pierna!
                          {
                             repeticiones--;
                             if (repeticiones == 0)
                                state = ESTADO.COMPLETADO;
                             else
                             {
                                movimiento_1 = false;
                                state = ESTADO.MOV_2;
                                mov_brazo_izq.detectar(); // listo para detectar el movimiento
                             }
                          }
                       }
                       else if (state == ESTADO.MOV_2)
                       {
                          mov_pierna_der.updateMovement(skel.Joints[JointType.HipRight], skel.Joints[JointType.KneeRight], skel);
                          mov_brazo_izq.actualizar(skel);
                          
                          Indicador barra_bizq = new Indicador(15, 70, dc, mov_brazo_izq.getShoulderPoint(), mov_brazo_izq.getWristPoint(),
                                      skel.Joints[JointType.ShoulderLeft], skel.Joints[JointType.WristLeft], this);
                          barra_bizq.dibujarPuntos();

                          Indicador barra_pder = new Indicador(10, 50, dc, new WriteableJoint(mov_pierna_der.getInitialHip()), new WriteableJoint(mov_pierna_der.getInitialKnee()),
                                      skel.Joints[JointType.HipRight], skel.Joints[JointType.KneeRight], this);
                          barra_pder.dibujarPuntos();

                          if ((Math.Abs(mov_brazo_izq.getAngulo() - mov_pierna_der.getAngle()) > ANGULO_SINC) || mov_pierna_der.getFAIL() || mov_brazo_izq.existeError())
                          {
                             state = ESTADO.FAIL;
                          }
                          else if (mov_brazo_izq.completado() && mov_pierna_der.getCOMPLETE()) // cuidado con el angulo de la pierna!
                          {
                             repeticiones--;
                             if (repeticiones == 0)
                                state = ESTADO.COMPLETADO;
                             else
                             {
                                movimiento_1 = true;
                                state = ESTADO.MOV_1;
                                mov_brazo_der.detectar(); // listo para detectar el movimiento
                             }
                          }
                       }
                       else if (state == ESTADO.FAIL)
                       {
                          sms_block.Text = "Coloque el cuerpo en la posición de reposo.";
                          state = ESTADO.INICIO;
                          fallos++;
                          mov_pierna_der.setINITIAL();
                          mov_pierna_izq.setINITIAL();
                       }
                       else if (state == ESTADO.COMPLETADO)
                       {
                           // fallos, total_segundos -> medallas
                           t_final = DateTime.Now;
                           TimeSpan diferencia = t_final - t_inicial;
                           double total_segundos = diferencia.TotalSeconds;
                           info.Text = diferencia.ToString();

                           double eval = (total_segundos / 20) * Math.Sqrt(fallos);

                           if (eval <= 1.0)
                           {
                                Image image = new Image();
                                image.Source = (ImageSource)new ImageSourceConverter().ConvertFromString("../../Images/oro_medal.png");
                                medalla.Source = image.Source;
                               // oro
                           }
                           else if (eval <= 2.0)
                           {
                               Image image = new Image();
                               image.Source = (ImageSource)new ImageSourceConverter().ConvertFromString("../../Images/plata_medal.png");
                               medalla.Source = image.Source;
                               // plata
                           }
                           else if (eval <= 3.0)
                           {
                               Image image = new Image();
                               image.Source = (ImageSource)new ImageSourceConverter().ConvertFromString("../../Images/bronce_medal.png");
                               medalla.Source = image.Source;
                               //bronce
                           }
                           else
                           {
                               sms_block.Text = "¡ Gracias por participar ! (Consejo: debes mejorar)";
                               // premio consolacion
                           }
                       }
                    }
                    else if (skel.TrackingState == SkeletonTrackingState.PositionOnly)
                    {
                       sms_block.Text = "Colóquese un poco más hacia atrás.";
                    }
                 }
              }

              // prevent drawing outside of our render area
              this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, RenderWidth, RenderHeight));
           }
        }

        /// <summary>
        /// Maps a SkeletonPoint to lie within our render space and converts to Point
        /// </summary>
        /// <param name="skelpoint">point to map</param>
        /// <returns>mapped point</returns>
        private Point SkeletonPointToScreen(SkeletonPoint skelpoint)
        {
           // Convert point to depth space.  
           // We are not using depth directly, but we do want the points in our 640x480 output resolution.
           DepthImagePoint depthPoint = this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skelpoint, DepthImageFormat.Resolution640x480Fps30);
           return new Point(depthPoint.X, depthPoint.Y);
        }

        private void bot_inicio_Click(object sender, RoutedEventArgs e)
        {
           state = ESTADO.INICIO;
           t_inicial = DateTime.Now;
           fallos = 0;
           repeticiones = 10;
           mov_pierna_der.setINITIAL();
           mov_pierna_izq.setINITIAL();
           mov_brazo_der.reset();
           mov_brazo_izq.reset();
           movimiento_1 = true;
           
           Image image = new Image();
           image.Source = (ImageSource)new ImageSourceConverter().ConvertFromString("../../Images/3_medals.png");
           medalla.Source = image.Source;
        }
    }
}