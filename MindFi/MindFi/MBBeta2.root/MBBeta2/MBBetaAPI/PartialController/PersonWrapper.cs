using System;
using System.ComponentModel;

namespace MBBetaAPI
{
    /// <summary>
    /// Class to force notifications to WPF when objects change
    /// Based on http://joshsmithonwpf.wordpress.com/2007/03/18/updating-the-ui-when-binding-directly-to-business-objects-that-are-modified/
    /// </summary>
    
    public class PersonWrapper : INotifyPropertyChanged
    {
		#region Value

		Person value;

		public Person Value
		{
			get { return this.value; }
			protected set
			{
				this.value = value;

				if( this.value is INotifyPropertyChanged )
					(this.value as INotifyPropertyChanged).PropertyChanged += value_PropertyChanged;
			}
		}

		void value_PropertyChanged( object sender, PropertyChangedEventArgs e )
		{
			// This method raises the PropertyChanged event twice.  First it nullifies the 'value'
			// field and raises PropertyChanged.  Then it sets field back to the actual object and 
			// raises it again.  This is necessary because the WPF binding system will ignore a 
			// PropertyChanged notification if the property returns the same object reference as before.

			Person temp = this.value;

			this.value = null;
			this.OnPropertyChanged( "Value" );

			this.value = temp;
			this.OnPropertyChanged( "Value" );
		}

		#endregion // Value

		#region Constructor

		public PersonWrapper( Person value )
		{
			this.Value = value;
		}

		#endregion // Constructor

		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged( string propertyName )
		{
			if( this.PropertyChanged != null )
				this.PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
		}

		#endregion // INotifyPropertyChanged
    }
}
