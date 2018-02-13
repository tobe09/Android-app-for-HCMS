using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Firebase.Messaging;
using System.Collections.Generic;

namespace PeoplePlusMobile
{
    //FOR GETTING NOTIFICATIONS WHEN THE APP IS FOREGROUNDED
    [Service]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class MyFirebaseMessagingService : FirebaseMessagingService
    {
        public override void OnMessageReceived(RemoteMessage message)
        {
            var notification = message.GetNotification();

            Dictionary<string, string> msg = new Dictionary<string, string>();
            msg.Add("MSG_TITLE", notification.Title);
            msg.Add("MSG_CONTENT", notification.Body);

            SendNotification(msg);     
        }

        //used to generate local notifications for remote messages
        void SendNotification(Dictionary<string, string> response)
        {
            TaskStackBuilder stackBuilder = TaskStackBuilder.Create(this);
            Intent wklIntent = new Intent(this, typeof(WorkListActivity));
            stackBuilder.AddNextIntent(wklIntent);

            PendingIntent pendingIntent = stackBuilder.GetPendingIntent(0, PendingIntentFlags.OneShot);  //id=0

            Notification.BigTextStyle style = new Notification.BigTextStyle();
            style.BigText(response["MSG_CONTENT"]);
            style.SetSummaryText(response["MSG_TITLE"]);

            Notification.Builder builder = new Notification.Builder(Application.Context)
                .SetContentTitle("PeoplePlus Notification")
                .SetContentText("Pending Task(s) in your WorkList Viewer")
                .SetSmallIcon(Resource.Drawable.neptunelogo)
                .SetLargeIcon(BitmapFactory.DecodeResource(Resources, Resource.Drawable.neptunelogo))
                .SetAutoCancel(true)
                .SetContentIntent(pendingIntent);

            if ((int)Build.VERSION.SdkInt >= 21)
            {
                builder.SetVisibility(NotificationVisibility.Private)
                .SetCategory(Notification.CategoryAlarm)
                .SetCategory(Notification.CategoryCall)
                .SetStyle(style);
            }

            builder.SetPriority((int)NotificationPriority.High);
            builder.SetDefaults(NotificationDefaults.Sound | NotificationDefaults.Vibrate);

            Notification notification = builder.Build();

            NotificationManager notificationManager = GetSystemService(NotificationService) as NotificationManager;
            notificationManager.Notify(0, notification);
        }
    }
}