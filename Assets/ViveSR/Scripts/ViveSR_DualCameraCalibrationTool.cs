using Microsoft.Win32;
using System;
using System.IO;
using UnityEngine;

namespace Vive.Plugin.SR
{
    enum ANGLE_PARAMS {
        RELATIVE_ANGLE_X,
        RELATIVE_ANGLE_Y,
        RELATIVE_ANGLE_Z,
        ABSOLUTE_ANGLE_X,
        ABSOLUTE_ANGLE_Y,
        ABSOLUTE_ANGLE_Z,
    };
    /**
    * @warning The calibration functions of this class will be removed in the future.
    */
    public class ViveSR_DualCameraCalibrationTool : MonoBehaviour
    {
        public static bool IsCalibrating;
        public static CalibrationType CurrentCalibrationType;

        private Vector3 relative_angle = new Vector3(0.0f, 0.0f, 0.0f);
        private Vector3 absolute_angle = new Vector3(0.0f, 0.0f, 0.0f);
        private bool load_file_value = false;
        private string[] angle_array;
        private string calibration_result_dir = System.IO.Path.GetDirectoryName(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData)) + "\\LocalLow\\HTC Corporation\\SR_Config\\";
        private string params_filename = "calibration_params.bin";
        BinaryWriter binary_writer;
        BinaryReader brnary_reader;

        public void SetCalibrationMode(bool active, CalibrationType calibrationType = CalibrationType.ABSOLUTE)
        {
            if (ViveSR_DualCameraRig.Instance.TrackedCameraLeft == null || ViveSR_DualCameraRig.Instance.TrackedCameraRight == null) return;
            CurrentCalibrationType = calibrationType;
            IsCalibrating = active;
            if (IsCalibrating)
            {
                ViveSR_DualCameraRig.Instance.TrackedCameraRight.ImagePlaneCalibration.gameObject.SetActive(calibrationType == CalibrationType.RELATIVE);
            }
            else
            {
                ViveSR_DualCameraRig.Instance.TrackedCameraRight.ImagePlaneCalibration.gameObject.SetActive(false);
            }
        }

        public void Calibration(CalibrationAxis axis, float angle)
        {
            if (ViveSR_DualCameraRig.Instance.TrackedCameraLeft == null || ViveSR_DualCameraRig.Instance.TrackedCameraRight == null) return;
            Vector3 vectorAxis = Vector3.zero;
            switch (axis)
            {
                case CalibrationAxis.X:
                    vectorAxis = Vector3.right;
                    break;
                case CalibrationAxis.Y:
                    vectorAxis = Vector3.up;
                    break;
                case CalibrationAxis.Z:
                    vectorAxis = Vector3.forward;
                    break;
            }
            if (CurrentCalibrationType == CalibrationType.RELATIVE)
            {
                ViveSR_DualCameraRig.Instance.TrackedCameraLeft.Anchor.transform.localEulerAngles += vectorAxis * angle;
                relative_angle += vectorAxis * angle;
            }
            if (CurrentCalibrationType == CalibrationType.ABSOLUTE)
            {
                ViveSR_DualCameraRig.Instance.TrackedCameraLeft.Anchor.transform.localEulerAngles += vectorAxis * angle;
                ViveSR_DualCameraRig.Instance.TrackedCameraRight.Anchor.transform.localEulerAngles += vectorAxis * angle;
                absolute_angle += vectorAxis * angle;
            }
        }

        public void ResetCalibration()
        {
            CurrentCalibrationType = CalibrationType.RELATIVE;
            Calibration(CalibrationAxis.X, -relative_angle.x);
            Calibration(CalibrationAxis.Y, -relative_angle.y);
            Calibration(CalibrationAxis.Z, -relative_angle.z);

            CurrentCalibrationType = CalibrationType.ABSOLUTE;
            Calibration(CalibrationAxis.X, -absolute_angle.x);
            Calibration(CalibrationAxis.Y, -absolute_angle.y);
            Calibration(CalibrationAxis.Z, -absolute_angle.z);
        }

        /// <summary>
        /// Load the custom calibration parameters from  DualCameraParameters.xml.
        /// </summary>
        public void LoadDeviceParameter()
        {
            foreach (DualCameraIndex camIndex in Enum.GetValues(typeof(DualCameraIndex)))
            {
                ViveSR_TrackedCamera trackedCamera = camIndex == DualCameraIndex.LEFT ? ViveSR_DualCameraRig.Instance.TrackedCameraLeft :
                                                                                        ViveSR_DualCameraRig.Instance.TrackedCameraRight;
                if (trackedCamera != null)
                {
                    for (int planeIndex = 0; planeIndex < 2; planeIndex++)
                    {
                        ViveSR_DualCameraImagePlane imagePlane = planeIndex == 0 ? trackedCamera.ImagePlane :
                                                                                   trackedCamera.ImagePlaneCalibration;
                        if (imagePlane != null)
                        {
                            imagePlane.DistortedImagePlaneWidth = ViveSR_DualCameraImageCapture.DistortedImageWidth;
                            imagePlane.DistortedImagePlaneHeight = ViveSR_DualCameraImageCapture.DistortedImageHeight;
                            imagePlane.UndistortedImagePlaneWidth = ViveSR_DualCameraImageCapture.UndistortedImageWidth;
                            imagePlane.UndistortedImagePlaneHeight = ViveSR_DualCameraImageCapture.UndistortedImageHeight;
                            if (camIndex == DualCameraIndex.LEFT)
                            {
                                imagePlane.DistortedImagePlaneCenterX = ViveSR_DualCameraImageCapture.DistortedCxLeft;
                                imagePlane.DistortedImagePlaneCenterY = ViveSR_DualCameraImageCapture.DistortedCyLeft;
                                imagePlane.UndistortedImagePlaneCenterX = ViveSR_DualCameraImageCapture.UndistortedCxLeft;
                                imagePlane.UndistortedImagePlaneCenterY = ViveSR_DualCameraImageCapture.UndistortedCyLeft;
                                imagePlane.CameraFocalLength = ViveSR_DualCameraImageCapture.FocalLengthLeft;
                                imagePlane.UndistortionedMap = ViveSR_DualCameraImageCapture.UndistortionMapLeft;
                            }
                            else if (camIndex == DualCameraIndex.RIGHT)
                            {
                                imagePlane.DistortedImagePlaneCenterX = ViveSR_DualCameraImageCapture.DistortedCxRight;
                                imagePlane.DistortedImagePlaneCenterY = ViveSR_DualCameraImageCapture.DistortedCyRight;
                                imagePlane.UndistortedImagePlaneCenterX = ViveSR_DualCameraImageCapture.UndistortedCxRight;
                                imagePlane.UndistortedImagePlaneCenterY = ViveSR_DualCameraImageCapture.UndistortedCyRight;
                                imagePlane.CameraFocalLength = ViveSR_DualCameraImageCapture.FocalLengthRight;
                                imagePlane.UndistortionedMap = ViveSR_DualCameraImageCapture.UndistortionMapRight;
                            }
                        }
                    }
                }
            }

            //load to temp variable which will update variable in calibiration function
            ReadCalibrationFile();
            Vector3 _RelativeAngle = new Vector3(LoadParamsValue((int)ANGLE_PARAMS.RELATIVE_ANGLE_X, 0.0f),
                                                 LoadParamsValue((int)ANGLE_PARAMS.RELATIVE_ANGLE_Y, 0.0f),
                                                 LoadParamsValue((int)ANGLE_PARAMS.RELATIVE_ANGLE_Z, 0.0f));
            Vector3 _AbsoluteAngle = new Vector3(LoadParamsValue((int)ANGLE_PARAMS.ABSOLUTE_ANGLE_X, 0.0f),
                                                 LoadParamsValue((int)ANGLE_PARAMS.ABSOLUTE_ANGLE_Y, 0.0f),
                                                 LoadParamsValue((int)ANGLE_PARAMS.ABSOLUTE_ANGLE_Z, 0.0f));

            CurrentCalibrationType = CalibrationType.RELATIVE;
            Calibration(CalibrationAxis.X, _RelativeAngle.x);
            Calibration(CalibrationAxis.Y, _RelativeAngle.y);
            Calibration(CalibrationAxis.Z, _RelativeAngle.z);

            CurrentCalibrationType = CalibrationType.ABSOLUTE;
            Calibration(CalibrationAxis.X, _AbsoluteAngle.x);
            Calibration(CalibrationAxis.Y, _AbsoluteAngle.y);
            Calibration(CalibrationAxis.Z, _AbsoluteAngle.z);
        }

        /// <summary>
        /// Save the custom calibration parameters. 
        /// </summary>
        public void SaveDeviceParameter()
        {
            SaveParamsValue(relative_angle, absolute_angle);
        }

        private void ReadCalibrationFile()
        {
            string file_path = calibration_result_dir + params_filename;
            if (!File.Exists(file_path))
                return;
            try
            {
                brnary_reader = new BinaryReader(new FileStream(file_path, FileMode.Open));
            }
            catch (IOException e)
            {
                Debug.Log(e.Message);
                return;
            }

            try {
                load_file_value = true;
                string text = brnary_reader.ReadString();
                Debug.Log(text);
                angle_array = text.Split(',');
            }
            catch (IOException e)
            {
                Debug.Log(e.Message);
                brnary_reader.Close();
                return;
            }
            brnary_reader.Close();
        }

        private float LoadParamsValue(int angle_index, float default_value) {
            if (!load_file_value)
                return default_value;

            return float.Parse(angle_array[angle_index]);
        }

        private void SaveParamsValue(Vector3 relative_angle, Vector3 absolute_angle)
        {            
            try
            {
                binary_writer = new BinaryWriter(new FileStream(calibration_result_dir + params_filename, FileMode.Create));
            }
            catch (IOException e)
            {
                Debug.Log(e.Message);
                return;
            }            
            try {
                string output = "";
                string str = relative_angle.ToString();
                str = str.Trim(new char[] { '(', ')', ' ' });
                str = str.Replace(" ", "");
                output += str + ",";
                str = absolute_angle.ToString();
                str = str.Trim(new char[] { '(', ')', ' ' });
                str = str.Replace(" ", "");
                output += str;
                binary_writer.Write(output);
            }
            catch (IOException e)
            {
                Debug.Log(e.Message);
                binary_writer.Close();
                return;
            }
            binary_writer.Close();
        }
    }
    public class CalibrationParams
    {
        public Vector3 relative_angle;
        public Vector3 absolute_angle;
    }
}