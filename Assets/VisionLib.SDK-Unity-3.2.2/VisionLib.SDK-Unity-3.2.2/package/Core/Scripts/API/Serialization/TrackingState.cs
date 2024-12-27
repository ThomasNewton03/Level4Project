using System;
using System.Collections.Generic;

namespace Visometry.VisionLib.SDK.Core.API
{
    /// <summary>
    ///  TrackingState stores the tracking states of all tracking object.
    /// </summary>
    /// @ingroup API
    [Serializable]
    public class TrackingState
    {
        /// <summary>
        ///  Anchor stores the tracking state of one <see cref="TrackingAnchor"/>.
        /// </summary>
        [Serializable]
        public class Anchor
        {
            /// <summary>
            /// Validation result of a specific model inside the anchor.
            /// </summary>
            [Serializable]
            public class ModelValidationResult
            {
                [Serializable]
                public class BoundingBox
                {
                    public double minX;
                    public double minY;
                    public double maxX;
                    public double maxY;
                }

                /// <summary>
                /// Name of the model. The modelName is unique inside one anchor.
                /// </summary>
                public string modelName;

                /// <summary>
                /// The number of correspondences which have been detected in the current frame for
                /// this particular model.
                /// </summary>
                public int numberOfCorrespondences;

                /// <summary>
                /// Tracking quality for this model in the current frame. The range of the quality
                /// is from 0.0 (worst quality) to 1.0 (best quality).
                /// </summary>
                public double quality;

                /// <summary>
                /// Value describing the area of the image which is covered by the visible parts of
                /// the model. The range of the imageCoverage is between 0.0 and 1.0.
                /// </summary>
                /// \deprecated The imageCoverage is no longer provided. Use the bounding box instead
                [Obsolete("The imageCoverage is no longer provided. Use the bounding box instead")]
                public double imageCoverage;

                /// <summary>
                /// Value describing the area of the image which is covered by the visible parts of
                /// the model. The single position values are normalized with the image size so they
                /// range from 0.0 to 1.0.
                /// </summary>
                public BoundingBox modelPosition;

                public string ToDisplayString()
                {
                    string str = "";
                    str += this.modelName + "\n";
                    str += "* Quality: " + this.quality + "\n";
                    str += $"* ModelPosition:\n" +
                           $"    ({this.modelPosition.minX:F4}, {this.modelPosition.minY:F4})\n" +
                           $"    ({this.modelPosition.maxX:F4}, {this.modelPosition.maxY:F4})\n";
                    str += "* NumberOfCorrespondences: " + this.numberOfCorrespondences + "\n";
                    return str;
                }
            }

            /// <summary>Name of the tracking object.</summary>
            /// <remarks>
            ///  Currently only one tracking object is supported and the name is
            ///  always 'TrackedObject'.
            /// </remarks>
            public string name;

            /// <summary>Tracking state</summary>
            /// <remarks>
            ///  Can be one of the following:
            ///  * "tracked": Object was tracked successful
            ///  * "critical": Object was tracked, but something disturbs the
            ///    tracking (e.g. motion blur or bad illumination). If the tracking
            ///    stays critical for too long, then the state might change to
            ///    "lost".
            ///  * "lost": Object could not be tracked.
            /// </remarks>
            public string state;

            /// <summary>
            ///  Quality value between 0.0 (worst quality) and 1.0 (best quality).
            ///  The concrete meaning depends on the used tracking method.
            /// </summary>
            public float quality;

            public float _InitInlierRatio;
            public int _InitNumOfCorresp;
            public float _TrackingInlierRatio;
            public int _TrackingNumOfCorresp;
            public float _SFHFrameDist;
            public int _NumberOfPatternRecognitions;
            public int _NumberOfTemplates;
            public int _NumberOfTemplatesDynamic;
            public int _NumberOfTemplatesStatic;
            public int _NumberOfLineModels;
            public float _AutoInitSetupProgress;
            public string _AutoInitSetupState;
            public float _AutoInitSuccessfullyLearnedPosesRatio;
            public int _TrackingImageWidth;
            public int _TrackingImageHeight;
            public float _PoseSingularValueRatio;
            public float _PoseInverseConditionNumber;
            public float finalError;

            /// <summary>
            ///  The timestamp in seconds from 1.1.1970 and parts of the seconds as fraction.
            ///  Describes, when the process of the image has been started.
            /// </summary>
            public double timeStamp;

            /// <summary>
            /// This array contains the quality and the number of correspondences for every model in
            /// the current TrackingObject.
            /// </summary>
            public ModelValidationResult[] models;

            public string ToDisplayString()
            {
                string str = "";
                str += this.name + "\n";
                str += "* State: " + this.state + "\n";
                str += "* Quality: " + this.quality + "\n";
                str += "* _InitInlierRatio: " + this._InitInlierRatio + "\n";
                str += "* _InitNumOfCorresp: " + this._InitNumOfCorresp + "\n";
                str += "* _TrackingInlierRatio: " + this._TrackingInlierRatio + "\n";
                str += "* _TrackingNumOfCorresp: " + this._TrackingNumOfCorresp + "\n";
                str += "* _SFHFrameDist: " + this._SFHFrameDist + "\n";
                str += "* _NumberOfTemplates: " + this._NumberOfTemplates + "\n";
                str += "* _NumberOfTemplatesDynamic: " + this._NumberOfTemplatesDynamic + "\n";
                str += "* _NumberOfTemplatesStatic: " + this._NumberOfTemplatesStatic + "\n";
                str += "* _TrackingImageSize: " + this._TrackingImageWidth + "x" +
                       this._TrackingImageHeight + "\n";
                str += "* _PoseSingularValueRatio: " + this._PoseSingularValueRatio + "\n";
                str += "* _PoseInverseConditionNumber: " + this._PoseInverseConditionNumber + "\n";
                str += this.models == null || this.models.Length == 0 ? "" : "\nModels:";
                foreach (var model in this.models)
                {
                    str += "\n" + model.ToDisplayString();
                }
                return str;
            }
        }

        [Serializable]
        public class Device
        {
            /// <summary>Name of the device as specified in the TrackingConfiguration.</summary>
            public string name;
            public string _WorldMappingStatus;

            public string ToDisplayString()
            {
                return this.name + "\n* _WorldMappingStatus: " + this._WorldMappingStatus + "\n";
            }
        }

        /// <summary>
        ///  Array with the tracking state of all tracking objects.
        /// </summary>
        public Anchor[] objects;
        public Device[] inputs;

        public string ToDisplayString()
        {
            List<string> entries = new List<string>();
            if (this.objects != null)
            {
                foreach (var obj in this.objects)
                {
                    entries.Add(obj.ToDisplayString());
                }
            }
            if (this.inputs != null)
            {
                foreach (var input in this.inputs)
                {
                    entries.Add(input.ToDisplayString());
                }
            }
            return String.Join("\n", entries.ToArray());
        }
    }
}
