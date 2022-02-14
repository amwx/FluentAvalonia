using Avalonia.Input;
using System;
using System.Collections.Generic;

namespace FluentAvalonia.UI.Data
{
	/// <summary>
	/// This class is part of the ListView logic, which has been suspended for now
	/// </summary>
	public class DataPackage : IDataObject
	{
		public DragDropEffects RequestedOperation { get; set; }

		public bool Contains(string dataFormat) =>
			_data.ContainsKey(dataFormat);

		public object Get(string dataFormat) =>
			_data.TryGetValue(dataFormat, out var value) ? value :
			throw new ArgumentException($"No data format of {dataFormat} was found in the data package");

		public IEnumerable<string> GetDataFormats() =>
			_data.Keys;

		public IEnumerable<string> GetFileNames() =>
			Get(DataFormats.FileNames) as IEnumerable<string>;

		public string GetText() =>
			Get(DataFormats.Text) as string;

		public void SetText(string txt) =>
			_data.Add(DataFormats.Text, txt);

		public void SetData(string format, object value) =>
			_data.Add(format, value);

		private readonly Dictionary<string, object> _data = new Dictionary<string, object>();
	}
}
