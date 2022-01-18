namespace FluentAvalonia.UI.Controls
{
    public class ComboBoxTextSubmittedEventArgs
    {
        internal ComboBoxTextSubmittedEventArgs(string txt)
        {
            Text = txt;
        }

        public bool Handled { get; set; }
        public string Text { get; }
    }
}
