using System;

namespace Visometry.VisionLib.SDK.Core.Details.Singleton
{
    public class DuplicateSingletonException : Exception
    {
        public DuplicateSingletonException(string message)
            : base(message) {}
    }

    public abstract class NullSingletonException : NullReferenceException
    {
        public NullSingletonException(string message)
            : base(message) {}
    }

    public class NullSingletonExceptionT<SingletonType> : NullSingletonException
    {
        public NullSingletonExceptionT()
            : base(
                "No object of type \"" + typeof(SingletonType) +
                "\" exists in the current scene.") {}
    }

    public class WrongTypeException<SingletonType> : InvalidCastException
    {
        public WrongTypeException(string desiredTypeName, string actualTypeName)
            : base(
                "Invalid Cast: The " + typeof(SingletonType).Name + " in the current scene is " +
                StringHelper.GetIndefiniteArticle(actualTypeName) + " " + actualTypeName +
                ", not " + StringHelper.GetIndefiniteArticle(desiredTypeName) + " " +
                desiredTypeName + ".") {}
    }

    /// <summary>
    ///     A <see cref="SingletonObjectReference"/> stores an immutable reference to a specific
    ///     object. This class is intended for use with objects of which only a single instance
    ///     exists in any give scene. A reference to an object can be registered exactly once in
    ///     each individual <see cref="SingletonObjectReference"/>.
    ///     Attempts to change the reference thereafter will raise a
    ///     <see cref="DuplicateSingletonException"/>.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Get/set the immutable object reference via
    ///         <see cref="SingletonObjectReference#Instance"/>.
    ///     </para>
    /// </remarks>
    /// @ingroup Core
    public class SingletonObjectReference<SingletonType> where SingletonType : UnityEngine.Object
    {
        private SingletonType instance;

        /// <summary>
        ///     Get/Set the stored immutable object reference. 
        /// </summary>
        /// <remarks>
        ///  <para>
        ///     Get the stored object reference: 
        ///     If the object reference is not set, an attempt to find a unique object of
        ///     type <see cref="SingletonType"/> in the scene is made.
        ///     If such an object is found (and if it is unique) the reference is irreversibly set
        ///     to this object and this new reference is returned.
        ///     The search attempt will fail with a <see cref="NullSingletonException"/> if no object
        ///     of type <see cref="SingletonType"/> was found. A <see cref="DuplicateSingletonException"/>
        ///     is thrown if more than one such object exists in the scene.
        ///  </para>
        ///  <para>
        ///     Set the stored object reference:
        ///     This only works if the reference is not set yet.
        ///     If the stored object reference is already set, attempting to overwrite Instance
        ///     with a reference to a different object will raise a
        ///     <see cref="DuplicateSingletonException"/>. Attempts to overwrite
        ///     the existing reference with a reference to the same object will be ignored.
        ///  </para>
        /// </remarks>
        /// @exception NullSingletonException, DuplicateSingletonException
        public SingletonType Instance
        {
            get
            {
                if (!this.instance)
                {
                    this.instance = GetUniqueObjectInstanceOrThrow();
                }
                return this.instance;
            }

            set
            {
                if (this.instance && !this.instance.Equals(value))
                {
                    throw new DuplicateSingletonException(
                        "Cannot overwrite the singleton object" +
                        " reference with a reference to a different object.");
                }
                this.instance = value;
            }
        }
        
        /// <summary>
        ///     Returns the instance reference stored in a <see cref="SingletonObjectReference"/>
        ///     as reference to DerivedType.
        /// </summary>
        /// <remarks>
        ///     In cases where an instance of a child class (DerivedType) is stored in a
        ///     <see cref="SingletonObjectReference"/> for its parent class (SingletonType),
        ///     this method returns the type-correct reference to the instance 
        ///     (with access to the full interface of DerivedType).
        ///  
        ///     Throws a <see cref="WrongTypeException"/> if unable to cast SingletonType 
        ///     into DerivedType.
        /// </remarks>
        /// <exception cref="WrongTypeException"/> 
        public DerivedType As<DerivedType>() where DerivedType : SingletonType
        {
            try
            {
                return (DerivedType) this.Instance;
            }
            catch (InvalidCastException)
            {
                throw new WrongTypeException<SingletonType>(
                    typeof(DerivedType).Name,
                    Instance.GetType().Name);
            }
        }

        private SingletonType GetUniqueObjectInstanceOrThrow()
        {
            var objectsOfTargetTypeInScene = UnityEngine.Object.FindObjectsOfType<SingletonType>();

            if (objectsOfTargetTypeInScene.Length == 0)
            {
                if (typeof(SingletonType) == typeof(TrackingManager))
                {
#pragma warning disable CS0618 // TrackingManagerNotFoundException is obsolete
                    throw new TrackingManagerReference.TrackingManagerNotFoundException();
#pragma warning disable CS0618 // TrackingManagerNotFoundException is obsolete
                }
                throw new NullSingletonExceptionT<SingletonType>();
            }

            if (objectsOfTargetTypeInScene.Length > 1)
            {
                throw new DuplicateSingletonException(
                    "Multiple objects of type \"" + typeof(SingletonType) +
                    "\" exist in the current scene.");
            }

            return objectsOfTargetTypeInScene[0];
        }
    }
}
