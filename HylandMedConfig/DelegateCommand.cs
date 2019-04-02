using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace HylandMedConfig
{
	/// <summary>
	/// Convenience class equivalent to the generic DelegateCommand of type object.
	/// <para>Encapsulates a Command with an execute Action and an optional canExecute Predicate.</para>
	/// </summary>
	public class DelegateCommand : DelegateCommand<object>
	{
		/// <summary>Create a DelegateCommand which takes an object input parameter during execution</summary>
		public DelegateCommand( Action<object> execute )
			: base( execute )
		{
		}

		/// <summary>Create a DelegateCommand which takes an object input parameter during test & execution</summary>
		public DelegateCommand( Predicate<object> canExecute, Action<object> execute )
			: base( canExecute, execute )
		{
		}

		/// <summary>Create a DelegateCommand which executes without input parameters</summary>
		public DelegateCommand( Action execute )
			: base( o => execute() )
		{
		}

		/// <summary>Create a DelegateCommand which executes without input parameters during test or execution</summary>
		public DelegateCommand( Func<bool> canExecute, Action execute )
			: base( o => canExecute(), o => execute() )
		{
		}
	}

	public abstract class CommandBase : ICommand
	{
		/// <summary>
		/// Defines the method that determines whether the command can execute in its current state.
		/// </summary>
		/// <returns>
		/// true if this command can be executed; otherwise, false.
		/// </returns>
		/// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
		public abstract bool CanExecute( object parameter );

		/// <summary>
		/// Defines the method to be called when the command is invoked.
		/// </summary>
		/// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
		public abstract void Execute( object parameter );

		/// <summary>
		/// Occurs when changes occur that affect whether or not the command should execute.
		/// </summary>
		public event EventHandler CanExecuteChanged
		{
			add
			{
				CommandManager.RequerySuggested += value;
			}
			remove
			{
				CommandManager.RequerySuggested -= value;
			}
		}
	}

	/// <summary>
	/// Encapsulates a Command with an execute Action and an optional canExecute Predicate.
	/// </summary>
	public class DelegateCommand<T> : CommandBase where T : class
	{
		private readonly Predicate<T> _canExecute;
		private readonly Action<T> _execute;

		#region Constructors

		/// <summary>
		/// Creates a new DelegateCommand which can always execute
		/// </summary>
		/// <param name="execute">The action to be executed when the command fires</param>
		public DelegateCommand( Action<T> execute )
			: this( null, execute )
		{
		}

		/// <summary>
		/// Creates a new DelegateCommand
		/// </summary>
		/// <param name="canExecute">A function to test whether the command can currently execute</param>
		/// <param name="execute">The action to be executed when the command fires</param>
		public DelegateCommand( Predicate<T> canExecute, Action<T> execute )
		{
			_canExecute = canExecute;
			_execute = execute;
		}

		#endregion

		#region Public Methods

		public override bool CanExecute( object parameter )
		{
			return _canExecute == null || _canExecute( (T)parameter );
		}

		public override void Execute( object parameter )
		{
			if( _execute == null )
				return;

			_execute( (T)parameter );
		}

		#endregion
	}
}
