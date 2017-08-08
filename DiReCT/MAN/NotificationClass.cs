/*
 * Copyright (c) 2016 Academia Sinica, Institude of Information Science
 * 
 *  This file is part of DiReCT.
 *
 *  DiReCT is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Foobar is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Foobar.  If not, see <http://www.gnu.org/licenses/>.
 *
 * Project Name:
 * 
 *      DiReCT(Disaster Record Capture Tool)
 * 
 * File Description:
 * File Name:
 * 
 *      NotificationClass.cs
 * 
 * Abstract:
 *      
 *      This file contains classes for monitoring time and device location, and
 *      pushing a notification. The notification class contains all information
 *      a notification needs. The notification manager manages notifications,
 *      such as push and cancel a notification.
 *
 * Authors:
 * 
 *      Hunter Hsieh, hunter205@iis.sinica.edu.tw  
 *      Jeff Chen, jeff@iis.sinica.edu.tw
 * 
 */

using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Threading;
using System.Diagnostics;
using System.IO;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Device.Location;
using System.Windows;

namespace DiReCT.MAN
{
    //-----------------------------------------//
    /* The way to use this notification class */
    //-----------------------------------------//
    /* Create a notification */
    //Notification.Builder mBuilder = new Notification.Builder();
    //mBuilder.SetWhen(DateTime.Now);
    //        mBuilder.SetContentText("This is a custom notification.");
    //        mBuilder.SetNotificationType(NotificationTypes.Toast);
    //        mBuilder.Build(10, null);
    //
    /* Push a notification */
    //NotificationManager.Notify(10);
    public static class VibratePatterns
    {
        //Vibrate patterns in milliseconds
        public static readonly long[] DEFAULT = { 1000 };
        public static readonly long[] SHORT = { 500 };
        public static readonly long[] LONG = { 1500 };
        public static readonly long[] TWO_SHORTS = { 500, 500 };
        public static readonly long[] TWO_LONGS = { 1000, 1000 };
        public static readonly long[] SHORT_LONG = { 500, 1000 };
        public static readonly long[] SHORT_SHORT_LONG = { 500, 500, 1000 };
    }
    public enum NotificationTypes
    {
        Dialogue,
        Toast,
        Snackbar,
        None
    };

    public enum NotificationPriority
    {
        High,
        Normal,
        Low
    };

    public class Notification
    {

        #region Fields...
        // Basic variables
        private string notificationTag;     // Name of notification
        private int notificationID;         // ID of notification
        private NotificationTypes notificationType; // Type of notification
        private Control templateContent;    // Customized style of notification
        private string contentTitle;
        private string contentText;
        private string audioPath;     // Is used when sound is needed
        private string imagePath;     // Is used when image is needed

        // Time variables
        private DateTime when;        // The time at which the notification is
                                      // going to be pushed.
        private int timeoutAfter;     // A duration in milliseconds after which 
                                      // this notification should be canceled, 
                                      // if it is not already canceled.
        private int repeatInterval;   // The time interval between invocations 
                                      // of callback, in milliseconds.

        // Location variables
        private GeoCoordinate[] where;
        private double radius;
        #endregion

        public Notification()
        {
            notificationTag = "";
            notificationID = -1;
            notificationType = NotificationTypes.None;
            templateContent = null;
            contentTitle = "";
            contentText = "";
            audioPath = "";
            imagePath = "";
            timeoutAfter = 0;
            repeatInterval = Timeout.Infinite;
            where = null;
            radius = 0;
        }

        public class Builder
        {
            Notification n;

            // Contructor
            public Builder()
            {
                n = new Notification();
            }

            /// <summary>
            /// Build the notification after comleting all the settings
            /// </summary>
            /// <param name="id">
            /// A unique identifer of the notification
            /// </param>
            /// <param name="tag">
            /// The name of the notification. Set null if none.
            /// </param>
            /// <returns></returns>
            public Notification Build(int id, string tag)
            {
                n.notificationID = id;
                n.notificationTag = tag;
                NotificationManager.AddNotificationToList(n);
                return n;
            }

            #region Set methods...
            //通知種類，設定通知的類別 EX彈出視窗，推播，通知欄等等
            public void SetNotificationType(NotificationTypes notificationType)     
            {
                n.notificationType = notificationType;
            }
            //通知顯示的標題
            public void SetContentTitle(string notificationTitle)
            {
                n.contentTitle = notificationTitle;
            }
            //通知內容，該通知主要的通知文字內容
            public void SetContentText(string notificationContent)
            {
                n.contentText = notificationContent;
            }
            //自定義通知視窗的樣式
            public void SetTemplateContent(string xamlFilePath)
            {
                try
                {
                    FileStream fs = new FileStream(xamlFilePath,
                                                   FileMode.Open,
                                                   FileAccess.Read);
                    Control myControl = (Control)XamlReader.Load(fs);
                    n.templateContent = myControl;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    Debug.WriteLine("Notification template failed to load.");
                }
            }
            //設定音訊位置，撥放音訊
            public void SetAudioPath(string audioPath, int times) 
            {
                n.audioPath = audioPath;
            }
            //設定圖片位置，已顯示圖片
            public void SetImagePath(string imagePath)
            {
                n.imagePath = imagePath;
            }
            //設置LED開啟
            public void SetLights(Color color, int onMs, int offMs)
            {
                //
                // To do...
                //
                throw new NotImplementedException();
            }
            //設置震動是否開啟
            public void SetVibrate(long[] pattern, int times) 
            {
                //
                // To do...
                //
                throw new NotImplementedException();
            }

            /// <summary>
            /// Specifies a time duration in milliseconds after which this 
            /// notification should be canceled, if it is not already cancelled
            /// </summary>
            /// <param name="dueTimeMs">
            /// Time duration in milliseconds
            /// </param>
            public void SetTimeoutAfter(int dueTimeMs)
            {
                n.timeoutAfter = dueTimeMs;
            }

            public void SetPriority(int priority)
            {
                //
                // To do...
                //
                throw new NotImplementedException();
            }
            //設置該則通知需要在何時發送的時間
            public void SetWhen(DateTime when)
            {
                n.when = when;
            }
            public void SetRepeating(DateTime when, int interval)
            {
                n.when = when;
                n.repeatInterval = interval;
            }

            /// <summary>
            /// Set an location area to post notification 
            /// when device is located in
            /// </summary>
            /// <param name="geoCoordinate">
            /// Coordinate where the notifying area be
            /// </param>
            /// <param name="radius">
            /// Radius area around specified coordinates in meters
            /// </param>
            /// <param name="checkLocationFreq">
            /// Frequency of checking device location in milliseconds
            /// </param>
            public void SetWhere(GeoCoordinate[] geoCoordinate,
                                 double radius)
            {
                n.where = geoCoordinate;
                n.radius = radius;
            }
            #endregion
        }

        #region Properties...
        public int NotificationID
        {
            get { return notificationID; }
        }
        public string NotificationTag
        {
            get { return notificationTag; }
        }
        public NotificationTypes NotificationType
        {
            get { return notificationType; }
        }

        public DateTime When
        {
            get { return when; }
        }

        public int TimeoutAfter
        {
            get { return timeoutAfter; }
        }

        public int RepeatInterval
        {
            get { return repeatInterval; }
        }

        public GeoCoordinate[] Where
        {
            get { return where; }
        }

        public string ContentTitle
        {
            get { return contentTitle; }
        }
        public string ContentText
        {
            get { return contentText; }
        }

        public string AudioPath
        {
            get { return audioPath; }
        }

        public string ImagePath
        {
            get { return imagePath; }
        }

        public double Radius
        {
            get { return radius; }
        }

        public Control TemplateContent
        {
            get { return templateContent; }
        }
        #endregion

    }

    public static class NotificationManager
    {
        static GeoCoordinateWatcher watcher = new GeoCoordinateWatcher();
        static bool hasWatcherStarted = false;

        private struct NotificationHandle
        {
            public Notification Notification;
            public Timer NotificationTimer;
            public bool HasStarted;

            public NotificationHandle(Notification notification)
            {
                this.Notification = notification;
                NotificationTimer = null;
                HasStarted = false;
            }
        }

        private static List<NotificationHandle>
            notificationHandleList = new List<NotificationHandle>();

        internal static void AddNotificationToList(Notification notification)
        {
            NotificationHandle nH = new NotificationHandle(notification);
            notificationHandleList.Add(nH);
        }

        /// <summary>
        /// Launch a notification with its ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static void Notify(int id)
        {
            NotificationHandle nH = notificationHandleList.Find
                (x => x.Notification.NotificationID == id);
            scheduleNotification(nH);
        }

        /// <summary>
        /// Launch a notification with its tag
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static void Notify(string tag)
        {
            NotificationHandle nH = notificationHandleList.Find
                (x => x.Notification.NotificationTag == tag);
            scheduleNotification(nH);
        }

        private static void scheduleNotification(NotificationHandle nH)
        {
            Notification n = nH.Notification;

            // Calculate delay time
            TimeSpan t = n.When - DateTime.Now;
            int delayTime = (int)t.TotalMilliseconds;
            if (delayTime < 0) delayTime = 0;

            //// Set coordinate monitor
            //if (!hasWatcherStarted)
            //{
            //    watcher.PositionChanged += 
            //        new EventHandler<
            //            GeoPositionChangedEventArgs<GeoCoordinate>>(
            //            watcher_PositionChanged);
            //    watcher.Start();
            //    hasWatcherStarted = true;
            //}

            // Set timer
            if (nH.NotificationTimer != null)
                nH.NotificationTimer.Change(delayTime, n.RepeatInterval);
            else
                nH.NotificationTimer
                    = new Timer(notify, nH, delayTime, n.RepeatInterval);
  
        }

        public delegate void UINotificationDelegate(string contentText);
        // Push the notification
        private static void notify(object state)
        {
            NotificationHandle nH = (NotificationHandle)state;
            Notification n = nH.Notification;

            switch (n.NotificationType)
            {
                case NotificationTypes.Dialogue:
                    //
                    // To do...
                    //
                    break;
                case NotificationTypes.Snackbar:
                    //
                    // To do...
                    //
                    break;
                case NotificationTypes.Toast:
                    if (nH.HasStarted == true &&
                        n.When.AddMilliseconds(n.TimeoutAfter) < DateTime.Now)
                        Cancel(n.NotificationID);
                    else
                    {
                        // Push a toast through popup window
                        Application.Current.Dispatcher
                            .BeginInvoke(DispatcherPriority.Background,
                            new UINotificationDelegate(CreateToast),
                            n.ContentText);

                        //
                        // Here, CreateToast is a temporary function defined in
                        // UI module. The function should be redesigned along
                        // with the UIFunctionDelegate,including its 
                        // functionality and parameters.
                        //
                    }
                    break;
                case NotificationTypes.None:
                    //
                    // To do...
                    //
                    break;
                default:
                    break;
            }
        }


        private static void watcher_PositionChanged(
            object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            //
            // To do...
            //
            // Check whether device coordinate matches 
            // locations set in n.where array.
            // If yes, push the notification.
            //

            throw new NotImplementedException();
        }

        /// <summary>
        /// Cancel a notification with its ID
        /// </summary>
        /// <param name="id"></param>
        public static void Cancel(int id)
        {
            NotificationHandle nH = notificationHandleList.Find
                (x => x.Notification.NotificationID == id);
            cancel(nH);
        }

        /// <summary>
        /// Cancel a notification with its tag
        /// </summary>
        /// <param name="tag"></param>
        public static void Cancel(string tag)
        {
            NotificationHandle nH = notificationHandleList.Find
                (x => x.Notification.NotificationTag == tag);
            cancel(nH);
        }

        /// <summary>
        /// Cancel all registered notifications
        /// </summary>
        public static void CancelAll()
        {
            foreach (NotificationHandle nH in notificationHandleList)
            {
                cancel(nH);
            }
        }

        private static void cancel(NotificationHandle nH)
        {
            Notification n = nH.Notification;
            // Stop the timer if timeout
            if (nH.HasStarted == true &&
                n.When.AddMilliseconds(n.TimeoutAfter) < DateTime.Now)
            {
                if (nH.NotificationTimer != null)
                    nH.NotificationTimer.Change(Timeout.Infinite,
                                                Timeout.Infinite);
                nH.HasStarted = false;
                return;
            }
        }
        private static void CreateToast(string contentText)
        {
            
            MessageBox.Show(contentText,"HI", MessageBoxButton.OK);
            //Popup codepopup = new Popup();
            //TextBlock popuptext = new TextBlock();

            //popuptext.Text = contentText;
            //popuptext.Background = Brushes.LightBlue;
            //popuptext.Foreground = Brushes.Blue;

            //codepopup.Child = popuptext;
            //codepopup.Placement = PlacementMode.Right;
            //codepopup.IsOpen = true;

        }
    }
}
