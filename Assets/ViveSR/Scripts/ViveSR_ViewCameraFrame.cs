using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vive.Plugin.SR
{
    public class ViveSR_ViewCameraFrame : MonoBehaviour
    {
        public void SetCorrectSize(int cameraWidth, int cameraHeight, float cameraFocalLength)
        {
            float ImageWidth = cameraWidth;
            float ImageHeight = cameraHeight;
            float ImageAspectRatio = ImageWidth / ImageHeight;

            // Get the distance of the image plane to the camera.
            // ASSUME the plane is in the z direction of the camera.
            float ImagePlaneDisanceZ = transform.localPosition.z;
            float FocalLength = (float)cameraFocalLength;

            // Calculate the correct size of the image plane according to the size of
            // the original images, the image plane distance and the focal length.
            var PlaneMesh = GetComponent<MeshFilter>().mesh;    // Copy from the default quad mesh.
            Vector3[] OriginalVertices = PlaneMesh.vertices;  // Get a copy of the vertices of the plane.
            Vector3 UpperRightMostVertex = new Vector3(float.MinValue, float.MinValue, 0);
            Vector3 LowerLeftMostVertex = new Vector3(float.MaxValue, float.MaxValue, 0);
            for (int i = 0; i < OriginalVertices.Length; i++)
            {
                UpperRightMostVertex = Vector3.Max(UpperRightMostVertex, OriginalVertices[i]);
                LowerLeftMostVertex = Vector3.Min(LowerLeftMostVertex, OriginalVertices[i]);
            }
            float ImagePlaneWidth = UpperRightMostVertex.x - LowerLeftMostVertex.x;
            float ImagePlaneHeight = UpperRightMostVertex.y - LowerLeftMostVertex.y;
            float ImagePlaneAspectRatio = ImagePlaneWidth / ImagePlaneHeight;
            // Create the transformation matrices.
            // Translate to the geometric center.
            Vector3 geometric_center = (UpperRightMostVertex + LowerLeftMostVertex) / 2;
            Matrix4x4 translation_to_geomatric_center = Matrix4x4.TRS(-1 * geometric_center, Quaternion.identity, Vector3.one);
            // Scale x and y to fit the vertical FOV.
            float fov_scale_factor = ((ImageHeight / FocalLength) * ImagePlaneDisanceZ) / ImagePlaneHeight;
            Matrix4x4 scaling_for_correct_fov = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(fov_scale_factor, fov_scale_factor, 1));
            // Scale x to fit the aspect ratio.
            float aspect_ratio_scale_factor = ImageAspectRatio / ImagePlaneAspectRatio;
            Matrix4x4 scaling_for_correct_aspect_ratio = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(aspect_ratio_scale_factor, 1, 1));
            // Translate back to the origin by the inverse matrix.
            // Combine all transformations.
            Matrix4x4 transformation_for_original_image = translation_to_geomatric_center.inverse * scaling_for_correct_aspect_ratio * scaling_for_correct_fov * translation_to_geomatric_center;
            // Apply the transformation.
            for (int i = 0; i < OriginalVertices.Length; i++)
            {
                OriginalVertices[i] = transformation_for_original_image.MultiplyPoint3x4(OriginalVertices[i]);
            }
            // Assign the vertices for the correct image plane size.
            PlaneMesh.vertices = OriginalVertices;
        }
        public void SetFrame(float blockRate)
        {
            int Height = 100;
            int Width = 100;
            Texture2D Tex= new Texture2D(Width, Height, TextureFormat.RGBA32, false);
            Color[] Texels = Tex.GetPixels();
            int Xbond = (int)((float)Width * (1.0f - blockRate) / 2.0f);
            int Ybond = (int)((float)Height * (1.0f - blockRate) / 2.0f);
            for (int i=0;i< Texels.Length;i++)
            {
                int x = i % Width;
                int y = i / Width;
                if(x >Xbond && x < Width-Xbond && y >Ybond && y < Height-Ybond)
                {
                    Texels[i] = Color.clear;
                }
                else
                {
                    Texels[i] = Color.black;
                }
            }
            Tex.SetPixels(Texels);
            Tex.Apply();

            GetComponent<Renderer>().material.mainTexture = Tex;
        }
    }
}
