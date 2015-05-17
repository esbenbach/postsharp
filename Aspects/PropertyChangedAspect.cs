// <copyright file="PropertyChangedAspect.cs">
// 
// </copyright>
// <author></author>
// <email></email>
// <date></date>
// <summary></summary>
namespace Aspects
{
   using System;
    using System.ComponentModel;
    using PostSharp.Aspects;
    using PostSharp.Aspects.Advices;
    using PostSharp.Aspects.Dependencies;
    using PostSharp.Extensibility;
    using PostSharp.Reflection;
 
    /// <summary>
    /// Aspect that, when applied on a class, fully implements the interface 
    /// <see cref="INotifyPropertyChanged"/> into that class, and overrides all properties to
    /// that they raise the event <see cref="INotifyPropertyChanged.PropertyChanged"/>.
    /// </summary>
    /// <remarks>
    /// Its a rather simple variety and for more complex tasks like notifying parents/children the one provided by postsharps model library is probably better</remarks>
    [Serializable]
    [IntroduceInterface(typeof(INotifyPropertyChanged), OverrideAction = InterfaceOverrideAction.Ignore)]
    [ProvideAspectRole(StandardRoles.DataBinding)]
    [MulticastAttributeUsage(MulticastTargets.Class, Inheritance = MulticastInheritance.Strict)]
    public sealed class PropertyChangedAspect : InstanceLevelAspect, INotifyPropertyChanged
    {
        /// <summary>
        /// Field for holding the delegate of the method <c>OnPropertyChanged</c>.
        /// </summary>
        [ImportMember("OnPropertyChanged", IsRequired = false)]
        public Action<string> PropertyChangedEventMethod;
 
        /// <summary>
        /// Event introduced in the target type (unless it is already present);
        /// raised whenever a property has changed.
        /// </summary>
        [IntroduceMember(OverrideAction = MemberOverrideAction.Ignore)]
        public event PropertyChangedEventHandler PropertyChanged;
 
        /// <summary>
        /// Method introduced in the target type (unless it is already present);
        /// raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        [IntroduceMember(Visibility = Visibility.Family, IsVirtual = true, OverrideAction = MemberOverrideAction.Ignore)]
        public void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this.Instance, new PropertyChangedEventArgs(propertyName));
            }
        }
 
        /// <summary>
        /// Method intercepting any call to a property setter.
        /// </summary>
        /// <param name="args">Aspect arguments.</param>
        [OnLocationSetValueAdvice, MulticastPointcut(Targets = MulticastTargets.Property, Attributes = MulticastAttributes.Instance)]
        public void OnPropertySet(LocationInterceptionArgs args)
        {
            // Don't go further if the new value is equal to the old one.
            if (args.Value == args.GetCurrentValue())
            {
                return;
            }
 
            // Actually sets the value.
            args.ProceedSetValue();
 
            // Invoke method OnPropertyChanged (our, the base one, or the overridden one).
            this.PropertyChangedEventMethod.Invoke(args.Location.Name);
        }
    }
}
