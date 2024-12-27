using System;

namespace Visometry.VisionLib.SDK.Core.API
{
    /// <summary>
    ///  ModelDeserializationResult stores associated data of the models.
    /// <remarks>
    /// <para>
    ///  ModelDeserializationResult stores associated data of the models, which have been
    ///  added to tracking system. It contains, name, id and licensing informations.
    ///  NOTE: THIS PIECE OF CODE IS CONSIDERED AS BETA AND MIGHT BE SUBJECT TO CHANGE 
    ///  </para>
    /// </remarks>
    /// </summary>
    /// @ingroup API
    [Serializable]
    public class ModelDeserializationResult
    {
        /// <summary>
        /// The name of the model added to the tracking system.
        /// </summary>
        public string name;

        /// <summary>
        /// Model hash of the data added to the tracking system. These strings can
        /// be used to acquire new license features for these models.
        /// </summary>
        public string licenseFeature;
    }

    /// <summary>
    ///  ModelDeserializationResultList stores the internal states of the model which
    ///  is used for model based tracking. Such an object is usually passed, when
    ///  calling Worker.PushJsonAndBinaryCommand with an addModelData command.
    /// </summary>
    /// @ingroup API
    [Serializable]
    public class ModelDeserializationResultList
    {
        public ModelDeserializationResult[] addedModels;
        
        /// <summary>
        /// Warnings which occurred during deserialization of models.
        /// </summary>
        public Issue[] warnings;
    }
}
