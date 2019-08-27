using System;
using System.Linq;
using System.Collections.Generic;

using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using WebSocketSharp;
using Newtonsoft.Json;
using System.Threading;



namespace MNML
{

    public class ViewportMonitorComponent : GH_Component
    {

        const double EPSILON = 0.001;

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        /// 
        public CameraInfo prevCameraInfo = new CameraInfo();
        String cameraName = null;
        WebSocket socket = null;
        double intervalMilliseconds = 1000.0;
        Timer timer = null;

        public ViewportMonitorComponent()
            : base("Monitor Viewport", "Minitor Viewport Info and send via WebSocket",
            "Retrieve the Viewport Info and send it to socket connection",
                   "MNML", "Communication")
        {
            SetupTimer();
        }

        public override void RemovedFromDocument(GH_Document document)
        {
            if (timer != null)
            {
                timer.Dispose();
            }
            prevCameraInfo = null;
            socket = null;
            base.RemovedFromDocument(document);
        }

        void SetupTimer()
        {

            var startTimeSpan = TimeSpan.Zero;
            var periodTimeSpan = TimeSpan.FromMilliseconds(intervalMilliseconds);
            if (timer != null) timer.Dispose();
            timer = new Timer((e) =>
            {
                if (socket != null) GetCameraInfo();
            }, null, startTimeSpan, periodTimeSpan);
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Socket", "S", "WebSocketClient", GH_ParamAccess.item);
            pManager.AddTextParameter("Camera Name", "CN", "Camera name in blender", GH_ParamAccess.item, "RhinoCamera");
            pManager.AddNumberParameter("Monitor Interval (ms)", "I", "Camera monitor interval", GH_ParamAccess.item, 1000.0);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double _intervalMilliseconds = 1000.0;
            if (!DA.GetData(0, ref socket)) { socket = null; }
            if (!DA.GetData(1, ref cameraName)) { cameraName = null; }
            DA.GetData(2, ref _intervalMilliseconds);

            if (_intervalMilliseconds != intervalMilliseconds)
            {
                intervalMilliseconds = _intervalMilliseconds;
                SetupTimer();
            }

        }

        void GetCameraInfo()
        {

            var doc = Rhino.RhinoDoc.ActiveDoc;

            if (doc != null)
            {
                Rhino.Display.RhinoViewport vp = doc.Views.ActiveView.ActiveViewport;
                CameraInfo cameraInfo = new CameraInfo
                {
                    Name = cameraName,
                    FocalLength = vp.Camera35mmLensLength,
                    Position = new List<double> { vp.CameraLocation.X, vp.CameraLocation.Y, vp.CameraLocation.Z },
                    Target = new List<double> { vp.CameraTarget.X, vp.CameraTarget.Y, vp.CameraTarget.Z },
                    Aspect = vp.FrustumAspect
                };

                Payload payload = new Payload
                {
                    Action = "camera",
                    Info = cameraInfo
                };

                if (!prevCameraInfo.Equals(cameraInfo))
                {
                    string message = JsonConvert.SerializeObject(payload);
                    socket.Send(message);

                    prevCameraInfo.Name = cameraInfo.Name;
                    prevCameraInfo.FocalLength = cameraInfo.FocalLength;
                    prevCameraInfo.Aspect = cameraInfo.Aspect;
                    prevCameraInfo.Position = cameraInfo.Position;
                    prevCameraInfo.Target = cameraInfo.Target;
                }
            }
        }


        // close is good for horseshoes, hand grenades, nuclear weapons, and doubles
        static bool ApproximatelyEquals(double value1, double value2, double acceptableDifference)
        {
            return Math.Abs(value1 - value2) <= acceptableDifference;
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("140ec2ee-674c-4abd-9037-ad0720b0bd21"); }
        }
    }


    [JsonObject(MemberSerialization.OptIn)]
    public class CameraInfo
    {
        static double EPSILON = 0.001;

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "focalLength")]
        public double FocalLength { get; set; }
        [JsonProperty(PropertyName = "position")]
        public List<double> Position { get; set; }
        [JsonProperty(PropertyName = "target")]
        public List<double> Target { get; set; }
        [JsonProperty(PropertyName = "aspect")]
        public double Aspect { get; set; }

        public bool Equals(CameraInfo b)
        {
            return Name == b.Name &&
                Math.Abs(FocalLength - b.FocalLength) < EPSILON &&
                Position.SequenceEqual(b.Position) &&
                Target.SequenceEqual(b.Target) &&
                Math.Abs(Aspect - b.Aspect) < EPSILON; 
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class Payload
    {
        [JsonProperty(PropertyName = "action")]
        public string Action { get; set; }
        [JsonProperty(PropertyName = "info")]
        public CameraInfo Info { get; set; }
    }
}
