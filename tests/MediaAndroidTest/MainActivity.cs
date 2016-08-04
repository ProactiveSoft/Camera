﻿using Android.App;
using Android.Widget;
using Android.OS;
using Android.Graphics;
using Android.Content.PM;

namespace MediaAndroidTest
{
    [Activity(Label = "MediaAndroidTest", MainLauncher = true, Icon = "@drawable/icon", ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            StartActivity(typeof(MainActivity2));
        }
    }
    [Activity(Label = "MediaAndroidTest", Icon = "@drawable/icon", ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    public class MainActivity2 : Activity
    {
        int count = 1;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            Button button = FindViewById<Button>(Resource.Id.MyButton);
            var image = FindViewById<ImageView>(Resource.Id.imageView1);

            var switchSize = FindViewById<Switch>(Resource.Id.switch_size);
            var switchSaveToAlbum = FindViewById<Switch>(Resource.Id.switch_save_album);

            button.Click += async delegate
            {
                var media = new Plugin.Media.MediaImplementation();
                var file = await Plugin.Media.CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
                {
                    Directory = "Sample",
                    Name = "test.jpg",
                    SaveToAlbum = switchSaveToAlbum.Checked,
                    PhotoSize = switchSize.Checked ? Plugin.Media.Abstractions.PhotoSize.Medium : Plugin.Media.Abstractions.PhotoSize.Full,
                    DefaultCamera = Plugin.Media.Abstractions.CameraDevice.Front
                });
                if (file == null)
                    return;
                var path = file.Path;
                Toast.MakeText(this, path, ToastLength.Long).Show();
                System.Diagnostics.Debug.WriteLine(path);

                image.SetImageBitmap(BitmapFactory.DecodeFile(file.Path));
                file.Dispose();
            };

            var pick = FindViewById<Button>(Resource.Id.button1);
            pick.Click += async (sender, args) =>
              {
                  var file = await Plugin.Media.CrossMedia.Current.PickPhotoAsync();
                  if (file == null)
                      return;
                  var path = file.Path;
                  Toast.MakeText(this, path, ToastLength.Long).Show();
                  System.Diagnostics.Debug.WriteLine(path);
                  image.SetImageBitmap(BitmapFactory.DecodeFile(file.Path));
                  file.Dispose();
              };

            FindViewById<Button>(Resource.Id.button2).Click += async (sender, args) =>
              {
                  var media = new Plugin.Media.MediaImplementation();
                  var file = await Plugin.Media.CrossMedia.Current.TakeVideoAsync(new Plugin.Media.Abstractions.StoreVideoOptions
                  {
                      Directory = "Sample",
                      Name = "test.jpg",
                      SaveToAlbum = true
                  });
                  if (file == null)
                      return;
                  var path = file.Path;
                  System.Diagnostics.Debug.WriteLine(path);
                  Toast.MakeText(this, path, ToastLength.Long).Show();


                  file.Dispose();
                  
              };


            FindViewById<Button>(Resource.Id.button3).Click += async (sender, args) =>
            {
                var media = new Plugin.Media.MediaImplementation();
                var file = await Plugin.Media.CrossMedia.Current.PickVideoAsync();
                if (file == null)
                    return;

                var path = file.Path;
                Toast.MakeText(this, path, ToastLength.Long).Show();
                System.Diagnostics.Debug.WriteLine(path);

                file.Dispose();
            };

        }
        
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            Plugin.Permissions.PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}
