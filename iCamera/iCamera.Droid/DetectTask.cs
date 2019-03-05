using System;
using System.Collections.Generic;

using Android.App;
using Android.OS;
using Android.Graphics;
using System.IO;
using Com.Microsoft.Projectoxford.Face;
using GoogleGson;
using Newtonsoft.Json;
using iCamera.Droid.Model;

namespace iCamera.Droid
{
    class DetectTask : AsyncTask<Stream, string, string>
    {
        private static string endPoint = "https://westcentralus.api.cognitive.microsoft.com/face/v1.0";
        private static string key = "78c17489df6e4db89ea91ad58f2d38ed"; 
        private MainActivity mainActivity;

        private ProgressDialog pd;

        public DetectTask(MainActivity mainActivity)
        {
            pd = new ProgressDialog(mainActivity);
            this.mainActivity = mainActivity;
            pd.SetCancelable(false);
        }

        protected override string RunInBackground(params Stream[] @params)
        {
            PublishProgress("Detecting...");
            var faceServiceClient = new FaceServiceRestClient(endPoint, key); 
            Com.Microsoft.Projectoxford.Face.Contract.Face[] result = faceServiceClient.Detect(@params[0],
               true, //FaceId 
               false, // Face LandMarks
                new FaceServiceClientFaceAttributeType[] {
                         FaceServiceClientFaceAttributeType.Gender,
                         FaceServiceClientFaceAttributeType.Age,
                         FaceServiceClientFaceAttributeType.Smile,
                         FaceServiceClientFaceAttributeType.FacialHair,

                         FaceServiceClientFaceAttributeType.Glasses });
      
            // return Face Attributes : age, gender ..etc

            if (result == null)
            {
                PublishProgress("Detection Finished. Nothing detected");
                return null;
            }
            PublishProgress($"Detection Finished. {result.Length} face(s) detected");

            Gson gson = new Gson();

            var strResult = gson.ToJson(result);
            Console.WriteLine(strResult + "ABC");
            return strResult;

        }

        protected override void OnPreExecute()
        {
            pd.Show();
        }

        protected override void OnProgressUpdate(params string[] values)
        {
            pd.SetMessage(values[0]);
        }

        protected override void OnPostExecute(string result)
        {
            var faces = JsonConvert.DeserializeObject<List<FaceModel>>(result);
            var bitmap = DrawRectangesOnBitMap(mainActivity.mBitmap, faces);
            mainActivity.imageView.SetImageBitmap(bitmap);
            pd.Dismiss();
        }

        private Bitmap DrawRectangesOnBitMap(Bitmap mBitMap, List<FaceModel> faces)
        {
            Bitmap bitmap = mBitMap.Copy(Bitmap.Config.Argb8888, true);
            Canvas canvas = new Canvas(bitmap);

            Paint paint = new Paint();
            paint.AntiAlias = true;
            paint.SetStyle(Paint.Style.Stroke);
            paint.Color = Color.White;
            paint.StrokeWidth = 12;

            Paint paintText = new Paint();
            paintText.SetStyle(Paint.Style.Fill);
            paintText.Color = Color.Green;
            paintText.TextSize = 55;


            foreach (var face in faces)
            {
                var faceRectangle = face.faceRectangle;
                canvas.DrawRect(faceRectangle.left, faceRectangle.top,
                                faceRectangle.left + faceRectangle.width,
                                faceRectangle.top + faceRectangle.height, paint);
                canvas.DrawText("Age: " + face.faceAttributes.age, faceRectangle.left, faceRectangle.top + faceRectangle.height + 75, paintText);
                canvas.DrawText("Gender: " + face.faceAttributes.gender, faceRectangle.left, faceRectangle.top + faceRectangle.height + 125, paintText);
                canvas.DrawText("Smile: " + (face.faceAttributes.smile * 100) + "%", faceRectangle.left, faceRectangle.top + faceRectangle.height + 175, paintText);
                canvas.DrawText("Glasses: " + (face.faceAttributes.glasses) , faceRectangle.left, faceRectangle.top + faceRectangle.height + 225, paintText);

            }

            return bitmap;
        }


    }
}