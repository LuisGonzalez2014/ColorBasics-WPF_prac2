//------------------------------------------------------------------------------
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

        // Tipos de datos necesarios
        public enum ESTADO { DETECTADO, MOV_1, MOV_2, COMPLETADO, FAIL, CALIBRAR, INICIO };
        bool movimiento_1 = true;
        const double ANGULO_SINC = 20;
        MovimientoBrazo mov_brazo_izq = new MovimientoBrazo(JointType.WristLeft, JointType.ShoulderLeft);
        MovimientoBrazo mov_brazo_der = new MovimientoBrazo(JointType.WristRight, JointType.ShoulderRight);
        MovimientoPierna mov_pierna_izq = new MovimientoPierna();
        MovimientoPierna mov_pierna_der = new MovimientoPierna();
        ESTADO state = ESTADO.INICIO;
        int repetetitions = 10;

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
                       //mov.actualizar(skel);
                       //mov.detectar();

                       mov_pierna_der.updateMovement(skel.Joints[JointType.HipRight], skel.Joints[JointType.KneeRight], skel);
                       //sms_block.Text = mov_pierna.getMessageError();

                       Indicador barra_pder = new Indicador(15, dc, new WriteableJoint(mov_pierna_izq.getInitialHip()), new WriteableJoint(mov_pierna_izq.getInitialKnee()),
                                      skel.Joints[JointType.HipRight], skel.Joints[JointType.KneeRight], this);
                       barra_pder.dibujarPuntos();

                       num_rep.Text = repetetitions.ToString();
                       /*
                       Indicador barra_pder, barra_pizq, barra_bder, barra_bizq;

                       if (state == ESTADO.INICIO && Posicion.IsAlignedBodyAndArms(skel) &&
                          (Posicion.AreFeetTogether(skel) || Posicion.AreFeetSeparate(skel)))
                       {
                          state = ESTADO.DETECTADO;
                       }
                       else if (state == ESTADO.DETECTADO)
                       {
                          mov_brazo_der.reset();
                          mov_brazo_izq.reset();

                          while (!(mov_brazo_der.preparado() && mov_brazo_izq.preparado()))
                          {
                             mov_brazo_der.actualizar(skel);
                             mov_brazo_izq.actualizar(skel);
                          }
                          if (movimiento_1)
                             state = ESTADO.MOV_1;
                          else
                             state = ESTADO.MOV_2;
                       }
                       else if (state == ESTADO.MOV_1)
                       {
                          mov_pierna_izq.updateMovement(skel.Joints[JointType.HipLeft], skel.Joints[JointType.KneeLeft], skel);
                          mov_brazo_der.actualizar(skel);

                          barra_bder = new Indicador(15, dc, mov_brazo_der.getShoulderPoint(), mov_brazo_der.getWristPoint(),
                                      skel.Joints[JointType.HipRight], skel.Joints[JointType.KneeRight], this);
                          barra_bder.dibujarPuntos();

                          barra_pizq = new Indicador(15, dc, new WriteableJoint(mov_pierna_izq.getInitialHip()), new WriteableJoint(mov_pierna_izq.getInitialKnee()),
                                      skel.Joints[JointType.HipLeft], skel.Joints[JointType.KneeLeft], this);
                          barra_pizq.dibujarPuntos();

                          if (Math.Abs(mov_brazo_der.getAngulo() - mov_pierna_izq.getAngle()) > ANGULO_SINC)
                          {
                             state = ESTADO.FAIL;
                          }
                          else if (mov_brazo_der.completado() && mov_pierna_izq.getState() == MovimientoPierna.ESTADO.ALCANZADO)
                          {
                             repetetitions--;
                             if (repetetitions == 0)
                                state = ESTADO.COMPLETADO;
                             else
                             {
                                movimiento_1 = false;
                                state = ESTADO.MOV_2;
                             }
                          }
                       }
                       else if (state == ESTADO.MOV_2)
                       {
                          mov_pierna_der.updateMovement(skel.Joints[JointType.HipRight], skel.Joints[JointType.KneeRight], skel);
                          mov_brazo_izq.actualizar(skel);

                          barra_bizq = new Indicador(15, dc, mov_brazo_izq.getShoulderPoint(), mov_brazo_izq.getWristPoint(),
                                      skel.Joints[JointType.HipLeft], skel.Joints[JointType.KneeLeft], this);
                          barra_bizq.dibujarPuntos();

                          barra_pder = new Indicador(15, dc, new WriteableJoint(mov_pierna_der.getInitialHip()), new WriteableJoint(mov_pierna_der.getInitialKnee()),
                                      skel.Joints[JointType.HipRight], skel.Joints[JointType.KneeRight], this);
                          barra_pder.dibujarPuntos();

                          if (Math.Abs(mov_brazo_izq.getAngulo() - mov_pierna_der.getAngle()) > ANGULO_SINC)
                          {
                             state = ESTADO.FAIL;
                          }
                          else if (mov_brazo_izq.completado() && mov_pierna_der.getState() == MovimientoPierna.ESTADO.ALCANZADO)
                          {
                             repetetitions--;
                             if (repetetitions == 0)
                                state = ESTADO.COMPLETADO;
                             else
                             {
                                movimiento_1 = true;
                                state = ESTADO.MOV_1;
                             }
                          }
                       }
                       else if (state == ESTADO.FAIL)
                       {
                          sms_block.Text = "Coloque el cuerpo en la posición de reposo.";
                          state = ESTADO.INICIO;
                       }*/
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

        /// <summary>
        /// Handles the user clicking on the screenshot button
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void ButtonScreenshotClick(object sender, RoutedEventArgs e)
        {
            if (null == this.sensor)
            {
                return;
            }

            // create a png bitmap encoder which knows how to save a .png file
            BitmapEncoder encoder = new PngBitmapEncoder();

            // create frame from the writable bitmap and add to encoder
            encoder.Frames.Add(BitmapFrame.Create(this.colorBitmap));

            string time = System.DateTime.Now.ToString("hh'-'mm'-'ss", CultureInfo.CurrentUICulture.DateTimeFormat);

            string myPhotos = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

            string path = Path.Combine(myPhotos, "KinectSnapshot-" + time + ".png");

            // write the new file to disk
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    encoder.Save(fs);
                }
            }
            catch (IOException)
            {
                //this.statusBarText.Text = string.Format(CultureInfo.InvariantCulture, "{0} {1}", Properties.Resources.ScreenshotWriteFailed, path);
            }
        }
    }
}