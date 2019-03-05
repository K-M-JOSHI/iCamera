using System;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using Android.Widget;
using Android.Support.V7.App;
using System.IO;
using Android.Support.V4.App;
using Android.Runtime;
using Environment = Android.OS.Environment;
using Uri = Android.Net.Uri;
using Android.Media;

namespace iCamera.Droid
{

    [Activity (Label = "iCamera", Theme ="@style/Theme.AppCompat.Light.NoActionBar")]
	public class MainActivity : AppCompatActivity
	{
        public ImageView imageView;
        public Bitmap mBitmap;
        public static readonly int TAKE_PIC = 1;
        public static readonly int PICK_IMAGE_ID = 1000;
        public Uri outPutfileUri;
        public string mCurrentPhotoPath;
       

        protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			SetContentView (Resource.Layout.Main);

			Button processButton = FindViewById<Button> (Resource.Id.processBtn);
            Button openCameraButton = FindViewById<Button>(Resource.Id.cameraBtn);
            Button openGalleryButton = FindViewById<Button>(Resource.Id.galleryBtn);

            imageView = FindViewById<ImageView>(Resource.Id.myImageView);

            processButton.Click += ImageProcess;
            openCameraButton.Click += OpenCamera;
            openGalleryButton.Click += OpenGallery;


        }

        private void ImageProcess(object sender, EventArgs eventArgs)
        {

            byte[] bitmapData;
            if (mBitmap != null)
            {
                if (mBitmap.Width >= 36 && mBitmap.Width <= 4096 && mBitmap.Height >= 36 && mBitmap.Height <= 4096)
                {
                    using (var stream = new MemoryStream())
                    {
                        mBitmap.Compress(Bitmap.CompressFormat.Jpeg, 100, stream);
                        bitmapData = stream.ToArray();
                    }

                    var inputStream = new MemoryStream(bitmapData);
                    try
                    {

                        new DetectTask(this).Execute(inputStream);
                        System.GC.Collect();
                    }
                    catch (Exception error)
                    {
                        Toast.MakeText(this, "Image size is too big", ToastLength.Long).Show();
                    }
                }
                else
                {
                    Toast.MakeText(this, "Image size is too large !! Can not process. Max allowed image size is 4096 X 4096 ", ToastLength.Long).Show();
                    Toast.MakeText(this,"Current Image Size: "+ mBitmap.Width + "X" + mBitmap.Height, ToastLength.Long).Show();

                }


            }
            else {
                Toast.MakeText(this, "Please select the image !!", ToastLength.Short).Show();
            }

        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
           
            if (requestCode == TAKE_PIC && resultCode == Result.Ok)
            {
                int targetW = imageView.Width;
                int targetH = imageView.Height;
             
                try
                {

                    BitmapFactory.Options bmOptions = new BitmapFactory.Options();
                    bmOptions.InJustDecodeBounds = true;
                    mBitmap = BitmapFactory.DecodeFile(mCurrentPhotoPath, bmOptions);
                    int photoW = bmOptions.OutWidth;
                    int photoH = bmOptions.OutHeight;

                    // Determine how much to scale down the image
                    int scaleFactor = Math.Min(photoW / targetW, photoH / targetH);

                    // Decode the image file into a Bitmap sized to fill the View
                    bmOptions.InJustDecodeBounds = false;
                    bmOptions.InSampleSize = scaleFactor;
                    bmOptions.InPurgeable = true;
                    mBitmap = null;
                    mBitmap = BitmapFactory.DecodeFile(mCurrentPhotoPath, bmOptions);
                    mBitmap = RotateImageIfRequired(this, mBitmap, outPutfileUri);
                    imageView.SetImageBitmap(mBitmap);
                    System.GC.Collect();
                }
                catch (IOException e)
                {
                   // e.printStackTrace();
                }


            }
            else if (requestCode == PICK_IMAGE_ID && resultCode == Result.Ok && data != null)
            {
                Android.Net.Uri selectedImageUri = data.Data;
                try
                {
                    mBitmap = null;
                    mBitmap = MediaStore.Images.Media.GetBitmap(ContentResolver, selectedImageUri);
                    var   xBitmap = RotateImageIfRequired(this, mBitmap, selectedImageUri);
                    mBitmap = null;
                    System.GC.Collect();
                    mBitmap = xBitmap;
                    xBitmap = null;
                    imageView.SetImageBitmap(mBitmap);
                    System.GC.Collect();
                }
                catch(Exception error)
                {
                    //Toast.MakeText(this, "Error1111", ToastLength.Long).Show();
                }

            }

        }

        private static Bitmap RotateImageIfRequired(Context context, Bitmap img, Uri selectedImage)
        {
            System.GC.Collect();
            System.GC.Collect();
            Java.Lang.Runtime.GetRuntime().FreeMemory();
            try
                {
                System.IO.Stream input = context.ContentResolver.OpenInputStream(selectedImage);

                 ExifInterface exif = new ExifInterface(input);
                    int orientation = exif.GetAttributeInt(ExifInterface.TagOrientation, 1);
                    Matrix matrix = new Matrix();
                    if (orientation == 6)
                    {
                        matrix.PostRotate(90);
                    }
                    else if (orientation == 3)
                    {
                        matrix.PostRotate(180);
                    }
                    else if (orientation == 8)
                    {
                        matrix.PostRotate(270);
                    }
                
                   var  imgTemp = Bitmap.CreateBitmap(img, 0, 0, img.Width, img.Height, matrix, true);
                    
                input.Flush();
                input.Close();
                matrix = null;
                img = null;
                System.GC.Collect();
                System.GC.Collect();
                Java.Lang.Runtime.GetRuntime().FreeMemory();
                return imgTemp ;

            }
            catch (Exception e)
            {
                   
            }
            System.GC.Collect();
            System.GC.Collect();
            Java.Lang.Runtime.GetRuntime().FreeMemory();

            return img;
        }

        private void OpenCamera(object sender, EventArgs eventArgs)
        {
          
            if (ActivityCompat.CheckSelfPermission(this, Android.Manifest.Permission.Camera) == Permission.Granted)
            {

                Intent intent = new Intent(MediaStore.ActionImageCapture);
                if (intent.ResolveActivity(PackageManager) != null)
                {
                    string timeStamp = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                    string imageFileName = "JPEG_" + timeStamp + "_";
                    String root = Environment.ExternalStorageDirectory.ToString();
                    Java.IO.File myDir = new Java.IO.File(root + "/saved_images");
                    myDir.Mkdirs();

                    Java.IO.File file = new Java.IO.File(myDir,
                    imageFileName+".jpg");
                    if (file.Exists()) file.Delete();
                    file.CreateNewFile();
                    mCurrentPhotoPath = file.AbsolutePath;
                    outPutfileUri = Uri.FromFile(file);

                    intent.PutExtra(MediaStore.ExtraOutput, outPutfileUri);
                    if (Build.VERSION.SdkInt > Build.VERSION_CODES.M)
                    {
                        intent.PutExtra("android.intent.extras.LENS_FACING_FRONT", 1);
                    }else{
                        intent.PutExtra("android.intent.extras.CAMERA_FACING", 1);
                    }
                    StartActivityForResult(intent, TAKE_PIC);
                }
            }
            else
            {
                Toast.MakeText(this, "Please provide camera access !!", ToastLength.Short).Show();
            }
        }
        private void OpenGallery(object sender, EventArgs eventArgs)
        {
            if (ActivityCompat.CheckSelfPermission(this, Android.Manifest.Permission.WriteExternalStorage) == Permission.Granted)
            {
                Intent = new Intent();
                Intent.SetType("image/*");
                Intent.SetAction(Intent.ActionGetContent);
                StartActivityForResult(Intent.CreateChooser(Intent, "Select Picture"), PICK_IMAGE_ID);
            }
            else
            {
                Toast.MakeText(this, "Please provide storage access !!", ToastLength.Short).Show();
            }
        }
    }

    
}


